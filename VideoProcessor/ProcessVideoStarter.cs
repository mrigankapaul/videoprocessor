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
            log.LogInformation($"Video - Mriganka - {video}");
            string orchestrationId = await starter.StartNewAsync<string>("O_ProcessVideo", video);
            return starter.CreateCheckStatusResponse(req, orchestrationId);
        }

        [FunctionName("SubmitVideoApproval")]
        public static async Task<IActionResult> SubmitVideoApproval(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SubmitVideoApproval/{id}")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            [Table("Approvals", "Approval", "{id}", Connection = "AzureWebJobsStorage")] Approval approval,
            ILogger log)
        {
            // nb if the approval code doesn't exist, framework just returns a 404 before we get here
            string result = req.Query["result"];

            if (result == null)
            {
                return new BadRequestObjectResult($"Need an approval result");

            }

            log.LogWarning($"Sending approval result to {approval.OrchestrationId} of {result}");
            // send the ApprovalResult external event to this orchestration
            await client.RaiseEventAsync(approval.OrchestrationId, "ApprovalResult", result);

            return new OkResult();
        }

        [FunctionName("StartPeriodicTask")]
        public static async Task<IActionResult> StartPeriodicTask(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            var instanceId = await client.StartNewAsync<int>("O_PeriodicTask", null, 0);
            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }



    public class RequestBody
    {
        public string video { get; set; }
    }
}
