using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class Destination
    {
        /// <summary>
        /// Internal Id
        /// -1 if untracked by database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Street Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// All menu items for this location
        /// </summary>
        private List<MenuItem> MenuReal { get; set; }
    }
}
