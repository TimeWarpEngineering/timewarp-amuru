#region Purpose
// The process operations the JSON-RPC session machine calls.
#endregion

#region Design
// Start, write one frame, read one frame, and stop. Tests substitute a fake.
// A written frame is one JSON object plus the newline that delimits NDJSON.
// A read frame is that JSON object without the delimiter. Null means the child closed stdout.
// StopAsync is what Stopping's activity awaits. Its completion posts ChildExited.
// Anonymous pipes, KillOnParentExit, and ProcessExitStatus are not this seam.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

internal interface IJsonRpcProcessSeam
{
  Task StartAsync(CancellationToken cancellationToken);

  Task WriteFrameAsync(string frame, CancellationToken cancellationToken);

  Task<string?> ReadFrameAsync(CancellationToken cancellationToken);

  Task StopAsync(CancellationToken cancellationToken);
}
