using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;
using OP_Engine.Scenes;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Move : Task
    {
        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (CharacterUtil.HeldByPlayer(character))
            {
                Direction direction = WorldUtil.GetDirection(Handler.GetPlayer().Location, character.Location, false);
                if (direction == Direction.Up)
                {
                    character.Animator.FaceNorth(character);
                }
                else if (direction == Direction.Right)
                {
                    character.Animator.FaceEast(character);
                }
                else if (direction == Direction.Down)
                {
                    character.Animator.FaceSouth(character);
                }
                else if (direction == Direction.Left)
                {
                    character.Animator.FaceWest(character);
                }
                return;
            }

            if (Name == "Sneak")
            {
                character.Speed = 0.5f;
            }
            else if (Name == "Walk")
            {
                character.Speed = 1;
            }
            else if (Name == "Run")
            {
                character.Speed = 2;
            }

            if (Direction == Direction.Up)
            {
                character.Destination = new Location(character.Location.X, character.Location.Y - 1, character.Location.Z);
            }
            else if (Direction == Direction.Right)
            {
                character.Destination = new Location(character.Location.X + 1, character.Location.Y, character.Location.Z);
            }
            else if (Direction == Direction.Down)
            {
                character.Destination = new Location(character.Location.X, character.Location.Y + 1, character.Location.Z);
            }
            else if (Direction == Direction.Left)
            {
                character.Destination = new Location(character.Location.X - 1, character.Location.Y, character.Location.Z);
            }

            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];

            if (WorldUtil.CanMove(character, map, character.Destination))
            {
                if (Direction != character.Direction)
                {
                    EndTime = new TimeHandler(TimeManager.Now);

                    character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        OwnerIDs = new List<long> { character.ID },
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                        Direction = Direction
                    });
                }
                else
                {
                    character.Moving = true;

                    Layer middle_tiles = map.GetLayer("MiddleTiles");
                    Tile tile = middle_tiles.GetTile(new Vector2(character.Destination.X, character.Destination.Y));
                    if (tile != null)
                    {
                        if (tile.Name.Contains("Window") &&
                            tile.Name.Contains("Closed"))
                        {
                            Tasker.BreakWindow(character);
                        }
                    }
                }
            }
            else
            {
                EndTime = new TimeHandler(TimeManager.Now);

                if (Direction != character.Direction)
                {
                    character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        OwnerIDs = new List<long> { character.ID },
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                        Direction = Direction
                    });
                }
                else if (character.Type != "Player")
                {
                    Layer middle_tiles = map.GetLayer("MiddleTiles");
                    Tile tile = middle_tiles.GetTile(new Vector2(character.Destination.X, character.Destination.Y));
                    if (tile != null)
                    {
                        if (tile.Name.Contains("Window") &&
                            tile.Name.Contains("Closed"))
                        {
                            character.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "OpenWindow",
                                OwnerIDs = new List<long> { character.ID },
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                Location = character.Destination,
                                Direction = Direction
                            });

                            character.Job.Tasks.Add(new Move
                            {
                                Name = Name,
                                OwnerIDs = new List<long> { character.ID },
                                StartTime = new TimeHandler(TimeManager.Now),
                                Location = character.Destination,
                                Direction = Direction
                            });

                            Location nextMove = null;

                            if (character.Direction == Direction.Up)
                            {
                                nextMove = new Location(character.Destination.X, character.Destination.Y - 1, character.Destination.Z);
                            }
                            else if (character.Direction == Direction.Right)
                            {
                                nextMove = new Location(character.Destination.X + 1, character.Destination.Y, character.Destination.Z);
                            }
                            else if (character.Direction == Direction.Down)
                            {
                                nextMove = new Location(character.Destination.X, character.Destination.Y + 1, character.Destination.Z);
                            }
                            else if (character.Direction == Direction.Left)
                            {
                                nextMove = new Location(character.Destination.X - 1, character.Destination.Y, character.Destination.Z);
                            }

                            character.Job.Tasks.Add(new Move
                            {
                                Name = Name,
                                OwnerIDs = new List<long> { character.ID },
                                StartTime = new TimeHandler(TimeManager.Now),
                                Location = nextMove,
                                Direction = Direction
                            });
                        }
                        else if (tile.Name.Contains("Door") &&
                                 tile.Name.Contains("Closed"))
                        {
                            character.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "OpenDoor",
                                OwnerIDs = new List<long> { character.ID },
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10)),
                                Location = character.Destination,
                                Direction = Direction
                            });

                            character.Job.Tasks.Add(new Move
                            {
                                Name = Name,
                                OwnerIDs = new List<long> { character.ID },
                                StartTime = new TimeHandler(TimeManager.Now),
                                Location = character.Destination,
                                Direction = Direction
                            });

                            Location nextMove = null;

                            if (character.Direction == Direction.Up)
                            {
                                nextMove = new Location(character.Destination.X, character.Destination.Y - 1, character.Destination.Z);
                            }
                            else if (character.Direction == Direction.Right)
                            {
                                nextMove = new Location(character.Destination.X + 1, character.Destination.Y, character.Destination.Z);
                            }
                            else if (character.Direction == Direction.Down)
                            {
                                nextMove = new Location(character.Destination.X, character.Destination.Y + 1, character.Destination.Z);
                            }
                            else if (character.Direction == Direction.Left)
                            {
                                nextMove = new Location(character.Destination.X - 1, character.Destination.Y, character.Destination.Z);
                            }

                            character.Job.Tasks.Add(new Move
                            {
                                Name = Name,
                                OwnerIDs = new List<long> { character.ID },
                                StartTime = new TimeHandler(TimeManager.Now),
                                Location = nextMove,
                                Direction = Direction
                            });
                        }
                    }
                    else
                    {
                        character.Job.Tasks.Add(new Wait
                        {
                            Name = "Wait",
                            OwnerIDs = new List<long> { character.ID },
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(21))
                        });
                    }
                }
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
                character.Region.X += character.Speed;
                character.Moved += character.Speed;

                if (character.Type == "Player")
                {
                    Something heldThing = WorldUtil.GetHeldThing();
                    if (heldThing != null)
                    {
                        if (heldThing.Location.Y == character.Location.Y)
                        {
                            heldThing.Region.X += character.Speed;
                        }
                        else if (heldThing.Location.Y < character.Location.Y)
                        {
                            heldThing.Region.Y += character.Speed;
                        }
                        else if (heldThing.Location.Y > character.Location.Y)
                        {
                            heldThing.Region.Y -= character.Speed;
                        }

                        if (heldThing.Type == "Citizen")
                        {
                            CharacterUtil.UpdateGear((Character)heldThing);
                        }
                    }
                }

                if (character.Moved == character.Move_TotalDistance)
                {
                    character.Location.X++;

                    if (character.Type == "Player")
                    {
                        Something heldThing = WorldUtil.GetHeldThing();
                        if (heldThing != null)
                        {
                            WorldUtil.MoveHeldThing(character, Direction.East, heldThing.Region.X < character.Region.X);
                        }
                    }

                    EndTime = new TimeHandler(TimeManager.Now);
                }
                else
                {
                    for (int i = 1; i <= character.Animator.Frames; i++)
                    {
                        if (character.Moved == (character.Move_TotalDistance / character.Animator.Frames) * i)
                        {
                            character.Animator.Animate(character);
                            CharacterUtil.UpdateGear(character);
                            break;
                        }
                    }
                }
            }
            else if (character.Destination.X < character.Location.X)
            {
                character.Region.X -= character.Speed;
                character.Moved += character.Speed;

                if (character.Type == "Player")
                {
                    Something heldThing = WorldUtil.GetHeldThing();
                    if (heldThing != null)
                    {
                        if (heldThing.Location.Y == character.Location.Y)
                        {
                            heldThing.Region.X -= character.Speed;
                        }
                        else if (heldThing.Location.Y < character.Location.Y)
                        {
                            heldThing.Region.Y += character.Speed;
                        }
                        else if (heldThing.Location.Y > character.Location.Y)
                        {
                            heldThing.Region.Y -= character.Speed;
                        }

                        if (heldThing.Type == "Citizen")
                        {
                            CharacterUtil.UpdateGear((Character)heldThing);
                        }
                    }
                }

                if (character.Moved == character.Move_TotalDistance)
                {
                    character.Location.X--;

                    if (character.Type == "Player")
                    {
                        Something heldThing = WorldUtil.GetHeldThing();
                        if (heldThing != null)
                        {
                            WorldUtil.MoveHeldThing(character, Direction.West, heldThing.Region.X > character.Region.X);
                        }
                    }

                    EndTime = new TimeHandler(TimeManager.Now);
                }
                else
                {
                    for (int i = 1; i <= character.Animator.Frames; i++)
                    {
                        if (character.Moved == (character.Move_TotalDistance / character.Animator.Frames) * i)
                        {
                            character.Animator.Animate(character);
                            CharacterUtil.UpdateGear(character);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (character.Destination.Y > character.Location.Y)
                {
                    character.Region.Y += character.Speed;
                    character.Moved += character.Speed;

                    if (character.Type == "Player")
                    {
                        Something heldThing = WorldUtil.GetHeldThing();
                        if (heldThing != null)
                        {
                            if (heldThing.Location.X == character.Location.X)
                            {
                                heldThing.Region.Y += character.Speed;
                            }
                            else if (heldThing.Location.X < character.Location.X)
                            {
                                heldThing.Region.X += character.Speed;
                            }
                            else if (heldThing.Location.X > character.Location.X)
                            {
                                heldThing.Region.X -= character.Speed;
                            }

                            if (heldThing.Type == "Citizen")
                            {
                                CharacterUtil.UpdateGear((Character)heldThing);
                            }
                        }
                    }

                    if (character.Moved == character.Move_TotalDistance)
                    {
                        character.Location.Y++;

                        if (character.Type == "Player")
                        {
                            Something heldThing = WorldUtil.GetHeldThing();
                            if (heldThing != null)
                            {
                                WorldUtil.MoveHeldThing(character, Direction.South, heldThing.Region.Y < character.Region.Y);
                            }
                        }

                        EndTime = new TimeHandler(TimeManager.Now);
                    }
                    else
                    {
                        for (int i = 1; i <= character.Animator.Frames; i++)
                        {
                            if (character.Moved == (character.Move_TotalDistance / character.Animator.Frames) * i)
                            {
                                character.Animator.Animate(character);
                                CharacterUtil.UpdateGear(character);
                                break;
                            }
                        }
                    }
                }
                else if (character.Destination.Y < character.Location.Y)
                {
                    character.Region.Y -= character.Speed;
                    character.Moved += character.Speed;

                    if (character.Type == "Player")
                    {
                        Something heldThing = WorldUtil.GetHeldThing();
                        if (heldThing != null)
                        {
                            if (heldThing.Location.X == character.Location.X)
                            {
                                heldThing.Region.Y -= character.Speed;
                            }
                            else if (heldThing.Location.X < character.Location.X)
                            {
                                heldThing.Region.X += character.Speed;
                            }
                            else if (heldThing.Location.X > character.Location.X)
                            {
                                heldThing.Region.X -= character.Speed;
                            }

                            if (heldThing.Type == "Citizen")
                            {
                                CharacterUtil.UpdateGear((Character)heldThing);
                            }
                        }
                    }

                    if (character.Moved == character.Move_TotalDistance)
                    {
                        character.Location.Y--;

                        if (character.Type == "Player")
                        {
                            Something heldThing = WorldUtil.GetHeldThing();
                            if (heldThing != null)
                            {
                                WorldUtil.MoveHeldThing(character, Direction.North, heldThing.Region.Y > character.Region.Y);
                            }
                        }

                        EndTime = new TimeHandler(TimeManager.Now);
                    }
                    else
                    {
                        for (int i = 1; i <= character.Animator.Frames; i++)
                        {
                            if (character.Moved == (character.Move_TotalDistance / character.Animator.Frames) * i)
                            {
                                character.Animator.Animate(character);
                                CharacterUtil.UpdateGear(character);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    EndTime = new TimeHandler(TimeManager.Now);
                }
            }

            character.Animator.Update(character);

            if (character.Type == "Player")
            {
                TimeTracker.Tick(Handler.ActionRate);
                GameUtil.CenterToPlayer_OnFrame();
            }
        }

        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            character.Moved = 0;
            character.Moving = false;
            character.Animator.Reset(character);

            Something stamina = character.GetStat("Stamina");
            Something endurance = character.GetStat("Endurance");

            if (Name == "Sneak")
            {
                stamina.DecreaseValue(0.0385f / endurance.Value);
            }
            else if (Name == "Walk")
            {
                stamina.DecreaseValue(0.077f / endurance.Value);
            }
            else if (Name == "Run")
            {
                stamina.DecreaseValue(0.154f / endurance.Value);
            }

            WorldUtil.SetCurrentMap(character);
            CharacterUtil.UpdateGear(character);

            if (character.Unconscious)
            {
                return;
            }

            CharacterUtil.UpdateSight(character);

            if (character.InCombat)
            {
                character.Path.Clear();
            }
            else
            {
                Scene scene = SceneManager.GetScene("Gameplay");
                Map map = scene.World.Maps[0];

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
                    Direction direction = WorldUtil.GetDirection(destination, character.Location, false);
                    if (direction == character.Direction)
                    {
                        if (direction == Direction.Up)
                        {
                            character.Destination = new Location(character.Location.X, character.Location.Y - 1, character.Location.Z);
                        }
                        else if (direction == Direction.Right)
                        {
                            character.Destination = new Location(character.Location.X + 1, character.Location.Y, character.Location.Z);
                        }
                        else if (direction == Direction.Down)
                        {
                            character.Destination = new Location(character.Location.X, character.Location.Y + 1, character.Location.Z);
                        }
                        else if (direction == Direction.Left)
                        {
                            character.Destination = new Location(character.Location.X - 1, character.Location.Y, character.Location.Z);
                        }

                        character.Job.Tasks.Add(new Move
                        {
                            Name = Name,
                            OwnerIDs = new List<long> { character.ID },
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
                            OwnerIDs = new List<long> { character.ID },
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
