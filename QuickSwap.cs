using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using static ItemDrop;
using static ItemDrop.ItemData;

namespace williammetcalf.UsefulArmorStands;

public class QuickSwap
{

    public static void SwapGearWithArmorStand(ArmorStand stand, Humanoid player)
    {
        List<ItemDrop> gearCache = CacheStandGear(stand);
        MovePlayerGearToStand(player, stand);
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
            if (!CheckItemAgainstConfig(itemDrop)) continue;

            LoadFromZDO(i, itemDrop.m_itemData, m_nview.GetZDO());
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
        List<ItemData> playerGear = player.GetInventory().GetEquipedtems().Where(CheckItemAgainstConfig).ToList();
        ZNetView m_nview = (ZNetView)AccessTools.Field(typeof(ArmorStand), "m_nview").GetValue(stand);

        playerGear.ForEach(item =>
        {
            string itemName = item.m_shared.m_name;
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
        gear.ForEach(item =>
        {
            player.GetInventory().AddItem(item.m_itemData);
            if (CheckItemAgainstConfig(item))
            {
                player.EquipItem(item.m_itemData);
            }
        });
    }

    private static int FindEmptySlot(ArmorStand stand, ItemData item)
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

    private static bool CheckItemAgainstConfig(ItemData item) {
        switch (item.m_shared.m_itemType)
        {
            case ItemType.Utility:
                return !ArmorStandConfig.IgnoreSlotConfig.ignoreUtility.Value;
            case ItemType.Shoulder:
                return !ArmorStandConfig.IgnoreSlotConfig.ignoreCape.Value;
            case ItemType.OneHandedWeapon:
            case ItemType.TwoHandedWeapon:
            case ItemType.Bow:
            case ItemType.Tool:
            case ItemType.Torch:
                return !ArmorStandConfig.IgnoreSlotConfig.ignoreWeapon.Value;
            case ItemType.Shield:
                return !ArmorStandConfig.IgnoreSlotConfig.ignoreShield.Value;
            case ItemType.Helmet:
                return !ArmorStandConfig.IgnoreSlotConfig.ignoreHelmet.Value;
            case ItemType.Chest:
                return !ArmorStandConfig.IgnoreSlotConfig.ignoreBody.Value;
            case ItemType.Legs:
                return !ArmorStandConfig.IgnoreSlotConfig.ignorePants.Value;
            default: return true;
        }
    }

    private static bool CheckItemAgainstConfig(ItemDrop itemDrop)
    {
        return CheckItemAgainstConfig(itemDrop.m_itemData);
    }
}