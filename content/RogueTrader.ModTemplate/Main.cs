using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace RogueTrader.ModTemplate {
#if DEBUG
    [EnableReloading]
#endif
    static class Main {
        internal static Harmony HarmonyInstance;

        static bool Load(UnityModManager.ModEntry modEntry) {
#if DEBUG
            modEntry.OnUnload = OnUnload;
#endif
            modEntry.OnGUI = OnGUI;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) {

        }

#if DEBUG
        static bool OnUnload(UnityModManager.ModEntry modEntry) {
            HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            return true;
        }
#endif
    }
}