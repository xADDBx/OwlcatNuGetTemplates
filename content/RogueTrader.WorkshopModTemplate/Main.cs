using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace RogueTrader.WorkshopModTemplate {
    [EnableReloading]
    static class Main {
        internal static Harmony HarmonyInstance;
        internal static UnityModManager.ModEntry.ModLogger log;

        static bool Load(UnityModManager.ModEntry modEntry) {
            log = modEntry.Logger;
            modEntry.OnUnload = OnUnload;
            modEntry.OnGUI = OnGUI;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) {

        }

        static bool OnUnload(UnityModManager.ModEntry modEntry) {
            HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            return true;
        }
    }
}