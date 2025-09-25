#!/usr/bin/dotnet --
#:package TimeWarp.Nuru
#:property TrimMode=partial
#:property NoWarn=IL2104;IL3053

using TimeWarp.Nuru;
using static System.Console;
using System.Globalization;

NuruAppBuilder builder = new();

builder.AddAutoHelp();

builder.AddRoute
(
  "{timestamp|Unix timestamp to convert (seconds since epoch)}",
  ConvertTimestamp,
  "Convert Unix timestamps to ISO 8601 format (yyyy-MM-ddTHH:mm:ssK)"
);

NuruApp app = builder.Build();
return await app.RunAsync(args);

static int ConvertTimestamp(string timestamp)
{
  if (long.TryParse(timestamp, out long unixTimestamp))
  {
    var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
    WriteLine(dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture));
    return 0;
  }
  else
  {
    WriteLine("Invalid timestamp");
    return 1;
  }
}