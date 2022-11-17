using HarmonyLib;

namespace TinyResort;

// loadInv is required because clients only run this when joining a server.

[HarmonyPatch(typeof(NetworkMapSharer), "OnStartClient")]
[HarmonyPriority(1)]
internal class OnStartClient {

    [HarmonyPostfix]
    public static void Postfix() {
        TRTools.LeavingMainMenu = true;
        TRData.trueLoadEvent?.Invoke();
        TRData.postLoadEvent?.Invoke();
        TRData.injectDataEvent?.Invoke();
        TRTools._InMainMenu = false;
    }
}
