using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Text;
using BepInEx;
using static ItemSets;

namespace williammetcalf.UsefulArmorStands;

[BepInPlugin(Plugin.ModGUID, "Useful Armor Stands", "1.0.3")]
[BepInProcess("valheim.exe")]
public class ArmorStandPatches : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(Plugin.ModGUID);

    void Awake()
    {
        harmony.PatchAll();
        UnityEngine.Debug.Log("Initializing Useful Armor Stands mod");
    }

    void OnDestroy()
    {
        harmony.UnpatchSelf();
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
                s.m_onHover = () => (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false)) ? Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess") : Localization.instance.Localize(slot.m_switch.m_hoverText + "\n[<color=yellow><b>1-8</b></color>] $piece_itemstand_attach" + ((__instance.GetNrOfAttachedItems() > 0) ? "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_take\n[<color=yellow><b>LShift + $KEY_Use</b></color>] Swap equipped gear with stand" : ""));
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArmorStand.UseItem))]
        static bool ArmorstandQuickchange(ArmorStand __instance, Humanoid user, ItemDrop.ItemData item)
        {
            string itemName = item == null ? "<empty>" : item.m_shared.m_name;
            bool specialKeyActive = CheckKeyDown(KeyCode.LeftShift);
            bool swapGear = specialKeyActive && item == null;

            UnityEngine.Debug.Log("ArmorStand.UseItem invoked with item " + itemName + ". Special Key: " + specialKeyActive);
            if (!swapGear) return true;

            UnityEngine.Debug.Log("Performing gear swap with armor stand");
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