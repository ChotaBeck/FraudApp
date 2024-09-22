using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FraudApp
{
    public class FraudCheckService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://fraudapi-0b7c.onrender.com"; // Replace with your actual API URL

        public FraudCheckService()
        {
            _httpClient = new HttpClient();
        }

        // public async Task<string> CheckMessageFraudAsync(string messageBody)
        // {
        //     try
        //     {
        //         var content = new StringContent(messageBody, System.Text.Encoding.UTF8, "text/plain");
        //         var response = await _httpClient.PostAsync($"{ApiBaseUrl}/predict", content);

        //         if (response.IsSuccessStatusCode)
        //         {
        //             return await response.Content.ReadAsStringAsync();
        //         }
        //         else
        //         {
        //             // Handle error cases
        //             Console.WriteLine($"Error: {response.StatusCode}");
        //             return "Error";
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // Handle exceptions
        //         Console.WriteLine($"Exception: {ex.Message}");
        //         return "Error";
        //     }
        // }

        public async Task<string> CheckMessageFraudAsync(string messageBody)
        {
            try
            {
                // Create a payload as JSON
                var payload = new { message = messageBody };

                // Serialize the payload into JSON
                var jsonContent = JsonConvert.SerializeObject(payload);

                // Use StringContent to send the serialized JSON with the correct content type
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Make the POST request
                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/predict", content);

                if (response.IsSuccessStatusCode)
                {
                    // Read and return the response as a string
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // Handle error cases
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                return "Error";
            }
        }
    }
}