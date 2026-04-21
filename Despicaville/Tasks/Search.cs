using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Scenes;
using OP_Engine.Time;
using OP_Engine.Menus;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Search : Task
    {
        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];
            Layer bottom_tiles = map.GetLayer("BottomTiles");

            Tile tile = WorldUtil.GetFurniture(Handler.MiddleFurniture, Location);
            if (tile == null)
            {
                tile = bottom_tiles.GetTile(Location.ToVector2);
            }

            if (tile.Texture != null)
            {
                if (tile.Inventory.Items.Count > 0)
                {
                    TimeManager.Paused = true;

                    Handler.Trading = true;
                    Handler.Trading_InventoryID.Add(tile.Inventory.ID);

                    Menu main = MenuManager.GetMenu("Inventory");
                    main.Load();
                    main.Active = true;
                    main.Visible = true;
                }
                else
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

        public Character GetOwner()
        {
            Army army = CharacterManager.GetArmy("Characters");
            if (army != null)
            {
                int squadCount = army.Squads.Count;
                for (int s = 0; s < squadCount; s++)
                {
                    Squad squad = army.Squads[s];

                    int charCount = squad.Characters.Count;
                    for (int c = 0; c < charCount; c++)
                    {
                        Character existing = squad.Characters[c];
                        if (existing.ID == OwnerID)
                        {
                            return existing;
                        }
                    }
                }
            }

            return null;
        }
    }
}
