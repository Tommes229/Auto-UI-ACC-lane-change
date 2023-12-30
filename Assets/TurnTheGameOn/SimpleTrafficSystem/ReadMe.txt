Support and Discussion Forum: https://forum.unity.com/threads/.794268/
Online Manual: https://simpletrafficsystem.turnthegameon.com/
Contact: stephen@turnthegameon.com


This package uses the new C# Job System and Burst Compiler, make the following project configurations to enable these Unity features.

1. Open the Player Settings (Edit -> Project Settings -> Player), set API Compatibility Level .Net 4.x

2. (This step is auto installed on import) Open the Package Manager (Window -> Package Manager), install Burst (1.2.3), install Collections (0.1.1)

3. To use the burst compiler in standalone builds, install the Windows SDK and VC++ toolkit from the Visual Studio Installer.