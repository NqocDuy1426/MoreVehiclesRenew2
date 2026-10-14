using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patching
{
    public sealed class MethodPatcher
    {
        private sealed class Patcher : IPatcher
        {
            private readonly Harmony harmony;

            public Patcher(Harmony harmony)
            {
                this.harmony = harmony;
            }

            public void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall, MethodInfo transformCall)
            {
                if (method == null)
                {
                    throw new ArgumentNullException("method");
                }
                if (prefixCall == null && postfixCall == null && transformCall == null)
                {
                    throw new ArgumentException("All call methods (prefixCall, postfixCall, transformCall) cannot be null at the same time.");
                }
                HarmonyMethod prefix = prefixCall == null ? null : new HarmonyMethod(prefixCall);
                HarmonyMethod postfix = postfixCall == null ? null : new HarmonyMethod(postfixCall);
                HarmonyMethod transpiler = transformCall == null ? null : new HarmonyMethod(transformCall);
                harmony.Patch(method, prefix, postfix, transpiler, null);
            }

            public void RevertPatch(MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException("method");
                }
                harmony.Unpatch(method, HarmonyPatchType.All, harmony.Id);
            }
        }

        private readonly Patcher patcher;
        private readonly string id;
        private readonly IEnumerable<IPatch> patches;

        public MethodPatcher(string id, IEnumerable<IPatch> patches)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The ID cannot be null or an empty string", "id");
            }
            if (patches == null)
            {
                throw new ArgumentException("Patches cannot be null", "patches");
            }
            this.id = id;
            this.patches = patches;
            Harmony harmony = new Harmony(id);
            patcher = new Patcher(harmony);
        }

        public HashSet<IPatch> Apply()
        {
            Revert();
            HashSet<IPatch> hashSet = new HashSet<IPatch>();
            foreach (IPatch patch in patches)
            {
                try
                {
                    patch.ApplyPatch(patcher);
                    hashSet.Add(patch);
                }
                catch (Exception ex)
                {
                    Log.Error("Harmony method patch failed for " + patch.GetType().Name + ": " + ex.Message);
                }
            }
            Log.Info(hashSet.Count + " Harmony method patches with ID '" + id + "' successfully applied.");
            return hashSet;
        }

        public void Revert()
        {
            foreach (IPatch patch in patches)
            {
                try
                {
                    patch.RevertPatch(patcher);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed reverting patch " + patch.GetType().Name + ": " + ex.Message);
                }
            }
        }
    }
}
