using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Scenes;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class OpenDoor : JobTask
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
            if (tile.Name.Contains("Open"))
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

            if (character.Direction == Direction.North)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (character.Direction == Direction.East)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }
            else if (character.Direction == Direction.South)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (character.Direction == Direction.West)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }

            tile.BlocksMovement = false;
            CharacterUtil.UpdateSight(character);

            if (character.Type != "Player")
            {
                CharacterUtil.UpdateSight(Handler.Player);

                if (!Handler.Player.Unconscious)
                {
                    Direction direction = WorldUtil.GetDirection(Handler.Player.Location, Location);

                    if (loudness == 1 &&
                        WorldUtil.InRange(Handler.Player.Location, Location, 2))
                    {
                        GameUtil.AddMessage("You hear a door quietly open to the " + direction.ToString() + ".");
                    }
                    else if (loudness == 2 &&
                             WorldUtil.InRange(Handler.Player.Location, Location, 4))
                    {
                        GameUtil.AddMessage("You hear a door open to the " + direction.ToString() + ".");
                    }
                    else if (loudness == 3 &&
                             WorldUtil.InRange(Handler.Player.Location, Location, 8))
                    {
                        GameUtil.AddMessage("You hear a door loudly open to the " + direction.ToString() + ".");
                    }
                }
            }
        }

        public Character GetOwner()
        {
            if (Handler.Player.ID == OwnerID)
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
