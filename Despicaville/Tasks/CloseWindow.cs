using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Scenes;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class CloseWindow : Task
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

            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(Location.ToVector2);
            if (tile.Name.Contains("Closed"))
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
                AssetManager.PlaySound_Random_AtDistance("WindowClose", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowClose", Handler.Player.Location.ToVector2, Location.ToVector2, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowClose", Handler.Player.Location.ToVector2, Location.ToVector2, 8);
            }

            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Tile bottom_tile = bottom_tiles.GetTile(tile.Location.ToVector2);
            tile.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);

            if (character.Direction == Direction.Up ||
                character.Direction == Direction.Down)
            {
                tile.Name = "Window_WestEast_Closed";
            }
            else if (character.Direction == Direction.Right ||
                     character.Direction == Direction.Left)
            {
                tile.Name = "Window_NorthSouth_Closed";
            }

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You softly closed a window.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You closed a window.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You slammed a window shut.");
                }
            }
            else if (!Handler.Player.Unconscious)
            {
                Direction direction = WorldUtil.GetDirection(Location, Handler.Player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(Handler.Player.Location, Location, 2))
                {
                    GameUtil.AddMessage("You hear a window softly closed to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(Handler.Player.Location, Location, 4))
                {
                    GameUtil.AddMessage("You hear a window close to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(Handler.Player.Location, Location, 8))
                {
                    GameUtil.AddMessage("You hear a window slammed shut to the " + direction.ToString() + ".");
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
