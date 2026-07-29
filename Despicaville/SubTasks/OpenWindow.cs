using Microsoft.Xna.Framework;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.SubTasks
{
    public class OpenWindow : JobTask
    {
        public override void Action_End()
        {
            if (Name == null ||
                Location == null ||
                Main.Game == null ||
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
            AssetManager.PlaySound_Random_In3D("WindowOpen", new Vector3(x_diff, y_diff, 1), 1, 20);

            if (Owner_Character.Direction == Direction.North ||
                Owner_Character.Direction == Direction.South)
            {
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width / 8, tile.Region.Height);
                tile.Name = "Window_WestEast_Open";
            }
            else if (Owner_Character.Direction == Direction.East ||
                     Owner_Character.Direction == Direction.West)
            {
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height / 8);
                tile.Name = "Window_NorthSouth_Open";
            }
        }
    }
}
