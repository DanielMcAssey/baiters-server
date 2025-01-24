using Serilog;
using System.Reflection;

namespace GLOKON.Baiters.Core.Plugins
{
    public sealed class PluginLoader
    {
        public static IList<BaitersPlugin> Plugins { get; private set; } = [];

        public static void LoadPlugins(GameManager gm)
        {
            UnloadPlugins();

            Log.Information("Loading plugins...");
            Log.Warning("WARNING: Plugin loading is enabled, only use trusted plugins, as plugins have the same privilidge as this application");

            string pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            IList<Assembly> pluginAssm = [];

            foreach (var pluginPath in Directory.GetFiles(pluginsDir, "*.BaitersPlugin.dll"))
            {
                try
                {
                    Assembly pluginAssembly = Assembly.Load(AssemblyName.GetAssemblyName(pluginPath));

                    foreach (var pluginDependency in pluginAssembly.GetReferencedAssemblies())
                    {
                        try
                        {
                            AppDomain.CurrentDomain.Load(pluginDependency);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to load plugin dependency ({0})", pluginDependency.FullName);
                        }
                    }

                    pluginAssm.Add(pluginAssembly);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load plugin file ({0})", pluginPath);
                }
            }

            foreach (var assembly in pluginAssm)
            {
                Type[] pluginTypes = assembly.GetTypes()
                    .Where((type) => type.IsClass && type.IsSubclassOf(typeof(BaitersPlugin)))
                    .ToArray();

                foreach (Type pluginType in pluginTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(pluginType, gm) is BaitersPlugin plugin)
                        {
                            plugin.OnInit();
                            Plugins.Add(plugin);
                            Log.Information("{0}:{1} plugin loaded by {2}", plugin.Name, plugin.Version, plugin.Author);
                        }
                        else
                        {
                            Log.Error("Failed to instantiate plugin ({0})", pluginType.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to load plugin ({0})", pluginType.FullName);
                    }
                }
            }
        }

        public static void UnloadPlugins()
        {
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.OnDestroy();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to unload plugin {0}:{1}", plugin.Name, plugin.Version);
                }
            }

            Plugins.Clear();
            GC.Collect();
        }
    }
}
