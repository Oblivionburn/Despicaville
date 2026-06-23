using OP_Engine.Characters;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class UseToilet : JobTask
    {
        public override void Action_Start()
        {
            Character? character = GetOwner();
            if (character?.Location == null)
            {
                return;
            }

            Map? block_map = WorldUtil.GetCurrentMap(character);
            Layer? middle_tiles = block_map?.GetLayer("MiddleTiles");
            if (middle_tiles == null)
            {
                return;
            }

            bool nextTo = false;

            Tile? toilet = null;

            foreach (Tile tile in middle_tiles.Tiles)
            {
                if (tile.Name != null &&
                    tile.Name.Contains("Toilet"))
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

                if (toilet.Texture != null &&
                    !toilet.Texture.Name.Contains("Used"))
                {
                    toilet.Texture = Handler.GetTexture(toilet.Texture.Name + "_Used");
                }
            }
        }

        public override void Action_End()
        {
            if (Location == null ||
                Handler.Player?.Location == null)
            {
                return;
            }

            Character? character = GetOwner();
            if (character == null)
            {
                return;
            }

            Tile? toilet = WorldUtil.GetFurniture(Handler.MiddleFurniture, Location);
            if (toilet?.Name != null)
            {
                if (toilet.Name.Contains("Toilet") &&
                    toilet.Texture != null)
                {
                    string[] name_parts = toilet.Texture.Name.Split('_');
                    toilet.Texture = Handler.GetTexture(name_parts[0] + "_" + name_parts[1]);
                }

                character.Stats.Bladder = 0;

                if (!Handler.Player.Unconscious)
                {
                    if (toilet.Sound != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance(toilet.Sound, Handler.Player.Location.ToVector2, toilet.Location.ToVector2, toilet.SoundRange);
                    }

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

        public Character? GetOwner()
        {
            if (Handler.Player?.ID == OwnerID)
            {
                return Handler.Player;
            }

            Army army = CharacterManager.Armies[0];
            Squad citizens = army.Squads[1];

            int count = citizens.Characters.Count;
            for (int c = 0; c < count; c++)
            {
                Character citizen = citizens.Characters[c];
                if (citizen.ID == OwnerID)
                {
                    return citizen;
                }
            }

            return null;
        }
    }
}
