# Baiters
WebFishing Server with support for custom plugins

Runs on Windows, Linux and MacOS

Inspired by the amazing work by DrMeepso [WebFishingCove](https://github.com/DrMeepso/WebFishingCove)

## About Baiters

Honestly? I just wanted to have a go at building a Steamworks server for a game that I enjoy.
I felt that the servers out there had a lot of room for improvement, in performance, maintainability and support.

## Setup
> [!IMPORTANT]
> Steam installed and a Steam account with WebFishing is required.

1. **Download** Baiters from the Releases page
2. **Copy** the `main_zone.tscn` from the game into the `scenes` folder (You need to extract it from the game ([Example Tool](https://github.com/bruvzg/gdsdecomp)))
3. **Configure** the `appsettings.json` or use the environment variables (See the config reference below)
4. **Run** the Steam client with the account with WebFishing
5. **Start** Baiters

## Plugins

Please see [PLUGINS.md](https://github.com/DanielMcAssey/baiters-server/blob/main/PLUGINS.md) on how to build your own plugins.

To install plugins, simply place the plugin file(s) into the `plugins` folder.


## Enviroment Variables
> [!NOTE]
> Please note the `double` underscore

| Variable                | Description                                                                                           |
| ----------------------- | ----------------------------------------------------------------------------------------------------- |
| `WebFishing__Admins__n` | Steam ID of the admins of the server (Replace `n` with an index in the array, Defaults to me (Please remove me!)) |
| `WebFishing__PluginsEnabled`   | Enable/Disable plugins from being loaded (Values are `true` or `false`, Defaults to `false`) |
| `WebFishing__ServerName`   | Name of the server to display on the server browser (Defaults to `My Baiters Server`) |
| `WebFishing__CommandPrefix`   | Chat command prefix for server commands (Defaults to `!`) |
| `WebFishing__MaxPlayers`   | Maximum players allowed on the server (Default is `50`) |
| `WebFishing__JoinType`   | Visibility of the server to others (Valid values are `Public`, `InviteOnly`, `FriendsOnly`, Defaults to `Public`) |
| `WebFishing__JoinMessage`   | Message to display to new users joining the server |
| `WebFishing__Tags__n`   | Tags displayed on the server browser (Replace `n` with an index in the array) (Valid values are `talkative`, `quiet`, `grinding`, `chill`, `silly`, `hardcore`, `mature`, `modded`) |
| `WebFishing__Modifiers__FishChance`   | Chance (from `0.0` to `1.0`) of a Fish spawning (Default is `0.50` (50%)) |
| `WebFishing__Modifiers__BirdChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.33` (33%)) |
| `WebFishing__Modifiers__RainChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.25` (25%)) |
| `WebFishing__Modifiers__MeteorChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.01` (1%)) |
| `WebFishing__Modifiers__VoidPortalChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.005` (0.5%)) |
| `WebFishing__Modifiers__MaxMetal`   | Maximum metal spawned (Defaults to `7`) |
| `WebFishing__Modifiers__MaxFish`   | Maximum Fish spawned in the server (Defaults to `30`) |
| `WebFishing__Modifiers__MaxBird`   | Maximum Birds spawned in the server (Defaults to `8`) |
| `WebFishing__Modifiers__MaxNpcActors`   | Maximum NPC (Non-Playable Characters, like fish, birds, metal, etc.) spawned (Defaults to `50`) |