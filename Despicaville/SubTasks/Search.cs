using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Menus;
using Despicaville.Util;

namespace Despicaville.SubTasks
{
    public class Search : JobTask
    {
        public override void Action_End()
        {
            if (Location == null)
            {
                return;
            }

            if (Owner_Character?.Type != "Player")
            {
                return;
            }

            Map? map = WorldUtil.GetMap();
            Layer? bottom_tiles = map?.GetLayer("BottomTiles");

            Tile? tile = WorldUtil.GetFurniture(Handler.MiddleFurniture, Location);
            if (tile == null)
            {
                tile = bottom_tiles?.GetTile(Location.ToVector2);
            }

            if (tile?.Texture != null)
            {
                if (tile.Inventory?.Items.Count > 0)
                {
                    Handler.Trading = true;
                    Handler.Trading_InventoryID.Add(tile.Inventory.ID);
                    MenuManager.GetMenu("Inventory")?.Open();
                }
            }
        }
    }
}
