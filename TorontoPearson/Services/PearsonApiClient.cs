using System.Text.Json;
using Microsoft.Extensions.Options;
using TorontoPearson.Models;
using TorontoPearson.Options;

namespace TorontoPearson.Services;

public class PearsonApiClient(IHttpClientFactory httpClientFactory, IOptions<PearsonApiOptions> options)
    : IPearsonApiClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly PearsonApiOptions _options = options.Value;

    public async Task<IReadOnlyList<FlightInfo>> GetFlightsAsync(string flightType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            return BuildDemoFlights(flightType);
        }

        var requestUri = BuildRequestUri(flightType);
        var flights = new List<FlightInfo>();

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return BuildDemoFlights(flightType);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var flightElement in EnumerateFlightElements(document.RootElement))
            {
                flights.Add(MapFlight(flightElement, flightType));
            }
        }
        catch
        {
            return BuildDemoFlights(flightType);
        }

        return flights.Count > 0 ? flights : BuildDemoFlights(flightType);
    }

    private string BuildRequestUri(string flightType)
    {
        var separator = _options.BaseUrl.Contains('?') ? '&' : '?';
        return $"{_options.BaseUrl}{separator}type={flightType}";
    }

    private static IEnumerable<JsonElement> EnumerateFlightElements(JsonElement rootElement)
    {
        if (rootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in rootElement.EnumerateArray())
            {
                yield return element;
            }

            yield break;
        }

        if (rootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in dataElement.EnumerateArray())
            {
                yield return element;
            }
        }
    }

    private static FlightInfo MapFlight(JsonElement element, string flightType)
    {
        return new FlightInfo
        {
            FlightType = flightType,
            Airline = GetString(element, "airlineName")
                ?? GetString(element, "airline")
                ?? GetString(element, "airline_name")
                ?? "Unknown airline",
            FlightNumber = GetString(element, "flightNumber")
                ?? GetString(element, "flight_number")
                ?? "–",
            City = GetString(element, "city")
                ?? GetString(element, "destination")
                ?? GetString(element, "origin")
                ?? "–",
            Gate = GetString(element, "gate")
                ?? GetString(element, "gateNumber")
                ?? "–",
            ScheduledTime = ParseDate(GetString(element, "scheduledTime")
                ?? GetString(element, "scheduled_time")
                ?? GetString(element, "scheduledDateTime")),
            Status = GetString(element, "status")
                ?? GetString(element, "flightStatus")
                ?? GetString(element, "remark")
                ?? "On time"
        };
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        if (DateTimeOffset.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static List<FlightInfo> BuildDemoFlights(string flightType)
    {
        var now = DateTimeOffset.UtcNow;
        return new List<FlightInfo>
        {
            new()
            {
                FlightType = flightType,
                Airline = "Porter Airlines",
                FlightNumber = "PD123",
                City = flightType == "arrivals" ? "Ottawa" : "Halifax",
                Gate = "B12",
                ScheduledTime = now.AddMinutes(45),
                Status = "Boarding"
            },
            new()
            {
                FlightType = flightType,
                Airline = "Air Canada",
                FlightNumber = "AC456",
                City = flightType == "arrivals" ? "Vancouver" : "Montreal",
                Gate = "C8",
                ScheduledTime = now.AddHours(1.5),
                Status = "On time"
            },
            new()
            {
                FlightType = flightType,
                Airline = "WestJet",
                FlightNumber = "WS789",
                City = flightType == "arrivals" ? "Calgary" : "Winnipeg",
                Gate = "A3",
                ScheduledTime = now.AddMinutes(25),
                Status = "Delayed"
            }
        };
    }
}
