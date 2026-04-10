using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Sounds;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class UseSink : Task
    {
        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Map block_map = WorldUtil.GetCurrentMap(character);
            Layer top_tiles = block_map.GetLayer("TopTiles");

            Tile sink = WorldUtil.StandingByFurniture(top_tiles, character.Location, "Sink");
            if (sink != null)
            {
                if (!sink.Texture.Name.Contains("Used"))
                {
                    sink.Texture = AssetManager.Textures[sink.Texture.Name + "_Used"];

                    Character player = Handler.GetPlayer();
                    if (!player.Unconscious)
                    {
                        Vector2 player_location = new Vector2(player.Location.X, player.Location.Y);
                        AssetManager.PlaySound_Random_AtDistance("WaterRunning", player_location, new Vector2(sink.Location.X, sink.Location.Y), 5);

                        if (character.Type != "Player")
                        {
                            Direction direction = WorldUtil.GetDirection(sink.Location, player.Location, true);
                            if (WorldUtil.InRange(player.Location, sink.Location, 5))
                            {
                                GameUtil.AddMessage("You hear a sink running to the " + direction.ToString() + ".");
                            }
                        }
                    }
                }
            }
        }

        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Vector2 location = new Vector2(Location.X, Location.Y);

            Tile sink = WorldUtil.GetFurniture(Handler.TopFurniture, new Location(location.X, location.Y, 0));
            if (sink != null)
            {
                if (sink.Name.Contains("Sink"))
                {
                    string[] name_parts = sink.Texture.Name.Split('_');
                    sink.Texture = AssetManager.Textures[name_parts[0] + "_" + name_parts[1]];
                }
            }

            Something thirst = character.GetStat("Thirst");
            Something bladder = character.GetStat("Bladder");

            bladder.IncreaseValue(thirst.Value / 2);
            thirst.Value = 0;

            if (character.Type == "Player")
            {
                GameUtil.AddMessage("You drank water from a sink.");
            }

            SoundManager.StopSound("WaterRunning");
        }

        public Character GetOwner()
        {
            if (OwnerIDs.Count > 0)
            {
                long id = OwnerIDs[0];

                Army army = CharacterManager.GetArmy("Characters");
                if (army != null)
                {
                    int squadCount = army.Squads.Count;
                    for (int s = 0; s < squadCount; s++)
                    {
                        Squad squad = army.Squads[s];

                        int charCount = squad.Characters.Count;
                        for (int c = 0; c < charCount; c++)
                        {
                            Character existing = squad.Characters[c];
                            if (existing.ID == id)
                            {
                                return existing;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
