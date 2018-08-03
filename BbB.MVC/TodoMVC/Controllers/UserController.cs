using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        public async Task<IActionResult> LookForDrives() 
        {
            var user = TempData.Get<User>("user");
            TempData.Put("user", user);

            if (TempData.Peek("username") == null)
            {
                TempData.Add("username", user.Name);
                TempData.Add("userId", user.Id);
            }else if((string)TempData.Peek("username") != user.Name)
            {
                TempData["username"] = user.Name;
                TempData["userId"] = user.Id;
            }

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "drive/" + user.Company + "/company");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Drive> drives = JsonConvert.DeserializeObject<List<Drive>>(jsonString);

            return View(drives);
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