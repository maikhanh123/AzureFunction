using System;
using System.Threading.Tasks;
using func02.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace func02
{
    public static class QueueTriggerCSharp
    {
        [FunctionName("QueueTriggerCSharp")]
        public static async Task Run(
            [QueueTrigger("todos", Connection = "AzureWebJobsStorage")]Todo todo, 
            [Blob("todos", Connection = "AzureWebJobsStorage")]CloudBlobContainer container,
            ILogger log
        )
        {
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference($"{todo.Id}.txt");
            await blob.UploadTextAsync($"Create a new Task: {todo.TaskDescription}");
            log.LogInformation($"C# Queue trigger function processed: {todo.TaskDescription}");
        }
    }
}
