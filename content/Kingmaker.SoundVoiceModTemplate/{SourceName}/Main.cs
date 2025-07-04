using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Visual.Sound;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace {SourceName};

public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;

    public static bool Load(UnityModManager.ModEntry modEntry) {
        Log = modEntry.Logger;
        modEntry.OnGUI = OnGUI;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        try {
            HarmonyInstance = new Harmony(modEntry.Info.Id);
        } catch {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            throw;
        }
        CopySoundbanks();
        return true;
    }

    public static void OnGUI(UnityModManager.ModEntry modEntry) {

    }

    // Since the version of Wwise included in KM cannot load soundbanks outside the vanilla folder, copy the mod's bank/s to that folder.
    public static void CopySoundbanks()
    {
        var banksPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var vanillaPath = Path.GetFullPath(Path.Combine(banksPath, @"..\..\Kingmaker_Data\StreamingAssets\Audio\GeneratedSoundBanks\Windows\"));

        try
        {
            log.Log($"Attempting to copy mod soundbank/s to the vanilla soundbanks folder \"{vanillaPath}\":");

            foreach (var f in Directory.EnumerateFiles(banksPath, "*.bnk"))
            {
                var bankName = Path.GetFileName(f);

                string[] vanillaBNKs = ["Init.bnk", "Amiri_GVR_ENG.bnk", "Cephal_GVR_ENG.bnk", "Ekundayo_GVR_ENG.bnk", "Harrim_GVR_ENG.bnk", "Jaethal_GVR_ENG.bnk",
                    "Jubilost_GVR_ENG.bnk", "Kalikke_GVR_ENG.bnk", "Kanerah_GVR_ENG.bnk", "Linzi_GVR_ENG.bnk", "NokNok_GVR_ENG.bnk", "NPC_Kressle_GVR_ENG.bnk",
                    "NPC_StagLord_GVR_ENG.bnk", "Octavia_GVR_ENG.bnk", "PC_Female_Aggressive_GVR_ENG.bnk", "PC_Female_Brave_GVR_ENG.bnk", "PC_Female_Carefree_GVR_ENG.bnk",
                    "PC_Female_Confident_GVR_ENG.bnk", "PC_Female_Cunning_GVR_ENG.bnk", "PC_Female_Madman_GVR_ENG.bnk", "PC_Female_Pious_GVR_ENG.bnk", "PC_Female_Pragmatic_GVR_ENG.bnk",
                    "PC_Female_Reserved_GVR_ENG.bnk", "PC_Male_Aggressive_GVR_ENG.bnk", "PC_Male_Brave_GVR_ENG.bnk", "PC_Male_Confident_GVR_ENG.bnk", "PC_Male_Grumpy_GVR_ENG.bnk",
                    "PC_Male_Madman_GVR_ENG.bnk", "PC_Male_Pious_GVR_ENG.bnk", "PC_Male_Pragmatic_GVR_ENG.bnk", "PC_Male_Reserved_GVR_ENG.bnk", "PC_Male_Wise_GVR_ENG.bnk",
                    "Regongar_GVR_ENG.bnk", "Tartuccio_GVR_ENG.bnk", "Tartuk_GVR_ENG.bnk", "Tristian_GVR_ENG.bnk", "Valerie_GVR_ENG.bnk", "Varn_GVR_ENG.bnk"
                ];

                // Safety check to prevent overwriting vanilla banks (non-exhaustive list).
                if (vanillaBNKs.Contains(bankName))
                {
                    log.Log($"Soundbank \"{bankName}\" uses a vanilla bank name! Skipping.");
                    continue;
                }

                var bankSource = Path.Combine(banksPath, bankName);
                var bankDest = Path.Combine(vanillaPath, bankName);

                if (File.Exists(bankDest))
                {
                    FileInfo newBNKWrite = new(bankSource);
                    FileInfo oldBNKWrite = new(bankDest);

                    // Compare the mod folder bank to an existing bank to see if it is newer to account for mod updates.
                    if (newBNKWrite.LastWriteTimeUtc > oldBNKWrite.LastWriteTimeUtc)
                    {
                        log.Log($"Soundbank \"{bankName}\" already exists. Mod folder version is newer ({newBNKWrite.LastWriteTime:HH:mm, dd MMM, yyyy} vs {oldBNKWrite.LastWriteTime:HH:mm, dd MMM, yyyy}), overwriting.");
                        File.Copy(bankSource, bankDest, true);
                    }
                    else
                    {
                        log.Log($"Soundbank \"{bankName}\" already exists. Mod folder version is the same or older ({newBNKWrite.LastWriteTime:HH:mm, dd MMM, yyyy} vs {oldBNKWrite.LastWriteTime:HH:mm, dd MMM, yyyy}), skipping.");
                    }
                }
                else
                {
                    File.Copy(bankSource, bankDest);
                    log.Log($"Copying soundbank \"{bankName}\".");
                }
            }
        }
        catch (Exception e)
        {
            log.Log($"ERROR: Caught an exception trying to copy soundbank/s to the game folder:\n{e}");
        }
    }

    [HarmonyPatch]
    public static class Soundbanks
    {
        public static Dictionary<string, LocalizedString> TextToLocalizedString = [];

        // Helper function to add a string to the localization pack.
        public static LocalizedString CreateString(string key, string value)
        {
            if (TextToLocalizedString.TryGetValue(value, out LocalizedString localized))
                return localized;

            var strings = LocalizationManager.CurrentPack.Strings;

//-:cnd:noEmit
#if DEBUG
            if (strings.TryGetValue(key, out string oldValue) && value != oldValue)
            {
                log.Log($"Info: found localized string with duplicate key: \"{key}\", but different value: \"{oldValue}\".");
            }
#endif
//+:cnd:noEmit

            strings[key] = value;
            localized = new LocalizedString() { m_Key = key };
            TextToLocalizedString[value] = localized;

            return localized;
        }
        
        // Create the UnitAsksList blueprint.
        [HarmonyPatch(typeof(LibraryScriptableObject), nameof(LibraryScriptableObject.LoadDictionary))]
        [HarmonyPostfix]
        static void AddAsksListBlueprint()
        {
            var blueprint = ScriptableObject.CreateInstance<BlueprintUnitAsksList>();
            // Every mod requires its own unique GUID. Autogenerated on template creation.
            blueprint.AssetGuid = Guid.Parse("{ModGUID}").ToString("N");
            blueprint.name = "{SourceName}_Barks";
            // Add the localized string (key + string) for the soundset that will be shown in the character creator UI. 
            blueprint.DisplayName = CreateString("{SourceName}", "{Description}");

            UnitAsksComponent asksComponent = ScriptableObject.CreateInstance<UnitAsksComponent>();
            // Change this to match the name of your primary soundbank if different.
            asksComponent.SoundBanks = ["{SourceName}_GVR_ENG"];
            // This is the name of your character creator preview sounds Event. Make sure
            // that it matches the name of the Event in your Wwise project.
            asksComponent.PreviewSound = "{SourceName}_Test";

            asksComponent.Aggro = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_CombatStart_01",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CombatStart_02",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CombatStart_03",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                InterruptOthers = true,
                Chance = 1.0f
            };

            asksComponent.Pain = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Pain",
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Cooldown = 2.0f,
                Chance = 1.0f
            };

            asksComponent.Fatigue = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Fatigue",
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Cooldown = 60.0f,
                Chance = 1.0f
            };

            asksComponent.Death = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Death",
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                InterruptOthers = true,
                Chance = 1.0f
            };

            asksComponent.Unconscious = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Unconscious",
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                InterruptOthers = true,
                Chance = 1.0f
            };

            asksComponent.LowHealth = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_LowHealth_01",
                            ExcludeTime = 1,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_LowHealth_02",
                            ExcludeTime = 1,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Cooldown = 10.0f,
                Chance = 1.0f
            };

            asksComponent.CriticalHit = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_CharCrit_01",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CharCrit_02",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CharCrit_03",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 0.7f
            };

            asksComponent.Order = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_AttackOrder_01",
                            ExcludeTime = 3,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_AttackOrder_02",
                            ExcludeTime = 3,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_AttackOrder_03",
                            ExcludeTime = 3,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_AttackOrder_04",
                            ExcludeTime = 3,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.OrderMove = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Move_01",
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Move_02",
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Move_03",
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Move_04",
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Move_05",
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Move_06",
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Move_07",
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Cooldown = 10.0f,
                Chance = 0.1f
            };

            asksComponent.Selected = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Select_01",
                            RandomWeight = 1.0f,
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Select_02",
                            RandomWeight = 1.0f,
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Select_03",
                            RandomWeight = 1.0f,
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Select_04",
                            RandomWeight = 1.0f,
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Select_05",
                            RandomWeight = 1.0f,
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Select_06",
                            RandomWeight = 1.0f,
                            ExcludeTime = 4,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_SelectJoke",
                            RandomWeight = 0.1f,
                            ExcludeTime = 30,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.RefuseEquip = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_CantEquip_01",
                            RandomWeight = 1.0f,
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CantEquip_02",
                            RandomWeight = 1.0f,
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.RefuseCast = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_CantCast",
                            ExcludeTime = 1,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.CheckSuccess = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_CheckSuccess_01",
                            RandomWeight = 1.0f,
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CheckSuccess_02",
                            RandomWeight = 1.0f,
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.CheckFail = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_CheckFail_01",
                            RandomWeight = 1.0f,
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_CheckFail_02",
                            RandomWeight = 1.0f,
                            ExcludeTime = 2,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.RefuseUnequip = new()
            {
                Entries = [],
                Chance = 1.0f
            };

            asksComponent.Discovery = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_Discovery_01",
                            ExcludeTime = 1,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        },
                        new()
                        {
                            AkEvent = "{SourceName}_Discovery_02",
                            ExcludeTime = 1,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.Stealth = new()
            {
                Entries =
                    [
                        new()
                        {
                            AkEvent = "{SourceName}_StealthMode",
                            RandomWeight = 1.0f,
                            ExcludeTime = 1,
                            RequiredFlags = [],
                            ExcludedFlags = []
                        }
                    ],
                Chance = 1.0f
            };

            asksComponent.StormRain = new()
            {
                Entries = [],
                Chance = 1.0f
            };

            asksComponent.StormSnow = new()
            {
                Entries = [],
                Chance = 1.0f
            };

            asksComponent.AnimationBarks =
            [
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.AttackShort,
                        Entries =
                        [
                            new()
                            {
                                AkEvent = "{SourceName}_AttackShort",
                                RequiredFlags = [],
                                ExcludedFlags = []
                            }
                        ],
                        Chance = 0.7f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.CoupDeGrace,
                        Entries =
                        [
                            new()
                            {
                                Text = null,
                                AkEvent = "{SourceName}_CoupDeGrace",
                                RequiredFlags = [],
                                ExcludedFlags = []
                            }
                        ],
                        InterruptOthers = true,
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.Cast,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.CastDirect,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.CastLong,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.CastShort,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.CastTouch,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.CastYourself,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.Omnicast,
                        Entries = [],
                        Chance = 1.0f
                    },
                    new()
                    {
                        AnimationEvent = MappedAnimationEventType.Precast,
                        Entries = [],
                        Chance = 1.0f
                    }
            ];

            blueprint.ComponentsArray = [asksComponent];

            // Add the custom blueprint to the Asks list and ResourcesLibrary.
            try
            {
                // Change this to Game.Instance.BlueprintRoot.CharGen.FemaleVoices if you are creating a female soundset.
                ref BlueprintUnitAsksList[] voices = ref Game.Instance.BlueprintRoot.CharGen.MaleVoices;

                // Check it doesn't already exist.
                if (ResourcesLibrary.LibraryObject.BlueprintsByAssetId.ContainsKey(blueprint.AssetGuid))
                {
                    log.Log($"BlueprintUnitAsksList \"{blueprint.name}\" ({blueprint.AssetGuid}) already exists in ResourcesLibrary, skipping.");
                    return;
                }
                else
                {
                    int length = voices.Length;

                    log.Log($"Mod BlueprintUnitAsksList \"{blueprint.name}\" not found in ResourcesLibrary, adding.");

                    Array.Resize<BlueprintUnitAsksList>(ref voices, length + 1);

                    voices[length] = blueprint;

                    ResourcesLibrary.LibraryObject.BlueprintsByAssetId[blueprint.AssetGuid] = blueprint;
                    ResourcesLibrary.LibraryObject.GetAllBlueprints().Add(blueprint);
                }
            }
            catch (Exception e)
            {
                log.Log($"ERROR: Trying to add the blueprint to the BlueprintUnitAsksList array resulted in the following exception:\n{e}");
            }
        }
    }
}