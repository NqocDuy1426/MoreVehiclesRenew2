using System;
using System.Reflection;

namespace MoreVehiclesRenew2.Patching
{
    public abstract class PatchBase : IPatch
    {
        private MethodInfo method;

        public void ApplyPatch(IPatcher patcher)
        {
            if (patcher == null)
            {
                throw new ArgumentNullException("patcher");
            }
            if (method != null)
            {
                return;
            }
            method = GetMethod();
            if (method != null)
            {
                Type type = GetType();
                MethodInfo prefix = type.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo postfix = type.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo transform = type.GetMethod("Transform", BindingFlags.Static | BindingFlags.NonPublic);
                if (prefix == null && postfix == null && transform == null)
                {
                    throw new InvalidOperationException("At least one of the methods must be defined: 'Prefix', 'Postfix', 'Transform'.");
                }
                patcher.ApplyPatch(method, prefix, postfix, transform);
            }
        }

        public void RevertPatch(IPatcher patcher)
        {
            if (patcher == null)
            {
                throw new ArgumentNullException("patcher");
            }
            MethodInfo targetMethod = method ?? GetMethod();
            if (targetMethod != null)
            {
                patcher.RevertPatch(targetMethod);
                method = null;
            }
        }

        protected abstract MethodInfo GetMethod();
    }
}
