using Microsoft.Extensions.Logging;
using RemixHub.Shared.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using TagLib;

namespace RemixHub.Server.Services
{
    public interface IAudioMetadataService
    {
        Task<AudioMetadata> ExtractMetadataAsync(string filePath);
    }

    public class AudioMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int? DurationSeconds { get; set; }
        public int BitRate { get; set; }
        public int SampleRate { get; set; }
        public int? Bpm { get; set; }
        public string MusicalKey { get; set; } = string.Empty;
    }

    public class AudioMetadataService : IAudioMetadataService
    {
        private readonly ILogger<AudioMetadataService> _logger;

        public AudioMetadataService(ILogger<AudioMetadataService> logger)
        {
            _logger = logger;
        }

        public async Task<AudioMetadata> ExtractMetadataAsync(string filePath)
        {
            // Wrap in Task.Run since TagLib operations are synchronous
            return await Task.Run(() => {
                try
                {
                    using (var file = TagLib.File.Create(filePath))
                    {
                        var metadata = new AudioMetadata
                        {
                            Title = file.Tag?.Title ?? Path.GetFileNameWithoutExtension(filePath),
                            Artist = file.Tag?.FirstPerformer ?? "Unknown Artist",
                            Album = file.Tag?.Album ?? "Unknown Album",
                            Genre = file.Tag?.FirstGenre ?? "Unknown Genre",
                            DurationSeconds = (int)file.Properties.Duration.TotalSeconds,
                            BitRate = file.Properties.AudioBitrate,
                            SampleRate = file.Properties.AudioSampleRate,
                            Bpm = null, // Will try to extract from metadata
                            MusicalKey = "Unknown" // Will try to extract from metadata
                        };

                        // Try to extract BPM if present
                        if (file.Tag != null && file.Tag.BeatsPerMinute > 0)
                        {
                            metadata.Bpm = (int)file.Tag.BeatsPerMinute;
                        }

                        // Try to find musical key in the tag fields
                        // Check for ID3v2 tags specifically
                        if (file.TagTypes.HasFlag(TagLib.TagTypes.Id3v2))
                        {
                            var id3v2 = file.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
                            if (id3v2 != null)
                            {
                                var frames = id3v2.GetFrames();
                                if (frames != null)
                                {
                                    var keyFrame = frames
                                        .FirstOrDefault(f => 
                                            (f?.FrameId != null && f.FrameId.StartsWith("TKEY")) || // Fix null reference
                                            (f != null && f.ToString() != null && f.ToString().Contains("Key")));
                                        
                                    if (keyFrame != null)
                                    {
                                        var keyText = keyFrame.ToString();
                                        if (!string.IsNullOrEmpty(keyText))
                                        {
                                            metadata.MusicalKey = keyText.Replace("TKEY:", "").Trim();
                                        }
                                    }
                                }
                            }
                        }

                        return metadata;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting metadata from file: {FilePath}", filePath);
                    
                    // Return basic metadata
                    return new AudioMetadata
                    {
                        Title = Path.GetFileNameWithoutExtension(filePath),
                        Artist = "Unknown Artist",
                        Album = "Unknown Album",
                        Genre = "Unknown Genre",
                        DurationSeconds = 0,
                        BitRate = 0,
                        SampleRate = 0
                    };
                }
            });
        }
    }
}
