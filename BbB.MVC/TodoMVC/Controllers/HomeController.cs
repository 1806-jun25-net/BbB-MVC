using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoMvc.Controllers;
using TodoMVC.Models;

namespace TodoMVC.Controllers
{
    public class HomeController : AServiceController
    {
        private readonly static string ServiceUri = "https://localhost:44318/api/";

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
            HttpRequestMessage apiRequest = CreateRequestToService(HttpMethod.Post, "api/user/login", user);

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
                    ModelState.AddModelError("", "Access Denied");
                    return View();
                }
                ModelState.AddModelError("", "Error2");
                return View();
            }

            PassCookiesToClient(apiResponse);

            return RedirectToAction("UserOptions", "User");
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CreateUser(User user)
        //{
        //}

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
    }
}
