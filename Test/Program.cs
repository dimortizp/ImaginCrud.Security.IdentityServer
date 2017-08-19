using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;

namespace Test
{
    class Program
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {

            Strat();

            resetEvent.WaitOne();
        }
        public static async void Strat()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            // request token

            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            var tokenClient2 = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            var tokenResponse2 = await tokenClient2.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

            if (tokenResponse2.IsError)
            {
                Console.WriteLine(tokenResponse2.Error);
                return;
            }

            Console.WriteLine(tokenResponse2.Json);
            Console.WriteLine("\n\n");

            Console.Read();
            resetEvent.Set();
        }
    }
}