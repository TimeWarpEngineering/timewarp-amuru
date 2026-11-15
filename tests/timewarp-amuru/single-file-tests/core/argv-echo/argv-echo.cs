#!/usr/bin/env -S dotnet --
#:property GeneratePackageOnBuild=false
#:property PublishAot=false

#region Purpose
// Child process for the argument round-trip property: reports the argv it received.
#endregion

#region Design
// Output is one line per argument: the Base64 of the argument's UTF-8 bytes. An empty argument
// is an empty line. Base64 keeps the report free of newlines, quotes, and console code pages,
// so the parent decodes exactly what the OS handed this process on either platform.
// The first line is the argument count, so an empty list and one empty argument differ.
// The last line is "end", so a trailing empty argument is never lost to line trimming.
#endregion

using System.Text;

using Stream stdout = System.Console.OpenStandardOutput();
StringBuilder report = new();
report.Append(args.Length).Append('\n');
foreach (string argument in args)
{
  report.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(argument))).Append('\n');
}

report.Append("end\n");
byte[] bytes = Encoding.ASCII.GetBytes(report.ToString());
stdout.Write(bytes, 0, bytes.Length);
return 0;
