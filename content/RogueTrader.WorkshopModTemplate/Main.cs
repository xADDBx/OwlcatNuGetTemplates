using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace RogueTrader.WorkshopModTemplate {
    [EnableReloading]
    static class Main {
        internal static Harmony HarmonyInstance;

        static bool Load(UnityModManager.ModEntry modEntry) {
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