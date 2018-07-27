using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TodoMVC.Models
{
    public class Driver : User
    {
        /// <summary>
        /// internal Id
        /// -1 if untracked by database
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Default number of people who driver will take on a join drive
        /// </summary>
        [Required]
        public int Seats { get; set; }

        /// <summary>
        /// Default message for where to meet
        /// </summary>
        [Required]
        [Display(Name = "Meet Location")]
        public string MeetLoc { get; set; }

        /// <summary>
        /// Rating of the user.
        /// Integral portion is number of reviews,
        /// Decimal is 1/10 of the users total rating.
        /// Use NumRating and AvgRating to access.
        /// </summary>
        public decimal DriverRating { get; set; }
    }
}
