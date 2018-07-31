using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoMvc.Controllers;

namespace TodoMVC.Controllers
{
    public class UserController : AServiceController
    {
        public UserController(HttpClient httpClient) : base(httpClient)
        { }

        public IActionResult UserOptions()
        {
            return View();
        }

        public IActionResult LookForDrives() 
        {
            var request = CreateRequestToService(HttpMethod.Get, "/drive");

            Request.Cookies.

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