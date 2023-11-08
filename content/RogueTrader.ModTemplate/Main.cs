using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;

namespace RogueTrader.ModTemplate {
#if DEBUG
    [EnableReloading]
#endif
	static class Main {
        	public static bool Enabled;
        	internal static Harmony HarmonyInstance;

		static bool Load(UnityModManager.ModEntry modEntry) {
#if DEBUG
			modEntry.OnUnload = OnUnload;
#endif
			modEntry.OnToggle = OnToggle;
			modEntry.OnGUI = OnGUI;
			HarmonyInstance = new Harmony(modEntry.Info.Id);
			HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            		return true;
        	}

		static void OnGUI() {

		}
#if DEBUG
        	static bool OnUnload(UnityModManager.ModEntry modEntry) {
            		HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            		return true;
        	}
#endif
    	}
}