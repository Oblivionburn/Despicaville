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

                    if (!Handler.Player.Unconscious)
                    {
                        AssetManager.PlaySound_Random_AtDistance(sink.Sound, Handler.Player.Location.ToVector2, sink.Location.ToVector2, sink.SoundRange);

                        if (character.Type != "Player")
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
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Tile sink = WorldUtil.GetFurniture(Handler.TopFurniture, Location);
            if (sink != null)
            {
                if (sink.Name.Contains("Sink"))
                {
                    string[] name_parts = sink.Texture.Name.Split('_');
                    sink.Texture = AssetManager.Textures[name_parts[0] + "_" + name_parts[1]];
                }
            }

            character.Stats.Bladder += character.Stats.Thirst / 2;
            if (character.Stats.Bladder > 100)
            {
                character.Stats.Bladder = 100;
            }

            character.Stats.Thirst = 0;
            SoundManager.StopSound("WaterRunning");
        }

        public Character GetOwner()
        {
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
                        if (existing.ID == OwnerID)
                        {
                            return existing;
                        }
                    }
                }
            }

            return null;
        }
    }
}
