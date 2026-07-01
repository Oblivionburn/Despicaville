using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class ToggleLight : JobTask
    {
        public override void Action_End()
        {
            if (Location == null)
            {
                return;
            }

            if (Handler.Player?.Location != null)
            {
                AssetManager.PlaySound_Random_AtDistance("Click", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
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
