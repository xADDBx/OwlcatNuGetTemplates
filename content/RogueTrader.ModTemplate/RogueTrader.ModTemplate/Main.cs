using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace RogueTrader.ModTemplate;

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

    //-:cnd:noEmit
#if DEBUG
    public static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif
    //+:cnd:noEmit
}
