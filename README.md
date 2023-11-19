# package-cs
Package compiled C# program for distribution

A common way to distribute a program (as opposed to a library) is as a compressed archive of binary files.

In C#, `dotnet publish` can produce output that looks like this:

```
 Directory of C:\package-cs\bin\Release\net7.0\publish

11/19/2023  03:59 PM    <DIR>          .
11/19/2023  03:59 PM    <DIR>          ..
11/19/2023  03:59 PM               241 package-cs.bat
11/19/2023  01:38 PM               422 package-cs.deps.json
11/19/2023  03:59 PM             9,728 package-cs.dll
11/19/2023  03:59 PM           154,624 package-cs.exe
11/19/2023  03:59 PM            11,564 package-cs.pdb
11/19/2023  01:38 PM               253 package-cs.runtimeconfig.json
               6 File(s)        176,832 bytes
```
