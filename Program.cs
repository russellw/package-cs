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
	}

	static void Help() {
		Console.WriteLine("Usage: package-cs [options] [file.csproj]");
		Console.WriteLine();
		Console.WriteLine("-h  Show help");
		Console.WriteLine("-V  Show version");
	}
}
