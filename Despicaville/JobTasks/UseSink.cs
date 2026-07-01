using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Sounds;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class UseSink : JobTask
    {
        public override void Action_Start()
        {
            if (Handler.Player?.Location == null)
            {
                return;
            }

            if (Owner_Character?.Location == null)
            {
                return;
            }

            if (Owner_Character.Map == null)
            {
                WorldUtil.SetCurrentMap(Owner_Character);
            }

            Layer? top_tiles = Owner_Character.Map?.GetLayer("TopTiles");
            if (top_tiles == null)
            {
                return;
            }

            Tile? sink = WorldUtil.StandingByFurniture(top_tiles, Owner_Character.Location, "Sink");
            if (sink?.Texture != null)
            {
                if (!sink.Texture.Name.Contains("Used"))
                {
                    sink.Texture = Handler.GetTexture(sink.Texture.Name + "_Used");

                    if (!Handler.Player.Unconscious &&
                        sink.Location != null)
                    {
                        if (sink.Sound != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance(sink.Sound, Handler.Player.Location.ToVector2, sink.Location.ToVector2, sink.SoundRange);
                        }

                        if (Owner_Character.Type != "Player")
                        {
                            Direction direction = WorldUtil.GetDirection(Handler.Player.Location, sink.Location);
                            if (WorldUtil.InRange(Handler.Player.Location, sink.Location, 5))
                            {
                                GameUtil.AddMessage("You hear a sink to the " + direction.ToString() + ".");
                            }
                        }
                    }
                }
            }
        }

        public override void Action_End()
        {
            if (Location == null)
            {
                return;
            }

            if (Owner_Character == null)
            {
                return;
            }

            Tile? sink = WorldUtil.GetFurniture(Handler.TopFurniture, Location);
            if (sink?.Name != null)
            {
                if (sink.Name.Contains("Sink") &&
                    sink.Texture != null)
                {
                    string[] name_parts = sink.Texture.Name.Split('_');
                    sink.Texture = Handler.GetTexture(name_parts[0] + "_" + name_parts[1]);
                }
            }

            Owner_Character.Stats.Bladder += Owner_Character.Stats.Thirst / 2;
            if (Owner_Character.Stats.Bladder > 100)
            {
                Owner_Character.Stats.Bladder = 100;
            }

            Owner_Character.Stats.Thirst = 0;
            SoundManager.StopSound("WaterRunning");
        }
    }
}
