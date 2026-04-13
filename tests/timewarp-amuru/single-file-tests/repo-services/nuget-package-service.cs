#!/usr/bin/dotnet --

#region Purpose
// Tests for NuGetPackageService - validates NuGet Protocol API operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Repo_Services
{
  [TestTag("Repo")]
  public class NuGetPackageService_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NuGetPackageService_Given_>();

    public static async Task SearchAsync_WithEmptyPackageId_ShouldThrow()
    {
      NuGetPackageService service = new();

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
    }

    public static async Task SearchAsync_WithNullPackageId_ShouldThrow()
    {
      NuGetPackageService service = new();

      Exception? exception = null;
      try
      {
        await service.SearchAsync(null!);
      }
      catch (ArgumentNullException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentNullException>();
    }

    public static async Task SearchAsync_WithValidPackage_ShouldReturnResult()
    {
      NuGetPackageService service = new();

      NuGetSearchResult? result = await service.SearchAsync("Newtonsoft.Json");

      result.ShouldNotBeNull();
      result.PackageId.ShouldBe("Newtonsoft.Json");
      result.Versions.Count.ShouldBeGreaterThan(0);
    }

    public static async Task SearchAsync_WithValidPackage_ShouldReturnMultipleVersions()
    {
      NuGetPackageService service = new();

      NuGetSearchResult? result = await service.SearchAsync("Newtonsoft.Json");

      result.ShouldNotBeNull();
      result.Versions.Count.ShouldBeGreaterThan(1);
    }

    public static async Task SearchAsync_WithPrereleasePackage_ShouldIncludePrereleaseVersions()
    {
      NuGetPackageService service = new();

      NuGetSearchResult? result = await service.SearchAsync("TimeWarp.Amuru");

      result.ShouldNotBeNull();
      result.Versions.Any(static v => v.Version.Contains('-', StringComparison.Ordinal)).ShouldBeTrue();
    }

    public static async Task SearchAsync_WithNonExistentPackage_ShouldReturnNull()
    {
      NuGetPackageService service = new();

      string nonExistentPackage = $"non-existent-package-{Guid.NewGuid()}";
      NuGetSearchResult? result = await service.SearchAsync(nonExistentPackage);

      result.ShouldBeNull();
    }
  }

  [TestTag("Repo")]
  public class NuGetPackageService_ParseVersion_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NuGetPackageService_ParseVersion_Given_>();

    public static Task ParseVersion_WithStandardSemver_ShouldReturnNormalized()
    {
      NuGetPackageService service = new();

      string? result = service.ParseVersion("1.2.3");

      result.ShouldBe("1.2.3");
      return Task.CompletedTask;
    }

    public static Task ParseVersion_WithLeadingV_ShouldStripV()
    {
      NuGetPackageService service = new();

      string? result = service.ParseVersion("v1.2.3");

      result.ShouldBe("1.2.3");
      return Task.CompletedTask;
    }

    public static Task ParseVersion_WithPrerelease_ShouldReturnNormalized()
    {
      NuGetPackageService service = new();

      string? result = service.ParseVersion("1.2.3-beta.1");

      result.ShouldBe("1.2.3-beta.1");
      return Task.CompletedTask;
    }

    public static Task ParseVersion_WithInvalidVersion_ShouldReturnNull()
    {
      NuGetPackageService service = new();

      string? result = service.ParseVersion("not-a-version");

      result.ShouldBeNull();
      return Task.CompletedTask;
    }

    public static Task ParseVersion_WithTrailingZeros_ShouldNormalize()
    {
      NuGetPackageService service = new();

      string? result = service.ParseVersion("1.2.3.0");

      result.ShouldBe("1.2.3");
      return Task.CompletedTask;
    }

    public static Task ParseVersion_WithEmptyString_ShouldThrow()
    {
      NuGetPackageService service = new();

      Exception? exception = null;
      try
      {
        service.ParseVersion("");
      }
      catch (ArgumentException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentException>();
      return Task.CompletedTask;
    }
  }

  [TestTag("Repo")]
  public class NuGetPackageService_CompareVersions_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NuGetPackageService_CompareVersions_Given_>();

    public static Task CompareVersions_MajorDifference_ShouldReturnNegative()
    {
      NuGetPackageService service = new();

      int result = service.CompareVersions("1.0.0", "2.0.0");

      result.ShouldBe(-1);
      return Task.CompletedTask;
    }

    public static Task CompareVersions_MinorDifference_ShouldReturnNegative()
    {
      NuGetPackageService service = new();

      int result = service.CompareVersions("1.0.0", "1.1.0");

      result.ShouldBe(-1);
      return Task.CompletedTask;
    }

    public static Task CompareVersions_PatchDifference_ShouldReturnNegative()
    {
      NuGetPackageService service = new();

      int result = service.CompareVersions("1.0.0", "1.0.1");

      result.ShouldBe(-1);
      return Task.CompletedTask;
    }

    public static Task CompareVersions_EqualVersions_ShouldReturnZero()
    {
      NuGetPackageService service = new();

      int result = service.CompareVersions("1.0.0", "1.0.0");

      result.ShouldBe(0);
      return Task.CompletedTask;
    }

    public static Task CompareVersions_PrereleaseVsStable_ShouldReturnNegative()
    {
      NuGetPackageService service = new();

      int result = service.CompareVersions("1.0.0-beta", "1.0.0");

      result.ShouldBeLessThan(0);
      return Task.CompletedTask;
    }

    public static Task CompareVersions_InvalidVersion1_ShouldThrow()
    {
      NuGetPackageService service = new();

      Exception? exception = null;
      try
      {
        service.CompareVersions("not-a-version", "1.0.0");
      }
      catch (ArgumentException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentException>();
      return Task.CompletedTask;
    }

    public static Task CompareVersions_InvalidVersion2_ShouldThrow()
    {
      NuGetPackageService service = new();

      Exception? exception = null;
      try
      {
        service.CompareVersions("1.0.0", "not-a-version");
      }
      catch (ArgumentException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentException>();
      return Task.CompletedTask;
    }
  }

  [TestTag("Repo")]
  public class NuGetPackageService_GetUpdateType_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NuGetPackageService_GetUpdateType_Given_>();

    public static Task GetUpdateType_MajorUpdate_ShouldReturnMajor()
    {
      NuGetPackageService service = new();

      string result = service.GetUpdateType("1.0.0", "2.0.0");

      result.ShouldBe("major");
      return Task.CompletedTask;
    }

    public static Task GetUpdateType_MinorUpdate_ShouldReturnMinor()
    {
      NuGetPackageService service = new();

      string result = service.GetUpdateType("1.0.0", "1.1.0");

      result.ShouldBe("minor");
      return Task.CompletedTask;
    }

    public static Task GetUpdateType_PatchUpdate_ShouldReturnPatch()
    {
      NuGetPackageService service = new();

      string result = service.GetUpdateType("1.0.0", "1.0.1");

      result.ShouldBe("patch");
      return Task.CompletedTask;
    }

    public static Task GetUpdateType_PrereleaseToStable_ShouldReturnStable()
    {
      NuGetPackageService service = new();

      string result = service.GetUpdateType("1.0.0-beta.1", "1.0.0");

      result.ShouldBe("stable");
      return Task.CompletedTask;
    }

    public static Task GetUpdateType_EqualVersions_ShouldReturnNone()
    {
      NuGetPackageService service = new();

      string result = service.GetUpdateType("1.0.0", "1.0.0");

      result.ShouldBe("none");
      return Task.CompletedTask;
    }

    public static Task GetUpdateType_CurrentNewer_ShouldReturnNone()
    {
      NuGetPackageService service = new();

      string result = service.GetUpdateType("2.0.0", "1.0.0");

      result.ShouldBe("none");
      return Task.CompletedTask;
    }

    public static Task GetUpdateType_InvalidCurrent_ShouldThrow()
    {
      NuGetPackageService service = new();

      Exception? exception = null;
      try
      {
        service.GetUpdateType("not-a-version", "1.0.0");
      }
      catch (ArgumentException ex)
      {
        exception = ex;
      }

      exception.ShouldBeOfType<ArgumentException>();
      return Task.CompletedTask;
    }
  }

  [TestTag("Repo")]
  public class NuGetPackageService_GetLatestVersionsAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NuGetPackageService_GetLatestVersionsAsync_Given_>();

    public static async Task GetLatestVersionsAsync_WithValidPackage_ShouldReturnVersions()
    {
      NuGetPackageService service = new();

      PackageVersionInfo? result = await service.GetLatestVersionsAsync("Newtonsoft.Json");

      result.ShouldNotBeNull();
      result.StableVersion.ShouldNotBeNull();
    }

    public static async Task GetLatestVersionsAsync_WithNonExistentPackage_ShouldReturnNull()
    {
      NuGetPackageService service = new();

      string nonExistentPackage = $"non-existent-package-{Guid.NewGuid()}";
      PackageVersionInfo? result = await service.GetLatestVersionsAsync(nonExistentPackage);

      result.ShouldBeNull();
    }

    public static async Task GetLatestVersionsAsync_WithPrereleasePackage_ShouldReturnPrereleaseVersion()
    {
      NuGetPackageService service = new();

      PackageVersionInfo? result = await service.GetLatestVersionsAsync("TimeWarp.Amuru");

      result.ShouldNotBeNull();
      result.PrereleaseVersion.ShouldNotBeNull();
    }
  }
}
