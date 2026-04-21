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
    public class OpenFridge : Task
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
            if (tile.Texture.Name.Contains("Used"))
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
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", Handler.Player.Location.ToVector2, Location.ToVector2, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", Handler.Player.Location.ToVector2, Location.ToVector2, 8);
            }

            if (character.Direction == Direction.Up &&
                tile.Direction == Direction.Down)
            {
                tile.Texture = AssetManager.Textures["Fridge_South_Used"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, Main.Game.TileSize.Y * 2);
            }
            else if (character.Direction == Direction.Right &&
                     tile.Direction == Direction.Left)
            {
                tile.Texture = AssetManager.Textures["Fridge_West_Used"];
                tile.Region = new Region(tile.Region.X - Main.Game.TileSize.X, tile.Region.Y, Main.Game.TileSize.X * 2, tile.Region.Height);
            }
            else if (character.Direction == Direction.Down &&
                     tile.Direction == Direction.Up)
            {
                tile.Texture = AssetManager.Textures["Fridge_North_Used"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y - Main.Game.TileSize.Y, tile.Region.Width, Main.Game.TileSize.Y * 2);
            }
            else if (character.Direction == Direction.Left &&
                     tile.Direction == Direction.Right)
            {
                tile.Texture = AssetManager.Textures["Fridge_East_Used"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y, Main.Game.TileSize.X * 2, tile.Region.Height);
            }
            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You quietly opened a fridge.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You opened a fridge.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You loudly opened a fridge.");
                }
            }
            else if (!Handler.Player.Unconscious)
            {
                Direction direction = WorldUtil.GetDirection(Location, Handler.Player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(Handler.Player.Location, Location, 2))
                {
                    GameUtil.AddMessage("You hear a fridge quietly open to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(Handler.Player.Location, Location, 4))
                {
                    GameUtil.AddMessage("You hear a fridge open to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(Handler.Player.Location, Location, 8))
                {
                    GameUtil.AddMessage("You hear a fridge loudly open to the " + direction.ToString() + ".");
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
