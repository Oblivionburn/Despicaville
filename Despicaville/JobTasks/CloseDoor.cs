using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class CloseDoor : JobTask
    {
        public override void Action_End()
        {
            if (Name == null ||
                Location == null ||
                Handler.Player?.Location == null)
            {
                return;
            }

            if (Owner_Character == null)
            {
                return;
            }

            Map? map = WorldUtil.GetMap();

            Layer? middle_tiles = map?.GetLayer("MiddleTiles");
            Tile? tile = middle_tiles?.GetTile(Location.ToVector2);
            if (tile?.Location == null)
            {
                return;
            }
            if (tile.Name != null &&
                tile.Name.Contains("Closed"))
            {
                return;
            }

            int loudness = 2;
            if (Name.Contains("Quiet"))
            {
                loudness = 1;
            }
            else if (Name.Contains("Loud"))
            {
                loudness = 3;
            }

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", Handler.Player.Location.ToVector2, Location.ToVector2, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", Handler.Player.Location.ToVector2, Location.ToVector2, 8);
            }

            Layer? bottom_tiles = map?.GetLayer("BottomTiles");
            Tile? bottom_tile = bottom_tiles?.GetTile(tile.Location.ToVector2);
            if (bottom_tile?.Region != null)
            {
                tile.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);
            }

            if (Owner_Character.Direction == Direction.North)
            {
                tile.Texture = Handler.GetTexture("Door_WestEast");
                tile.Name = "Door_WestEast_Closed";
            }
            else if (Owner_Character.Direction == Direction.East)
            {
                tile.Texture = Handler.GetTexture("Door_NorthSouth");
                tile.Name = "Door_NorthSouth_Closed";
            }
            else if (Owner_Character.Direction == Direction.South)
            {
                tile.Texture = Handler.GetTexture("Door_WestEast");
                tile.Name = "Door_WestEast_Closed";
            }
            else if (Owner_Character.Direction == Direction.West)
            {
                tile.Texture = Handler.GetTexture("Door_NorthSouth");
                tile.Name = "Door_NorthSouth_Closed";
            }

            tile.BlocksMovement = true;
            CharacterUtil.UpdateSight(Owner_Character);

            if (Owner_Character.Type != "Player")
            {
                CharacterUtil.UpdateSight(Handler.Player);

                if (!Handler.Player.Unconscious)
                {
                    Direction direction = WorldUtil.GetDirection(Handler.Player.Location, Location);

                    if (loudness == 1 &&
                        WorldUtil.InRange(Handler.Player.Location, Location, 2))
                    {
                        GameUtil.AddMessage("You hear a door softly closed to the " + direction.ToString() + ".");
                    }
                    else if (loudness == 2 &&
                             WorldUtil.InRange(Handler.Player.Location, Location, 4))
                    {
                        GameUtil.AddMessage("You hear a door close to the " + direction.ToString() + ".");
                    }
                    else if (loudness == 3 &&
                             WorldUtil.InRange(Handler.Player.Location, Location, 8))
                    {
                        GameUtil.AddMessage("You hear a door slammed shut to the " + direction.ToString() + ".");
                    }
                }
            }
        }
    }
}
