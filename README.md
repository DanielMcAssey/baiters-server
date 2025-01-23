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

1. **Download** the latest Baiters from the Releases page
2. **Copy** the `main_zone.tscn` from the game into the `scenes` folder (You need to extract it from the game ([Example Tool](https://github.com/bruvzg/gdsdecomp)))
3. **Configure** the `appsettings.json` or use the environment variables (See the config reference below, you can also create a `appsettings.Production.json` in the same folder to override anything in `appsettings.json`)
4. **Run** the Steam client with the account with WebFishing
5. **Start** Baiters

## Plugins

Please see [PLUGINS.md](https://github.com/DanielMcAssey/baiters-server/blob/main/PLUGINS.md) on how to build your own plugins.

To install plugins, simply place the plugin file(s) into the `plugins` folder.


## Updating

Follow these steps to update your Baiters server to the latest version.

1. **Download** the latest Baiters from the Releases page
2. **Settings** copy from the current version your `appsettings.Production.json` (If it exists, or if you changed the `appsettings.json` directly, then manually copy over your changes to the new `appsettings.json`, **do not overwite it**)
3. **State** copy from the current version your `bans.json` (If it exists), `chalk_canvases.json` (If it exists), your `scenes/main_zone.tscn`
4. **Plugins** copy from the current version your plugins from the `plugins/` folder (**do not overwite any bundled plugins in the new version**)
5. **Stop** your old version of Baiters (If its running)
6. **Start** Baiters and your upgrade is complete


## SSL
> [!IMPORTANT]
> To use **LetsEncrypt** you will need a domain that points to this server and change `Server.HttpPort` to `80`. (You may need to run the server as administrator to use port `80`)

The server supports access to the admin panel via SSL, and the server can manage the certificates for you via **LetsEncrypt**.

Simply enter your email address to `Server.LetsEncrypt.EmailAddress` and enter the domain(s) you want to generate the certificate for to `Server.LetsEncrypt.Domains`


## Reverse Proxy
> [!IMPORTANT]
> Read this section if you are using or intend to use reverse proxy to access the admin panel, this does not affect the game server

If your reverse proxy is not on the local network or you want fine-grained control, you can add your reverse proxies address (in CIDR format) to the `Server.TrustedProxies` list.
It defaults to the common local networks CIDRs: `10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16` (For IPv4) and `fd00::/8` (For IPv6)

**NOTE:** Do NOT use `0.0.0.0/0` as this will allow anyone to mask their IP address to the server.


## Enviroment Variables
> [!NOTE]
> Please note the `double` underscore

| Variable                | Description                                                                                           |
| ----------------------- | ----------------------------------------------------------------------------------------------------- |
| `WebFishing__Admins__n` | Steam ID of the admins of the server (Replace `n` with an index in the array, Defaults to me (Please remove me!)) |
| `WebFishing__PluginsEnabled`   | Enable/Disable plugins from being loaded (Values are `true` or `false`, Defaults to `false`) |
| `WebFishing__ServerName`   | Name of the server to display on the server browser (Defaults to `My Baiters Server`) |
| `WebFishing__CustomLobbyCode`   | (Optional) Custom lobby code to use for the server (Server will fail to start if its taken) (Defaults to an auto-generated code) |
| `WebFishing__HideMaxPlayers`   | Hides the max player from the server list (Values are `true` or `false`, Defaults to `false`) |
| `WebFishing__CommandPrefix`   | Chat command prefix for server commands (Defaults to `!`) |
| `WebFishing__MaxPlayers`   | Maximum players allowed on the server (Default is `50`) |
| `WebFishing__JoinType`   | Visibility of the server to others (Valid values are `Public`, `InviteOnly`, `FriendsOnly`, Defaults to `Public`) |
| `WebFishing__JoinMessage`   | Message to display to new users joining the server |
| `WebFishing__SaveChalkCanvases`   | Wether to persist and save the chalk canvases on the server (Values are `true` or `false`, Defaults to `false`) |
| `WebFishing__Tags__n`   | Tags displayed on the server browser (Replace `n` with an index in the array) (Valid values are `talkative`, `quiet`, `grinding`, `chill`, `silly`, `hardcore`, `mature`, `modded`) |
| `WebFishing__Modifiers__FishChance`   | Chance (from `0.0` to `1.0`) of a Fish spawning (Default is `0.50` (50%)) |
| `WebFishing__Modifiers__BirdChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.33` (33%)) |
| `WebFishing__Modifiers__RainChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.25` (25%)) |
| `WebFishing__Modifiers__MeteorChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.01` (1%)) |
| `WebFishing__Modifiers__VoidPortalChance`   | Chance (from `0.0` to `1.0`) of a Void Portal spawning (Default is `0.005` (0.5%)) |
| `WebFishing__Modifiers__MaxMetal`   | Maximum metal spawned (Defaults to `7`) |
| `WebFishing__Modifiers__MaxFish`   | Maximum Fish spawned in the server (Defaults to `30`) |
| `WebFishing__Modifiers__MaxBird`   | Maximum Birds spawned in the server (Defaults to `8`) |
| `WebFishing__Modifiers__MaxRainCloud`   | Maximum Rain Clouds spawned in the server (Defaults to `3`) |
| `WebFishing__Modifiers__MaxMeteor`   | Maximum Meteors spawned in the server (Defaults to `3`) |
| `WebFishing__Modifiers__MaxVoidPortal`   | Maximum Void Portals spawned in the server (Defaults to `1`) |
| `WebFishing__Modifiers__MaxNpcActors`   | Maximum NPC (Non-Playable Characters, like fish, birds, metal, etc.) spawned (Defaults to `50`) |
| `WebFishing__Modifiers__TicksPerSecond`   | Server ticks per second, essentially the server FPS, lower this value to reduce CPU load as a trade-off for responsiveness (Defaults to `24`) |