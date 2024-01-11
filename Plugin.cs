using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ShowAmmoCount;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource logger;

    private void Awake()
    {
        // Plugin startup logic
        logger = Logger;
        logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(typeof(Plugin));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShotgunItem), "SetControlTipsForItem")]
    static bool SetControlTipsForItem(ShotgunItem __instance)
    {
        string[] toolTips = __instance.itemProperties.toolTips;
        if (toolTips.Length <= 2)
        {
            logger.LogError("Shotgun control tips array length is too short to set tips!");
            return false;
        }
        if (__instance.safetyOn)
        {
            toolTips[2] = string.Format("Turn safety off ({0}/0): [Q]", __instance.shellsLoaded);
        }
        else
        {
            toolTips[2] = string.Format("Turn safety on ({0}/2): [Q]", __instance.shellsLoaded);
        }
        HUDManager.Instance.ChangeControlTipMultiple(toolTips, true, __instance.itemProperties);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShotgunItem), "SetSafetyControlTip")]
    static bool SetSafetyControlTip(ShotgunItem __instance)
    {
        string changeTo;
        if (__instance.safetyOn)
        {
            changeTo = string.Format("Turn safety off ({0}/0): [Q]", __instance.shellsLoaded);
        }
        else
        {
            changeTo = string.Format("Turn safety on ({0}/2): [Q]", __instance.shellsLoaded);
        }
        if (__instance.IsOwner)
        {
            HUDManager.Instance.ChangeControlTip(3, changeTo, false);
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShotgunItem), "ShootGun")]
    [HarmonyPatch(typeof(ShotgunItem), "ReloadGunEffectsClientRpc")]
    static void UpdateControlTips(ShotgunItem __instance)
    {
        __instance.SetSafetyControlTip();
    }
}
