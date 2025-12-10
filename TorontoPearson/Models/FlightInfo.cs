using System;

namespace TorontoPearson.Models;

public class FlightInfo
{
    public string FlightType { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Gate { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
