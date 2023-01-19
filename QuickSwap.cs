using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using static ItemDrop;

namespace williammetcalf.UsefulArmorStands;

public class QuickSwap
{

    public static void SwapGearWithArmorStand(ArmorStand stand, Humanoid player)
    {
        // 1. cache armor stand gear
        List<ItemDrop> gearCache = CacheStandGear(stand);
        Debug.Log("Caching " + gearCache.Count + " items from armor stand: " + gearCache.Join(i => i.m_itemData.m_shared.m_name, ", "));
        // 2. move equipment from player to stand
        MovePlayerGearToStand(player, stand);
        // 3. equip cached gear from stand to player
        EquipGearToPlayer(gearCache, player);
    }

    private static List<ItemDrop> CacheStandGear(ArmorStand stand)
    {
        List<ItemDrop> gearCache = new();
        ZNetView m_nview = (ZNetView)AccessTools.Field(typeof(ArmorStand), "m_nview").GetValue(stand);

        for (int i = 0; i < stand.m_slots.Count; i++)
        {
            if (!stand.HaveAttachment(i)) continue;
            string itemstring = m_nview.GetZDO().GetString(i + "_item");
            ItemDrop itemDrop = ObjectDB.instance.GetItemPrefab(itemstring).GetComponent<ItemDrop>();
            ItemDrop.LoadFromZDO(i, itemDrop.m_itemData, m_nview.GetZDO());
            gearCache.Add(itemDrop);

            stand.DestroyAttachment(i);
            m_nview.GetZDO().Set(i.ToString() + "_item", "");
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetVisualItem", i, "", 0);
            AccessTools.Method(typeof(ArmorStand), "UpdateSupports").Invoke(stand, new object[] { });
            AccessTools.Field(typeof(ArmorStand), "m_cloths").SetValue(stand, stand.GetComponentsInChildren<Cloth>());
        }

        return gearCache;
    }

    private static void MovePlayerGearToStand(Humanoid player, ArmorStand stand) {
        List<ItemData> playerGear = player.GetInventory().GetEquipedtems();
        Debug.Log("Found " + playerGear.Count + " equiped items on player: " + playerGear.Join(i => i.m_shared.m_name, ", "));
        ZNetView m_nview = (ZNetView)AccessTools.Field(typeof(ArmorStand), "m_nview").GetValue(stand);

        playerGear.ForEach(item =>
        {
            string itemName = item.m_shared.m_name;
            Debug.Log("About to attempt to put item " + itemName + " into armor stand");
            int slot = FindEmptySlot(stand, item);
            if (slot < 0) return;

            bool canAttach = (bool)AccessTools.Method(typeof(ArmorStand), "CanAttach").Invoke(stand, new object[] { stand.m_slots[slot], item });
            if (!canAttach || stand.HaveAttachment(slot)) throw new TargetException("Unexpected scenario!");
            player.UnequipItem(item);
            player.GetInventory().RemoveOneItem(item);
            ItemData clonedItem = item.Clone();
            m_nview.GetZDO().Set(slot + "_item", item.m_dropPrefab.name);
            SaveToZDO(slot, clonedItem, m_nview.GetZDO());
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetVisualItem", slot, clonedItem.m_dropPrefab.name, clonedItem.m_variant);
            AccessTools.Field(typeof(ArmorStand), "m_queuedItem").SetValue(stand, null);
        });
    }

    private static void EquipGearToPlayer(List<ItemDrop> gear, Humanoid player) {
        Debug.Log("About to equip " + gear.Count + " cached items to player: " + gear.Join(i => i.m_itemData.m_shared.m_name, ", "));
        gear.ForEach(item =>
        {
            player.GetInventory().AddItem(item.m_itemData);
            player.EquipItem(item.m_itemData);
        });
    }



    public static void MovePlayerEqToStand(Switch caller, ArmorStand stand, Humanoid player)
    {
        List<ItemDrop.ItemData> playerItems = player.GetInventory().GetEquipedtems();
        playerItems.ForEach(item =>
        {
            int slot = FindEmptySlot(stand, item);
            Switch s = stand.m_slots[slot].m_switch;
            string original_m_name = s.m_name;
            s.m_name += Plugin.ModGUID;
            stand.UseItem(s, player, item);
            s.m_name = original_m_name;
        });
    }

    public static int FindEmptySlot(ArmorStand stand, ItemDrop.ItemData item)
    {
        int slot = -1;
        for (int i = 0; i < stand.m_slots.Count; i++)
        {
            object[] p = new object[] { stand.m_slots[i], item };
            bool r = (bool)AccessTools.Method(typeof(ArmorStand), "CanAttach").Invoke(stand, p);
            if (r)
            {
                slot = i;
                break;
            }
        }
        return slot;
    }
}