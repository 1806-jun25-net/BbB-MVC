using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public async Task<User> CheckTempUser()
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

            return user;
        }

        public async Task<Driver> CheckTempDriver()
        {
            await GetUserInfo(TempData.Peek("name").ToString());
            Driver driver = TempData.Get<Driver>("user");
            TempData.Put("user", driver);
            if (TempData.Peek("username") == null)
            {
                TempData.Add("username", driver.Name);
            }
            else if (TempData.Peek("username").ToString() != driver.Name)
            {
                TempData["username"] = driver.Name;
            }

            return driver;
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

            // Get all the Ids of the join drives the user has joined
            request = CreateRequestToService(HttpMethod.Get, "drive/" + user.Id + "/JoinedDrives");
            response = await HttpClient.SendAsync(request);
            jsonString = await response.Content.ReadAsStringAsync();
            List<int> joinedDrives = JsonConvert.DeserializeObject<List<int>>(jsonString);
            TempData.Add("joinedDrives", joinedDrives);

            //// Get all the Ids of the pickup drives the user has joined
            request = CreateRequestToService(HttpMethod.Get, "drive/" + user.Id + "/JoinedPickups");
            response = await HttpClient.SendAsync(request);
            jsonString = await response.Content.ReadAsStringAsync();
            List<int> joinedPickups = JsonConvert.DeserializeObject<List<int>>(jsonString);
            TempData.Add("joinedPickups", joinedPickups);

            // Get the number of people in the join drive
            List<int> OrdersRealCount = new List<int>();

            foreach (var item in drives)
            {
                request = CreateRequestToService(HttpMethod.Get, "drive/" + item.Id + "/ORCount");
                response = await HttpClient.SendAsync(request);
                jsonString = await response.Content.ReadAsStringAsync();
                OrdersRealCount.Add(int.Parse(jsonString));
            }

            //// Get the number of people in the pickup drive
            List<int> joinedPickupCount = new List<int>();

            foreach (var item in drives)
            {
                if (item.IsPickup)
                {
                    if(item.UsersReal != null) joinedPickupCount.Add(item.UsersReal.Count());
                }
            }

            TempData.Add("count", OrdersRealCount);
            TempData.Add("pickupsCount", joinedPickupCount);

            return View(drives);
        }

        public async Task<IActionResult> MakeOrder(int destId, int driveId)
        {
            if (TempData.ContainsKey("destId")) TempData.Remove("destId");
            TempData.Add("destId", destId);

            if (TempData.ContainsKey("driveId")) TempData.Remove("driveId");
            TempData.Add("driveId", driveId);

            // request for the destination
            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "destination/" + destId);

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            Destination destination = JsonConvert.DeserializeObject<Destination>(jsonString);

            if (TempData.ContainsKey("destName")) TempData.Remove("destName");
            TempData.Add("destName", destination.Name);

            if (TempData.ContainsKey("destAdd")) TempData.Remove("destAdd");
            TempData.Add("destAdd", destination.Address);

            // request for the menu
            request = CreateRequestToService(HttpMethod.Get, "destination/" + destId + "/menu");

            response = await HttpClient.SendAsync(request);

            jsonString = await response.Content.ReadAsStringAsync();

            List<MenuItem> menu = JsonConvert.DeserializeObject<List<MenuItem>>(jsonString);

            if (TempData.ContainsKey("menu")) TempData.Remove("menu");
            TempData.Put("menu", menu);

            return View(menu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeOrder(IFormCollection collection)
        {
            // The drive id to insert into the UserPickup table
            var driveId = int.Parse(TempData.Peek("driveId").ToString());

            // The user, we need the id for the same reason
            var user = TempData.Get<User>("user");
            TempData.Put("user", user);

            // Request to insert the order in user Pickup 
            // Here the user finally joins the drive and we get the Id of the pickup 
            HttpRequestMessage request = CreateRequestToService(HttpMethod.Post, "drive/" + driveId + "/" + user.Id + "/pickup");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            int pickupId = JsonConvert.DeserializeObject<int>(jsonString);

            if (pickupId > 0)
            {
                // The menu to know what the user ordered
                var menu = TempData.Get<List<MenuItem>>("menu");
                string number = "number";
                string text = "text";

                // Add every item the user ordered to OrderItem table
                foreach (var item in menu)
                {
                    number = "number" + item.Id;
                    text = "text" + item.Id;
                    if (int.Parse(collection[number].ToString()) > 0)
                    {
                        var orderItem = new OrderItem
                        {
                            Item = item,
                            Quantity = int.Parse(collection[number].ToString()),
                            Message = collection[text]
                        };

                        request = CreateRequestToService(HttpMethod.Post, "drive/" + pickupId + "/NewOrderItem", orderItem);

                        response = await HttpClient.SendAsync(request);
                    }
                }
            }

            return RedirectToAction(nameof(LookForDrives));
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

        public async Task<IActionResult> CreateDrives()
        {
            if (TempData.ContainsKey("isBad"))
            {
                ModelState.AddModelError("", "Invalid drive time");
            }
            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Get, "destination");

            try
            {
                HttpResponseMessage apiResponse = await HttpClient.SendAsync(apiRequest);

                string jsonString = await apiResponse.Content.ReadAsStringAsync();

                IEnumerable<Destination> destinations = JsonConvert.DeserializeObject<IEnumerable<Destination>>(jsonString);

                ViewData.Add("destinations", destinations);
            }
            catch (AggregateException ex)
            {
                ModelState.AddModelError("", "Error");
                return View();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDrives(Drive drive)
        {
            if (drive.Time < DateTime.Now.AddMinutes(Drive.Buffer))
            {
                TempData.Add("isBad", true);
                return RedirectToAction("CreateDrives");
            }
            Driver driver = await CheckTempDriver();

            drive.Driver = driver;

            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post, "drive/create", drive);

            HttpResponseMessage apiResponse;

            try
            {
                apiResponse = await HttpClient.SendAsync(apiRequest);
                string jsonString = await apiResponse.Content.ReadAsStringAsync();
            }
            catch (AggregateException ex)
            {
                ModelState.AddModelError("", "Error");
                return View();
            }

            return RedirectToAction("MyDrives", "User");
        }

        [HttpGet]
        public async Task<IActionResult> MyDrives()
        {
            Driver driver = await CheckTempDriver();

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
            User user = await CheckTempUser();

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, $"user/{message.User.Name}/user");

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            User toUser = JsonConvert.DeserializeObject<Driver>(jsonString);

            message.ToId = toUser.Id;
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
            User user = await CheckTempUser();

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "message/" + user.Id + "/received");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Message> messages = JsonConvert.DeserializeObject<List<Message>>(jsonString);

            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetInbox()
        {
            User user = await CheckTempUser();

            HttpRequestMessage request = CreateRequestToService(HttpMethod.Get, "message/" + user.Id + "/sent");

            var response = await HttpClient.SendAsync(request);

            string jsonString = await response.Content.ReadAsStringAsync();

            List<Message> messages = JsonConvert.DeserializeObject<List<Message>>(jsonString);

            return View(messages);
        }
    }
}