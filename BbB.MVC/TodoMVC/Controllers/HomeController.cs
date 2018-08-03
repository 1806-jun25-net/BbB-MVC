﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TodoMvc.Controllers;
using TodoMVC.Models;

namespace TodoMVC.Controllers
{
    public class HomeController : AServiceController
    {

        public HomeController(HttpClient httpClient) : base(httpClient)
        { }

        public IActionResult Index()    
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {
            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post, "user/login", user);

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

            if (!apiResponse.IsSuccessStatusCode)
            {
                if (apiResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    ModelState.AddModelError("", "Incorrect Username or Passwor");
                    return View();
                }
                ModelState.AddModelError("", "Error2");
                return View();
            }

            PassCookiesToClient(apiResponse);

            // This is a test
            if(!(await GetUserInfo(user.Name)))
            {
                ModelState.AddModelError("", "There was an error");
            }

            return RedirectToAction("UserOptions", "User");
        }
                
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public async Task<ActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post, "user/register", user);

            HttpResponseMessage apiResponse;
            try
            {
                apiResponse = await HttpClient.SendAsync(apiRequest);
            }
            catch
            {
                return View("Error");
            }

            if (!apiResponse.IsSuccessStatusCode)
            {
                return View("Error");
            }

            PassCookiesToClient(apiResponse);

            // This is a test
            if (!(await GetUserInfo(user.Name)))
            {
                ModelState.AddModelError("", "There was an error");
            }

            return RedirectToAction("UserOptions", "User");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool PassCookiesToClient(HttpResponseMessage apiResponse)
        {
            if (apiResponse.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                string authValue = values.FirstOrDefault(x => x.StartsWith(s_CookieName));

                if (authValue != null)
                {
                    Response.Headers.Add("Set-Cookie", authValue);

                    return true;
                }
            }
            return false;
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
