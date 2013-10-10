GAPP
====

Globalcaching Application, the geocache manager


First Build
===========

- Open the solution in Visual C# Express 2010 (or higher)
- Select target x86
- Build the GlobalcachingApplication project first
- Build the complete solution

The fact that you need to build the application first is that other projects copy their output to the output folder
of the application (like Plugins). Initially the folder does not exist.
