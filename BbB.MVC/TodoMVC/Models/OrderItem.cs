using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        /// <summary>
        /// Item if the order is active.
        /// Is an untracked item otherwise.
        /// </summary>
        public MenuItem Item { get; set; }

        /// <summary>
        /// how many of the item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Message attached to item
        /// </summary>
        public string Message { get; set; }
    }
}
