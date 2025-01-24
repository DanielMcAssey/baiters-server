using Serilog;
using System.Reflection;
using System.Runtime.Loader;

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
                    pluginAssm.Add(Assembly.LoadFrom(pluginPath));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load plugin file ({0})", pluginPath);
                }
            }

            AssemblyLoadContext.Default.Resolving += Default_Resolving;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var assembly in pluginAssm)
            {
                try
                {
                    Type[] pluginTypes = assembly.GetTypes()
                        .Where((type) => type.IsClass && type.IsSubclassOf(typeof(BaitersPlugin)))
                        .ToArray();

                    foreach (Type pluginType in pluginTypes)
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
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load plugin ({0})", assembly.FullName);
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

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            AssemblyLoadContext.Default.Resolving -= Default_Resolving;
            Plugins.Clear();
            GC.Collect();
        }

        private static Assembly? Default_Resolving(AssemblyLoadContext context, AssemblyName assembly)
        {
            Log.Verbose("Attempting to resolve {0}", assembly);

            string assemblyPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"./plugins/{assembly.Name}.dll"));
            return context.LoadFromAssemblyPath(assemblyPath);
        }

        private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            Assembly? assembly = null;

            // Ignore version for internal dependencies
            if (args.Name.Contains("GLOKON.Baiters.Core") || args.Name.Contains("GLOKON.Baiters.GodotInterop"))
            {
                var assemblyName = new AssemblyName(args.Name);

                // Prevents infinte loop, as args will contain the assembly FullName once resolved
                if (assemblyName.Name != args.Name)
                {
                    assembly = ((AppDomain)sender).Load(assemblyName.Name);
                }
            }

            return assembly;
        }
    }
}
