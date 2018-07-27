using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class Drive
    {
        public bool IsPickup { get; set; }

        public List<User> UsersReal;

        public static readonly int MAX_PICKUP_SIZE = 12;

        public Dictionary<User, List<OrderItem>> OrdersReal;

        /// <summary>
        /// internal Id. -1 if untracked by database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Destinaton for the drive
        /// </summary>
        public Destination Dest { get; set; }

        /// <summary>
        /// Time the order was placed or will occur
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Number of minutes prior to Time that the drive is finalized
        /// only for active orders
        /// </summary>
        public static int Buffer = 30;
        //public int Buffer { get; set; }

        /// <summary>
        /// Driver exectuing the drive
        /// </summary>
        public Driver Driver { get; set; }
    }
}
