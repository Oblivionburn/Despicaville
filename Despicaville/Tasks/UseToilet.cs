using OP_Engine.Characters;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class UseToilet : Task
    {
        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Map block_map = WorldUtil.GetCurrentMap(character);
            Layer middle_tiles = block_map.GetLayer("MiddleTiles");

            bool nextTo = false;

            Tile toilet = null;

            foreach (Tile tile in middle_tiles.Tiles)
            {
                if (tile.Name.Contains("Toilet"))
                {
                    if (WorldUtil.NextTo(tile.Location, character.Location) &&
                        character.Gender == "Male")
                    {
                        nextTo = true;
                        toilet = tile;
                        break;
                    }
                    else if (tile.Location.X == character.Location.X &&
                             tile.Location.Y == character.Location.Y)
                    {
                        toilet = tile;
                        break;
                    }
                }
            }

            if (toilet != null)
            {
                if (nextTo)
                {
                    Direction furniture_direction = WorldUtil.GetDirection(character.Location, toilet.Location);
                    if (character.Direction != furniture_direction)
                    {
                        if (furniture_direction == Direction.North)
                        {
                            character.FaceNorth();
                        }
                        else if (furniture_direction == Direction.East)
                        {
                            character.FaceEast();
                        }
                        else if (furniture_direction == Direction.South)
                        {
                            character.FaceSouth();
                        }
                        else if (furniture_direction == Direction.West)
                        {
                            character.FaceWest();
                        }
                    }
                }
                else
                {
                    if (character.Direction != toilet.Direction)
                    {
                        if (toilet.Direction == Direction.North)
                        {
                            character.FaceNorth();
                        }
                        else if (toilet.Direction == Direction.East)
                        {
                            character.FaceEast();
                        }
                        else if (toilet.Direction == Direction.South)
                        {
                            character.FaceSouth();
                        }
                        else if (toilet.Direction == Direction.West)
                        {
                            character.FaceWest();
                        }
                    }
                }

                CharacterUtil.UpdateGear(character);

                if (!toilet.Texture.Name.Contains("Used"))
                {
                    toilet.Texture = AssetManager.Textures[toilet.Texture.Name + "_Used"];
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

            Tile toilet = WorldUtil.GetFurniture(Handler.MiddleFurniture, Location);
            if (toilet != null)
            {
                if (toilet.Name.Contains("Toilet"))
                {
                    string[] name_parts = toilet.Texture.Name.Split('_');
                    toilet.Texture = AssetManager.Textures[name_parts[0] + "_" + name_parts[1]];
                }

                character.Stats.Bladder = 0;

                if (!Handler.Player.Unconscious)
                {
                    AssetManager.PlaySound_Random_AtDistance(toilet.Sound, Handler.Player.Location.ToVector2, toilet.Location.ToVector2, toilet.SoundRange);

                    if (character.Type != "Player")
                    {
                        Direction direction = WorldUtil.GetDirection(Handler.Player.Location, toilet.Location);
                        if (WorldUtil.InRange(Handler.Player.Location, toilet.Location, 5))
                        {
                            GameUtil.AddMessage("You hear a toilet to the " + direction.ToString() + ".");
                        }
                    }
                }
            }
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
