# dev CLI telemetry

`dev` calls `UseTelemetry()` with the service name `timewarp-amuru-dev`. The OTLP endpoint is not set in code. A run exports traces, metrics, and logs only when the endpoint variable below is set. An unset variable exports nothing.

Every route, including the DevCli routes (`clean`, `check-version`, `release`, `self-install`), gets one `TimeWarp.Nuru.Behavior` command span and a duration metric. This repo has no `ActivitySource` of its own.

The `dotnet` processes themselves are not instrumented. When a route calls `DotNet.Build()`, `DotNet.Publish()`, `DotNet.Pack()`, or `DotNet.Test()` from TimeWarp.Amuru.Tools, Amuru adds three tags to `Activity.Current` (the command span). No other argv, path, or property value is exported:

| Tag | Value |
|-----|-------|
| `build.command` | `build`, `publish`, `pack`, or `test` |
| `build.configuration` | The configuration passed with `WithConfiguration` or a `Configuration` property. Absent when none is passed. |
| `build.publish_aot` | `true` for a Native AOT publish: a `PublishAot` property, a `.cs` runfile (the SDK default) without `#:property PublishAot=false`, or a project file with `<PublishAot>true</PublishAot>`. `false` otherwise. |

`dev build` goes through `DotNet.Build()`, and `dev self-install` publishes `dev.cs` through `DotNet.Publish()`, so the two are told apart by `build.command` and `build.publish_aot`. `dev test` runs the test runfile with `dotnet run` and carries no `build.*` tags.

The GitHub Actions workflows in this repo do not set a collector or an endpoint.

## Environment

Set these on the `dev` process when a GitHub Actions job or a ganda walk wants the trace:

| Variable | Required | Meaning |
|----------|----------|---------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | to export | OTLP collector base URL. Unset means `dev` exports nothing. |
| `OTEL_SERVICE_NAME` | no | Overrides `timewarp-amuru-dev`. |
| `TRACEPARENT` | no | W3C trace context (`00-{trace-id}-{span-id}-{flags}`) when ganda, or another parent process, is the caller. |
| `TRACESTATE` | no | Optional W3C tracestate that accompanies `TRACEPARENT`. |

`OTEL_EXPORTER_OTLP_PROTOCOL` may be `grpc` (the SDK default) or `http/protobuf`.

`TelemetryBehavior` parents the command span to `Activity.Current`. When `TRACEPARENT` is valid, `dev` restores that context onto `Activity.Current` before `RunAsync`, using the caller's trace id and span id, so the command span joins the caller's trace. The restored activity is not itself exported.
