[![Build Status](https://ci.appveyor.com/api/projects/status/b6i7awj29e0tl71m?svg=true)](https://ci.appveyor.com/project/bezzad/downloader) 
[![NuGet](https://img.shields.io/nuget/dt/antidebugging.svg)](https://www.nuget.org/packages/downloader) 
[![NuGet](https://img.shields.io/nuget/vpre/antidebugging.svg)](https://www.nuget.org/packages/downloader)

![Anti Debugging](https://raw.githubusercontent.com/bezzad/AntiDebugging/master/src/AntiDebugging/AntiDebugging/logo.ico)

# Anti Debugging

C# Anti-Debug and Anti-Dumping techniques using Win32/NT API functions. There are certain functions/methods like the anti-dump that were created by other people.

### Features at a glance

* PoC: Prevent a debugger from attaching to managed .NET processes via a watcher process code pattern.
* Anti Virtual Machine & VPS
* Anti Dump - Clears headers and some secret magic ontop (WARNING! It breaks applications which are obfuscated)
* Check for managed debugger
* Check for unmanaged debugger
* Check for remote debugger
* Check debug port
* Detach from debugger process
* Check for kernel debugger
* Hides current process OS thread ( managed threads soon )
* Scan and Kill debuggers (ollydbg, x32dbg, x64dbg, Immunity, MegaDumper, etc)

### How to use

Get it on [NuGet](https://www.nuget.org/packages/AntiDebugger):

    PM> Install-Package AntiDebugging

Or via the .NET Core command line interface:

    dotnet add package AntiDebugging

