using Microsoft.Xna.Framework;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.SubTasks
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

            int x_diff = (int)(Handler.Player.Location.X - Location.X) * -1;
            int y_diff = (int)(Handler.Player.Location.Y - Location.Y) * -1;
            AssetManager.PlaySound_Random_In3D("DoorOpen", new Vector3(x_diff, y_diff, 1), 1, 20);

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
            }
        }
    }
}
