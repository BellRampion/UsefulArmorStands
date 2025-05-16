using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UsefulArmorStand
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("valheim.exe")]
    public class Main : BaseUnityPlugin
    {
        const string pluginGUID = "com.example.GUID";
        const string pluginName = "bellrampion.UsefulArmorStands";
        const string pluginVersion = "1.0.0";

        private readonly Harmony HarmonyInstance = new Harmony(pluginGUID);

        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        public void Awake()
        {
            Main.logger.LogInfo("Thank you for using my mod!");
            ArmorStandConfig.LoadConfig(this);
            Assembly assembly = Assembly.GetExecutingAssembly();

            HarmonyInstance.PatchAll(assembly);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ArmorStand), "UseItem")]
        public static class ArmorStandQuickchangePatch
        {
            [HarmonyPatch(typeof(ArmorStand)), HarmonyPatch("Awake")]
            static void Postfix(ArmorStand __instance)
            {
                for (int i = 0; i < __instance.m_slots.Count; i++)
                {
                    ArmorStand.ArmorStandSlot slot = __instance.m_slots[i];
                    Switch s = slot.m_switch;
                    s.m_onHover = () => "Armor Stand" + "\n[<color=yellow><b>1-8</b></color>] Place item on stand" + ((__instance.GetNrOfAttachedItems() > 0) ? "\n[<color=yellow><b>E</b></color>] Take item\n[<color=yellow><b>LShift + E</b></color>] Swap equipped gear with stand" : "\n[<color=yellow><b>LShift + E</b></color>] Store equipped gear on stand");
                }
            }

            public static bool Prefix(ArmorStand __instance, Switch caller, Humanoid user, ItemDrop.ItemData item)
            {
                bool specialKeyActive = CheckKeyDown(KeyCode.LeftShift);
                bool swapGear = specialKeyActive && item == null;
                Debug.Log("Trying to swap gear with armor stand.");
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
}