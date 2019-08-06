# TCore.Logging
Logging support for TCore projects


CorellationID
An ID that can be used to follow a log or command through the system. Easy to create, globally unique. If someone gives you one, pass it to the
logging function (to preserve the chain). If you didn't get one, then just create one.

Some code (like web services) don't want a complicated class like a CRID, so CRIDS is provided (CorrelationID.Crids) -- this is just the guid

Some code is even more picky any just wants an "int" as an indentifier for a correlation. Use CorrelationID.Hash2 if you need this (though its no longer
globally unique)

EventType
Strongly based on windows event types. just allows filtering/sorting

ILogProvider
This is the interface provided by all logging providers, regardless of their backing protocol (Azure, System.Diagnostics, logging to file...).
This allows you to start out with a simple LogProviderFile and swap in a more complicated logging later without changing all your code that
is making logging calls...


Getting Started:
For win32 C# clients, easiest way to get started is to 