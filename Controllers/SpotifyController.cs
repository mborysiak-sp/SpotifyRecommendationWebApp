using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyMVC.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SpotifyMVC.Controllers
{

    public class SpotifyController : Controller
    {
        #region PROPERTIES
        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
        SpotifyAuth sAuth = new SpotifyAuth();

        private readonly ILogger<SpotifyController> _logger;

        public SpotifyController(ILogger<SpotifyController> logger)
        {
            _logger = logger;
        }
        private static Random random = new Random();
        #endregion

        #region SUPPORT_FUNCTIONS
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region REQUESTS
        public TokensResponse GetTokens(string code)
        {
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
            return JsonConvert.DeserializeObject<TokensResponse>(responseString, settings);
        }

        public Paging GetTracks(string access_token)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var responseContent = client.GetAsync("https://api.spotify.com/v1/me/tracks").Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<Paging>(responseString, settings);
        }

        public Paging GetAlbums(string access_token, string artistID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/artists/" + artistID + "/albums";
                var responseContent = client.GetAsync(adres).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<Paging>(responseString, settings);
        }
        #endregion

        #region VIEWS
        public IActionResult Auth()
        {
            var state = RandomString(8);
            var qb = new QueryBuilder();
            qb.Add("client_id", sAuth.clientID);
            qb.Add("response_type", "code");
            qb.Add("redirect_uri", sAuth.redirectURL);
            qb.Add("scope", "user-read-private user-library-read");
            qb.Add("state", state);
            TempData["state"] = state;
            ViewData["params"] = qb.ToQueryString().ToString();
            return View();
        }
        public IActionResult Callback(string code, string state)
        {
            if ((string)TempData["state"] == state)
            {
                @ViewData["state"] = "Authentication Successfull";
                @ViewData["status"] = "ok";
            }
            else
            {
                @ViewData["state"] = "Authentication Failed: Invalid State";
                @ViewData["status"] = null;
            }
            TempData["state"] = null;
            return View();
        }
        public IActionResult Dashboard(String code)
        {
            var tokens = GetTokens(code);
            var tracksPaging = GetTracks(tokens.access_token);
            var artists = new HashSet<String>();
            foreach (var i in tracksPaging.items) foreach (var j in i.track.artists) artists.Add(j.id);
            var albums = new HashSet<Paging>();
            foreach (String a in artists) albums.Add(GetAlbums(tokens.access_token, a));
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}
