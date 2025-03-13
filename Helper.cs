
using System;
using System.IO;
using System.Text.Json;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Unity.Collections;
using Unity.Entities;

namespace NightbaneHardcore;
public static class Helper
{
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

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

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