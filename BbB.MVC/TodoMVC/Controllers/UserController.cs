using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TodoMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly static string ServiceUri = "http://localhost:61443/api/";

        public HttpClient HttpClient { get; }

        public UserController(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

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