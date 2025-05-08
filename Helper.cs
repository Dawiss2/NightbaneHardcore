
using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NightbaneHardcore;
public static class Helper
{
	public static IEnumerator RepeatingCoroutine(Action action, float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			action?.Invoke();
		}
	}
	public static void SendSystemMessageToClient(EntityManager entityManager, User user, string message)
	{
		FixedString512Bytes fixedMessage = message;
		ServerChatUtils.SendSystemMessageToClient(entityManager, user, ref fixedMessage);
	}
	public static void SendSystemMessageToAllClients(EntityManager entityManager, string message)
	{
		FixedString512Bytes fixedMessage = message;
		ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref fixedMessage);
	}
	public static void TeleportUser(Entity User, Entity Character, float3 position)
	{
		var entity = Core.Server.EntityManager.CreateEntity(
			ComponentType.ReadWrite<FromCharacter>(),
			ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
		);

		FromCharacter fromCharacter = new()
		{
			Character = Character,
			User = User
		};

		Core.Server.EntityManager.SetComponentData(entity, fromCharacter);

		Core.Server.EntityManager.SetComponentData<PlayerTeleportDebugEvent>(
			entity,
			new() { Position = position, Target = PlayerTeleportDebugEvent.TeleportTarget.Self });
	}

	public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.Server.EntityManager.CreateEntityQuery(new ComponentType[] {
					new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)});

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}
	public static T LoadJson<T>(string filePath) where T : new()
	{
		if (!File.Exists(filePath))
		{
			return new T();
		}

		try
		{
			string jsonData = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<T>(jsonData, new JsonSerializerOptions { WriteIndented = true }) ?? new T();
		}
		catch (Exception ex)
		{
			Plugin.logger.LogError($"[NightbaneHardcore] Error loading JSON from {filePath}: {ex.Message}");
			return new T();
		}
	}
}