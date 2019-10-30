using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyMVC.Models;

namespace SpotifyMVC.Controllers
{
    class SpotifyAuthentication
    {
        public string clientID = "826fdbe3b212461793a5024c3d89f96b";
        public string clientSecret = "b487b822b2704b4f81a4dcf2d4882412";
        public string redirectURL = "https://localhost:5001/spotify/callback";
    }
    public class SpotifyController : Controller
    {
        SpotifyAuthentication sAuth = new SpotifyAuthentication();

        private readonly ILogger<SpotifyController> _logger;

        public SpotifyController(ILogger<SpotifyController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var qb = new QueryBuilder();
            qb.Add("client_id", sAuth.clientID);
            qb.Add("response_type", "code");
            qb.Add("redirect_uri", sAuth.redirectURL);
            qb.Add("scope", "user-read-private user-read-email");
            ViewData["params"] = qb.ToQueryString().ToString();
            return View();
        }

        public IActionResult Test(){
            return View();
        }

        public IActionResult Callback(string code)
        {
            @ViewData["code"] = code;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
