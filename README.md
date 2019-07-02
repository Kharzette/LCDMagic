# LCDMagic
Make LCD screen's content related to nearby devices

![alt text](https://github.com/Kharzette/MiscMedia/blob/master/ammobox.gif?raw=true "Ammo Box LCD")

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

Variables can be defined in the custom name of the LCD.  Just add it to a group to get the ability to set a name.  So far I have $Fnt for font size (allows you to go beyond the limits of the gui), and $CW for column width for psuedo faked columns that look bad in the variable width font.

LCDs will try to find the nearest usable device within the BlockScanDistance set in Constants.txt.  I'm not really sure how the origin of most of the devices is determined so results might get strange in a super compact base or vehicle.
