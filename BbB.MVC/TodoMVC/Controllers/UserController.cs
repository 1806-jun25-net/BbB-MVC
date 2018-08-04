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

        public async Task<IActionResult> UserOptions(string name)
        {
            if (TempData.ContainsKey("username"))
            {
                name = TempData.Peek("username").ToString();
            }
            if (!(await GetUserInfo(name)))
            {
                ModelState.AddModelError("", "There was an error loging in please try agian");
                RedirectToAction("Login", "Home");
            }
            var user = TempData.Get<User>("user");
            try
            {
                TempData.Add("name", user.Name);
            }
            catch(Exception ex){ }            
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

            // still need a way to check if the user already joined the pickup drive

            // Get all the Ids of the drives the user has joined
                        
            request = CreateRequestToService(HttpMethod.Get, "drive/" + user.Id + "/JoinedDrives");
            response = await HttpClient.SendAsync(request);
            jsonString = await response.Content.ReadAsStringAsync();
            List<int> joinedDrives = JsonConvert.DeserializeObject<List<int>>(jsonString);

            TempData.Add("joinedDrives", joinedDrives);

            // Get the number of people in the pickup drive
            List<int> OrdersRealCount = new List<int>();

            foreach(var item in drives)
            {
                request = CreateRequestToService(HttpMethod.Get, "drive/" + item.Id + "/ORCount");
                response = await HttpClient.SendAsync(request);
                jsonString = await response.Content.ReadAsStringAsync();
                OrdersRealCount.Add(int.Parse(jsonString));
            }

            TempData.Add("count", OrdersRealCount);

            return View(drives);
        }

        public async Task<IActionResult> JoinedDrives()
        {
            var user = TempData.Get<User>("user");
            TempData.Put("user", user);

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "drive/" + user.Id + "/user");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Drive> drives = JsonConvert.DeserializeObject<List<Drive>>(jsonString);

            return View(drives);
        }

        public IActionResult History()
        {
            return View();
        }

        public IActionResult Upgrade()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upgrade(Driver driver)
        {
            driver.Name = TempData.Peek("username").ToString();
            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post, "user/upgrade", driver);

            HttpResponseMessage apiResponse;

            try
            {
                
                apiResponse = await HttpClient.SendAsync(apiRequest);
            }
            catch (AggregateException ex)
            {
                ModelState.AddModelError("", "Error");
                return View();
            }

            return RedirectToAction("UserOptions", "User");
        }

        private async Task<bool> GetUserInfo(string userName)
        {
            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "user/" + userName);
            try
            {
                var response = await HttpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                string jsonString = await response.Content.ReadAsStringAsync();

                User user = JsonConvert.DeserializeObject<User>(jsonString);

                TempData.Put("user", user);

                return true;
            }
            catch (HttpRequestException ex)
            {
                return false;
            }
        }
    }
}