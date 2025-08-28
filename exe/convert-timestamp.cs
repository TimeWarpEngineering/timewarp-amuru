#!/usr/bin/dotnet --
#:package TimeWarp.Nuru

using TimeWarp.Nuru;
using static System.Console;
using System.Globalization;

NuruApp app = new NuruAppBuilder()
    .AddRoute("--GitCommitTimestamp {timestamp}", (string timestamp) =>
    {
      if (long.TryParse(timestamp, out long unixTimestamp))
      {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        WriteLine(dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture));
      }
      else
      {
        WriteLine("Invalid timestamp");
        return 1;
      }

      return 0;
    })
    .AddAutoHelp()
    .Build();

return await app.RunAsync(args);