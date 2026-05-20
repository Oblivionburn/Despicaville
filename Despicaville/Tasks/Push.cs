using System;
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
        public bool fall;

        public override void Action_Start()
        {
            Character owner = GetOwner();
            if (owner == null)
            {
                return;
            }

            character = null;
            tile = null;
            destination = null;
            newLocation = null;
            travelled = 0;
            moved = false;
            isBlocked = false;
            fall = false;

            character = WorldUtil.GetCharacter(Location);
            if (character == null)
            {
                tile = WorldUtil.GetFurniture_Movable(Handler.MiddleFurniture, Location);
            }

            Map map = WorldUtil.GetMap();

            if (character != null)
            {
                #region Character

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

                Tile blockingTile = WorldUtil.GetTile(destination);
                if (blockingTile != null)
                {
                    if (blockingTile.BlocksMovement)
                    {
                        isBlocked = true;
                        EndTime = new TimeHandler(TimeManager.Now);

                        string bodyPart = CombatUtil.RandomBodyPart(owner, character);
                        CombatUtil.AddWound(owner, character, character.GetBodyPart(bodyPart), "Bruise");
                    }
                    else if (blockingTile.Layer.Name != "BottomTiles")
                    {
                        fall = true;
                    }
                }

                if (blockingTile == null)
                {
                    Character blockingCharacter = WorldUtil.GetCharacter(destination);
                    if (blockingCharacter != null)
                    {
                        isBlocked = true;
                        EndTime = new TimeHandler(TimeManager.Now);
                    }
                }
                
                if (!isBlocked)
                {
                    character.ResetAnimation();
                    character.Path.Clear();
                    character.Job.Tasks.Clear();
                    character.Moving = false;
                    character.Moved = 0;
                }

                #endregion
            }
            else if (tile != null)
            {
                #region Tile

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

                Region size = WorldUtil.GetSize(tile);
                for (int y = 0; y <= size.Height; y++)
                {
                    float Y = destination.Y + y;

                    for (int x = 0; x <= size.Width; x++)
                    {
                        float X = destination.X + x;

                        Location location = new Location(X, Y, 0);

                        Tile blockingTile = WorldUtil.GetTile(location);
                        if (blockingTile != null &&
                            blockingTile.ID != tile.ID &&
                            (blockingTile.Layer.Name == "MiddleTiles" ||
                             blockingTile.BlocksMovement))
                        {
                            isBlocked = true;
                            EndTime = new TimeHandler(TimeManager.Now);
                            break;
                        }

                        if (!isBlocked)
                        {
                            Character blockingCharacter = WorldUtil.GetCharacter(location);
                            if (blockingCharacter != null)
                            {
                                if (tile.BlocksMovement)
                                {
                                    isBlocked = true;
                                    EndTime = new TimeHandler(TimeManager.Now);
                                    break;
                                }
                            }
                        }
                    }

                    if (isBlocked)
                    {
                        break;
                    }
                }

                #endregion
            }

            if (!isBlocked &&
                destination != null)
            {
                #region Break Window

                Layer middle_tiles = map.GetLayer("MiddleTiles");

                if (character != null)
                {
                    Tile middle_tile = middle_tiles.GetTile(destination.ToVector2);
                    if (middle_tile != null)
                    {
                        if (middle_tile.Name.Contains("Window") &&
                            middle_tile.Name.Contains("Closed"))
                        {
                            Tasker.BreakWindow(destination.ToVector2, Direction);
                        }
                    }
                }
                else if (tile != null)
                {
                    bool foundWindow = false;

                    Region size = WorldUtil.GetSize(tile);
                    for (int y = 0; y <= size.Height; y++)
                    {
                        float Y = destination.Y + y;

                        for (int x = 0; x <= size.Width; x++)
                        {
                            float X = destination.X + x;

                            Location location = new Location(X, Y, 0);

                            Tile middle_tile = WorldUtil.GetFurniture(Handler.MiddleFurniture, location);
                            if (middle_tile != null)
                            {
                                if (middle_tile.Name.Contains("Window") &&
                                    middle_tile.Name.Contains("Closed"))
                                {
                                    foundWindow = true;
                                    Tasker.BreakWindow(location.ToVector2, Direction);
                                    break;
                                }
                            }
                        }

                        if (foundWindow)
                        {
                            break;
                        }
                    }
                }

                #endregion
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
                    CharacterUtil.UpdateGear(character);

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
                    CharacterUtil.UpdateGear(character);

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
                        CharacterUtil.UpdateGear(character);

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
                        CharacterUtil.UpdateGear(character);

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
            Character owner = GetOwner();
            if (owner == null)
            {
                return;
            }

            if (moved)
            {
                if (character != null)
                {
                    WorldUtil.Push_Character(character, newLocation);

                    Map map = WorldUtil.GetMap();
                    Layer bottom_tiles = map.GetLayer("BottomTiles");
                    Tile bottom_tile = bottom_tiles.GetTile(character.Location.ToVector2);
                    if (bottom_tile != null)
                    {
                        character.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);
                        CharacterUtil.UpdateGear(character);
                    }

                    if (fall ||
                        Utility.RandomPercent(owner.Stats.Strength))
                    {
                        character.Laying = true;

                        float standTime = CharacterUtil.GetStandTime(character);
                        character.Job.Tasks.Add(new Stand
                        {
                            Name = "Stand",
                            OwnerID = character.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(standTime)),
                            Direction = character.Direction
                        });
                    }
                }
                else if (tile != null)
                {
                    WorldUtil.Push_Tile(tile, newLocation);

                    Character blockingCharacter = WorldUtil.GetCharacter(tile.Location);
                    if (blockingCharacter != null)
                    {
                        blockingCharacter.ResetAnimation();
                        blockingCharacter.Path.Clear();
                        blockingCharacter.Job.Tasks.Clear();

                        blockingCharacter.Moving = false;
                        blockingCharacter.Laying = true;

                        Map map = WorldUtil.GetMap();
                        Layer bottom_tiles = map.GetLayer("BottomTiles");
                        Tile bottom_tile = bottom_tiles.GetTile(blockingCharacter.Location.ToVector2);
                        if (bottom_tile != null)
                        {
                            blockingCharacter.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);
                            CharacterUtil.UpdateGear(blockingCharacter);
                        }

                        float standTime = CharacterUtil.GetStandTime(blockingCharacter);
                        blockingCharacter.Job.Tasks.Add(new Stand
                        {
                            Name = "Stand",
                            OwnerID = blockingCharacter.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(standTime)),
                            Direction = blockingCharacter.Direction
                        });
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
