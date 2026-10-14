namespace MoreVehiclesRenew2.Patching
{
    public interface IPatch
    {
        void ApplyPatch(IPatcher patcher);
        void RevertPatch(IPatcher patcher);
    }
}
