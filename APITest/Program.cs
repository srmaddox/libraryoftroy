using System.Net.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace APITest;

class Program {
    static async Task Main ( string [] args ) {
        // Create HTTP client
        using ( var client = new HttpClient ( ) ) {
            string baseUrl = "http://localhost:7801";

            try {
                // Step 1: Get a session ID
                var sessionResponse = await GetSessionId(client, baseUrl);
                string sessionId = sessionResponse["session_id"].ToString();

                // Step 2: Get the model list
                var modelListResponse = await GetModelList(client, baseUrl, sessionId);

                // Just dump the raw response to console
                Console.WriteLine ( modelListResponse.ToString ( Formatting.Indented ) );
            } catch ( Exception ex ) {
                Console.WriteLine ( $"Error: {ex.Message}" );
            }
        }
    }

    static async Task<JObject> GetSessionId ( HttpClient client, string baseUrl ) {
        var content = new StringContent("{}", System.Text.Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue ( "application/json" );

        var response = await client.PostAsync($"{baseUrl}/API/GetNewSession", content);
        var responseString = await response.Content.ReadAsStringAsync();

        return JObject.Parse ( responseString );
    }

    static async Task<JObject> GetModelList ( HttpClient client, string baseUrl, string sessionId ) {
        var requestData = new JObject
            {
            ["session_id"] = sessionId
        };

        var content = new StringContent(requestData.ToString(), Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue ( "application/json" );

        var response = await client.PostAsync($"{baseUrl}/API/ListT2IParams", content);
        var responseString = await response.Content.ReadAsStringAsync();

        return JObject.Parse ( responseString );
    }
}
