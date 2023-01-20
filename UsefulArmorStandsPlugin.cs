using HarmonyLib;
using UnityEngine;
using BepInEx;

namespace williammetcalf.UsefulArmorStands;

[BepInPlugin(ModGUID, "Useful Armor Stands", "1.0.4")]
[BepInProcess("valheim.exe")]
public class UsefulArmorStandsPlugin : BaseUnityPlugin
{
    private const string ModName = "UsefulArmorStand";
    internal const string ModVersion = "1.0.0";
    private const string Author = "williammetcalf";
    public const string ModGUID = Author + "." + ModName;
    private readonly Harmony harmony = new Harmony(ModGUID);

    void Awake()
    {
        Debug.Log("Initializing Useful Armor Stands mod");
        ArmorStandConfig.LoadConfig(this);
        
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(ArmorStand))]
    static class ArmorStandPatch
    {
        [HarmonyPatch(typeof(ArmorStand)), HarmonyPatch("Awake")]
        static void Postfix(ArmorStand __instance)
        {
            for (int i = 0; i < __instance.m_slots.Count; i++)
            {
                ArmorStand.ArmorStandSlot slot = __instance.m_slots[i];
                Switch s = slot.m_switch;
                s.m_onHover = () => (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false)) ? Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess") : Localization.instance.Localize(slot.m_switch.m_hoverText + "\n[<color=yellow><b>1-8</b></color>] $piece_itemstand_attach" + ((__instance.GetNrOfAttachedItems() > 0) ? "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_take\n[<color=yellow><b>LShift + $KEY_Use</b></color>] Swap equipped gear with stand" : "\n[<color=yellow><b>LShift + $KEY_Use</b></color>] Store equipped gear on stand"));
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArmorStand.UseItem))]
        static bool ArmorstandQuickchange(ArmorStand __instance, Humanoid user, ItemDrop.ItemData item)
        {
            bool specialKeyActive = CheckKeyDown(KeyCode.LeftShift);
            bool swapGear = specialKeyActive && item == null;

            if (!swapGear) return true;

            Debug.Log("Performing gear swap with armor stand");
            QuickSwap.SwapGearWithArmorStand(__instance, user);
            return false;
        }

        private static bool CheckKeyDown(KeyCode value)
        {
            try
            {
                return Input.GetKey(value);
            }
            catch
            {
                return false;
            }
        }
    }
}