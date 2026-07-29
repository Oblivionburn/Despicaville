using Microsoft.Xna.Framework;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.SubTasks
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

            int x_diff = (int)(Handler.Player.Location.X - Location.X) * -1;
            int y_diff = (int)(Handler.Player.Location.Y - Location.Y) * -1;
            AssetManager.PlaySound_Random_In3D("DoorClose", new Vector3(x_diff, y_diff, 1), 1, 20);

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
            }
        }
    }
}
