using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int FromId { get; }
        public int ToId { get; }
        public string Content { get; }
    }
}
