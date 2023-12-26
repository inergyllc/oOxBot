using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OxBotAdvisorConsole
{

    public class Program
    {
        private static readonly HttpClient client = new();

        public static async Task Main()
        {
            string prompt = "Your query for OxBot Advisor";
            string response = await CallAiModel(prompt);
            Console.WriteLine(response);
        }

        private static async Task<string> CallAiModel(string prompt)
        {
            var requestData = new
            {
                prompt,
                max_tokens = 1000
            };
            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ami@oilwrx.com openai key
            var apiKey = "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"; // Replace with your API key
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.PostAsync("https://api.openai.com/v1/engines/gpt-4-1106-preview/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
    }
}