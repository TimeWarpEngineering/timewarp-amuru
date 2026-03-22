#!/usr/bin/dotnet --

#region Purpose
// Tests for NuGetPackageService - validates NuGet search operations
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

    public static async Task SearchAsync_WithNonExistentPackage_ShouldReturnNull()
    {
      NuGetPackageService service = new();

      string nonExistentPackage = $"non-existent-package-{Guid.NewGuid()}";
      NuGetSearchResult? result = await service.SearchAsync(nonExistentPackage);

      result.ShouldBeNull();
    }
  }
}
