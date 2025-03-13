![image](https://github.com/user-attachments/assets/f05d0627-cbdb-495b-8513-c9720995f640)

# NightbaneHardcore

Welcome to **NightbaneHardcore**, a hardcore survival experience designed to test your skills and resilience! This mod introduces punishing mechanics to elevate your gameplay. Ready to face the ultimate challenge?

---

## Warning
- **This mod was designed for solo gameplay. When you die, your entire castle gets deleted, SteamID of your character is set to 0 and you are kicked from the server.**

## Known Bugs

- **Dying from sun will be detected as PvP Death.**

## Features

- **Total Progress Wipe on Death**: Die, and your entire journey resets. No second chances.
- **Death Announcements**: Every death is broadcasted in chat for all to witness.
- **Discord Deathlog**: Log every demise to Discord for a permanent record.
- **Bloodmoon PvP**: PvP activates during bloodmoons—survive or conquer.
- **Boosted Drops During Bloodmoon**: Higher drop rates make the chaos rewarding.
- **Disable dropping items from your inventory**: Use it to avoid situations where high level player crafts high end gear for his low level friend that just joined the server.
- **Last Words**: Your final message echoes in the chat announcement.

---

## Commands

- `.dropenable`  
  Unlock item deletion if item drops are disabled.
- `.dropdisable`  
  Lock item deletion to prevent accidental removals.

---

## Configuration

Customize your **NightbaneHardcore** experience with the following config options:

### `[Bloodmoon]`

- **`PvPOnBloodmoon`**  
  *Enable PvP during Bloodmoon.*  
  - Type: `Boolean`  
  - Default: `true`

- **`HigherDropOnBloodmoon`**  
  *Enable higher item drop rates during Bloodmoon.*  
  - Type: `Boolean`  
  - Default: `true`

- **`DropRateOnBloodmoon`**  
  *Set the drop rate multiplier during Bloodmoon (if enabled).*  
  - Type: `Int32`  
  - Default: `3`

### `[Discord]`

- **`DiscordDeathlog`**  
  *Log deaths to a Discord webhook.*  
  - Type: `Boolean`  
  - Default: `false`

- **`DiscordWebhookID`**  
  *Discord webhook ID for PvE death logging.*  
  - Type: `String`  
  - Default: `(empty)`

- **`DiscordWebhookToken`**  
  *Discord webhook token for PvE death logging.*  
  - Type: `String`  
  - Default: `(empty)`

- **`DiscordWebhookIDPvP`**  
  *Discord webhook ID for PvP kill logging.*  
  - Type: `String`  
  - Default: `(empty)`

- **`DiscordWebhookTokenPvP`**  
  *Discord webhook token for PvP kill logging.*  
  - Type: `String`  
  - Default: `(empty)`

### `[General]`

- **`AnnounceDeaths`**  
  *Announce player deaths in chat.*  
  - Type: `Boolean`  
  - Default: `true`

- **`LockItemDropping`**  
  *Prevent players from dropping items on ground.*  
  - Type: `Boolean`  
  - Default: `true`

- **`LastWords`**  
  *Add last words of player that just died to the chat announcement.*  
  - Type: `Boolean`  
  - Default: `true`
    
---
If you have any questions, feel free to join Nightbane Discord: https://discord.gg/23Bd9ryzUH
