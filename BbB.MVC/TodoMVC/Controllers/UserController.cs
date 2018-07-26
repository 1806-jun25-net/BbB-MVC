using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TodoMVC.Controllers
{
    public class UserController : Controller
    {
        public IActionResult UserOptions()
        {
            return View();
        }

        public IActionResult LookForDrives()
        {
            return View();
        }

        public IActionResult JoinedDrives()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }

        public IActionResult Upgrade()
        {
            return View();
        }
    }
}