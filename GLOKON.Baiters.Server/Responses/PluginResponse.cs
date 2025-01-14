using GLOKON.Baiters.Core.Plugins;

namespace GLOKON.Baiters.Server.Responses
{
    internal struct PluginResponse(BaitersPlugin plugin)
    {
        public string ID { get; } = plugin.ID;

        public string Version { get; } = plugin.Version;

        public string Name { get; } = plugin.Name;

        public string Description { get; } = plugin.Description;

        public string Author { get; } = plugin.Author;
    }
}
