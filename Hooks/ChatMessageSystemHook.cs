

using System.IO;
using System.Text.Json;
using Bloody.Core;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using NightbaneHardcore;

[HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
public static class ChatMessageSystemHook
{
    public static void Prefix(ChatMessageSystem __instance)
    {
        var entities = __instance.__query_661171423_0.ToEntityArray(Allocator.Temp);
        try
        {
            foreach (var entity in entities)
            {
                ChatMessageEvent chatMessageEvent = entity.Read<ChatMessageEvent>();
                FromCharacter fromCharacter = entity.Read<FromCharacter>();
                User user = fromCharacter.User.Read<User>();

                if (Hardcore.LastWords.ContainsKey(user.PlatformId))
                {
                    Hardcore.LastWords[user.PlatformId] = chatMessageEvent.MessageText.Value;
                    var jsonData = JsonSerializer.Serialize(Hardcore.LastWords, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(Hardcore.LastWordsJson, jsonData);
                }
                else
                {
                    Hardcore.LastWords.Add(user.PlatformId, chatMessageEvent.MessageText.Value);
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}