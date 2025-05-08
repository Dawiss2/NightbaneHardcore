
using HarmonyLib;
using NightbaneHardcore;
using ProjectM;
using Unity.Collections;

[HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
public static class VampireDownedHook
{
    public static void Prefix(VampireDownedServerEventSystem __instance)
    {
        var downedEvents = __instance.__query_1174204813_0.ToEntityArray(Allocator.Temp);
        try
        {
            foreach (var entity in downedEvents)
            {
                Hardcore.HardcoreDeath(entity);
            }
        }
        finally
        {
            downedEvents.Dispose();
        }
    }
}