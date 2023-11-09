## Existing Template:

- `rtmod`  - UnityModManager Template for Warhammer 40,000: Rogue Trader
- `wrathmod`  - UnityModManager Template for Pathfinder: Wrath of the Righteous

## Usage

- Open command prompt in directory which should contain project directory
- `dotnet new --install Owlcat.Templates`
- `dotnet new rtmod -n ModID -D "Mod Name"`
- **Build resulting project twice (there will be errors when first building, this is normal**
- Restart your IDE to rebuild cache if there are still red underlines

After that you should working setup for a UnityModManager mod which:

- automatically installs the mod when building
- has the correct path and already references a few assemblies (and even pubclizies three of them where I know it's often needed)
- has Hotreloading as an option by default; it's in both Release and Debug builds since I haven't found a way to ship Compiler Conditionals.