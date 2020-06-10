# Anti Debugging

C# Anti-Debug and Anti-Dumping techniques using Win32/NT API functions. There are certain functions/methods like the anti-dump that were created by other people.

### Features

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
