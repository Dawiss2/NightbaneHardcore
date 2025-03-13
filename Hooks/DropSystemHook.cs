

using Bloodstone.API;
using Bloody.Core;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Collections;
using Unity.Entities;
using NightbaneHardcore;

[HarmonyPatch(typeof(DropInventoryItemSystem), nameof(DropInventoryItemSystem.OnUpdate))]
public static class DropItemThrowSystemHook
{
    [HarmonyPrefix]
    public static void Prefix(DropInventoryItemSystem __instance)
    {
        if (Configuration.LockItemDropping)
        {
            var entities = __instance.__query_1470978867_0.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var entity in entities)
                {
                    if (entity.Has<DropInventoryItemEvent>())
                    {
                        DropInventoryItemEvent dropEvent = entity.Read<DropInventoryItemEvent>();
                        FromCharacter fromCharacter = entity.Read<FromCharacter>();
                        Hardcore.networkIdSystem._NetworkIdLookupMap.TryGetValue(dropEvent.Inventory, out var inventoryEntity);

                        if (!inventoryEntity.Has<Attached>())
                        {
                            Hardcore.HandleDropItemChest(inventoryEntity, fromCharacter, dropEvent.SlotIndex);
                        }
                        else
                        {
                            Hardcore.HandleDropItem(fromCharacter, dropEvent.SlotIndex);
                        }

                        VWorld.Server.EntityManager.DestroyEntity(entity);
                    }
                }
            }
            finally
            {
                entities.Dispose();
            }
        }
    }
}

[HarmonyPatch(typeof(DropItemSystem), nameof(DropItemSystem.OnUpdate))]
public static class DropItemSystemHook
{
    [HarmonyPrefix]
    public static void Prefix(DropItemSystem __instance)
    {
        if (Configuration.LockItemDropping)
        {
            var dropEntireInventoryEvents = __instance.__query_1470978519_2.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var entity in dropEntireInventoryEvents)
                {
                    if (entity.Has<DropEntireInventoryEvent>())
                    {
                        FromCharacter fromCharacter = entity.Read<FromCharacter>();
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, fromCharacter.User.Read<User>(), $"<color=red>You can't drop your inventory in Hardcore Mode!");
                        VWorld.Server.EntityManager.DestroyEntity(entity);
                    }
                }
            }
            finally
            {
                dropEntireInventoryEvents.Dispose();
            }

            var dropItemEvents = __instance.__query_1470978519_0.ToEntityArray(Allocator.Temp);

            try
            {
                foreach (var entity in dropItemEvents)
                {
                    if (entity.Has<DropItemAtSlotEvent>())
                    {
                        DropItemAtSlotEvent dropItemAtSlotEvent = entity.Read<DropItemAtSlotEvent>();
                        FromCharacter fromCharacter = entity.Read<FromCharacter>();
                        Hardcore.HandleDropItem(fromCharacter, dropItemAtSlotEvent.SlotIndex);

                        VWorld.Server.EntityManager.DestroyEntity(entity);
                    }
                }
            }
            finally
            {
                dropItemEvents.Dispose();
            }

            var dropItemEvents2 = __instance.__query_1470978519_1.ToEntityArray(Allocator.Temp);

            try
            {
                foreach (var entity in dropItemEvents2)
                {
                    VWorld.Server.EntityManager.DestroyEntity(entity);
                }
            }
            finally
            {
                dropItemEvents2.Dispose();
            }

        }
    }
}