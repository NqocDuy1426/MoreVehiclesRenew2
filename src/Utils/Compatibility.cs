using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ICities;

namespace MoreVehiclesRenew2.Utils
{
    internal static class Compatibility
    {
        // Incompatible mods that break vehicle tracking
        private static readonly ulong[] IncompatibleModIds = new ulong[]
        {
            583429740uL // Very old broken Traffic Manager: President Edition
        };

        // Old More Vehicles variants that shouldn't run concurrently
        public static readonly ulong[] LegacyMoreVehiclesIds = new ulong[]
        {
            1764208250uL, // Original More Vehicles
            3684679695uL  // More Vehicles Renewed v1
        };

        public static bool AreAnyIncompatibleModsActive()
        {
            HashSet<ulong> activeMods = new HashSet<ulong>();
            foreach (PluginManager.PluginInfo p in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (p.isEnabled)
                {
                    activeMods.Add(p.publishedFileID.AsUInt64);
                }
            }
            return IncompatibleModIds.Any(id => activeMods.Contains(id));
        }

        public static List<string> GetActiveConflictingVehicleMods(IUserMod selfMod)
        {
            List<string> conflicts = new List<string>();
            foreach (PluginManager.PluginInfo p in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (p.isEnabled && p.userModInstance != selfMod)
                {
                    ulong id = p.publishedFileID.AsUInt64;
                    if (LegacyMoreVehiclesIds.Contains(id))
                    {
                        conflicts.Add(p.name + " (Workshop ID: " + id + ")");
                    }
                    else if (p.name != null && p.name.ToLower().Contains("morevehicles") && !p.name.Contains("Renew 2"))
                    {
                        conflicts.Add(p.name);
                    }
                }
            }
            return conflicts;
        }
    }
}
