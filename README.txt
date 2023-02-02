PlateUp! Modding Template

Using this template:
 - You can rename the ModName.csproj to your liking.
 - There are two build configurations, DEBUG and RELEASE. 
   They are identical except for a boolean value set in 
   the Mod.cs file, allowing you to easily test debug code 
   by building DEBUG vs RELEASE.
 - Build artifacts are automatically placed in the content/
   folder. When uploading your mod, select the project folder
   (i.e., the folder which *contains* content/) as your "mod
   folder."

Using asset bundles:
 - If you do not need to use asset bundles in your mod, you
   can safely delete the UnityProject/ folder.
 - Otherwise, enable asset bundle support by uncommenting
   the corresponding lines in the Mod.cs file as well as
   changing <EnableAssetBundleDeploy> to 'true' in
   ModName.csproj.
 - The Unity Project is pre-configured to make creating
   asset bundles quick and easy. Just tag your prefabs and
   other assets (sounds, JSON files, etc.) with the
   "mod.assets" bundle tag and click the
   "PlateUp!>Build Asset Bundle" option in the menu bar.
 - After building your asset bundle (which will be placed
   in content/), you will need to rebuild your C# project
   in Visual Studio by using the "Build>Rebuild Solution"
   menu option.