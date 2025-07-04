using HarmonyLib;
using System.Text;
using System.Reflection;
using UnityModManagerNet;
using Kingmaker.Sound;
using Kingmaker.Blueprints.JsonSystem;
using System.Xml;
using Kingmaker.Localization;

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
        return true;
    }

    public static void OnGUI(UnityModManager.ModEntry modEntry) {

    }

    [HarmonyPatch]
    public static class Soundbanks {
        public static readonly HashSet<uint> LoadedBankIds = [];

        [HarmonyPatch(typeof(AkAudioService), nameof(AkAudioService.Initialize))]
        [HarmonyPostfix]
        public static void LoadSoundbanks() {
            var banksPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try {
                Log.Log($"Add soundbank base path {banksPath}");
                AkSoundEngine.AddBasePath(banksPath);

                foreach (var f in Directory.EnumerateFiles(banksPath, "*.bnk")) {
                    var bankName = Path.GetFileName(f);

                    if (bankName == "Init.bnk")
                        throw new InvalidOperationException("Do not include Init.bnk");

                    Log.Log($"Load soundbank {f}");

                    var akResult = AkSoundEngine.LoadBank(bankName, out var bankId);

                    if (akResult == AKRESULT.AK_Success) {
                        LoadedBankIds.Add(bankId);
                    } else {
                        Log.Error($"Loading soundbank {f} failed with result {akResult}");
                    }
                }
            } catch (Exception e) {
                Log.LogException(e);
                UnloadSoundbanks();
            }
        }

        public static void UnloadSoundbanks() {
            foreach (var bankId in LoadedBankIds) {
                try {
                    AkSoundEngine.UnloadBank(bankId, IntPtr.Zero);
                    LoadedBankIds.Remove(bankId);
                } catch (Exception e) {
                    Log.LogException(e);
                }
            }
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void AddDialogue() {
            var xmlDoc = new XmlDocument();
            try {
                xmlDoc.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SoundbanksInfo.xml"));

                foreach (XmlElement n in xmlDoc["SoundBanksInfo"]["SoundBanks"]) {
                    var bankName = n["ShortName"].InnerText;

                    if (bankName == "Init") continue;

                    Log.Log($"Adding dialog events from bank {bankName}");

                    foreach (XmlElement e in n["IncludedEvents"]) {
                        var eventId = e.Attributes["Id"].Value;
                        var eventName = e.Attributes["Name"].Value;

                        Log.Log($"Add {eventName} -> {eventId}");

                        LocalizationManager.SoundPack.PutString(eventName, eventName);
                    }
                }
            } catch (Exception e) {
                Log.LogException(e);
            }
        }
    }
}
