using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace SpotifyR
{
    public class DashboardModel : PageModel
    {
        [BindProperty]
        public List<Track> NEW_RELEASES { get; set; }

        [BindProperty]
        public List<Track> DISCOVER { get; set; }
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
        public Track GetTrackById(string access_token, string tID)
        {
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var responseContent = client.GetAsync("https://api.spotify.com/v1/tracks/" + tID).Result.Content;
                responseString += responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<Track>(responseString, settings);
        }

        public PagingAlbum GetArtistsAlbums(string access_token, string artistID)
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
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public PagingAlbum GetAlbumById(string access_token, string albumID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/albums/" + albumID + "/tracks";
                var responseContent = client.GetAsync(adres).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public IActionResult OnGet(String code)
        {
            var access_token = GetTokens(code).access_token;
            var NEW_RELEASES = NewReleases(access_token);
            return Page();
        }

        public List<Track> NewReleases(String access_token){
            // get ALL followed artists
            var followedArtists = GetFollowedArtists(access_token, null);
            // get their newest (6tyg) albums
            // get the 3 most popular songs
            // shuffle the list
            return null;
        }

        public List<Artist> GetFollowedArtists(String access_token, String next){
            string responseString;
            List<Artist> resultList = new List<Artist>();
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = next==null ? "https://api.spotify.com/v1/me/following?type=artist&limit=50" : next + "&limit=50";
                var responseContent = client.GetAsync(adres).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            var responseContainer = JsonConvert.DeserializeObject<ArtistsContainer>(responseString, settings).artists;
            resultList.AddRange(responseContainer.items);
            if(responseContainer.next!=null){
                resultList.AddRange(GetFollowedArtists(access_token, responseContainer.next));
            }
            return resultList;
        }
    }
}