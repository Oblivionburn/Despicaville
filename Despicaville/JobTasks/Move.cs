using System;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Scenes;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Move : JobTask
    {
        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (CharacterUtil.PulledByPlayer(character))
            {
                Direction direction = WorldUtil.GetDirection(character.Location, Handler.Player.Location);
                if (direction == Direction.North)
                {
                    character.FaceNorth();
                }
                else if (direction == Direction.East)
                {
                    character.FaceEast();
                }
                else if (direction == Direction.South)
                {
                    character.FaceSouth();
                }
                else if (direction == Direction.West)
                {
                    character.FaceWest();
                }
                return;
            }

            if (Name == "Sneak")
            {
                character.MoveSpeed = 0.5f;
            }
            else if (Name == "Walk")
            {
                character.MoveSpeed = 1;
            }
            else if (Name == "Run")
            {
                character.MoveSpeed = 2;
            }

            if (Direction == Direction.North)
            {
                character.Destination = new Location(character.Location.X, character.Location.Y - 1, character.Location.Z);
            }
            else if (Direction == Direction.East)
            {
                character.Destination = new Location(character.Location.X + 1, character.Location.Y, character.Location.Z);
            }
            else if (Direction == Direction.South)
            {
                character.Destination = new Location(character.Location.X, character.Location.Y + 1, character.Location.Z);
            }
            else if (Direction == Direction.West)
            {
                character.Destination = new Location(character.Location.X - 1, character.Location.Y, character.Location.Z);
            }

            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];

            if (WorldUtil.CanMove(character, map, character.Destination))
            {
                #region Move

                if (Direction != character.Direction)
                {
                    EndTime = new TimeHandler(TimeManager.Now);

                    character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        OwnerID = character.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                        Direction = Direction
                    });
                }
                else
                {
                    character.Moving = true;

                    if (character.Type == "Player" &&
                        Handler.Pull_Character != null)
                    {
                        if (!Handler.Pull_Character.Unconscious &&
                            !Handler.Pull_Character.Dead)
                        {
                            float break_chance = (100 - character.Stats.Strength) / 10;
                            if (Utility.RandomPercent(break_chance))
                            {
                                Handler.ResetPull();
                            }
                        }

                        if (Handler.Pull_Character != null)
                        {
                            if (character.Destination.X > character.Location.X)
                            {
                                Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X + 1, Handler.Pull_Character.Location.Y, 0);
                            }
                            else if (character.Destination.X < character.Location.X)
                            {
                                Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X - 1, Handler.Pull_Character.Location.Y, 0);
                            }
                            else
                            {
                                if (character.Destination.Y > character.Location.Y)
                                {
                                    Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X, Handler.Pull_Character.Location.Y + 1, 0);
                                }
                                else if (character.Destination.Y < character.Location.Y)
                                {
                                    Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X, Handler.Pull_Character.Location.Y - 1, 0);
                                }
                            }
                        }
                    }

                    Layer middle_tiles = map.GetLayer("MiddleTiles");
                    Tile tile = middle_tiles.GetTile(character.Destination.ToVector2);
                    if (tile != null)
                    {
                        if (tile.Name.Contains("Window") &&
                            tile.Name.Contains("Closed"))
                        {
                            Tasker.BreakWindow(character.Destination.ToVector2, character.Direction);
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region Do Something Else

                EndTime = new TimeHandler(TimeManager.Now);

                if (Direction != character.Direction)
                {
                    character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        OwnerID = character.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                        Direction = Direction
                    });
                }
                else if (character.Type != "Player")
                {
                    Layer middle_tiles = map.GetLayer("MiddleTiles");
                    Tile tile = middle_tiles.GetTile(character.Destination.ToVector2);
                    if (tile != null)
                    {
                        if (tile.Name.Contains("Window") &&
                            tile.Name.Contains("Closed"))
                        {
                            character.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "OpenWindow",
                                OwnerID = character.ID,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                Location = character.Destination,
                                Direction = Direction
                            });
                        }
                        else if (tile.Name.Contains("Door") &&
                                 tile.Name.Contains("Closed"))
                        {
                            character.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "OpenDoor",
                                OwnerID = character.ID,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10)),
                                Location = character.Destination,
                                Direction = Direction
                            });
                        }
                    }
                    else
                    {
                        character.Job.Tasks.Add(new Wait
                        {
                            Name = "Wait",
                            OwnerID = character.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(21))
                        });
                    }
                }

                #endregion
            }
        }

        public override void Action()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (character.Type != "Player" &&
                !Handler.Action)
            {
                return;
            }

            if (!character.Moving ||
                character.Region == null)
            {
                return;
            }

            character.Move_TotalDistance = Main.Game.TileSize.X;

            if (character.Destination.X > character.Location.X)
            {
                #region East

                character.Region.X += character.MoveSpeed;
                character.Moved += character.MoveSpeed;

                if (character.Type == "Player")
                {
                    if (Handler.Pull_Character != null)
                    {
                        Handler.Pull_Character.Region.X += character.MoveSpeed;
                        CharacterUtil.UpdateGear(Handler.Pull_Character);
                    }
                    else if (Handler.Pull_Tile != null)
                    {
                        Handler.Pull_Tile.Region.X += character.MoveSpeed;
                    }
                }

                if (character.Moved == character.Move_TotalDistance)
                {
                    character.Location.X++;

                    if (character.Type == "Player")
                    {
                        if (Handler.Pull_Character != null)
                        {
                            Handler.Pull_Character.Location = new Location(Handler.Pull_Character.Destination.X, Handler.Pull_Character.Destination.Y);
                            CharacterUtil.UpdateGear(Handler.Pull_Character);
                        }
                        else if (Handler.Pull_Tile != null)
                        {
                            WorldUtil.PullTile_SetLocation(Direction.East);
                        }
                    }

                    EndTime = new TimeHandler(TimeManager.Now);
                }
                else
                {
                    for (int i = 1; i <= character.Frames; i++)
                    {
                        if (character.Moved == (character.Move_TotalDistance / character.Frames) * i)
                        {
                            character.Animate();
                            CharacterUtil.UpdateGear(character);
                            break;
                        }
                    }
                }

                #endregion
            }
            else if (character.Destination.X < character.Location.X)
            {
                #region West

                character.Region.X -= character.MoveSpeed;
                character.Moved += character.MoveSpeed;

                if (character.Type == "Player")
                {
                    if (Handler.Pull_Character != null)
                    {
                        Handler.Pull_Character.Region.X -= character.MoveSpeed;
                        CharacterUtil.UpdateGear(Handler.Pull_Character);
                    }
                    else if (Handler.Pull_Tile != null)
                    {
                        Handler.Pull_Tile.Region.X -= character.MoveSpeed;
                    }
                }

                if (character.Moved == character.Move_TotalDistance)
                {
                    character.Location.X--;

                    if (character.Type == "Player")
                    {
                        if (Handler.Pull_Character != null)
                        {
                            Handler.Pull_Character.Location = new Location(Handler.Pull_Character.Destination.X, Handler.Pull_Character.Destination.Y);
                            CharacterUtil.UpdateGear(Handler.Pull_Character);
                        }
                        else if (Handler.Pull_Tile != null)
                        {
                            WorldUtil.PullTile_SetLocation(Direction.West);
                        }
                    }

                    EndTime = new TimeHandler(TimeManager.Now);
                }
                else
                {
                    for (int i = 1; i <= character.Frames; i++)
                    {
                        if (character.Moved == (character.Move_TotalDistance / character.Frames) * i)
                        {
                            character.Animate();
                            CharacterUtil.UpdateGear(character);
                            break;
                        }
                    }
                }

                #endregion
            }
            else
            {
                if (character.Destination.Y > character.Location.Y)
                {
                    #region South

                    character.Region.Y += character.MoveSpeed;
                    character.Moved += character.MoveSpeed;

                    if (character.Type == "Player")
                    {
                        if (Handler.Pull_Character != null)
                        {
                            Handler.Pull_Character.Region.Y += character.MoveSpeed;
                            CharacterUtil.UpdateGear(Handler.Pull_Character);
                        }
                        else if (Handler.Pull_Tile != null)
                        {
                            Handler.Pull_Tile.Region.Y += character.MoveSpeed;
                        }
                    }

                    if (character.Moved == character.Move_TotalDistance)
                    {
                        character.Location.Y++;

                        if (character.Type == "Player")
                        {
                            if (Handler.Pull_Character != null)
                            {
                                Handler.Pull_Character.Location = new Location(Handler.Pull_Character.Destination.X, Handler.Pull_Character.Destination.Y);
                                CharacterUtil.UpdateGear(Handler.Pull_Character);
                            }
                            else if (Handler.Pull_Tile != null)
                            {
                                WorldUtil.PullTile_SetLocation(Direction.South);
                            }
                        }

                        EndTime = new TimeHandler(TimeManager.Now);
                    }
                    else
                    {
                        for (int i = 1; i <= character.Frames; i++)
                        {
                            if (character.Moved == (character.Move_TotalDistance / character.Frames) * i)
                            {
                                character.Animate();
                                CharacterUtil.UpdateGear(character);
                                break;
                            }
                        }
                    }

                    #endregion
                }
                else if (character.Destination.Y < character.Location.Y)
                {
                    #region North

                    character.Region.Y -= character.MoveSpeed;
                    character.Moved += character.MoveSpeed;

                    if (character.Type == "Player")
                    {
                        if (Handler.Pull_Character != null)
                        {
                            Handler.Pull_Character.Region.Y -= character.MoveSpeed;
                            CharacterUtil.UpdateGear(Handler.Pull_Character);
                        }
                        else if (Handler.Pull_Tile != null)
                        {
                            Handler.Pull_Tile.Region.Y -= character.MoveSpeed;
                        }
                    }

                    if (character.Moved == character.Move_TotalDistance)
                    {
                        character.Location.Y--;

                        if (character.Type == "Player")
                        {
                            if (Handler.Pull_Character != null)
                            {
                                Handler.Pull_Character.Location = new Location(Handler.Pull_Character.Destination.X, Handler.Pull_Character.Destination.Y);
                                CharacterUtil.UpdateGear(Handler.Pull_Character);
                            }
                            else if (Handler.Pull_Tile != null)
                            {
                                WorldUtil.PullTile_SetLocation(Direction.North);
                            }
                        }

                        EndTime = new TimeHandler(TimeManager.Now);
                    }
                    else
                    {
                        for (int i = 1; i <= character.Frames; i++)
                        {
                            if (character.Moved == (character.Move_TotalDistance / character.Frames) * i)
                            {
                                character.Animate();
                                CharacterUtil.UpdateGear(character);
                                break;
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    EndTime = new TimeHandler(TimeManager.Now);
                }
            }

            if (character.Type == "Player")
            {
                TimeTracker.Tick(Handler.ActionRate);
            }
        }

        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (character.Unconscious ||
                !character.Moving)
            {
                return;
            }

            Map map = WorldUtil.GetMap();

            if (Name == "Sneak")
            {
                character.Stats.Stamina -= (0.0385f / character.Stats.Endurance);
            }
            else if (Name == "Walk")
            {
                character.Stats.Stamina -= (0.077f / character.Stats.Endurance);
            }
            else if (Name == "Run")
            {
                character.Stats.Stamina -= (0.154f / character.Stats.Endurance);
            }

            if (character.Stats.Stamina < 0)
            {
                character.Stats.Stamina = 0;
            }

            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Tile bottom_tile = bottom_tiles.GetTile(character.Location.ToVector2);
            if (bottom_tile != null)
            {
                character.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);
            }
            CharacterUtil.UpdateGear(character);

            WorldUtil.SetCurrentMap(character);
            CharacterUtil.UpdateSight(character);

            character.Moved = 0;
            character.Moving = false;
            character.ResetAnimation();

            if (character.InCombat)
            {
                character.Path.Clear();
            }
            else if (character.Type != "Player")
            {
                if (WorldUtil.PassedOpenDoor(map.GetLayer("MiddleTiles"), character))
                {
                    Tasker.CloseDoor_Behind(character);
                }
                else if (WorldUtil.PassedOpenWindow(map.GetLayer("MiddleTiles"), character))
                {
                    Tasker.CloseWindow_Behind(character);
                }
            }

            if (character.Path.Count > 0)
            {
                ALocation first_path = character.Path[0];
                if (first_path.X == character.Location.X &&
                    first_path.Y == character.Location.Y)
                {
                    character.Path.Remove(first_path);
                    if (character.Path.Count > 0)
                    {
                        first_path = character.Path[0];
                    }
                }
                Location destination = new Location(first_path.X, first_path.Y, 0);

                bool reached_destination = false;
                if (character.Path.Count > 0)
                {
                    ALocation last_path = character.Path[character.Path.Count - 1];
                    if (last_path.X == character.Location.X &&
                        last_path.Y == character.Location.Y)
                    {
                        reached_destination = true;
                        character.Path.Remove(last_path);
                    }
                }
                else
                {
                    reached_destination = true;
                }

                if (!reached_destination)
                {
                    Direction direction = WorldUtil.GetDirection(character.Location, destination);
                    if (direction == character.Direction)
                    {
                        if (direction == Direction.North)
                        {
                            character.Destination = new Location(character.Location.X, character.Location.Y - 1, character.Location.Z);
                        }
                        else if (direction == Direction.East)
                        {
                            character.Destination = new Location(character.Location.X + 1, character.Location.Y, character.Location.Z);
                        }
                        else if (direction == Direction.South)
                        {
                            character.Destination = new Location(character.Location.X, character.Location.Y + 1, character.Location.Z);
                        }
                        else if (direction == Direction.West)
                        {
                            character.Destination = new Location(character.Location.X - 1, character.Location.Y, character.Location.Z);
                        }

                        character.Job.Tasks.Add(new Move
                        {
                            Name = Name,
                            OwnerID = character.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Location = character.Destination,
                            Direction = direction
                        });
                    }
                    else
                    {
                        character.Job.Tasks.Add(new Turn
                        {
                            Name = "Turn",
                            OwnerID = character.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                            Location = character.Destination,
                            Direction = direction
                        });
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
