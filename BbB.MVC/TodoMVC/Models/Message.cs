using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public User User { get; set; }
    }
}
