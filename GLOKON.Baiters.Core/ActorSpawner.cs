using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Utils;
using GLOKON.Baiters.GodotInterop;
using GLOKON.Baiters.GodotInterop.Models;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core
{
    public sealed class ActorSpawner(BaitersServer server)
    {
        private readonly Random random = new();
        private readonly TextScene mainZone = LoadScene("main_zone");
        private readonly WeightedList<string> spawnProbabilities = [];

        public void Setup()
        {
            spawnProbabilities.Add("none", (long)(server.Options.Modifiers.FishChance * 1000));
            spawnProbabilities.Add("fish_spawn", (long)(server.Options.Modifiers.FishChance * 1000)); // Fish and None share the same probability, as one or the other is more likely
            spawnProbabilities.Add("raincloud", (long)(server.Options.Modifiers.RainChance * 1000));
            spawnProbabilities.Add("fish_spawn_alien", (long)(server.Options.Modifiers.MeteorChance * 1000));
            spawnProbabilities.Add("void_portal", (long)(server.Options.Modifiers.VoidPortalChance * 1000));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (server.NpcActorCount < server.Options.Modifiers.MaxNpcActors)
                {
                    try
                    {
                        switch (spawnProbabilities.Next())
                        {
                            case "fish_spawn":
                                if (server.GetActorsByType("fish_spawn").Count() < server.Options.Modifiers.MaxFish)
                                {
                                    SpawnFish();
                                }

                                break;
                            case "raincloud":
                                SpawnRainCloud();
                                break;
                            case "fish_spawn_alien":
                                SpawnFish("fish_spawn_alien");
                                break;
                            case "void_portal":
                                SpawnVoidPortal();
                                break;
                        }

                        if (server.GetActorsByType("metal_spawn").Count() < server.Options.Modifiers.MaxMetal)
                        {
                            SpawnMetal(); // Always spawn metal every iteration, if needed
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to run server spawner");
                    }
                }

                await Task.Delay(8_000, cancellationToken);
            }
        }

        public void SpawnRainCloud()
        {
            Vector3 position = new(random.Next(-100, 150), 42f, random.Next(-150, 100));
            var rainCloud = new RainCloud(position);
            SpawnActor(rainCloud);
            rainCloud.Despawn();
        }

        public void SpawnFish(string type = "fish_spawn")
        {
            if (mainZone.SceneLocations.TryGetValue("fish_spawn", out var fishPoints))
            {
                Vector3 position = fishPoints[random.Next(fishPoints.Length)] + new Vector3(0, .08f, 0);
                Fish actor = new(type, position);
                SpawnActor(actor);
                actor.Despawn();
            }
        }

        public void SpawnVoidPortal()
        {
            if (mainZone.SceneLocations.TryGetValue("hidden_spot", out var hiddenPoints))
            {
                Vector3 position = hiddenPoints[random.Next(hiddenPoints.Length)];
                var voidPortal = new VoidPortal(position);
                SpawnActor(voidPortal);
                voidPortal.Despawn();
            }
        }

        public void SpawnMetal()
        {
            if (mainZone.SceneLocations.TryGetValue("trash_point", out var trashPoints) && mainZone.SceneLocations.TryGetValue("shoreline_point", out var shorelinePoints))
            {
                Vector3 position = trashPoints[random.Next(trashPoints.Length)];
                if (random.NextSingle() < .15f)
                {
                    position = shorelinePoints[random.Next(shorelinePoints.Length)];
                }

                SpawnActor(new Metal(position));
            }
        }

        private void SpawnActor(Actor actor)
        {
            long actorId = random.NextInt64();

            Dictionary<string, object> spawnPkt = new()
            {
                ["type"] = "instance_actor"
            };

            Dictionary<string, object> instanceSpaceParams = [];
            spawnPkt["params"] = instanceSpaceParams;

            instanceSpaceParams["actor_type"] = actor.Type;

            if (actor is MovableActor movableActor)
            {
                instanceSpaceParams["at"] = movableActor.Position;
                instanceSpaceParams["rot"] = movableActor.Rotation;
            }
            else
            {
                instanceSpaceParams["at"] = Vector3.Zero;
                instanceSpaceParams["rot"] = Vector3.Zero;
            }

            instanceSpaceParams["zone"] = "main_zone";
            instanceSpaceParams["zone_owner"] = -1;
            instanceSpaceParams["actor_id"] = actorId;
            instanceSpaceParams["creator_id"] = (long)SteamClient.SteamId.Value;

            server.AddActor(actorId, actor);
            server.SendPacket(spawnPkt);
        }

        private static TextScene LoadScene(string scene)
        {
            Log.Information($"Loading {scene} scene");

            var sceneFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scenes", $"{scene}.tscn");

            if (!File.Exists(sceneFile))
            {
                throw new ArgumentException($"Scene File is missing from {sceneFile}");
            }

            TextScene loadedScene = TscnReader.ReadTextScene(sceneFile, ["fish_spawn", "trash_point", "shoreline_point", "hidden_spot"]);

            Log.Information($"{scene} Scene loaded");

            return loadedScene;
        }
    }
}
