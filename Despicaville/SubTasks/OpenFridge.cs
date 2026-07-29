using Microsoft.Xna.Framework;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.SubTasks
{
    public class OpenFridge : JobTask
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
            if (tile == null)
            {
                return;
            }

            if (tile.Region == null ||
                tile.Texture == null)
            {
                return;
            }
            if (tile.Texture.Name != null &&
                tile.Texture.Name.Contains("Used"))
            {
                return;
            }

            int x_diff = (int)(Handler.Player.Location.X - Location.X) * -1;
            int y_diff = (int)(Handler.Player.Location.Y - Location.Y) * -1;
            AssetManager.PlaySound_Random_In3D("DoorOpen", new Vector3(x_diff, y_diff, 1), 1, 20);

            if (tile.Direction == Direction.South)
            {
                tile.Texture = Handler.GetTexture("Fridge_South_Used");
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, Main.Game.TileSize.Y * 2);
            }
            else if (tile.Direction == Direction.West)
            {
                tile.Texture = Handler.GetTexture("Fridge_West_Used");
                tile.Region = new Region(tile.Region.X - Main.Game.TileSize.X, tile.Region.Y, Main.Game.TileSize.X * 2, tile.Region.Height);
            }
            else if (tile.Direction == Direction.North)
            {
                tile.Texture = Handler.GetTexture("Fridge_North_Used");
                tile.Region = new Region(tile.Region.X, tile.Region.Y - Main.Game.TileSize.Y, tile.Region.Width, Main.Game.TileSize.Y * 2);
            }
            else if (tile.Direction == Direction.East)
            {
                tile.Texture = Handler.GetTexture("Fridge_East_Used");
                tile.Region = new Region(tile.Region.X, tile.Region.Y, Main.Game.TileSize.X * 2, tile.Region.Height);
            }

            if (tile.Texture != null)
            {
                tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
            }
        }
    }
}
