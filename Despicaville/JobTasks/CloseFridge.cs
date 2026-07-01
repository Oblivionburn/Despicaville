using Microsoft.Xna.Framework;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class CloseFridge : JobTask
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
            if (tile?.Region == null)
            {
                return;
            }
            if (tile.Texture?.Name != null &&
                !tile.Texture.Name.Contains("Used"))
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
                AssetManager.PlaySound_Random_AtDistance("DoorClose", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", Handler.Player.Location.ToVector2, Location.ToVector2, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", Handler.Player.Location.ToVector2, Location.ToVector2, 8);
            }

            if (Owner_Character.Direction == Direction.North &&
                tile.Direction == Direction.South)
            {
                tile.Texture = Handler.GetTexture("Fridge_South");
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, Main.Game.TileSize.Y);
            }
            else if (Owner_Character.Direction == Direction.East &&
                     tile.Direction == Direction.West)
            {
                tile.Texture = Handler.GetTexture("Fridge_West");
                tile.Region = new Region(tile.Region.X + Main.Game.TileSize.X, tile.Region.Y, Main.Game.TileSize.X, tile.Region.Height);
            }
            else if (Owner_Character.Direction == Direction.South &&
                     tile.Direction == Direction.North)
            {
                tile.Texture = Handler.GetTexture("Fridge_North");
                tile.Region = new Region(tile.Region.X, tile.Region.Y + Main.Game.TileSize.Y, tile.Region.Width, Main.Game.TileSize.Y);
            }
            else if (Owner_Character.Direction == Direction.West &&
                     tile.Direction == Direction.East)
            {
                tile.Texture = Handler.GetTexture("Fridge_East");
                tile.Region = new Region(tile.Region.X, tile.Region.Y, Main.Game.TileSize.X, tile.Region.Height);
            }

            if (tile.Texture != null)
            {
                tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
            }

            tile.BlocksMovement = true;

            if (Owner_Character.Type != "Player" &&
                !Handler.Player.Unconscious)
            {
                Direction direction = WorldUtil.GetDirection(Handler.Player.Location, Location);

                if (loudness == 1 &&
                    WorldUtil.InRange(Handler.Player.Location, Location, 2))
                {
                    GameUtil.AddMessage("You hear a fridge softly closed to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(Handler.Player.Location, Location, 4))
                {
                    GameUtil.AddMessage("You hear a fridge close to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(Handler.Player.Location, Location, 8))
                {
                    GameUtil.AddMessage("You hear a fridge slammed shut to the " + direction.ToString() + ".");
                }
            }
        }
    }
}
