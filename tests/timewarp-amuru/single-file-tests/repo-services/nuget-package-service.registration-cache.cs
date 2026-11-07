#!/usr/bin/env -S dotnet --

#region Purpose
// Proves NuGetPackageService shares one registration walk per package id.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace NuGetPackageService_
{
  [TestTag("Repo")]
  public class RegistrationCache_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RegistrationCache_Given_>();

    public static async Task RepeatSearchAndLatest_Should_DownloadTheWalkOnce()
    {
      using RecordingHandler handler = new(static (request, _) =>
      {
        string uri = request.RequestUri!.AbsoluteUri;
        if (uri.EndsWith("/index.json", StringComparison.Ordinal))
        {
          return Task.FromResult(Json("""
            {
              "items": [
                { "@id": "https://registration.test/contoso.widget/page.json" }
              ]
            }
            """));
        }

        return Task.FromResult(Json("""
          {
            "items": [
              { "catalogEntry": { "version": "1.0.0", "listed": true } },
              { "catalogEntry": { "version": "1.2.0" } },
              { "catalogEntry": { "version": "2.0.0", "listed": false } },
              { "catalogEntry": { "version": "1.5.0", "listed": true } },
              { "catalogEntry": { "version": "3.0.0-beta", "listed": true } },
              { "catalogEntry": { "version": "4.0.0-beta", "listed": false } }
            ]
          }
          """));
      });

      NuGetPackageService service = new(handler);

      NuGetSearchResult? search = await service.SearchAsync("Contoso.Widget");
      PackageVersionInfo? latest = await service.GetLatestVersionsAsync("contoso.widget");
      NuGetSearchResult? again = await service.SearchAsync("CONTOSO.WIDGET");

      search.ShouldNotBeNull();
      again.ShouldNotBeNull();
      search.Versions.Count.ShouldBe(6);
      again.Versions.Select(static version => version.Version).ShouldBe(search.Versions.Select(static version => version.Version));
      Version(search, "2.0.0").Listed.ShouldBeFalse();
      Version(search, "4.0.0-beta").Listed.ShouldBeFalse();
      Version(search, "1.5.0").Listed.ShouldBeTrue();
      Version(search, "1.2.0").Listed.ShouldBeTrue();
      Version(search, "3.0.0-beta").Listed.ShouldBeTrue();
      latest.ShouldNotBeNull();
      latest.StableVersion.ShouldBe("1.5.0");
      latest.PrereleaseVersion.ShouldBe("3.0.0-beta");
      handler.RequestCount.ShouldBe(2);
      handler.RequestUris.ShouldContain("https://api.nuget.org/v3/registration5-gz-semver2/contoso.widget/index.json");
      handler.RequestUris.ShouldContain("https://registration.test/contoso.widget/page.json");
    }

    public static async Task UnlistedOnly_Should_ReturnNullLatestAndKeepSearchVersions()
    {
      using RecordingHandler handler = new(static (_, _) => Task.FromResult(Json("""
        {
          "items": [
            {
              "items": [
                { "catalogEntry": { "version": "2.0.0", "listed": false } },
                { "catalogEntry": { "version": "2.1.0-beta", "listed": false } }
              ]
            }
          ]
        }
        """)));
      NuGetPackageService service = new(handler);

      PackageVersionInfo? latest = await service.GetLatestVersionsAsync("Contoso.Widget");
      NuGetSearchResult? search = await service.SearchAsync("Contoso.Widget");

      latest.ShouldBeNull();
      search.ShouldNotBeNull();
      search.Versions.Count.ShouldBe(2);
      search.Versions.All(static version => !version.Listed).ShouldBeTrue();
      handler.RequestCount.ShouldBe(1);
    }

    public static async Task NotFound_Should_ReturnNullAndLetTheNextCallRetry()
    {
      using RecordingHandler handler = new(static (_, _) =>
        Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));
      NuGetPackageService service = new(handler);

      NuGetSearchResult? search = await service.SearchAsync("Missing.Pkg");
      PackageVersionInfo? latest = await service.GetLatestVersionsAsync("missing.pkg");

      search.ShouldBeNull();
      latest.ShouldBeNull();
      handler.RequestCount.ShouldBe(2);
    }

    public static async Task FailedRead_Should_ThrowAndLetTheNextCallRetry()
    {
      int calls = 0;
      using RecordingHandler handler = new((_, _) =>
      {
        int call = Interlocked.Increment(ref calls);
        if (call == 1)
        {
          return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
        }

        return Task.FromResult(Json("""
          {
            "items": [
              { "items": [ { "catalogEntry": { "version": "1.2.3", "listed": true } } ] }
            ]
          }
          """));
      });
      NuGetPackageService service = new(handler);

      Exception? exception = null;
      try
      {
        await service.SearchAsync("Contoso.Flaky");
      }
      catch (HttpRequestException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<HttpRequestException>();

      NuGetSearchResult? result = await service.SearchAsync("Contoso.Flaky");
      result.ShouldNotBeNull();
      result.Versions.Count.ShouldBe(1);
      result.Versions[0].Version.ShouldBe("1.2.3");
      handler.RequestCount.ShouldBe(2);
    }

    public static async Task ConcurrentCallers_Should_ShareOneInFlightRead()
    {
      TaskCompletionSource<bool> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
      using RecordingHandler handler = new(async (_, _) =>
      {
        await release.Task;
        return Json("""
          {
            "items": [
              { "items": [ { "catalogEntry": { "version": "9.9.9", "listed": true } } ] }
            ]
          }
          """);
      });
      NuGetPackageService service = new(handler);

      try
      {
        Task<NuGetSearchResult?> first = service.SearchAsync("Contoso.Widget");
        Task<PackageVersionInfo?> second = service.GetLatestVersionsAsync("Contoso.Widget");
        await WaitUntilAsync(() => handler.RequestCount >= 1);
        await Task.Delay(50);
        handler.RequestCount.ShouldBe(1);
        release.TrySetResult(true);

        NuGetSearchResult? search = await first;
        PackageVersionInfo? latest = await second;
        search.ShouldNotBeNull();
        latest.ShouldNotBeNull();
        latest.StableVersion.ShouldBe("9.9.9");
        search.Versions.Count.ShouldBe(1);
        handler.RequestCount.ShouldBe(1);
      }
      finally
      {
        release.TrySetResult(true);
      }
    }

    public static async Task InFlightNotFound_Should_ShareOneRequestAndLetALaterCallRetry()
    {
      TaskCompletionSource<bool> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
      int calls = 0;
      using RecordingHandler handler = new(async (_, _) =>
      {
        int call = Interlocked.Increment(ref calls);
        if (call == 1)
        {
          await release.Task;
          return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        }

        return Json("""
          {
            "items": [
              { "items": [ { "catalogEntry": { "version": "1.0.0", "listed": true } } ] }
            ]
          }
          """);
      });
      NuGetPackageService service = new(handler);

      try
      {
        Task<NuGetSearchResult?> first = service.SearchAsync("Contoso.Missing");
        Task<PackageVersionInfo?> second = service.GetLatestVersionsAsync("contoso.missing");
        await WaitUntilAsync(() => Volatile.Read(ref calls) >= 1);
        calls.ShouldBe(1);
        release.TrySetResult(true);

        NuGetSearchResult? search = await first;
        PackageVersionInfo? latest = await second;
        search.ShouldBeNull();
        latest.ShouldBeNull();

        PackageVersionInfo? retried = await service.GetLatestVersionsAsync("Contoso.Missing");
        retried.ShouldNotBeNull();
        retried.StableVersion.ShouldBe("1.0.0");
        calls.ShouldBe(2);
      }
      finally
      {
        release.TrySetResult(true);
      }
    }

    public static async Task CallerCancel_Should_LeaveTheSharedReadForTheOtherCaller()
    {
      TaskCompletionSource<bool> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
      bool sharedTokenCanBeCanceled = true;
      using RecordingHandler handler = new(async (_, cancellationToken) =>
      {
        sharedTokenCanBeCanceled = cancellationToken.CanBeCanceled;
        await release.Task;
        return Json("""
          {
            "items": [
              { "items": [ { "catalogEntry": { "version": "1.0.0" } } ] }
            ]
          }
          """);
      });
      NuGetPackageService service = new(handler);
      using CancellationTokenSource source = new();

      try
      {
        Task<NuGetSearchResult?> cancelled = service.SearchAsync("Contoso.Widget", source.Token);
        Task<PackageVersionInfo?> other = service.GetLatestVersionsAsync("Contoso.Widget");
        await WaitUntilAsync(() => handler.RequestCount >= 1);
        await source.CancelAsync();

        Exception? exception = null;
        try
        {
          await cancelled;
        }
        catch (OperationCanceledException ex)
        {
          exception = ex;
        }

        exception.ShouldNotBeNull();
        exception.ShouldBeAssignableTo<OperationCanceledException>();
        release.TrySetResult(true);

        PackageVersionInfo? latest = await other;
        latest.ShouldNotBeNull();
        latest.StableVersion.ShouldBe("1.0.0");

        NuGetSearchResult? again = await service.SearchAsync("Contoso.Widget");
        again.ShouldNotBeNull();
        handler.RequestCount.ShouldBe(1);
        sharedTokenCanBeCanceled.ShouldBeFalse();
      }
      finally
      {
        release.TrySetResult(true);
      }
    }

    public static async Task EmptyPackageId_Should_ThrowWithoutARequest()
    {
      using RecordingHandler handler = new(static (_, _) => Task.FromResult(Json("{}")));
      NuGetPackageService service = new(handler);

      Exception? exception = null;
      try
      {
        await service.SearchAsync("");
      }
      catch (ArgumentException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentException>();
      handler.RequestCount.ShouldBe(0);
    }

    public static Task NullHandler_Should_Throw()
    {
      Exception? exception = null;
      try
      {
        _ = new NuGetPackageService(null!);
      }
      catch (ArgumentNullException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentNullException>();
      return Task.CompletedTask;
    }

    private static NuGetPackageVersion Version(NuGetSearchResult result, string version) =>
      result.Versions.Single(entry => entry.Version == version);

    private static HttpResponseMessage Json(string json)
    {
      return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
      {
        Content = new StringContent(json)
      };
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
      using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
      while (!condition())
      {
        await Task.Delay(10, timeout.Token);
      }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
      private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder;
      private readonly List<string> Uris = [];
      private int RequestCountField;

      public RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
      {
        Responder = responder;
      }

      public int RequestCount => Volatile.Read(ref RequestCountField);

      public IReadOnlyList<string> RequestUris
      {
        get
        {
          lock (Uris)
          {
            return Uris.ToList();
          }
        }
      }

      protected override async Task<HttpResponseMessage> SendAsync
      (
        HttpRequestMessage request,
        CancellationToken cancellationToken
      )
      {
        lock (Uris)
        {
          Uris.Add(request.RequestUri!.AbsoluteUri);
        }

        Interlocked.Increment(ref RequestCountField);
        return await Responder(request, cancellationToken);
      }
    }
  }
}
