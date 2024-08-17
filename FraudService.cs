using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FraudApp
{
    public class FraudCheckService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "YOUR_API_BASE_URL_HERE"; // Replace with your actual API URL

        public FraudCheckService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> CheckMessageFraudAsync(string messageBody)
        {
            try
            {
                var content = new StringContent(messageBody, System.Text.Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/check-fraud", content);

                if (response.IsSuccessStatusCode)
                {
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