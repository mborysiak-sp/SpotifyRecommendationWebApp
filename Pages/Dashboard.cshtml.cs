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

        public PagingTrack GetTracks(string access_token, int i)
        {
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var responseContent = client.GetAsync("https://api.spotify.com/v1/me/tracks?offset=" + i).Result.Content;
                responseString += responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingTrack>(responseString, settings);
        }

        public PagingAlbum GetAlbums(string access_token, string artistID)
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

        public List<Track> NewReleases(TokensResponse tokens)
        {
            var artists = new HashSet<String>();
            var albums = new HashSet<Album>();
            var tracks = new HashSet<Track>();

            //wszyscy obserwowani artysci do "artists"
            //dla kazdego obswerowanego artysty wszystkie albumny nowsze niz X dni wstecz od dzisiaj do "albums"
            //dla kazdego z tych albumow top X najpopularniejszych utworow do "tracks"

            var resultList = tracks.ToList();
            resultList.Shuffle();

            return resultList;
        }

        public IActionResult OnGet(String code)
        {
            var tokens = GetTokens(code);
            NEW_RELEASES = NewReleases(tokens);
            // DISCOVER = Discover(tokens);
            return Page();
        }
    }

    static class MyExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}