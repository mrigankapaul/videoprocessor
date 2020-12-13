using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;

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
                log.LogInformation($"About to call transcode video activity {videoLocation}");

            string transcodedLocation = null;
            string thumbnailLocation = null;
            string withIntroLocation = null;

            try
            {
                transcodedLocation = await
                    ctx.CallActivityAsync<string>("A_TranscodeVideo", videoLocation);

                if (!ctx.IsReplaying)
                    log.LogInformation("About to call extract thumbnail");

                thumbnailLocation = await
                    ctx.CallActivityAsync<string>("A_ExtractThumbnail", transcodedLocation);

                if (!ctx.IsReplaying)
                    log.LogInformation("About to call prepend intro");

                withIntroLocation = await
                    ctx.CallActivityAsync<string>("A_PrependIntro", transcodedLocation);

            }
            catch (Exception e)
            {
                if (!ctx.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}");

                await
                    ctx.CallActivityAsync<string>("A_Cleanup",
                        new[] { transcodedLocation, thumbnailLocation, withIntroLocation });

                return new
                {
                    Error = "Failed to process uploaded video",
                    Message = e.Message
                };

            }

            return new
            {
                Transcoded = transcodedLocation,
                Thumbnail = thumbnailLocation,
                WithIntro = withIntroLocation
            };

        }
    }
}
