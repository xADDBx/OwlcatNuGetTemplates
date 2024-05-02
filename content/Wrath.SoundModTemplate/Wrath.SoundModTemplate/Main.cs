using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityModManagerNet;
using Kingmaker.Sound;
using System.IO;
using Kingmaker.Blueprints.JsonSystem;
using System.Xml;
using Kingmaker.Localization;

namespace Wrath.SoundModTemplate;

//-:cnd:noEmit
#if DEBUG
[EnableReloading]
#endif
//+:cnd:noEmit
public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;

    public static bool Load(UnityModManager.ModEntry modEntry) {
        log = modEntry.Logger;
//-:cnd:noEmit
#if DEBUG
        modEntry.OnUnload = OnUnload;
#endif
//+:cnd:noEmit
        modEntry.OnGUI = OnGUI;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
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
                log.Log($"Add soundbank base path {banksPath}");
                AkSoundEngine.AddBasePath(banksPath);

                foreach (var f in Directory.EnumerateFiles(banksPath, "*.bnk")) {
                    var bankName = Path.GetFileName(f);

                    if (bankName == "Init.bnk")
                        throw new InvalidOperationException("Do not include Init.bnk");

                    log.Log($"Load soundbank {f}");

                    var akResult = AkSoundEngine.LoadBank(bankName, out var bankId);

                    if (akResult == AKRESULT.AK_Success) {
                        LoadedBankIds.Add(bankId);
                    } else {
                        log.Error($"Loading soundbank {f} failed with result {akResult}");
                    }
                }
            } catch (Exception e) {
                log.LogException(e);
                UnloadSoundbanks();
            }
        }

        public static void UnloadSoundbanks() {
            foreach (var bankId in LoadedBankIds) {
                try {
                    AkSoundEngine.UnloadBank(bankId, IntPtr.Zero);
                    LoadedBankIds.Remove(bankId);
                } catch (Exception e) {
                    log.LogException(e);
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

                    log.Log($"Adding dialog events from bank {bankName}");

                    foreach (XmlElement e in n["IncludedEvents"]) {
                        var eventId = e.Attributes["Id"].Value;
                        var eventName = e.Attributes["Name"].Value;

                        log.Log($"Add {eventName} -> {eventId}");

                        LocalizationManager.SoundPack.PutString(eventName, eventName);
                    }
                }
            } catch (Exception e) {
                log.LogException(e);
            }
        }
    }
//-:cnd:noEmit
#if DEBUG
    public static bool OnUnload(UnityModManager.ModEntry modEntry) {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif
//+:cnd:noEmit
}
