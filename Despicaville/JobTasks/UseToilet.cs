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
            if (Owner_Character?.Location == null)
            {
                return;
            }

            if (Owner_Character.Map == null)
            {
                WorldUtil.SetCurrentMap(Owner_Character);
            }

            Layer? middle_tiles = Owner_Character.Map?.GetLayer("MiddleTiles");
            if (middle_tiles == null)
            {
                return;
            }

            bool nextTo = false;

            Tile? toilet = null;

            foreach (Tile tile in middle_tiles.Tiles)
            {
                if (tile.Location != null &&
                    tile.Name != null &&
                    tile.Name.Contains("Toilet"))
                {
                    if (WorldUtil.NextTo(tile.Location, Owner_Character.Location) &&
                        Owner_Character.Gender == "Male")
                    {
                        nextTo = true;
                        toilet = tile;
                        break;
                    }
                    else if (tile.Location.X == Owner_Character.Location.X &&
                             tile.Location.Y == Owner_Character.Location.Y)
                    {
                        toilet = tile;
                        break;
                    }
                }
            }

            if (toilet?.Location != null)
            {
                if (nextTo)
                {
                    Direction furniture_direction = WorldUtil.GetDirection(Owner_Character.Location, toilet.Location);
                    if (Owner_Character.Direction != furniture_direction)
                    {
                        if (furniture_direction == Direction.North)
                        {
                            Owner_Character.FaceNorth();
                        }
                        else if (furniture_direction == Direction.East)
                        {
                            Owner_Character.FaceEast();
                        }
                        else if (furniture_direction == Direction.South)
                        {
                            Owner_Character.FaceSouth();
                        }
                        else if (furniture_direction == Direction.West)
                        {
                            Owner_Character.FaceWest();
                        }
                    }
                }
                else
                {
                    if (Owner_Character.Direction != toilet.Direction)
                    {
                        if (toilet.Direction == Direction.North)
                        {
                            Owner_Character.FaceNorth();
                        }
                        else if (toilet.Direction == Direction.East)
                        {
                            Owner_Character.FaceEast();
                        }
                        else if (toilet.Direction == Direction.South)
                        {
                            Owner_Character.FaceSouth();
                        }
                        else if (toilet.Direction == Direction.West)
                        {
                            Owner_Character.FaceWest();
                        }
                    }
                }

                CharacterUtil.UpdateGear(Owner_Character);

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

            if (Owner_Character == null)
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

                Owner_Character.Stats.Bladder = 0;

                if (!Handler.Player.Unconscious &&
                    toilet.Location != null)
                {
                    if (toilet.Sound != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance(toilet.Sound, Handler.Player.Location.ToVector2, toilet.Location.ToVector2, toilet.SoundRange);
                    }

                    if (Owner_Character.Type != "Player")
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
    }
}
