# MouseUnSnag
On a Windows system with multiple monitors, allow the mouse to move freely and not get stuck on corners or edges.

## Getting Started
This is a relatively simple, one-file, command-line C# program that solves the problem of the mouse getting "stuck" on corners and edges when moving between multiple monitors on a Windows system.

You can download and run this project's "released" executable (currently less than 15Kbytes!!)  [RELEASE LINK](https://github.com/dale-roberts/MouseUnSnag/releases), or click on "release" in the GitHub GUI above.

If you like, you may instead compile the program at the command line. It requires at least version 7 of the C# compiler, and an up to date .NET Framework SDK and targeting pack. You can install these with [Visual Studio Installer](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio)). Make sure to select ".NET Framework x.y.z SDK" and ".NET Framework x.y.z targeting pack" under individual components.

This will install the C# compiler, which you can access using the "Developer Command Prompt" n the start menu. You can then run the following command in the source repository's root directory to build the solution:

```
MSBuild MouseUnSnag.sln
```

The executable is build in the `bin` (`debug`) by default. You can run the program by just typing the name of the executable, `.\bin\Debug\MouseUnSnag`. That's it!!

## What does it do?
The program endeavors to fix two separate problems related to multiple monitors, which were brought up in a couple of **superuser.com** posts. The respective authors also provided nice graphical representations of the problems.

1. Mouse gets stuck on corners. This is an intentional feature of Windows, to keep the mouse from "sliding off" the corner of the monitor when you are trying to get to the task bar at the bottom of the screen. But if you find it annoying rather than helpful, **MouseUnSnag** will fix it for you.

   [How to disable sticky corners in Windows 10 - superuser.com](https://superuser.com/questions/947817/how-to-disable-sticky-corners-in-windows-10)
   
   <img src=https://i.stack.imgur.com/RxDz4.png width="400"/>

2. Mouse gets stuck on an edge, where one monitor is taller than the adjacent monitor.

   [How to make the mouse wrap from corners when moving between monitors? - superuser.com](https://superuser.com/questions/865469/how-to-make-the-mouse-wrap-from-corners-when-moving-between-monitors)
   
   <img src=https://i.stack.imgur.com/5Rlji.png width="400"/>

3. **MouseUnSnag** also wraps the cursor around from the right edge of the rightmost monitor, to the left edge of the leftmost monitor, and vice versa. (I don't have a fancy graphic for that one!)

## Command Line Options

The MouseUnSnag executable takes the following command line options in the format: `MouseUnSnag.exe [options ...]`. All options are enabled by default.

* `-s`, `+s`

  Disable or enable respectively unsticking the cursor from sticky corners.

* `-j`, `+j`

  Disable or enable respectively jumping the mose from an edge to the ajaccent monitor.

* `-w`, `+w`

  Disable or enable respectively wraping the cursor from one far edge to the other.

## How does it do it?
**MouseUnSnag** uses the low-level Win32 WH_MOUSE_LL callback to monitor the user's intended movement of the mouse. It also monitors the current position of the cursor on the screen. When **MouseUnSnag** detects that the mouse has tried to move beyond the edge/corner of the screen, but the cursor was not able to move, then it knows that the cursor is "stuck", and will attempt to sensibly move the cursor to an adjacent monitor, if one exists.

## Future Directions
Although I believe this is quite a *useful* program, it is obviously not very "user friendly". It just runs at the command line (you must "minimize" the CMD console window, to get it out of the way), and prints out some debugging information on the console as you move the mouse around. If more than three or four people actually wind up using the program, and there is interest, it may be worth investing some effort towards some improvements, such as:

* Allow more complex options for removing cursor "stickiness" only on particular edges or corners.
