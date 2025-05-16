
using BepInEx.Configuration;

namespace UsefulArmorStand
{

    public class ArmorStandConfig
    {
        public static ConfigFile Config;

        internal class IgnoreSlotConfig
        {
            internal static ConfigEntry<bool> ignoreUtility;
            internal static ConfigEntry<bool> ignoreCape;
            internal static ConfigEntry<bool> ignoreWeapon;
            internal static ConfigEntry<bool> ignoreShield;
            internal static ConfigEntry<bool> ignoreBody;
            internal static ConfigEntry<bool> ignorePants;
            internal static ConfigEntry<bool> ignoreHelmet;
        }

        internal static void LoadConfig(Main plugin)
        {
            Config = plugin.Config;
            string sectionName = "Ignore Individual Gear Slots";
            IgnoreSlotConfig.ignoreUtility = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignoreUtility)), true, new ConfigDescription("Ignore the character's utility slot when swapping gear with stand."));
            IgnoreSlotConfig.ignoreCape = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignoreCape)), true, new ConfigDescription("Ignore the character's cape slot when swapping gear with stand."));
            IgnoreSlotConfig.ignoreWeapon = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignoreWeapon)), false, new ConfigDescription("Ignore the character's weapon slot when swapping gear with stand."));
            IgnoreSlotConfig.ignoreShield = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignoreShield)), false, new ConfigDescription("Ignore the character's shield slot when swapping gear with stand."));
            IgnoreSlotConfig.ignoreBody = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignoreBody)), false, new ConfigDescription("Ignore the character's body slot when swapping gear with stand."));
            IgnoreSlotConfig.ignorePants = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignorePants)), false, new ConfigDescription("Ignore the character's pants slot when swapping gear with stand."));
            IgnoreSlotConfig.ignoreHelmet = Config.Bind(new ConfigDefinition(sectionName, nameof(IgnoreSlotConfig.ignoreHelmet)), false, new ConfigDescription("Ignore the character's helmet slot when swapping gear with stand."));
        }
    }
}
