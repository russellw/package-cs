# package-cs
Package compiled C# program for distribution

A common way to distribute a program (as opposed to a library) is as a compressed archive of binary files.

In C#, `dotnet publish` can produce output that looks like this:

```
 Directory of C:\package-cs\bin\Release\net7.0\publish

11/19/2023  04:40 PM    <DIR>          .
11/19/2023  03:59 PM    <DIR>          ..
11/19/2023  01:38 PM               422 package-cs.deps.json
11/19/2023  03:59 PM             9,728 package-cs.dll
11/19/2023  03:59 PM           154,624 package-cs.exe
11/19/2023  03:59 PM            11,564 package-cs.pdb
11/19/2023  01:38 PM               253 package-cs.runtimeconfig.json
               5 File(s)        176,591 bytes
```

The above is the simplest case; in the general case, many programs will use various libraries that will also be included as DLLs in the `publish` directory.

The `.exe` is the program that is directly run by the user, but it needs to be accompanied by the other files.

`package-cs` runs `dotnet publish` and makes an archive containing all the files in the `publish` directory. If the input is a project file like `foo.csproj`, the output is an archive like `foo-1.0.zip` (placed in the `bin` directory so it will be ignored by version control). Project version is read from `foo.csproj`, defaults to 1.0.
