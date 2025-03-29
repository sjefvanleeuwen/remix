using Microsoft.AspNetCore.Mvc;
using RemixHub.Server.Services;

namespace RemixHub.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService _captchaService;

        public CaptchaController(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        [HttpGet("generate")]
        public IActionResult Generate()
        {
            var (key, captcha, image) = _captchaService.GenerateCaptcha();
            // Return the key and image
            return Ok(new { Key = key, Image = image });
        }
    }
}
