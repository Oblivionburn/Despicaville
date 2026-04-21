using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Push : Task
    {
        public Character character;
        public Tile tile;
        public Location destination;
        public Location newLocation;
        public float travelled;
        public bool moved;
        public bool isBlocked;

        public override void Action_Start()
        {
            character = null;
            tile = null;
            destination = null;
            newLocation = null;
            travelled = 0;
            moved = false;
            isBlocked = true;

            character = WorldUtil.GetCharacter(Location);
            if (character == null)
            {
                tile = WorldUtil.GetFurniture_Movable(Handler.MiddleFurniture, Location);
            }

            Map map = WorldUtil.GetMap();

            if (character != null)
            {
                newLocation = new Location(character.Location.X, character.Location.Y, 0);

                if (Direction == Direction.Up)
                {
                    destination = new Location(character.Location.X, character.Location.Y - 1, 0);
                }
                else if (Direction == Direction.Right)
                {
                    destination = new Location(character.Location.X + 1, character.Location.Y, 0);
                }
                else if (Direction == Direction.Down)
                {
                    destination = new Location(character.Location.X, character.Location.Y + 1, 0);
                }
                else if (Direction == Direction.Left)
                {
                    destination = new Location(character.Location.X - 1, character.Location.Y, 0);
                }

                isBlocked = WorldUtil.Blocked(map, destination, false);
                if (isBlocked)
                {
                    EndTime = new TimeHandler(TimeManager.Now);
                }
            }
            else if (tile != null)
            {
                newLocation = new Location(tile.Location.X, tile.Location.Y, 0);

                if (Direction == Direction.Up)
                {
                    destination = new Location(tile.Location.X, tile.Location.Y - 1, 0);
                }
                else if (Direction == Direction.Right)
                {
                    destination = new Location(tile.Location.X + 1, tile.Location.Y, 0);
                }
                else if (Direction == Direction.Down)
                {
                    destination = new Location(tile.Location.X, tile.Location.Y + 1, 0);
                }
                else if (Direction == Direction.Left)
                {
                    destination = new Location(tile.Location.X - 1, tile.Location.Y, 0);
                }

                isBlocked = WorldUtil.Blocked(map, destination, true);
                if (isBlocked)
                {
                    EndTime = new TimeHandler(TimeManager.Now);
                }
            }
        }

        public override void Action()
        {
            Character owner = GetOwner();
            if (owner == null ||
                isBlocked)
            {
                return;
            }

            float distance = Main.Game.TileSize.X;
            float speed = distance / 8;

            if (character != null)
            {
                if (destination.X > newLocation.X)
                {
                    character.Region.X += speed;
                    travelled += speed;

                    if (travelled == distance)
                    {
                        newLocation.X++;
                        moved = true;
                    }
                }
                else if (destination.X < newLocation.X)
                {
                    character.Region.X -= speed;
                    travelled += speed;

                    if (travelled == distance)
                    {
                        newLocation.X--;
                        moved = true;
                    }
                }
                else
                {
                    if (destination.Y > newLocation.Y)
                    {
                        character.Region.Y += speed;
                        travelled += speed;

                        if (travelled == distance)
                        {
                            newLocation.Y++;
                            moved = true;
                        }
                    }
                    else if (destination.Y < newLocation.Y)
                    {
                        character.Region.Y -= speed;
                        travelled += speed;

                        if (travelled == distance)
                        {
                            newLocation.Y--;
                            moved = true;
                        }
                    }
                    else
                    {
                        moved = true;
                    }
                }
            }
            else if (tile != null)
            {
                if (destination.X > newLocation.X)
                {
                    tile.Region.X += speed;
                    travelled += speed;

                    if (travelled == distance)
                    {
                        newLocation.X++;
                        moved = true;
                    }
                }
                else if (destination.X < newLocation.X)
                {
                    tile.Region.X -= speed;
                    travelled += speed;

                    if (travelled == distance)
                    {
                        newLocation.X--;
                        moved = true;
                    }
                }
                else
                {
                    if (destination.Y > newLocation.Y)
                    {
                        tile.Region.Y += speed;
                        travelled += speed;

                        if (travelled == distance)
                        {
                            newLocation.Y++;
                            moved = true;
                        }
                    }
                    else if (destination.Y < newLocation.Y)
                    {
                        tile.Region.Y -= speed;
                        travelled += speed;

                        if (travelled == distance)
                        {
                            newLocation.Y--;
                            moved = true;
                        }
                    }
                    else
                    {
                        moved = true;
                    }
                }
            }
            else
            {
                moved = true;
            }

            if (moved)
            {
                EndTime = new TimeHandler(TimeManager.Now);
            }

            if (owner.Type == "Player")
            {
                TimeTracker.Tick(Handler.ActionRate);
            }
        }

        public override void Action_End()
        {
            if (moved)
            {
                if (character != null)
                {
                    WorldUtil.Push_Character(character, newLocation);
                }
                else if (tile != null)
                {
                    WorldUtil.Push_Tile(tile, newLocation);
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
