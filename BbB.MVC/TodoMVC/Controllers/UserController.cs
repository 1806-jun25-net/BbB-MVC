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
            if (TempData.ContainsKey("username") && name == null)
            {
                name = TempData.Peek("username").ToString();
            }
            if (!(await GetUserInfo(name)))
            {
                ModelState.AddModelError("", "There was an error logging in please try agian");
                RedirectToAction("Login", "Home");
            }
            var user = TempData.Get<User>("user");
            try
            {
                TempData.Add("name", user.Name);
            }
            catch (Exception ex) { }
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
            }
            else if ((string)TempData.Peek("username") != user.Name)
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

            foreach (var item in drives)
            {
                request = CreateRequestToService(HttpMethod.Get, "drive/" + item.Id + "/ORCount");
                response = await HttpClient.SendAsync(request);
                jsonString = await response.Content.ReadAsStringAsync();
                OrdersRealCount.Add(int.Parse(jsonString));
            }

            TempData.Add("count", OrdersRealCount);

            return View(drives);
        }

        public async Task<IActionResult> MakeOrder(int destId)
        {
            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "destination/" + destId + "/menu");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<MenuItem> drives = JsonConvert.DeserializeObject<List<MenuItem>>(jsonString);

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
            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "user/" + userName + "/driver");
            try
            {
                var response = await HttpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                string jsonString = await response.Content.ReadAsStringAsync();

                if (jsonString.Contains("driverId"))
                {
                    Driver driver = JsonConvert.DeserializeObject<Driver>(jsonString);
                    TempData.Put("driverCheck", "yes");
                    TempData.Put("user", driver);
                }

                else
                {
                    User user = JsonConvert.DeserializeObject<Driver>(jsonString);
                    TempData.Put("user", user);
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                return false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyDrives()
        {
            await GetUserInfo(TempData.Peek("name").ToString());
            Driver driver = TempData.Get<Driver>("user");
            TempData.Put("user", driver);
            if (TempData.Peek("username") == null)
            {
                TempData.Add("username", driver.Name);
            }
            else if(TempData.Peek("username").ToString() != driver.Name)
            {
                TempData["username"] = driver.Name;
            }
            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "drive/" + driver.DriverId + "/driver");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Drive> drives = JsonConvert.DeserializeObject<List<Drive>>(jsonString);

            // Get the number of people in the pickup drive
            List<int> OrdersRealCount = new List<int>();

            foreach (var item in drives)
            {
                request = CreateRequestToService(HttpMethod.Get, "drive/" + item.Id + "/ORCount");
                response = await HttpClient.SendAsync(request);
                jsonString = await response.Content.ReadAsStringAsync();
                OrdersRealCount.Add(int.Parse(jsonString));
            }

            TempData.Add("count", OrdersRealCount);

            return View(drives);
        }

        public IActionResult CreateMsg()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMsg(Message message)
        {
            await GetUserInfo(TempData.Peek("name").ToString());
            User user = TempData.Get<User>("user");
            TempData.Put("user", user);
            if (TempData.Peek("username") == null)
            {
                TempData.Add("username", user.Name);
            }
            else if (TempData.Peek("username").ToString() != user.Name)
            {
                TempData["username"] = user.Name;
            }

            message.FromId = user.Id;
            message.Time = DateTime.Now;

            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post,
                $"message/{message.FromId}:{message.ToId}:{message.Content}", message);

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

            return RedirectToAction("GetOutbox");
        }

        [HttpGet]
        public async Task<IActionResult> GetOutbox()
        {
            await GetUserInfo(TempData.Peek("name").ToString());
            User user = TempData.Get<User>("user");
            TempData.Put("user", user);
            if (TempData.Peek("username") == null)
            {
                TempData.Add("username", user.Name);
            }
            else if (TempData.Peek("username").ToString() != user.Name)
            {
                TempData["username"] = user.Name;
            }

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "message/" + user.Id + "/received");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Message> messages = JsonConvert.DeserializeObject<List<Message>>(jsonString);

            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetInbox()
        {
            await GetUserInfo(TempData.Peek("name").ToString());
            User user = TempData.Get<User>("user");
            TempData.Put("user", user);
            if (TempData.Peek("username") == null)
            {
                TempData.Add("username", user.Name);
            }
            else if (TempData.Peek("username").ToString() != user.Name)
            {
                TempData["username"] = user.Name;
            }

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "message/" + user.Id + "/sent");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Message> messages = JsonConvert.DeserializeObject<List<Message>>(jsonString);

            return View(messages);
        }
    }
}