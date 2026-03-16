namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Disposable scope that ensures mock state is cleaned up.
/// </summary>
internal sealed class MockScope : IDisposable
{
  private readonly Action cleanupAction;
  private bool disposed;
  
  internal MockScope(Action cleanupAction)
  {
    this.cleanupAction = cleanupAction ?? throw new ArgumentNullException(nameof(cleanupAction));
  }
  
  public void Dispose()
  {
    if (!disposed)
    {
      cleanupAction();
      disposed = true;
    }
  }
}