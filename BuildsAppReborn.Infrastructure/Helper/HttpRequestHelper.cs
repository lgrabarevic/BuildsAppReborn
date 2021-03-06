﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BuildsAppReborn.Infrastructure
{
    public static class HttpRequestHelper
    {
        public static async Task<HttpResponseMessage> GetRequestResponseAsync(String url, ICredentials credentials)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.Credentials = credentials;
                using (var client = new HttpClient(handler))
                {
                    AddUserAgent(client);
                    return await client.GetAsync(url).ConfigureAwait(false);
                }
            }
        }

        public static async Task<HttpResponseMessage> GetRequestResponseAsync(String url, String personalAccessToken)
        {
            using (var client = new HttpClient())
            {
                AddUserAgent(client);
                AddAccessTokenToHeader(personalAccessToken, client);
                return await client.GetAsync(url).ConfigureAwait(false);
            }
        }

        public static async Task<HttpResponseMessage> GetRequestResponseAsync(String url)
        {
            using (var client = new HttpClient())
            {
                AddUserAgent(client);
                return await client.GetAsync(url).ConfigureAwait(false);
            }
        }

        private static void AddAccessTokenToHeader(String personalAccessToken, HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}")));
        }

        private static void AddUserAgent(HttpClient client)
        {
            // fix for issue with GitHub - https://stackoverflow.com/questions/2482715#comment64584330_22134980
            client.DefaultRequestHeaders.Add("User-Agent", "Anything");
        }
    }
}