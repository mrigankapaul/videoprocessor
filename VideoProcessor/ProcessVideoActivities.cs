using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.IO;
using System;
using System.Linq;

namespace VideoProcessor
{
    public static class ProcessVideoActivities
    {
        [FunctionName("A_TranscodeVideo")]
        public static async Task<string> TranscodeVideo(
            [ActivityTrigger] string inputVideo,
            ILogger log)
        {
            log.LogInformation($"Transcoding {inputVideo}");
            // simulate doing the activity
            await Task.Delay(5000);
            log.LogInformation($"Path of the file - {inputVideo}");
            return $"{Path.GetFileNameWithoutExtension(inputVideo)}-transcoded.mp4";
        }

        [FunctionName("A_ExtractThumbnail")]
        public static async Task<string> ExtractThumbnail(
            [ActivityTrigger] string inputVideo,
            ILogger log)
        {
            log.LogInformation($"Extracting Thumbnail {inputVideo}");
            if (inputVideo.Contains("error"))
            {
                throw new InvalidOperationException("Could not extract thumbnail");
            }
            // simulate doing the activity
            await Task.Delay(5000);
            return "thumbnail.png";
        }

        [FunctionName("A_PrependIntro")]
        public static async Task<string> PrependIntro(
            [ActivityTrigger] string inputVideo,
            ILogger log)
        {
            log.LogInformation($"Appending intro to video {inputVideo}");
            //var introLocation = ConfigurationManager.AppSettings["IntroLocation"];
            // simulate doing the activity
            await Task.Delay(5000);

            return "withIntro.mp4";
        }

        [FunctionName("A_Cleanup")]
        public static async Task<string> Cleanup(
            [ActivityTrigger] string[] filesToCleanUp,
            ILogger log)
        {
            foreach (var file in filesToCleanUp.Where(f => f != null))
            {
                log.LogInformation($"Deleting {file}");
                // simulate doing the activity
                await Task.Delay(1000);
            }
            return "Cleaned up successfully";
        }

    }
}
