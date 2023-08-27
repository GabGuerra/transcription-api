using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using transcription.api.domain.Services;

namespace transcription.api.infrastructure.Services
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly AmazonTranscribeServiceClient _transcriptionClient;
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName = "transcriptions-word-cloud";        

        public TranscriptionService()
        {
            _transcriptionClient = new AmazonTranscribeServiceClient();
            _s3Client = new AmazonS3Client();
        }



        //TODO: make it return a transcript result with bot transcription and its id
        public async Task<string> TranscriptAudioAsync(MemoryStream audioStream)
        {
            var transcriptionJobName = Guid.NewGuid().ToString();
            await UploadToS3Async(audioStream, transcriptionJobName);
            var transcriptionRequest = new StartTranscriptionJobRequest
            {
                LanguageCode = LanguageCode.PtBR,
                Media = new Media
                {
                    MediaFileUri = "https://s3.amazonaws.com/aws-transcribe-demo-us-east-1/hello_world.wav"
                },
                MediaFormat = MediaFormat.Wav,
                TranscriptionJobName = transcriptionJobName,
                OutputBucketName = _bucketName,
            };

            await _transcriptionClient.StartTranscriptionJobAsync(transcriptionRequest);

            return await GetTranscriptionResultAsync(transcriptionJobName);
        }

        private async Task<string> GetTranscriptionResultAsync(string transcriptionJobName)
        {
            var file = await PollForTranscriptionFileAsync(transcriptionJobName);

            using var reader = new StreamReader(file);
            var result = await reader.ReadToEndAsync();

            return result;
        }

        private async Task<Stream> PollForTranscriptionFileAsync(string transcriptionJobName)
        {
            var getTranscriptionRequest = new GetTranscriptionJobRequest
            {
                TranscriptionJobName = transcriptionJobName
            };

            var finishedStatuses = new string[] { TranscriptionJobStatus.COMPLETED, TranscriptionJobStatus.FAILED };
            TranscriptionJobStatus? status;
            Stream stream = null;
            var attempts = 0;
            do
            {
                attempts++;
                var transcription = await _transcriptionClient.GetTranscriptionJobAsync(getTranscriptionRequest);
                status = transcription.TranscriptionJob.TranscriptionJobStatus;

                if (status == TranscriptionJobStatus.COMPLETED)
                    stream = await GetTranscriptionFileAsync(transcription.TranscriptionJob);
                else if (status == TranscriptionJobStatus.FAILED)
                    throw new Exception("Transcription failed");

                await Task.Delay(5000);

            } while (!finishedStatuses.Contains(status.Value) && attempts <= 5);            

            return stream;
        }

        private async Task UploadToS3Async(MemoryStream audioStream, string transcriptionId)
        {

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(audioStream, _bucketName, transcriptionId);
        }

        private async Task<Stream> GetTranscriptionFileAsync(TranscriptionJob job)
        {
            var s3fileKey = $"{job.TranscriptionJobName}.json";
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = s3fileKey
            };

            var response = await _s3Client.GetObjectAsync(request);

            return response.ResponseStream;
        }
    }
}
