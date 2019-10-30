using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyMVC.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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

        public TokensResponse GetTokens(string code){
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(sAuth.clientID + ":" + sAuth.clientSecret));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
                var parameters = new FormUrlEncodedContent(new Dictionary<string, string>{
                        {"code", code},
                        {"redirect_uri", sAuth.redirectURL},
                        {"grant_type", "authorization_code"},
                    });
                var responseContent = client.PostAsync("https://accounts.spotify.com/api/token", parameters).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<TokensResponse>(responseString);
        }
        public IActionResult Callback(string code)
        {
            @ViewData["access_token"] = GetTokens(code).access_token;
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
