# Toronto Pearson flight viewer

A minimal ASP.NET Core Razor Pages app that surfaces arrivals and departures from the Toronto Pearson Airport API. Both pages share the same feed and include a featured airline filter for Porter Airlines.

## Running locally

```bash
cd TorontoPearson
dotnet run
```

Then browse to `http://localhost:5000` (or the port shown in the console) for arrivals and departures.

Update `appsettings.json` if you need to change the airport API endpoint.
