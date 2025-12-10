using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TorontoPearson.Models;
using TorontoPearson.Services;

namespace TorontoPearson.Pages;

public class ArrivalsModel(IPearsonApiClient apiClient) : PageModel
{
    private readonly IPearsonApiClient _apiClient = apiClient;

    public List<FlightInfo> Flights { get; private set; } = new();

    public List<string> Airlines { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Airline { get; set; } = "Porter Airlines";

    public async Task OnGet(CancellationToken cancellationToken)
    {
        var flights = await _apiClient.GetFlightsAsync("arrivals", cancellationToken);

        Airlines = flights
            .Select(f => f.Airline)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        if (!Airlines.Contains("Porter Airlines", StringComparer.OrdinalIgnoreCase))
        {
            Airlines.Insert(0, "Porter Airlines");
        }

        Flights = FilterAndSortFlights(flights, Airline);
    }

    private static List<FlightInfo> FilterAndSortFlights(IEnumerable<FlightInfo> flights, string? airline)
    {
        var filtered = string.IsNullOrWhiteSpace(airline) || airline.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? flights
            : flights.Where(f => f.Airline.Equals(airline, StringComparison.OrdinalIgnoreCase));

        return filtered
            .OrderByDescending(f => string.Equals(f.Airline, "Porter Airlines", StringComparison.OrdinalIgnoreCase))
            .ThenBy(f => f.Airline)
            .ThenBy(f => f.ScheduledTime ?? DateTimeOffset.MaxValue)
            .ToList();
    }
}
