using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace RogueTrader.WorkshopModTemplate;

//-:cnd:noEmit
#if DEBUG
[EnableReloading]
#endif
//+:cnd:noEmit
static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;

    static bool Load(UnityModManager.ModEntry modEntry)
    {
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

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {

    }

//-:cnd:noEmit
#if DEBUG
    static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif
//+:cnd:noEmit
}