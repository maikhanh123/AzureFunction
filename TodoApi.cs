using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using func02.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using Microsoft.WindowsAzure.Storage;

namespace func02
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable, //output binding
            [Queue("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<Todo> todoQueue, //output binding
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);
            var todo = new Todo() { TaskDescription = input.TaskDescription };

            await todoTable.AddAsync(todo.ToTableEntity());
            await todoQueue.AddAsync(todo);

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log
        )
        {
            log.LogInformation("Getting todo list items");
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todoTable,
            ILogger log,
            string id
        )
        {
            log.LogInformation("Getting todo by id: " + id);
            if (todoTable == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(todoTable.ToTodo());
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log,
            string id
            )
        {
            log.LogInformation("Update a todo Id: " + id);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);

            if (findResult == null)
            {
                return new NotFoundResult();
            }

            var existingRow = (TodoTableEntity)findResult.Result;
            existingRow.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                existingRow.TaskDescription = updated.TaskDescription;
            }

            var replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToTodo());
        }

        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route="todo/{id}")] HttpRequest req,
            [Table("todos", Connection="AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log,
            string id
        )
        {
            log.LogInformation($"Item id: {id} will be delete");
            var deleteOperation = TableOperation.Delete(new TableEntity{
                PartitionKey = "TODO" , RowKey = id, ETag = "*"
            });

            try{
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
            }
            catch(StorageException e) when (e.RequestInformation.HttpStatusCode == 404) {
                return new NotFoundResult();
            }

            return new OkResult();

        }
    }
}
