using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class OpenDoor : JobTask
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
            if (tile?.Region == null)
            {
                return;
            }
            if (tile.Name != null &&
                tile.Name.Contains("Open"))
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
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", Handler.Player.Location.ToVector2, Location.ToVector2, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", Handler.Player.Location.ToVector2, Location.ToVector2, 8);
            }

            if (Owner_Character.Direction == Direction.North)
            {
                tile.Texture = Handler.GetTexture("Door_NorthSouth");
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (Owner_Character.Direction == Direction.East)
            {
                tile.Texture = Handler.GetTexture("Door_WestEast");
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }
            else if (Owner_Character.Direction == Direction.South)
            {
                tile.Texture = Handler.GetTexture("Door_NorthSouth");
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (Owner_Character.Direction == Direction.West)
            {
                tile.Texture = Handler.GetTexture("Door_WestEast");
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }

            tile.BlocksMovement = false;
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
                        GameUtil.AddMessage("You hear a door quietly open to the " + direction.ToString() + ".");
                    }
                    else if (loudness == 2 &&
                             WorldUtil.InRange(Handler.Player.Location, Location, 4))
                    {
                        GameUtil.AddMessage("You hear a door open to the " + direction.ToString() + ".");
                    }
                    else if (loudness == 3 &&
                             WorldUtil.InRange(Handler.Player.Location, Location, 8))
                    {
                        GameUtil.AddMessage("You hear a door loudly open to the " + direction.ToString() + ".");
                    }
                }
            }
        }
    }
}
