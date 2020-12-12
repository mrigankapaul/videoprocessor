using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VideoProcessor
{
    public static class ProcessVideoStarter
    {
        [FunctionName("ProcessVideoStarter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // parse query parameter
            string video = req.Query["video"];


            // Get request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<RequestBody>(requestBody);

            // Set name to query string or body data
            video = video ?? data?.video;

            if (video == null)
            {
                
                return new BadRequestObjectResult(
                    $"Please pass the video location the query string or in the request body"
                    );
                
            }

            log.LogInformation($"About to start orchestration for {video}");
            string orchestrationId = await starter.StartNewAsync("O_ProcessVideo", video);
            return starter.CreateCheckStatusResponse(req, orchestrationId);
        }
    }

    public class RequestBody
    {
        public string video { get; set; }
    }
}
