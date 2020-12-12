using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace VideoProcessor
{
    public static class ProcessVideoOrchestrators
    {
        [FunctionName("O_ProcessVideo")]
        public static async Task<object> ProcessVideo(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            var videoLocation = ctx.GetInput<string>();

            if (!ctx.IsReplaying)
                log.LogInformation("About to call transcode video activity");

            var transcodedLocation = await
                ctx.CallActivityAsync<string>("A_TranscodeVideo", videoLocation);

            if (!ctx.IsReplaying)
                log.LogInformation("About to call extract thumbnail");

            var thumbnailLocation = await
                ctx.CallActivityAsync<string>("A_ExtractThumbnail", transcodedLocation);

            if (!ctx.IsReplaying)
                log.LogInformation("About to call prepend intro");

            var withIntroLocation = await
                ctx.CallActivityAsync<string>("A_PrependIntro", transcodedLocation);

            return new
            {
                Transcoded = transcodedLocation,
                Thumbnail = thumbnailLocation,
                WithIntro = withIntroLocation
            };

        }
    }
}
