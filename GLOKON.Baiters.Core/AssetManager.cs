using GLOKON.Baiters.Core.Converters.Json;
using GLOKON.Baiters.Core.Models.Asset;
using Serilog;
using System.Text.Json;

namespace GLOKON.Baiters.Core
{
    public sealed class AssetManager
    {
        private readonly string _defaultItemsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./storage/default_items.json");
        private readonly string _defaultCosmeticsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./storage/default_cosmetics.json");

        private readonly IList<Item> _defaultItems = [];
        private readonly IList<Cosmetic> _defaultCosmetics = [];

        public IEnumerable<Item> DefaultItems => _defaultItems;

        public IEnumerable<Cosmetic> DefaultCosmetics => _defaultCosmetics;

        public AssetManager()
        {
            if (File.Exists(_defaultItemsPath))
            {
                try
                {
                    _defaultItems = JsonSerializer.Deserialize<List<Item>>(File.ReadAllText(_defaultItemsPath), JsonOptions.Default) ?? _defaultItems;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to default items file");
                }
            }

            if (File.Exists(_defaultCosmeticsPath))
            {
                try
                {
                    _defaultCosmetics = JsonSerializer.Deserialize<List<Cosmetic>>(File.ReadAllText(_defaultCosmeticsPath), JsonOptions.Default) ?? _defaultCosmetics;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to default cosmetics file");
                }
            }
        }
    }
}
