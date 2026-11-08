#region Purpose
// Anonymous pipe ends for one JSON-RPC child.
#endregion

#region Design
// Each pipe has a child end passed to ProcessStartInfo and a parent end kept by this process.
// Taking a parent end clears it so disposing the set does not dispose a handle a stream owns.
// The child ends are disposed after Process.Start, which has already duplicated them.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using Microsoft.Win32.SafeHandles;

internal sealed partial class JsonRpcChildProcess
{
  private static PipeEnds OpenPipes()
  {
    SafeFileHandle.CreateAnonymousPipe
    (
      out SafeFileHandle childInput,
      out SafeFileHandle parentInput,
      asyncRead: false,
      asyncWrite: true
    );
    SafeFileHandle.CreateAnonymousPipe
    (
      out SafeFileHandle parentOutput,
      out SafeFileHandle childOutput,
      asyncRead: true,
      asyncWrite: false
    );
    SafeFileHandle.CreateAnonymousPipe
    (
      out SafeFileHandle parentError,
      out SafeFileHandle childError,
      asyncRead: true,
      asyncWrite: false
    );
    return new PipeEnds(childInput, parentInput, childOutput, parentOutput, childError, parentError);
  }

  private sealed class PipeEnds : IDisposable
  {
    private SafeFileHandle? ChildInputHandle;
    private SafeFileHandle? ParentInputHandle;
    private SafeFileHandle? ChildOutputHandle;
    private SafeFileHandle? ParentOutputHandle;
    private SafeFileHandle? ChildErrorHandle;
    private SafeFileHandle? ParentErrorHandle;

    public PipeEnds
    (
      SafeFileHandle childInput,
      SafeFileHandle parentInput,
      SafeFileHandle childOutput,
      SafeFileHandle parentOutput,
      SafeFileHandle childError,
      SafeFileHandle parentError
    )
    {
      ChildInputHandle = childInput;
      ParentInputHandle = parentInput;
      ChildOutputHandle = childOutput;
      ParentOutputHandle = parentOutput;
      ChildErrorHandle = childError;
      ParentErrorHandle = parentError;
    }

    public SafeFileHandle ChildInput => ChildInputHandle ?? throw new ObjectDisposedException(nameof(PipeEnds));

    public SafeFileHandle ParentInput => Take(ref ParentInputHandle);

    public SafeFileHandle ChildOutput => ChildOutputHandle ?? throw new ObjectDisposedException(nameof(PipeEnds));

    public SafeFileHandle ParentOutput => Take(ref ParentOutputHandle);

    public SafeFileHandle ChildError => ChildErrorHandle ?? throw new ObjectDisposedException(nameof(PipeEnds));

    public SafeFileHandle ParentError => Take(ref ParentErrorHandle);

    public void DisposeChildEnds()
    {
      DisposeHandle(ref ChildInputHandle);
      DisposeHandle(ref ChildOutputHandle);
      DisposeHandle(ref ChildErrorHandle);
    }

    public void DisposeAll()
    {
      DisposeChildEnds();
      DisposeHandle(ref ParentInputHandle);
      DisposeHandle(ref ParentOutputHandle);
      DisposeHandle(ref ParentErrorHandle);
    }

    public void Dispose() => DisposeAll();

    private static SafeFileHandle Take(ref SafeFileHandle? handle)
    {
      SafeFileHandle taken = handle ?? throw new ObjectDisposedException(nameof(PipeEnds));
      handle = null;
      return taken;
    }

    private static void DisposeHandle(ref SafeFileHandle? handle)
    {
      handle?.Dispose();
      handle = null;
    }
  }
}
