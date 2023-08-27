using Microsoft.Extensions.DependencyInjection;
using transcription.api.domain.Services;
using transcription.api.infrastructure.Services;

namespace transcription.api.infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITranscriptionService, TranscriptionService>();
        }
    }
}
