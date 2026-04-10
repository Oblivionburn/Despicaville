using Microsoft.Xna.Framework;
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
                    Direction furniture_direction = WorldUtil.GetDirection(toilet.Location, character.Location, false);
                    if (character.Direction != furniture_direction)
                    {
                        if (furniture_direction == Direction.Up)
                        {
                            character.Animator.FaceNorth(character);
                        }
                        else if (furniture_direction == Direction.Right)
                        {
                            character.Animator.FaceEast(character);
                        }
                        else if (furniture_direction == Direction.Down)
                        {
                            character.Animator.FaceSouth(character);
                        }
                        else if (furniture_direction == Direction.Left)
                        {
                            character.Animator.FaceWest(character);
                        }
                    }
                }
                else
                {
                    if (character.Direction != toilet.Direction)
                    {
                        if (toilet.Direction == Direction.Up)
                        {
                            character.Animator.FaceNorth(character);
                        }
                        else if (toilet.Direction == Direction.Right)
                        {
                            character.Animator.FaceEast(character);
                        }
                        else if (toilet.Direction == Direction.Down)
                        {
                            character.Animator.FaceSouth(character);
                        }
                        else if (toilet.Direction == Direction.Left)
                        {
                            character.Animator.FaceWest(character);
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

            Vector2 location = new Vector2(Location.X, Location.Y);

            Tile toilet = WorldUtil.GetFurniture(Handler.MiddleFurniture, new Location(location.X, location.Y, 0));
            if (toilet != null)
            {
                if (toilet.Name.Contains("Toilet"))
                {
                    string[] name_parts = toilet.Texture.Name.Split('_');
                    toilet.Texture = AssetManager.Textures[name_parts[0] + "_" + name_parts[1]];
                }
            }

            Something bladder = character.GetStat("Bladder");
            bladder.Value = 0;

            Character player = Handler.GetPlayer();
            if (!player.Unconscious)
            {
                Vector2 player_location = new Vector2(player.Location.X, player.Location.Y);
                AssetManager.PlaySound_Random_AtDistance("Flush", player_location, new Vector2(toilet.Location.X, toilet.Location.Y), 5);

                if (character.Type == "Player")
                {
                    GameUtil.AddMessage("You used a toilet.");
                }
                else
                {
                    Direction direction = WorldUtil.GetDirection(toilet.Location, player.Location, true);
                    if (WorldUtil.InRange(player.Location, toilet.Location, 5))
                    {
                        GameUtil.AddMessage("You hear a toilet flush to the " + direction.ToString() + ".");
                    }
                }
            }
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
