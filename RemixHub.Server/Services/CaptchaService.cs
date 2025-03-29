using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace RemixHub.Server.Services
{
    public interface ICaptchaService
    {
        (string Key, string CaptchaText, string Image) GenerateCaptcha();
        bool ValidateCaptcha(string key, string userInput);
    }

    public class CaptchaService : ICaptchaService
    {
        private static readonly ConcurrentDictionary<string, CaptchaEntry> CaptchaStore = new();

        private class CaptchaEntry
        {
            public string Answer { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        private readonly float _rotationRange;
        private readonly float _offsetRange;
        private readonly int _noiseLineCount;
        private readonly int _dotCount;
        private readonly SKColor _foregroundColor;
        private readonly SKColor _backgroundColor;
        private readonly byte _noiseOpacity;
        private readonly Random rnd = new Random();
        private readonly ILogger<CaptchaService> _logger;

        public CaptchaService(IConfiguration configuration, ILogger<CaptchaService> logger)
        {
            _rotationRange = configuration.GetValue<float>("Captcha:RotationRange", 30);
            _offsetRange = configuration.GetValue<float>("Captcha:OffsetRange", 5);
            _noiseLineCount = configuration.GetValue<int>("Captcha:NoiseLineCount", 5);
            _dotCount = configuration.GetValue<int>("Captcha:DotCount", 100);
            var fgColorString = configuration.GetValue<string>("Captcha:ForegroundColor", "#000000");
            var bgColorString = configuration.GetValue<string>("Captcha:BackgroundColor", "#D3D3D3");
            _foregroundColor = SKColor.Parse(fgColorString);
            _backgroundColor = SKColor.Parse(bgColorString);
            _noiseOpacity = configuration.GetValue<byte>("Captcha:NoiseOpacity", 128);
            _logger = logger;
        }

        public (string Key, string CaptchaText, string Image) GenerateCaptcha()
        {
            CleanExpiredCaptchas();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 5;
            char[] captchaChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                captchaChars[i] = chars[rnd.Next(chars.Length)];
            }
            string captcha = new string(captchaChars);

            string key = Guid.NewGuid().ToString();
            CaptchaStore[key] = new CaptchaEntry
            {
                Answer = captcha,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            };

            _logger.LogInformation("Generated captcha with text '{CaptchaText}' and key '{Key}'", captcha, key);

            int width = 220, height = 80;
            using SKBitmap bitmap = new SKBitmap(width, height);
            using SKCanvas canvas = new SKCanvas(bitmap);
            canvas.Clear(_backgroundColor);

            using SKPaint paint = new SKPaint
            {
                Color = _foregroundColor,
                IsAntialias = true,
                TextSize = 40,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            float charWidth = width / (float)length;
            for (int i = 0; i < length; i++)
            {
                string letter = captcha[i].ToString();
                canvas.Save();
                float x = i * charWidth + charWidth / 2;
                float y = height / 2 + 15;
                float rotation = rnd.NextDouble() < 0.5 ? -rnd.Next(0, (int)_rotationRange) : rnd.Next(0, (int)_rotationRange);
                canvas.Translate(x, y);
                canvas.RotateDegrees(rotation);
                float offsetX = rnd.Next((int)-_offsetRange, (int)_offsetRange + 1);
                float offsetY = rnd.Next((int)-_offsetRange, (int)_offsetRange + 1);
                var bounds = new SKRect();
                paint.MeasureText(letter, ref bounds);
                canvas.DrawText(letter, offsetX - bounds.MidX, offsetY - bounds.MidY, paint);
                canvas.Restore();
            }

            using SKPaint linePaint = new SKPaint
            {
                StrokeWidth = 2,
                IsAntialias = true,
                Color = SKColors.DarkGray.WithAlpha(_noiseOpacity)
            };
            for (int i = 0; i < _noiseLineCount; i++)
            {
                float x1 = rnd.Next(0, width);
                float y1 = rnd.Next(0, height);
                float x2 = rnd.Next(0, width);
                float y2 = rnd.Next(0, height);
                canvas.DrawLine(x1, y1, x2, y2, linePaint);
            }

            using SKPaint dotPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.DarkGray.WithAlpha(_noiseOpacity)
            };
            for (int i = 0; i < _dotCount; i++)
            {
                float dotX = rnd.Next(0, width);
                float dotY = rnd.Next(0, height);
                float radius = rnd.Next(1, 4);
                canvas.DrawCircle(dotX, dotY, radius, dotPaint);
            }

            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            string base64Image = Convert.ToBase64String(data.ToArray());
            string imageDataUrl = $"data:image/png;base64,{base64Image}";

            return (key, captcha, imageDataUrl);
        }

        private void CleanExpiredCaptchas()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = CaptchaStore
                .Where(kvp => kvp.Value.ExpirationTime < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                CaptchaStore.TryRemove(key, out _);
            }
        }

        public bool ValidateCaptcha(string key, string userInput)
        {
            _logger.LogDebug("Validating CAPTCHA - Key: '{Key}', UserInput: '{UserInput}'", key, userInput);

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(userInput))
            {
                _logger.LogDebug("CAPTCHA validation failed: key or input is empty");
                return false;
            }

            _logger.LogDebug("Current CAPTCHA store has {Count} entries", CaptchaStore.Count);
            foreach (var entry in CaptchaStore)
            {
                _logger.LogDebug("Store entry - Key: {StoreKey}, Answer: {Answer}, Expires: {ExpiryTime}",
                    entry.Key, entry.Value.Answer, entry.Value.ExpirationTime);
            }

            if (CaptchaStore.TryGetValue(key, out var captchaEntry))
            {
                bool valid = userInput.Trim().Equals(captchaEntry.Answer, StringComparison.OrdinalIgnoreCase);
                _logger.LogDebug("CAPTCHA found in store - Correct Answer: '{CorrectAnswer}', User Input: '{UserInput}', Valid: {Valid}",
                    captchaEntry.Answer, userInput, valid);

                if (valid)
                {
                    CaptchaStore.TryRemove(key, out _);
                }
                return valid;
            }

            _logger.LogWarning("CAPTCHA key '{Key}' not found in store", key);
            return false;
        }
    }
}
