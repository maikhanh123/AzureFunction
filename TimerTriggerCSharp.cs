using System;
using System.Threading.Tasks;
using func02.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace func02
{
    public static class TimerTriggerCSharp
    {
        [FunctionName("TimerTriggerCSharp")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log
        )
        {
            
            var todosQuery = new TableQuery<TodoTableEntity>();
            var todos = await todoTable.ExecuteQuerySegmentedAsync(todosQuery, null);
            var deleted = 0;

            foreach(var todo in todos) {
                if(todo.IsCompleted) {
                    await todoTable.ExecuteAsync(TableOperation.Delete(todo));
                    deleted++;
                }
            }

            log.LogInformation($"C# Timer trigger function executed delete {deleted} at: {DateTime.Now}");
        }
    }
}
