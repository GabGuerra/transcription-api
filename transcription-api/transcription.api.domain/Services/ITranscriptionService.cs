namespace transcription.api.domain.Services
{
    public interface ITranscriptionService
    {
        Task<string> TranscriptAudioAsync(MemoryStream audioStream);
    }
}
