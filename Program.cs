using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

internal class Program {
	static string projectName = null!;
	static string projectVersion = null!;
	static string publishPath = null!;

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
				continue;
			}
			if (csproj != null) {
				Console.WriteLine(s + ": csproj already specified");
				Environment.Exit(1);
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

		// Parameters from csproj
		var targetFramework = "net7.0";
		var targetFrameworkRegex = new Regex("<TargetFramework>(.*)</TargetFramework>",
											 RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
		var version = "1.0";
		var versionRegex = new Regex(@"<AssemblyVersion>(\d+\.\d+)\.\d+\.\d+\</AssemblyVersion>",
									 RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
		foreach (var s in File.ReadLines(csproj)) {
			var match = targetFrameworkRegex.Match(s);
			if (match.Success)
				targetFramework = match.Groups[1].Value;

			match = versionRegex.Match(s);
			if (match.Success)
				version = match.Groups[1].Value;
		}
		projectName = Path.GetFileNameWithoutExtension(csproj);
		projectVersion = $"{projectName}-{version}";
		publishPath = $"bin/Release/{targetFramework}/publish";

		// Make archives
		Tar();
		Zip();
	}

	static void Help() {
		Console.WriteLine("Usage: package-cs [options] [file.csproj]");
		Console.WriteLine();
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-V  Show version");
	}

	static void Tar() {
		var archiveName = $"bin/{projectVersion}.tar.gz";
		using var archiveStream = File.Create(archiveName);
		using var gzipStream = new GZipOutputStream(archiveStream);
		using var archive = TarArchive.CreateOutputTarArchive(gzipStream);
		archive.RootPath = projectVersion;

		// Add all the published files
		foreach (var path in Directory.GetFileSystemEntries(publishPath)) {
			var entry = TarEntry.CreateEntryFromFile(path);
			entry.Name = Path.GetFileName(path);
			archive.WriteEntry(entry, false);
		}

		// Report success
		Console.WriteLine(archiveName);
	}

	static void Zip() {
		var archiveName = $"bin/{projectVersion}.zip";
		using var archiveStream = File.Create(archiveName);
		using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create);

		// Add all the published files
		foreach (var path in Directory.GetFileSystemEntries(publishPath)) {
			using var inputStream = File.OpenRead(path);
			var entry = archive.CreateEntry($"{projectVersion}/{Path.GetFileName(path)}", CompressionLevel.SmallestSize);
			using var outputStream = entry.Open();
			inputStream.CopyTo(outputStream);
		}

		// Write a script
		var scriptEntry = archive.CreateEntry($"{projectVersion}/{projectName}.bat", CompressionLevel.SmallestSize);
		using var scriptStream = scriptEntry.Open();
		using var writer = new StreamWriter(scriptStream);
		writer.NewLine = "\n";
		writer.WriteLine("@echo off");
		writer.WriteLine("rem This file can provide a convenient command to run " + projectName);
		writer.WriteLine("rem To use it as such,");
		writer.WriteLine("rem change it to point to where you put your copy of " + projectName);
		writer.WriteLine("rem and put it in a directory in your PATH");
		writer.WriteLine($"C:\\{projectVersion}\\{projectName}.exe %*");

		// Report success
		Console.WriteLine(archiveName);
	}
}
