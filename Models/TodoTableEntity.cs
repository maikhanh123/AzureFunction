using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace func02.Models
{
    public class TodoTableEntity : TableEntity
    {
        public DateTime CreateTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}