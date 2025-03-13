
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Bloody.Core;
using UnityEngine;
using System.Collections;
using ProjectM.Physics;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using VampireCommandFramework;
using ProjectM.Scripting;
using ProjectM.Shared;
using System.Collections.Generic;
using Bloody.Core.Models.v1;
using Bloody.Core.Methods;
using Bloody.Core.API.v1;
using Unity.Scenes;
using HarmonyLib;
using System.IO;
using System.Text.Json;
using System;
using Il2CppInterop.Runtime;
using System.Net.Http;
using System.Text;
using Unity.Mathematics;

namespace NightbaneHardcore;
public static class Hardcore
{
    static MonoBehaviour monoBehaviour;
    private static HttpClient httpClient = new HttpClient();
    public static ServerScriptMapper serverScriptMapper;
    public static ServerGameManager serverGameManager;
    public static ServerGameSettingsSystem serverGameSettingsSystem;
    public static DebugEventsSystem debugEventsSystem;
    public static NetworkIdSystem.Singleton networkIdSystem;
    public static string LastWordsJson = "BepInEx/config/NightbaneHardcore/LastWords.json";
    public static string DeathsJson = "BepInEx/config/NightbaneHardcore/DeathsCount.json";
    public static string ItemDropsJson = "BepInEx/config/NightbaneHardcore/ItemDropsConfirms.json";
    public static Dictionary<ulong, string> LastWords = new();
    public static List<ulong> ItemDropConfirms = new();
    public static int Deaths = 0;
    public static bool PvPEnabled;
    public static half DefaultDropRate;
    public static bool HigherDropRateEnabled = false;
    public static bool Initialized = false;
    [HarmonyPatch]
    internal static class InitializationPatch
    {
        [HarmonyPatch(typeof(SceneSystem), nameof(SceneSystem.ShutdownStreamingSupport))]
        [HarmonyPostfix]
        static void ShutdownStreamingSupportPostfix()
        {
            Initialize();
        }
    }
    public static void Initialize()
    {
        if (Initialized) return;

        serverGameSettingsSystem = VWorld.Server.GetExistingSystemManaged<ServerGameSettingsSystem>();
        serverScriptMapper = VWorld.Server.GetExistingSystemManaged<ServerScriptMapper>();
        serverGameManager = serverScriptMapper._ServerGameManager;
        debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
        networkIdSystem = serverScriptMapper.GetSingleton<NetworkIdSystem.Singleton>();

        var serverGameBalanceSettings = serverGameSettingsSystem._Settings.ToStruct();
        if (serverGameBalanceSettings.GameModeType == GameModeType.PvP)
        {
            PvPEnabled = true;
        }
        else
        {
            PvPEnabled = false;
        }
        DefaultDropRate = serverGameBalanceSettings.DropTableModifier_General;

        string folderPath = "BepInEx/config/NightbaneHardcore";
        Directory.CreateDirectory(folderPath);

        LastWords = Helper.LoadJson<Dictionary<ulong, string>>(LastWordsJson);
        Deaths = Helper.LoadJson<int>(DeathsJson);
        ItemDropConfirms = Helper.LoadJson<List<ulong>>(ItemDropsJson);

        CoroutineHandler.StartRepeatingCoroutine(() => { PvPOnBloodmoon(); }, 2f);
        Initialized = true;
    }
    public static void HandleDropItem(FromCharacter fromCharacter, int slotIndex)
    {
        User user = fromCharacter.User.Read<User>();

        if (!ItemDropConfirms.Contains(user.PlatformId))
        {
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"<color=red>WARNING! item drops on ground is disabled in Hardcore Mode! If you want to remove your items, use <color=yellow>.dropenable<color=red> command, it will let you remove items instead of dropping.");
            return;
        }

        DynamicBuffer<InventoryInstanceElement> inventoryInstanceElementBuffer = fromCharacter.Character.ReadBuffer<InventoryInstanceElement>();
        InventoryInstanceElement inventoryInstanceElement = inventoryInstanceElementBuffer[0];

        InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, inventoryInstanceElement.ExternalInventoryEntity._Entity, slotIndex);
    }
    public static void HandleDropItemChest(Entity chestEntity, FromCharacter fromCharacter, int slotIndex)
    {
        User user = fromCharacter.User.Read<User>();

        if (!ItemDropConfirms.Contains(user.PlatformId))
        {
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"<color=red>WARNING! item drops on ground is disabled in Hardcore Mode! If you want to remove your items, use <color=yellow>.dropenable<color=red> command, it will let you remove items instead of dropping.");
            return;
        }

        DynamicBuffer<InventoryInstanceElement> inventoryInstanceElementBuffer = chestEntity.ReadBuffer<InventoryInstanceElement>();
        InventoryInstanceElement inventoryInstanceElement = inventoryInstanceElementBuffer[0];

        InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, inventoryInstanceElement.ExternalInventoryEntity._Entity, slotIndex);
    }
    public static void HardcoreDeath(Entity deathEntity)
    {
        Entity vampireEntity;
        VampireDownedServerEventSystem.TryFindRootOwner(deathEntity, 1, VWorld.Server.EntityManager, out vampireEntity);

        var downBuff = deathEntity.Read<VampireDownedBuff>();

        VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, VWorld.Server.EntityManager, out var killerEntity);

        var playerKiller = killerEntity.Has<PlayerCharacter>();

        Entity UserEntity = vampireEntity.Read<ControlledBy>().Controller;
        User vampireUser = UserEntity.Read<User>();
        Equipment vampireEquipment = vampireEntity.Read<Equipment>();
        float vampireLevel = vampireEquipment.ArmorLevel + vampireEquipment.SpellLevel + vampireEquipment.WeaponLevel;

        if (playerKiller)
        {
            Equipment killerEquipment = killerEntity.Read<Equipment>();
            float killerLevel = killerEquipment.ArmorLevel + killerEquipment.SpellLevel + killerEquipment.WeaponLevel;


            if (Configuration.DiscordDeathlog)
            {
                var WebHookId = Configuration.DiscordWebhookIDPvP;
                var WebHookToken = Configuration.DiscordWebhookTokenPvP;
                const string colorRed = "FF0000";
                var SuccessWebHook = new
                {
                    embeds = new List<object>
                {
                    new
                    {
                        title = "PvP Kill",
                        description=$"Player {killerEntity.Read<PlayerCharacter>().Name.Value} ({Mathf.Floor(killerLevel)}) has slain {vampireEntity.Read<PlayerCharacter>().Name} ({Mathf.Floor(vampireLevel)})!",
                        color= int.Parse(colorRed, System.Globalization.NumberStyles.HexNumber)
                    }
                }
                };

                string EndPoint = string.Format("https://discordapp.com/api/webhooks/{0}/{1}", WebHookId, WebHookToken);

                var content = new StringContent(JsonSerializer.Serialize(SuccessWebHook), Encoding.UTF8, "application/json");

                httpClient.PostAsync(EndPoint, content);
            }

            StatChangeUtility.KillEntity(VWorld.Server.EntityManager, vampireEntity, serverGameManager.ServerTime, StatChangeReason.Default);
        }
        else
        {

            if (Configuration.DiscordDeathlog)
            {
                var WebHookId = Configuration.DiscordWebhookID;
                var WebHookToken = Configuration.DiscordWebhookToken;
                const string colorRed = "FF0000";
                var SuccessWebHook = new
                {
                    embeds = new List<object>
                {
                    new
                    {
                        title = "PvE Death",
                        description=$"Player {vampireEntity.Read<PlayerCharacter>().Name} has died at level {Mathf.Floor(vampireLevel)}!",
                        color= int.Parse(colorRed, System.Globalization.NumberStyles.HexNumber)
                    }
                }
                };

                string EndPoint = string.Format("https://discordapp.com/api/webhooks/{0}/{1}", WebHookId, WebHookToken);

                var content = new StringContent(JsonSerializer.Serialize(SuccessWebHook), Encoding.UTF8, "application/json");
                httpClient.PostAsync(EndPoint, content);
            }

        }
        UserModel userModel = Bloody.Core.GameData.v1.GameData.Users.FromEntity(UserEntity);
        userModel.TeleportTo(new Unity.Mathematics.float3(-2016.0348f, 5f, -2778.0544f));
        if (Configuration.AnnounceDeaths)
        {
            ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"<color=red><size=20>Vampire {vampireUser.CharacterName} Has died at level {Mathf.Floor(vampireLevel)}!");
        }
        StartCoroutine(KickPlayerWithDelay(UserEntity));
        StartCoroutine(DestroyCastleOnDeath(UserEntity));
    }
    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        if (monoBehaviour == null)
        {
            var go = new GameObject("NightbaneHardcore");
            monoBehaviour = go.AddComponent<IgnorePhysicsDebugSystem>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
        return monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
    }

    public static IEnumerator KickPlayerWithDelay(Entity userEntity)
    {
        Deaths++;
        var jsonDataDeaths = JsonSerializer.Serialize(Deaths, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(DeathsJson, jsonDataDeaths);

        User user = userEntity.Read<User>();
        string newName = user.CharacterName + Deaths.ToString();
        UserModel userModel = Bloody.Core.GameData.v1.GameData.Users.FromEntity(userEntity);
        if (LastWords.ContainsKey(user.PlatformId) && Configuration.LastWords)
        {
            ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"<color=red>Last Words: <color=yellow>{LastWords[user.PlatformId]}");
            LastWords.Remove(user.PlatformId);

            var jsonData = JsonSerializer.Serialize(LastWords, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(LastWordsJson, jsonData);
        }
        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"<color=red>You Died! Your character and castle is being removed.");
        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"<color=red>You will be kicked from server in 5 seconds...");


        yield return new WaitForSeconds(5f);
        Entity entity = VWorld.Server.EntityManager.CreateEntity(new ComponentType[3]
        {
            ComponentType.ReadOnly<NetworkEventType>(),
            ComponentType.ReadOnly<SendEventToUser>(),
            ComponentType.ReadOnly<KickEvent>()
        });

        entity.Write(new KickEvent()
        {
            PlatformId = user.PlatformId
        });
        entity.Write(new SendEventToUser()
        {
            UserIndex = user.Index
        });
        entity.Write(new NetworkEventType()
        {
            EventId = NetworkEvents.EventId_KickEvent,
            IsAdminEvent = false,
            IsDebugEvent = false
        });

        user.PlatformId = 0;
        userEntity.Write(user);




        var networkId = VWorld.Server.EntityManager.GetComponentData<NetworkId>(userEntity);
        var renameEvent = new RenameUserDebugEvent
        {
            NewName = newName,
            Target = networkId
        };
        var fromCharacter = new FromCharacter
        {
            User = userEntity,
            Character = userModel.Character.Entity
        };

        debugEventsSystem.RenameUser(fromCharacter, renameEvent);

    }
    public static IEnumerator DestroyCastleOnDeath(Entity userEntity)
    {
        User userOwner = userEntity.Read<User>();

        EntityManager entityManager = VWorld.Server.EntityManager;

        EntityQueryOptions entityQueryOptions = EntityQueryOptions.IncludeAll;

        EntityQueryDesc entityQueryDesc = new()
        {
            All = new ComponentType[] { new(Il2CppType.Of<UserOwner>(), ComponentType.AccessMode.ReadOnly) },
            Options = entityQueryOptions

        };

        EntityQuery query = entityManager.CreateEntityQuery(
            entityQueryDesc
        );

        NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
        List<Entity> entitiesToDestroy = new List<Entity>();

        foreach (var entity in entities)
        {
            UserOwner ownerUser = entityManager.GetComponentData<UserOwner>(entity);
            if (ownerUser.Owner._Entity == Entity.Null) continue;

            User owner = entityManager.GetComponentData<User>(ownerUser.Owner._Entity);
            if (owner.CharacterName.Value == userOwner.CharacterName.Value)
            {
                entitiesToDestroy.Add(entity);
            }
        }

        entities.Dispose();

        // Destroy in batches (avoids lag)
        int batchSize = 20;
        float delayBetweenBatches = 0.5f;

        for (int i = 0; i < entitiesToDestroy.Count; i += batchSize)
        {
            for (int j = 0; j < batchSize && (i + j) < entitiesToDestroy.Count; j++)
            {
                DestroyUtility.Destroy(entityManager, entitiesToDestroy[i + j]);
            }

            yield return new WaitForSeconds(delayBetweenBatches);
        }

    }

    public static IEnumerable<List<T>> Chunk<T>(this List<T> source, int chunkSize)
    {
        for (int i = 0; i < source.Count; i += chunkSize)
        {
            yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
        }
    }
    public static void PvPOnBloodmoon()
    {

        bool IsBloodMoonDay = serverGameManager.DayNightCycle.IsBloodMoonDay();

        var entityGameBalanceSettings = Helper.GetEntitiesByComponentType<ServerGameBalanceSettings>()[0];
        var serverGameBalanceSettings = serverGameSettingsSystem._Settings.ToStruct();


        float timeSinceDayStart = serverGameManager.DayNightCycle.TimeSinceDayStart;

        if (timeSinceDayStart < 361f && IsBloodMoonDay)
        {
            return;
        }

        if (Configuration.HigherDropOnBloodmoon)
        {
            if (IsBloodMoonDay && !HigherDropRateEnabled)
            {
                HigherDropRateEnabled = true;
                serverGameBalanceSettings.DropTableModifier_General = (half)3;
                VWorld.Server.EntityManager.SetComponentData(entityGameBalanceSettings, serverGameBalanceSettings);
                ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"<color=red>The Blood Moon rises, X{Configuration.DropRateOnBloodmoon} Drop Rate is now ACTIVE!");
            }
            else if (!IsBloodMoonDay && HigherDropRateEnabled)
            {
                HigherDropRateEnabled = false;
                serverGameBalanceSettings.DropTableModifier_General = DefaultDropRate;
                VWorld.Server.EntityManager.SetComponentData(entityGameBalanceSettings, serverGameBalanceSettings);
                ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"<color=green>The Blood Moon fades, drop rates are back to normal.");
            }
        }

        if (Configuration.PvPOnBloodmoon)
        {
            if (IsBloodMoonDay && !PvPEnabled)
            {
                PvPEnabled = true;
                serverGameBalanceSettings.GameModeType = GameModeType.PvP;
                VWorld.Server.EntityManager.SetComponentData(entityGameBalanceSettings, serverGameBalanceSettings);
            }
            else if (!IsBloodMoonDay && PvPEnabled)
            {
                PvPEnabled = false;
                serverGameBalanceSettings.GameModeType = GameModeType.PvE;
                VWorld.Server.EntityManager.SetComponentData(entityGameBalanceSettings, serverGameBalanceSettings);
            }
        }
    }

    [Command("dropenable")]
    public static void DropEnableCommand(ChatCommandContext ctx)
    {
        if (ItemDropConfirms.Contains(ctx.Event.User.PlatformId))
        {
            ctx.Reply($"<color=red>Item removing is already enabled! Use .dropdisable command to disable it.");
            return;
        }
        ItemDropConfirms.Add(ctx.Event.User.PlatformId);

        var jsonData = JsonSerializer.Serialize(ItemDropConfirms, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ItemDropsJson, jsonData);

        ctx.Reply($"<color=green>Item removing is now enabled! Use .dropdisable command to disable it.");
    }

    [Command("dropdisable")]
    public static void DropDisableCommand(ChatCommandContext ctx)
    {
        if (!ItemDropConfirms.Contains(ctx.Event.User.PlatformId))
        {
            ctx.Reply($"<color=red>Item removing is already disabled! Use .dropenable command to enable it.");
            return;
        }
        ItemDropConfirms.Remove(ctx.Event.User.PlatformId);

        var jsonData = JsonSerializer.Serialize(ItemDropConfirms, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ItemDropsJson, jsonData);

        ctx.Reply($"<color=green>Item removing is now disabled! Use .dropenable command to enable it again.");
    }
}