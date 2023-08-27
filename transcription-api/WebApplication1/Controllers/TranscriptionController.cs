using Microsoft.AspNetCore.Mvc;
using transcription.api.domain.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranscriptionController : ControllerBase
    {
        private readonly int FIVE_MB = 5 * 1024 * 1024;
        private readonly ITranscriptionService _transcriptionService;

        public TranscriptionController(ITranscriptionService transcriptionService)
        {
            _transcriptionService = transcriptionService;
        }

        [HttpPost("/transcribe")]
        public async Task<IActionResult> GetTranscription(IFormFile file)
        {

            if (file.Length > FIVE_MB)
                return BadRequest("Please, input a file with 5MB or less.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var transcription = _transcriptionService.TranscriptAudioAsync(memoryStream);

            return Ok(transcription);
        }
    }
}