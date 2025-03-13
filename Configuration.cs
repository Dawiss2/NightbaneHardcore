using BepInEx.Configuration;
using Epic.OnlineServices;

internal class Configuration
{
    internal static bool PvPOnBloodmoon { get; private set; }
    internal static bool HigherDropOnBloodmoon { get; private set; }
    internal static int DropRateOnBloodmoon { get; private set; }
    internal static bool AnnounceDeaths { get; private set; }
    internal static bool LockItemDropping { get; private set; }
    internal static bool LastWords { get; private set; }
    internal static bool DiscordDeathlog { get; private set; }
    internal static string DiscordWebhookID { get; private set; }
    internal static string DiscordWebhookToken { get; private set; }
    internal static string DiscordWebhookIDPvP { get; private set; }
    internal static string DiscordWebhookTokenPvP { get; private set; }

    internal static void InitConfig(ConfigFile config)
    {
        PvPOnBloodmoon = config.Bind("Bloodmoon", "PvPOnBloodmoon", true, "Enable PvP during Bloodmoon.").Value;
        HigherDropOnBloodmoon = config.Bind("Bloodmoon", "HigherDropOnBloodmoon", true, "Enable higher item drop rates during Bloodmoon.").Value;
        DropRateOnBloodmoon = config.Bind("Bloodmoon", "DropRateOnBloodmoon", 3, "Drop rate during Bloodmoon. (if enabled)").Value;

        AnnounceDeaths = config.Bind("General", "AnnounceDeaths", true, "Announce player deaths in chat.").Value;
        LockItemDropping = config.Bind("General", "LockItemDropping", true, "Prevent players from dropping items.").Value;
        LastWords = config.Bind("General", "LastWords", true, "Add last words of player that just died to the chat announcement.").Value;


        DiscordDeathlog = config.Bind("Discord", "DiscordDeathlog", false, "Log deaths to a Discord webhook.").Value;

        DiscordWebhookID = config.Bind("Discord", "DiscordWebhookID", "", "Discord webhook ID for PvE death logging.").Value;
        DiscordWebhookToken = config.Bind("Discord", "DiscordWebhookToken", "", "Discord webhook token for PvE death logging.").Value;

        DiscordWebhookIDPvP = config.Bind("Discord", "DiscordWebhookIDPvP", "", "Discord webhook ID for PvP kill logging.").Value;
        DiscordWebhookTokenPvP = config.Bind("Discord", "DiscordWebhookTokenPvP", "", "Discord webhook token for PvP kill logging.").Value;
    }
}