# Plugin Development

Building plugins for Baiters is really easy!

Save time by using the [plugin template repository](https://github.com/DanielMcAssey/baiters-server-plugin-example) to create your plugin.

Alternatively, there is an example plugin on this repository, the ChatCommands plugin, so feel free to use that as a basis.


## Steps

1. **Create** a new .NET Core library project
2. **Install** the `GLOKON.Baiters.Core` NuGet package using your package manager
3. **Extend** the `GLOKON.Baiters.Core.Plugins.BaitersPlugin` class to create your own Plugin entry point (It `must` have `GLOKON.Baiters.Core.GameManager` as its only parameter for the constructor)
4. **Build** your plugin, use the [GLOKON.Baiters.Core.GameManager](https://github.com/DanielMcAssey/baiters-server/blob/main/GLOKON.Baiters.Core/GameManager.cs) to interact with the server, listen to events, etc.
5. **Release** your plugin, make sure the plugin assembly file name ends with `.BaitersPlugin.dll`