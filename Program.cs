using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class Prediction
{
    public string home_team { get; set; }
    public string away_team { get; set; }
    public string status { get; set; }
    public string start_date { get; set; }
    public string result { get; set; } // Assuming the API provides a 'result' property for the score
}

public class ApiResponse
{
    public List<Prediction> data { get; set; }
}

class Program
{
    private static readonly TimeZoneInfo apiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
    private static readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");

    static async Task Main()
    {
        Console.WriteLine("Welcome to the Football Prediction App!");

        // Set API key and host
        var apiKey = "a5c80f2834mshf39eb4d14e60d61p1d5810jsn1937d59003c1";
        var apiHost = "football-prediction-api.p.rapidapi.com";

        // Set query parameters
        var queryParams = new Dictionary<string, string>
        {
            { "iso_date", "2018-12-01" },
            { "federation", "UEFA" },
            { "market", "classic" }
        };

        // Build the URI
        var uriBuilder = new UriBuilder("https://football-prediction-api.p.rapidapi.com/api/v2/predictions")
        {
            Query = ToQueryString(queryParams)
        };

        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uriBuilder.Uri,
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", apiHost },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                // Parse and display relevant information
                ParseAndDisplayPredictions(body);
            }
        }
    }

    private static void ParseAndDisplayPredictions(string responseBody)
    {
        var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(responseBody);

        if (apiResponse != null && apiResponse.data != null)
        {
            foreach (var prediction in apiResponse.data)
            {
                var homeTeam = prediction.home_team;
                var awayTeam = prediction.away_team;
                var predictionResult = prediction.status;
                var score = prediction.result; // Assuming 'result' represents the score
                var startDate = ToLocalDateTime(prediction.start_date);

                Console.WriteLine($"Match: {homeTeam} vs {awayTeam}, Prediction: {predictionResult}, Score: {score}, Start Time: {startDate}");
            }
        }
        else
        {
            Console.WriteLine("Invalid or missing data in the API response.");
        }
    }

    private static string ToQueryString(Dictionary<string, string> parameters)
    {
        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return queryString.Length > 0 ? "?" + queryString : string.Empty;
    }

    private static DateTime ToLocalDateTime(string startDate)
    {
        if (DateTime.TryParse(startDate, out var dt))
        {
            return TimeZoneInfo.ConvertTime(dt, apiTimeZone, localTimeZone);
        }
        return DateTime.MinValue;
    }
}
