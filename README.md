## Existing Template:

- `rtmod`  - UnityModManager Template for Warhammer 40,000: Rogue Trader
- `rtmodworkshop`  - UnityModManager Template for Warhammer 40,000: Rogue Trader. Adds a publishing task which will directly publish to Steam Workshop.
- `rtbpmod`  - UnityModManager Template for Warhammer 40,000: Rogue Trader. Adds a patch for modifying the Blueprint cache.
- `rtbpmodworkshop`  - UnityModManager Template for Warhammer 40,000: Rogue Trader. Adds a patch for modifying the Blueprint cache. Adds a publishing task which will directly publish to Steam Workshop.
- `wrathmod`  - UnityModManager Template for Pathfinder: Wrath of the Righteous
- `wrathsoundmod`  - Wwise Template to add new sounds/voices to the game, bundled with a UnityModManager Template for Pathfinder: Wrath of the Righteous. Read the docs (WIP) to find out how to use the Wwise setup!
- `wrathbpcoremod` - BPCore UnityModManager Template for Pathfinder: Wrath of the Righteous. BPCore is a community library to make certain aspects of modding easier in Wrath.
- `kmmod` - UnityModManager Template for Pathfinder: Kingmaker

## Requirements

- The target game needs to be installed. The game must've been started once (for a Player.log file).
- Kingmaker and Wrath: Have UnityModManager applied to the game.
- For the sound templates you additionally need a compatible version of Wwise (Audiokinetic) installed. For Wrath, that's any `2019.2` version.

## Usage

- Open command prompt in directory which should contain project directory
- Install .NET SDK with the command:  
  `winget install Microsoft.Dotnet.SDK.8`
- If you haven't done this before (If you're not sure, just execute in anyways), you might need to add the NuGet repository as a source with the following command:  
  `dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org`
- `dotnet new --install Owlcat.Templates`
- `dotnet new <TemplateName> -n <ModID> -D "<Mod Name>"`  (Replace the <value> placeholder with actual values)
- Press y to confirm action (necessary to find Steam Installation Directory)
- I suggest using Visual Studio 2022 to edit the project. Use the following cmd command to install it:
  `winget install Microsoft.VisualStudio.2022.Community --override "--add Microsoft.VisualStudio.Workload.ManagedDesktop --add Microsoft.Net.Component.4.7.2.SDK"`
- Open the resulting project (open the .sln file with Visual Studio) and **Build resulting project once to publicize**
- Restart your IDE to rebuild cache if there are still red underlines

After that you should working setup for a UnityModManager mod which:

- automatically installs the mod when building
- has the correct path and already references a few assemblies (and even pubclizies three of them where I know it's often needed)
- has Hotreloading as an option by default; it's in both Release and Debug builds since I haven't found a way to ship Compiler Conditionals.

For sound mods, they additionally contain:

- A Wwise template in which you can add sounds (and create sound events). The template should automatically include the created Soundbanks in the final output.
- The UnityModManager mod part will automatically load the Soundbanks contained in the mod directory during runtime.
- If the event name matches an answer/cue/dialog guid, the sound event should automatically play when that answer/cue/dialog is displayed.
