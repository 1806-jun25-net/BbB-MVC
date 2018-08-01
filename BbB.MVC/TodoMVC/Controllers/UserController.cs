using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoMvc.Controllers;
using TodoMVC.Models;

namespace TodoMVC.Controllers
{
    public class UserController : AServiceController
    {
        public UserController(HttpClient httpClient) : base(httpClient)
        { }

        public IActionResult UserOptions()
        {
            var user = TempData.Get<User>("user");
            TempData.Put("user", user);

            return View(user);
        }

        public IActionResult LookForDrives() 
        {
            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post, "/drive");

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