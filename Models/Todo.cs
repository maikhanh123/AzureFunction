using System;

namespace func02.Models
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        //Why ToString("n"): https://stackoverflow.com/questions/7513391/newguid-vs-system-guid-newguid-tostringd

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}