using System.Reflection;

namespace MoreVehiclesRenew2.Patching
{
    public interface IPatcher
    {
        void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall, MethodInfo transformCall);
        void RevertPatch(MethodInfo method);
    }
}
