using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    /// <summary>
    /// Simple compound of string and decimal, with Id for database
    /// </summary>
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
    }
}
