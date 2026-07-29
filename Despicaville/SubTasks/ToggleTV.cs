using Microsoft.Xna.Framework;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using Despicaville.Util;

namespace Despicaville.SubTasks
{
    public class ToggleTV : JobTask
    {
        public override void Action_End()
        {
            if (Location == null)
            {
                return;
            }

            if (Handler.Player?.Location != null)
            {
                int x_diff = (int)(Handler.Player.Location.X - Location.X) * -1;
                int y_diff = (int)(Handler.Player.Location.Y - Location.Y) * -1;
                AssetManager.PlaySound_Random_In3D("Click", new Vector3(x_diff, y_diff, 1), 1, 20);
            }

            Map? map = WorldUtil.GetMap();

            Layer? middle_tiles = map?.GetLayer("MiddleTiles");
            Tile? tile = middle_tiles?.GetTile(Location.ToVector2);
            if (tile != null)
            {
                tile.IsLightSource = !tile.IsLightSource;
            }
        }
    }
}
