# LCDMagic
Make LCD screen's content related to nearby devices

For building, define EmpyrionInstallDir variable to point to where Empyrion is installed.  The build needs to get at the Unity core dll to have access to color and such from Unity, and I don't want to just include the dll here.

I set it via tasks like:

```json
	"tasks": [
		{
			"label": "build",
			"command": "dotnet build /p:EmpyrionInstallDir=\"D:/SteamLibrary/steamapps/common/Empyrion - Galactic Survival\"",
			"type": "shell",
```
etc...
