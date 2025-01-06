using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Utils;
using GLOKON.Baiters.GodotInterop;
using GLOKON.Baiters.GodotInterop.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace GLOKON.Baiters.Core
{
    public sealed class ActorSpawner(
        IOptions<WebFishingOptions> options,
        BaitersServer server)
    {
        private readonly Random random = new();
        private readonly TextScene mainZone = LoadScene("main_zone");
        private readonly WeightedList<string> spawnProbabilities = [];

        public void Setup()
        {
            spawnProbabilities.Add("none", (long)(options.Value.Modifiers.FishChance * 1000));
            spawnProbabilities.Add(ActorType.Fish, (long)(options.Value.Modifiers.FishChance * 1000)); // Fish and None share the same probability, as one or the other is more likely
            spawnProbabilities.Add(ActorType.RainCloud, (long)(options.Value.Modifiers.RainChance * 1000));
            spawnProbabilities.Add(ActorType.Meteor, (long)(options.Value.Modifiers.MeteorChance * 1000));
            spawnProbabilities.Add(ActorType.VoidPortal, (long)(options.Value.Modifiers.VoidPortalChance * 1000));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (server.NpcActorCount < options.Value.Modifiers.MaxNpcActors)
                {
                    try
                    {
                        switch (spawnProbabilities.Next())
                        {
                            case ActorType.Fish:
                                if (server.GetActorsByType(ActorType.Fish).Count() < options.Value.Modifiers.MaxFish)
                                {
                                    SpawnFish();
                                }

                                break;
                            case ActorType.RainCloud:
                                SpawnRainCloud();
                                break;
                            case ActorType.Meteor:
                                SpawnFish(ActorType.Meteor);
                                break;
                            case ActorType.VoidPortal:
                                SpawnVoidPortal();
                                break;
                        }

                        if (server.GetActorsByType(ActorType.Metal).Count() < options.Value.Modifiers.MaxMetal)
                        {
                            SpawnMetal(); // Always spawn metal every iteration, if needed
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

        public void SpawnRainCloud()
        {
            Vector3 position = new(random.Next(-100, 150), 42f, random.Next(-150, 100));
            var rainCloud = new RainCloud(position);
            server.SpawnActor(rainCloud);
            rainCloud.Despawn();
        }

        public void SpawnFish(string type = ActorType.Fish)
        {
            if (mainZone.SceneLocations.TryGetValue(MainZoneGroup.FishSpawns, out var fishPoints))
            {
                Vector3 position = fishPoints[random.Next(fishPoints.Length)] + new Vector3(0, .08f, 0);
                var fish = new Fish(type, position);
                server.SpawnActor(fish);
                fish.Despawn();
            }
        }

        public void SpawnVoidPortal()
        {
            if (mainZone.SceneLocations.TryGetValue(MainZoneGroup.HiddenSpots, out var hiddenPoints))
            {
                Vector3 position = hiddenPoints[random.Next(hiddenPoints.Length)];
                var voidPortal = new VoidPortal(position);
                server.SpawnActor(voidPortal);
                voidPortal.Despawn();
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

                server.SpawnActor(new Metal(position));
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
