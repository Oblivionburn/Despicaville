using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Menus;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Search : JobTask
    {
        public override void Action_End()
        {
            if (Location == null)
            {
                return;
            }

            Character? character = GetOwner();
            if (character == null)
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
                    TimeManager.Paused = true;

                    Handler.Trading = true;
                    Handler.Trading_InventoryID.Add(tile.Inventory.ID);

                    Menu? main = MenuManager.GetMenu("Inventory");
                    if (main != null)
                    {
                        main.Load();
                        main.Active = true;
                        main.Visible = true;
                    }
                }
                else if (Name != null)
                {
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
                        GameUtil.AddMessage("You quietly searched the " + WorldUtil.GetTile_Name(tile) + ", but found nothing.");
                    }
                    else if (loudness == 2)
                    {
                        GameUtil.AddMessage("You searched the " + WorldUtil.GetTile_Name(tile) + ", but found nothing.");
                    }
                    else if (loudness == 3)
                    {
                        GameUtil.AddMessage("You loudly searched the " + WorldUtil.GetTile_Name(tile) + ", but found nothing.");
                    }
                }
            }
        }

        public Character? GetOwner()
        {
            if (Handler.Player?.ID == OwnerID)
            {
                return Handler.Player;
            }

            Army army = CharacterManager.Armies[0];
            Squad citizens = army.Squads[1];

            int count = citizens.Characters.Count;
            for (int c = 0; c < count; c++)
            {
                Character citizen = citizens.Characters[c];
                if (citizen.ID == OwnerID)
                {
                    return citizen;
                }
            }

            return null;
        }
    }
}
