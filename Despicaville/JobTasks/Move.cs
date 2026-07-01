using System;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Move : JobTask
    {
        public override void Action_Start()
        {
            if (Handler.Player?.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            if (Owner_Character?.Location == null)
            {
                return;
            }

            if (CharacterUtil.PulledByPlayer(Owner_Character))
            {
                Direction direction = WorldUtil.GetDirection(Owner_Character.Location, Handler.Player.Location);
                if (direction == Direction.North)
                {
                    Owner_Character.FaceNorth();
                }
                else if (direction == Direction.East)
                {
                    Owner_Character.FaceEast();
                }
                else if (direction == Direction.South)
                {
                    Owner_Character.FaceSouth();
                }
                else if (direction == Direction.West)
                {
                    Owner_Character.FaceWest();
                }
                return;
            }

            if (Name == "Sneak")
            {
                Owner_Character.MoveSpeed = 0.5f;
            }
            else if (Name == "Walk")
            {
                Owner_Character.MoveSpeed = 1;
            }
            else if (Name == "Run")
            {
                Owner_Character.MoveSpeed = 2;
            }

            if (Direction == Direction.North)
            {
                Owner_Character.Destination = new Location(Owner_Character.Location.X, Owner_Character.Location.Y - 1, Owner_Character.Location.Z);
            }
            else if (Direction == Direction.East)
            {
                Owner_Character.Destination = new Location(Owner_Character.Location.X + 1, Owner_Character.Location.Y, Owner_Character.Location.Z);
            }
            else if (Direction == Direction.South)
            {
                Owner_Character.Destination = new Location(Owner_Character.Location.X, Owner_Character.Location.Y + 1, Owner_Character.Location.Z);
            }
            else if (Direction == Direction.West)
            {
                Owner_Character.Destination = new Location(Owner_Character.Location.X - 1, Owner_Character.Location.Y, Owner_Character.Location.Z);
            }

            Map? map = WorldUtil.GetMap();
            if (map == null ||
                Owner_Character.Destination == null)
            {
                return;
            }

            if (WorldUtil.CanMove(Owner_Character, map, Owner_Character.Destination))
            {
                #region Move

                if (Direction != Owner_Character.Direction)
                {
                    EndTime = new TimeHandler(TimeManager.Now);

                    Owner_Character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        Owner_Character = Owner_Character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                        Direction = Direction
                    });
                }
                else
                {
                    Owner_Character.Moving = true;

                    if (Owner_Character.Type == "Player" &&
                        Handler.Pull_Character != null)
                    {
                        if (!Handler.Pull_Character.Unconscious &&
                            !Handler.Pull_Character.Dead)
                        {
                            float break_chance = (100 - Owner_Character.Stats.Strength) / 10;
                            if (Utility.RandomPercent(break_chance))
                            {
                                Handler.ResetPull();
                            }
                        }

                        if (Handler.Pull_Character.Location != null)
                        {
                            if (Owner_Character.Destination.X > Owner_Character.Location.X)
                            {
                                Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X + 1, Handler.Pull_Character.Location.Y, 0);
                            }
                            else if (Owner_Character.Destination.X < Owner_Character.Location.X)
                            {
                                Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X - 1, Handler.Pull_Character.Location.Y, 0);
                            }
                            else
                            {
                                if (Owner_Character.Destination.Y > Owner_Character.Location.Y)
                                {
                                    Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X, Handler.Pull_Character.Location.Y + 1, 0);
                                }
                                else if (Owner_Character.Destination.Y < Owner_Character.Location.Y)
                                {
                                    Handler.Pull_Character.Destination = new Location(Handler.Pull_Character.Location.X, Handler.Pull_Character.Location.Y - 1, 0);
                                }
                            }
                        }
                    }

                    Layer? middle_tiles = map?.GetLayer("MiddleTiles");
                    Tile? tile = middle_tiles?.GetTile(Owner_Character.Destination.ToVector2);
                    if (tile != null)
                    {
                        if (tile.Name != null &&
                            tile.Name.Contains("Window") &&
                            tile.Name.Contains("Closed"))
                        {
                            Tasker.BreakWindow(Owner_Character.Destination.ToVector2, Owner_Character.Direction);
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region Do Something Else

                EndTime = new TimeHandler(TimeManager.Now);

                if (Direction != Owner_Character.Direction)
                {
                    Owner_Character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        Owner_Character = Owner_Character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                        Direction = Direction
                    });
                }
                else if (Owner_Character.Type != "Player")
                {
                    Layer? middle_tiles = map.GetLayer("MiddleTiles");
                    Tile? tile = middle_tiles?.GetTile(Owner_Character.Destination.ToVector2);
                    if (tile != null &&
                        !string.IsNullOrEmpty(tile.Name))
                    {
                        if (tile.Name.Contains("Window") &&
                            tile.Name.Contains("Closed"))
                        {
                            Owner_Character.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "OpenWindow",
                                Owner_Character = Owner_Character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                Location = Owner_Character.Destination,
                                Direction = Direction
                            });
                        }
                        else if (tile.Name.Contains("Door") &&
                                 tile.Name.Contains("Closed"))
                        {
                            Owner_Character.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "OpenDoor",
                                Owner_Character = Owner_Character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                Location = Owner_Character.Destination,
                                Direction = Direction
                            });
                        }
                    }
                    else
                    {
                        Owner_Character.Path.Clear();
                        Tasker.Wander(Owner_Character);
                    }
                }

                #endregion
            }
        }

        public override void Action()
        {
            if (Main.Game == null ||
                TimeManager.Now == null)
            {
                return;
            }

            if (Owner_Character?.Location == null ||
                Owner_Character.Destination == null)
            {
                return;
            }

            if (Owner_Character.Type != "Player" &&
                !Handler.Action)
            {
                return;
            }

            if (!Owner_Character.Moving ||
                Owner_Character.Region == null)
            {
                return;
            }

            Owner_Character.Move_TotalDistance = Main.Game.TileSize.X;

            if (Owner_Character.Destination.X > Owner_Character.Location.X)
            {
                #region East

                Owner_Character.Region.X += Owner_Character.MoveSpeed;
                Owner_Character.Moved += Owner_Character.MoveSpeed;

                if (Owner_Character.Type == "Player")
                {
                    if (Handler.Pull_Character?.Region != null)
                    {
                        Handler.Pull_Character.Region.X += Owner_Character.MoveSpeed;
                        CharacterUtil.UpdateGear(Handler.Pull_Character);
                    }
                    else if (Handler.Pull_Tile?.Region != null)
                    {
                        Handler.Pull_Tile.Region.X += Owner_Character.MoveSpeed;
                    }
                }

                if (Owner_Character.Moved == Owner_Character.Move_TotalDistance)
                {
                    Owner_Character.Location.X++;

                    if (Owner_Character.Type == "Player")
                    {
                        if (Handler.Pull_Character?.Destination != null)
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
                    for (int i = 1; i <= Owner_Character.Frames; i++)
                    {
                        if (Owner_Character.Moved == (Owner_Character.Move_TotalDistance / Owner_Character.Frames) * i)
                        {
                            Owner_Character.Animate();
                            CharacterUtil.UpdateGear(Owner_Character);
                            break;
                        }
                    }
                }

                #endregion
            }
            else if (Owner_Character.Destination.X < Owner_Character.Location.X)
            {
                #region West

                Owner_Character.Region.X -= Owner_Character.MoveSpeed;
                Owner_Character.Moved += Owner_Character.MoveSpeed;

                if (Owner_Character.Type == "Player")
                {
                    if (Handler.Pull_Character?.Region != null)
                    {
                        Handler.Pull_Character.Region.X -= Owner_Character.MoveSpeed;
                        CharacterUtil.UpdateGear(Handler.Pull_Character);
                    }
                    else if (Handler.Pull_Tile?.Region != null)
                    {
                        Handler.Pull_Tile.Region.X -= Owner_Character.MoveSpeed;
                    }
                }

                if (Owner_Character.Moved == Owner_Character.Move_TotalDistance)
                {
                    Owner_Character.Location.X--;

                    if (Owner_Character.Type == "Player")
                    {
                        if (Handler.Pull_Character?.Destination != null)
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
                    for (int i = 1; i <= Owner_Character.Frames; i++)
                    {
                        if (Owner_Character.Moved == (Owner_Character.Move_TotalDistance / Owner_Character.Frames) * i)
                        {
                            Owner_Character.Animate();
                            CharacterUtil.UpdateGear(Owner_Character);
                            break;
                        }
                    }
                }

                #endregion
            }
            else
            {
                if (Owner_Character.Destination.Y > Owner_Character.Location.Y)
                {
                    #region South

                    Owner_Character.Region.Y += Owner_Character.MoveSpeed;
                    Owner_Character.Moved += Owner_Character.MoveSpeed;

                    if (Owner_Character.Type == "Player")
                    {
                        if (Handler.Pull_Character?.Region != null)
                        {
                            Handler.Pull_Character.Region.Y += Owner_Character.MoveSpeed;
                            CharacterUtil.UpdateGear(Handler.Pull_Character);
                        }
                        else if (Handler.Pull_Tile?.Region != null)
                        {
                            Handler.Pull_Tile.Region.Y += Owner_Character.MoveSpeed;
                        }
                    }

                    if (Owner_Character.Moved == Owner_Character.Move_TotalDistance)
                    {
                        Owner_Character.Location.Y++;

                        if (Owner_Character.Type == "Player")
                        {
                            if (Handler.Pull_Character?.Destination != null)
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
                        for (int i = 1; i <= Owner_Character.Frames; i++)
                        {
                            if (Owner_Character.Moved == (Owner_Character.Move_TotalDistance / Owner_Character.Frames) * i)
                            {
                                Owner_Character.Animate();
                                CharacterUtil.UpdateGear(Owner_Character);
                                break;
                            }
                        }
                    }

                    #endregion
                }
                else if (Owner_Character.Destination.Y < Owner_Character.Location.Y)
                {
                    #region North

                    Owner_Character.Region.Y -= Owner_Character.MoveSpeed;
                    Owner_Character.Moved += Owner_Character.MoveSpeed;

                    if (Owner_Character.Type == "Player")
                    {
                        if (Handler.Pull_Character?.Region != null)
                        {
                            Handler.Pull_Character.Region.Y -= Owner_Character.MoveSpeed;
                            CharacterUtil.UpdateGear(Handler.Pull_Character);
                        }
                        else if (Handler.Pull_Tile?.Region != null)
                        {
                            Handler.Pull_Tile.Region.Y -= Owner_Character.MoveSpeed;
                        }
                    }

                    if (Owner_Character.Moved == Owner_Character.Move_TotalDistance)
                    {
                        Owner_Character.Location.Y--;

                        if (Owner_Character.Type == "Player")
                        {
                            if (Handler.Pull_Character?.Destination != null)
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
                        for (int i = 1; i <= Owner_Character.Frames; i++)
                        {
                            if (Owner_Character.Moved == (Owner_Character.Move_TotalDistance / Owner_Character.Frames) * i)
                            {
                                Owner_Character.Animate();
                                CharacterUtil.UpdateGear(Owner_Character);
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

            if (Owner_Character.Type == "Player")
            {
                TimeTracker.Tick(Handler.ActionRate);
            }
        }

        public override void Action_End()
        {
            if (Owner_Character?.Location == null)
            {
                return;
            }

            if (Owner_Character.Unconscious ||
                !Owner_Character.Moving)
            {
                return;
            }

            Map? map = WorldUtil.GetMap();

            if (Name == "Sneak")
            {
                Owner_Character.Stats.Stamina -= (0.0385f / Owner_Character.Stats.Endurance);
            }
            else if (Name == "Walk")
            {
                Owner_Character.Stats.Stamina -= (0.077f / Owner_Character.Stats.Endurance);
            }
            else if (Name == "Run")
            {
                Owner_Character.Stats.Stamina -= (0.154f / Owner_Character.Stats.Endurance);
            }

            if (Owner_Character.Stats.Stamina < 0)
            {
                Owner_Character.Stats.Stamina = 0;
            }

            Layer? bottom_tiles = map?.GetLayer("BottomTiles");
            Tile? bottom_tile = bottom_tiles?.GetTile(Owner_Character.Location.ToVector2);
            if (bottom_tile?.Region != null)
            {
                Owner_Character.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);
            }
            
            WorldUtil.SetCurrentMap(Owner_Character);
            CharacterUtil.UpdateSight(Owner_Character);

            Owner_Character.Moved = 0;
            Owner_Character.Moving = false;
            Owner_Character.ResetAnimation();

            CharacterUtil.UpdateGear(Owner_Character);

            if (Owner_Character.InCombat)
            {
                Owner_Character.Path.Clear();
            }
            else if (Owner_Character.Type != "Player")
            {
                Layer? middle_tiles = map?.GetLayer("MiddleTiles");
                if (middle_tiles != null)
                {
                    if (WorldUtil.PassedOpenDoor(middle_tiles, Owner_Character))
                    {
                        Tasker.CloseDoor_Behind(Owner_Character);
                    }
                    else if (WorldUtil.PassedOpenWindow(middle_tiles, Owner_Character))
                    {
                        Tasker.CloseWindow_Behind(Owner_Character);
                    }
                }
            }

            if (Owner_Character.Path.Count > 0)
            {
                ALocation first_path = Owner_Character.Path[0];
                if (first_path.X == Owner_Character.Location.X &&
                    first_path.Y == Owner_Character.Location.Y)
                {
                    Owner_Character.Path.Remove(first_path);
                    if (Owner_Character.Path.Count > 0)
                    {
                        first_path = Owner_Character.Path[0];
                    }
                }
                Location destination = new(first_path.X, first_path.Y, 0);

                bool reached_destination = false;
                if (Owner_Character.Path.Count > 0)
                {
                    ALocation last_path = Owner_Character.Path[Owner_Character.Path.Count - 1];
                    if (last_path.X == Owner_Character.Location.X &&
                        last_path.Y == Owner_Character.Location.Y)
                    {
                        reached_destination = true;
                        Owner_Character.Path.Remove(last_path);
                    }
                }
                else
                {
                    reached_destination = true;
                }

                if (!reached_destination &&
                    TimeManager.Now != null)
                {
                    Direction direction = WorldUtil.GetDirection(Owner_Character.Location, destination);
                    if (direction == Owner_Character.Direction)
                    {
                        if (direction == Direction.North)
                        {
                            Owner_Character.Destination = new Location(Owner_Character.Location.X, Owner_Character.Location.Y - 1, Owner_Character.Location.Z);
                        }
                        else if (direction == Direction.East)
                        {
                            Owner_Character.Destination = new Location(Owner_Character.Location.X + 1, Owner_Character.Location.Y, Owner_Character.Location.Z);
                        }
                        else if (direction == Direction.South)
                        {
                            Owner_Character.Destination = new Location(Owner_Character.Location.X, Owner_Character.Location.Y + 1, Owner_Character.Location.Z);
                        }
                        else if (direction == Direction.West)
                        {
                            Owner_Character.Destination = new Location(Owner_Character.Location.X - 1, Owner_Character.Location.Y, Owner_Character.Location.Z);
                        }

                        Owner_Character.Job.Tasks.Add(new Move
                        {
                            Name = Name,
                            Owner_Character = Owner_Character,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Location = Owner_Character.Destination,
                            Direction = direction
                        });
                    }
                    else
                    {
                        Owner_Character.Job.Tasks.Add(new Turn
                        {
                            Name = "Turn",
                            Owner_Character = Owner_Character,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                            Location = Owner_Character.Destination,
                            Direction = direction
                        });
                    }
                }
            }
        }
    }
}
