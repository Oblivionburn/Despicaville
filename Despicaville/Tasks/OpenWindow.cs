using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Scenes;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class OpenWindow : Task
    {
        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Vector2 location = new Vector2(Location.X, Location.Y);

            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];

            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);
            if (tile.Name.Contains("Open"))
            {
                return;
            }

            Character player = Handler.GetPlayer();

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
                AssetManager.PlaySound_Random_AtDistance("WindowOpen", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowOpen", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowOpen", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            if (character.Direction == Direction.Up ||
                character.Direction == Direction.Down)
            {
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width / 8, tile.Region.Height);
                tile.Name = "Window_WestEast_Open";
            }
            else if (character.Direction == Direction.Right ||
                     character.Direction == Direction.Left)
            {
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height / 8);
                tile.Name = "Window_NorthSouth_Open";
            }

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You quietly opened a window.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You opened a window.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You loudly opened a window.");
                }
            }
            else if (!player.Unconscious)
            {
                Direction direction = WorldUtil.GetDirection(Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, Location, 2))
                {
                    GameUtil.AddMessage("You hear a window quietly open to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, Location, 4))
                {
                    GameUtil.AddMessage("You hear a window open to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, Location, 8))
                {
                    GameUtil.AddMessage("You hear a window loudly open to the " + direction.ToString() + ".");
                }
            }
        }

        public Character GetOwner()
        {
            if (OwnerIDs.Count > 0)
            {
                long id = OwnerIDs[0];

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
                            if (existing.ID == id)
                            {
                                return existing;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
