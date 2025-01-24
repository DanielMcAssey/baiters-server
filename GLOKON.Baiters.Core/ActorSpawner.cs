using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Utils;
using GLOKON.Baiters.GodotInterop;
using GLOKON.Baiters.GodotInterop.Models;
using System.Numerics;
using Microsoft.Extensions.Options;
using Serilog;

namespace GLOKON.Baiters.Core
{
    public sealed class ActorSpawner
    {
        private readonly WebFishingOptions options;
        private readonly BaitersServer server;
        private readonly Random random = new();
        private readonly TextScene mainZone = LoadScene("main_zone");
        private readonly WeightedList<string> spawnProbabilities = [];

        public static IReadOnlyCollection<string> Spawnable => [ActorType.Fish, ActorType.RainCloud, ActorType.Meteor, ActorType.VoidPortal, ActorType.Metal, ActorType.Bird];

        public ActorSpawner(IOptions<WebFishingOptions> _options, BaitersServer server)
        {
            options = _options.Value;
            this.server = server;
            spawnProbabilities.Add("none", (long)(options.Modifiers.FishChance * 1000));
            spawnProbabilities.Add(ActorType.Fish, (long)(options.Modifiers.FishChance * 1000)); // Fish and None share the same probability, as one or the other is more likely
            spawnProbabilities.Add(ActorType.Bird, (long)(options.Modifiers.BirdChance * 1000));
            spawnProbabilities.Add(ActorType.RainCloud, (long)(options.Modifiers.RainChance * 1000));
            spawnProbabilities.Add(ActorType.Meteor, (long)(options.Modifiers.MeteorChance * 1000));
            spawnProbabilities.Add(ActorType.VoidPortal, (long)(options.Modifiers.VoidPortalChance * 1000));
        }

        internal async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (server.OwnedActors.Count() < options.Modifiers.MaxNpcActors)
                {
                    try
                    {
                        string nextSpawn = spawnProbabilities.Next();
                        if (Spawn(nextSpawn))
                        {
                            Log.Debug("Spawned {0}", nextSpawn);
                        }

                        // Always spawn metal every iteration, if needed
                        if (Spawn(ActorType.Metal))
                        {
                            Log.Debug("Spawned {0}", ActorType.Metal);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to run actor spawner");
                    }
                }

                await Task.Delay(8_000, cancellationToken);
            }
        }

        public bool Spawn(string type)
        {
            Log.Verbose("Attempting to spawn {type}", type);

            switch (type)
            {
                case ActorType.Fish:
                    if (server.GetActorsByType(ActorType.Fish, server.ServerId).Count() < options.Modifiers.MaxFish)
                    {
                        SpawnFish();
                        return true;
                    }

                    break;
                case ActorType.Bird:
                    if (server.GetActorsByType(ActorType.Bird, server.ServerId).Count() < options.Modifiers.MaxBird)
                    {
                        SpawnBird();
                        return true;
                    }

                    break;
                case ActorType.RainCloud:
                    if (server.GetActorsByType(ActorType.RainCloud, server.ServerId).Count() < options.Modifiers.MaxRainCloud)
                    {
                        SpawnRainCloud();
                        return true;
                    }

                    break;
                case ActorType.Meteor:
                    if (server.GetActorsByType(ActorType.Meteor, server.ServerId).Count() < options.Modifiers.MaxMeteor)
                    {
                        SpawnFish(ActorType.Meteor);
                        return true;
                    }

                    break;
                case ActorType.VoidPortal:
                    if (server.GetActorsByType(ActorType.VoidPortal, server.ServerId).Count() < options.Modifiers.MaxVoidPortal)
                    {
                        SpawnVoidPortal();
                        return true;
                    }

                    break;
                case ActorType.Metal:
                    if (server.GetActorsByType(ActorType.Metal, server.ServerId).Count() < options.Modifiers.MaxMetal)
                    {
                        SpawnMetal();
                        return true;
                    }

                    break;
            }

            return false;
        }

        public void SpawnBird()
        {
            if (mainZone.SceneLocations.TryGetValue(MainZoneGroup.TrashPoints, out var trashPoints))
            {
                Vector3 position = trashPoints[random.Next(trashPoints.Length)] + new Vector3(random.Next(-3, 3), 0, random.Next(-3, 3));
                server.SpawnActor(new Bird(position, server.ServerId));
            }
        }

        public void SpawnRainCloud()
        {
            Vector3 position = new(random.Next(-100, 150), 42f, random.Next(-150, 100));
            server.SpawnActor(new RainCloud(position, server.ServerId));
        }

        public void SpawnFish(string type = ActorType.Fish)
        {
            if (mainZone.SceneLocations.TryGetValue(MainZoneGroup.FishSpawns, out var fishPoints))
            {
                Vector3 position = fishPoints[random.Next(fishPoints.Length)] + new Vector3(0, .08f, 0);
                server.SpawnActor(new Fish(type, position, server.ServerId));
            }
        }

        public void SpawnVoidPortal()
        {
            if (mainZone.SceneLocations.TryGetValue(MainZoneGroup.HiddenSpots, out var hiddenPoints))
            {
                Vector3 position = hiddenPoints[random.Next(hiddenPoints.Length)];
                server.SpawnActor(new VoidPortal(position, server.ServerId));
            }
        }

        public void SpawnMetal()
        {
            if (mainZone.SceneLocations.TryGetValue(MainZoneGroup.TrashPoints, out var trashPoints) && mainZone.SceneLocations.TryGetValue(MainZoneGroup.ShorelinePoints, out var shorelinePoints))
            {
                Vector3 position = trashPoints[random.Next(trashPoints.Length)];
                if (random.NextSingle() < .15f)
                {
                    position = shorelinePoints[random.Next(shorelinePoints.Length)];
                }

                server.SpawnActor(new Metal(position, server.ServerId));
            }
        }

        private static TextScene LoadScene(string scene)
        {
            Log.Information("Loading {scene} scene", scene);

            var sceneFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scenes", $"{scene}.tscn");

            if (!File.Exists(sceneFile))
            {
                throw new ArgumentException($"Scene file is missing from {sceneFile}");
            }

            TextScene loadedScene = TscnReader.ReadTextScene(sceneFile, [MainZoneGroup.FishSpawns, MainZoneGroup.TrashPoints, MainZoneGroup.ShorelinePoints, MainZoneGroup.HiddenSpots]);

            Log.Information("{scene} scene loaded", scene);

            return loadedScene;
        }
    }
}
