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
    public class OpenDoor : Task
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
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            if (character.Direction == Direction.Up)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (character.Direction == Direction.Right)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }
            else if (character.Direction == Direction.Down)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (character.Direction == Direction.Left)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }

            tile.BlocksMovement = false;
            CharacterUtil.UpdateSight(character);

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You quietly opened a door.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You opened a door.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You loudly opened a door.");
                }
            }
            else if (!player.Unconscious)
            {
                Direction direction = WorldUtil.GetDirection(Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, Location, 2))
                {
                    GameUtil.AddMessage("You hear a door quietly open to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, Location, 4))
                {
                    GameUtil.AddMessage("You hear a door open to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, Location, 8))
                {
                    GameUtil.AddMessage("You hear a door loudly open to the " + direction.ToString() + ".");
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
