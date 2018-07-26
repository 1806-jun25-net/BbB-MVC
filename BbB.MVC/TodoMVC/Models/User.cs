using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class User
    {
        /// <summary>
        /// Id for use internal identification
        /// -1 used for "not in database"
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// Email Address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Number of Credit on our site
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// Name of company/employer
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Rating of the user.
        /// Integral portion is number of reviews,
        /// Decimal is 1/10 of the users total rating.
        /// Use NumRating and AvgRating to access.
        /// </summary>
        public decimal Rating { get; set; }
    }
}
