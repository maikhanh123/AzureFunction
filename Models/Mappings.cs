namespace func02.Models
{
    public static class Mappings
    {
        public static TodoTableEntity ToTableEntity(this Todo todo)
        {
            return new TodoTableEntity
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreateTime = todo.CreateTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
            };
        }
        public static Todo ToTodo(this TodoTableEntity todo)
        {
            return new Todo
            {
                Id = todo.RowKey,
                CreateTime = todo.CreateTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
            };
        }
    }
}