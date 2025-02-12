# FF7 Rebirth Data Object Editor
FF7 Rebirth Data Object Editor is a free & simple editor for Final Fantasy VII Rebirth's `DataObject` `.uasset` files, and easily edit them straight after extraction, in their `IOStore` state. (Such as when obtained with `FModel`)

# Features
- Open `DataObject` `.uasset` files and see the contents
- Edit primitives, FNames & adjust array sizes
- Provide human-interface, for an easier modding experience
- Export & Import from `.csv` format

# Usage
1. Double Click `FF7RebirthDataObjectEditor.exe` to open it
2. Press `Open File...` on the top right, and choose a `.uasset` that is compatible
3. (Optional) Customize any values you'd like to by selecting them and typing the new value.
   - Invalid values will not be submitted (such as typing words into a numeric field)
   - Arrays open up a floating panel for further editing
5. Press `Save File To...` and choose a save file name and location.
6. Pack the edited `.uasset` file into a mod such as with `UnrealReZen`.
Note: The editor creates `appsettings.json` on its first time running, near the executable. This stores the last file paths opened and saved, to increase iteration convenience in the same directories.

# Requirements
- .NET 8 Desktop Runtime

# Build
- The solution's projects use some C# 12 syntax, so whatever can build that should work

# Credits
- FF7 Rebirth Data Object Editor is written by Yoraiz0r
- [FF7R2-Data-Parser](https://github.com/Synthlight/FF7R2-DataObject-Parser) by LordGregory
- Special thanks to Ray Cooper, normie, and the entire OpenFF7R community!
