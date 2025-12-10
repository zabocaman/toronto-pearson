using TorontoPearson.Models;

namespace TorontoPearson.Services;

public interface IPearsonApiClient
{
    Task<IReadOnlyList<FlightInfo>> GetFlightsAsync(string flightType, CancellationToken cancellationToken);
}
