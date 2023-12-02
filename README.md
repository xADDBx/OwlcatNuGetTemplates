## Existing Template:

- `rtmod`  - UnityModManager Template for Warhammer 40,000: Rogue Trader
- `rtmodworkshop`  - UnityModManager Template for Warhammer 40,000: Rogue Trader. Adds a publishing task which will directly publish to Steam Workshop. Included is a tool necessary for publishing.
- `wrathmod`  - UnityModManager Template for Pathfinder: Wrath of the Righteous

## Usage

- Open command prompt in directory which should contain project directory
- `dotnet new --install Owlcat.Templates`
- `dotnet new rtmod -n ModID -D "Mod Name"`
- Press y to confirm action (necessary to find Steam Installation Directory)
- **Build resulting project once to publicize**
- Restart your IDE to rebuild cache if there are still red underlines

After that you should working setup for a UnityModManager mod which:

- automatically installs the mod when building
- has the correct path and already references a few assemblies (and even pubclizies three of them where I know it's often needed)
- has Hotreloading as an option by default; it's in both Release and Debug builds since I haven't found a way to ship Compiler Conditionals.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- The target game needs to be installed. The game must've been started once (for a Player.log file).
- Wrath: Have UnityModManager applied to the game.