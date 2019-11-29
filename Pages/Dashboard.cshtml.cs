using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace SpotifyR
{
    public class DashboardModel : PageModel
    {
        private SpotifyAuth sAuth = new SpotifyAuth();

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

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

        // public List<User> ZrobJebanyAlgorytmRafałKurwa(TokensResponse tokens){
        //     var tracksPaging = GetTracks(tokens.access_token);
        //     var artists = new HashSet<String>();
        //     foreach (var i in tracksPaging.items) foreach (var j in i.track.artists) artists.Add(j.id);
        //     var albums = new HashSet<Paging>();
        //     foreach (String a in artists) albums.Add(GetAlbums(tokens.access_token, a));

        //     return null;
        // }

        public IActionResult Dashboard(String code)
        {
            var tokens = GetTokens(code);
            // ZrobJebanyAlgorytmRafałKurwa(tokens);
            return Page();
        }
    }
}