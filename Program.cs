using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

internal class Program {
	static void Main(string[] args) {
		var options = true;
		string? csproj = null;
		foreach (var arg in args) {
			var s = arg;
			if (options) {
				if (s == "--") {
					options = false;
					continue;
				}
				if (s.StartsWith('-')) {
					while (s.StartsWith('-'))
						s = s[1..];
					switch (s) {
					case "?":
					case "h":
					case "help":
						Help();
						return;
					case "V":
					case "v":
					case "version":
						Console.WriteLine("package-cs " + typeof(Program).Assembly.GetName()?.Version?.ToString(2));
						return;
					default:
						Console.WriteLine(arg + ": unknown option");
						Environment.Exit(1);
						break;
					}
				}
			}
			csproj = s;
		}
		if (csproj == null) {
			foreach (var entry in new DirectoryInfo(".").EnumerateFileSystemInfos())
				if (entry is FileInfo && entry.Extension == ".csproj") {
					csproj = entry.FullName;
					break;
				}
			if (csproj == null) {
				Console.WriteLine("csproj not specified, and not found in current directory");
				Environment.Exit(1);
			}
		}

		// Build
		var process = new Process();
		process.StartInfo.FileName = "dotnet";
		process.StartInfo.Arguments = "publish /p:Configuration=Release /p:Platform=\"Any CPU\"";
		process.Start();
		process.WaitForExit();
		if (process.ExitCode != 0)
			Environment.Exit(process.ExitCode);

		var targetFramework = "net7.0";
		var targetFrameworkRegex = new Regex("<TargetFramework>(.*)</TargetFramework>",
											 RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

		// Check csproj for parameters
		var version = "1.0";
		foreach (var s in File.ReadLines(csproj)) {
			var match = targetFrameworkRegex.Match(s);
			if (match.Success)
				targetFramework = match.Groups[1].Value;
		}

		// Zip
		var projectVersion = $"{Path.GetFileNameWithoutExtension(csproj)}-{version}";
		var zipName = $"bin/{projectVersion}.zip";
		using var zip = File.Create(zipName);
		using var archive = new ZipArchive(zip, ZipArchiveMode.Update);
		foreach (var path in Directory.GetFileSystemEntries($"bin/Release/{targetFramework}/publish")) {
			var entry = archive.CreateEntry($"{projectVersion}/{Path.GetFileName(path)}");
			using var reader = new StreamReader(path);
			using var writer = new StreamWriter(entry.Open());
			writer.Write(reader.ReadToEnd());
		}
		Console.WriteLine(zipName);
	}

	static void Help() {
		Console.WriteLine("Usage: package-cs [options] [file.csproj]");
		Console.WriteLine();
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-V  Show version");
	}
}
