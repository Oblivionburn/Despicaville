using System;
using System.Collections.Generic;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Time;
using Despicaville.Util;
using Despicaville.SubTasks;

namespace Despicaville.MajorTasks
{
    public class FindToilet : JobTask
    {
        public override void Action_Start()
        {
            if (Owner_Character?.Location == null)
            {
                return;
            }

            //Find the nearest toilet
            List<Tile> toilets = WorldUtil.GetFurniture_Unused(Handler.MiddleFurniture, "Toilet");
            if (toilets.Count > 0)
            {
                Tile? toilet = WorldUtil.GetClosestTile(toilets, Owner_Character.Location);
                if (toilet?.Location != null)
                {
                    Location = toilet.Location;
                    Direction = toilet.Direction;
                }
            }
        }

        public override void Action()
        {
            if (Location == null ||
                Owner_Character?.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            if (Owner_Character.Stats.Bladder == 0)
            {
                //We don't need to use the toilet anymore
                EndTime = new TimeHandler(TimeManager.Now);
                return;
            }

            if (SubTasks.Count > 0)
            {
                //We're still busy doing stuff
                return;
            }

            if (Owner_Character.Path.Count > 0)
            {
                //We're still busy getting there
                if (!Owner_Character.Moving)
                {
                    ContinuePathing();
                }
                
                return;
            }

            bool nextTo = false;
            bool okay = false;

            if (WorldUtil.NextTo(Location, Owner_Character.Location) &&
                Owner_Character.Gender == "Male")
            {
                nextTo = true;
                okay = true;
            }
            else if (Location.X == Owner_Character.Location.X &&
                     Location.Y == Owner_Character.Location.Y)
            {
                okay = true;
            }

            if (okay)
            {
                //We're at the toilet
                if (nextTo)
                {
                    Direction direction = WorldUtil.GetDirection(Owner_Character.Location, Location);
                    if (direction != Owner_Character.Direction)
                    {
                        SubTasks.Add(new Turn
                        {
                            Name = "Turn",
                            Owner_Character = Owner_Character,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                            Direction = direction
                        });
                        return;
                    }
                }
                else if (Direction != Owner_Character.Direction)
                {
                    SubTasks.Add(new Turn
                    {
                        Name = "Turn",
                        Owner_Character = Owner_Character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                        Direction = Direction
                    });
                    return;
                }

                TimeSpan duration = TimeSpan.FromSeconds(Owner_Character.Stats.Bladder);

                SubTasks.Add(new UseToilet
                {
                    Name = "UseToilet",
                    Owner_Character = Owner_Character,
                    Location = Location,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, duration),
                    TaskBar = CharacterUtil.GenTaskbar(Owner_Character, (int)duration.TotalMilliseconds)
                });
            }
            else
            {
                //Find a path to the toilet
                PathTo();
            }
        }

        private void PathTo()
        {
            if (Location == null ||
                Owner_Character?.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            Map? map = WorldUtil.GetMap();
            Layer? bottom_tiles = map?.GetLayer("BottomTiles");
            Layer? middle_tiles = map?.GetLayer("MiddleTiles");

            if (middle_tiles == null)
            {
                return;
            }

            Tile? middle_tile = middle_tiles.GetTile(Owner_Character.Location.ToVector2);
            if (middle_tile?.Name != null &&
                middle_tile.Name.Contains("Door"))
            {
                if (Owner_Character.Stats.Bladder >= 60)
                {
                    SubTasks.Add(new Move
                    {
                        Name = "Run",
                        Owner_Character = Owner_Character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        Direction = Owner_Character.Direction
                    });
                }
                else
                {
                    SubTasks.Add(new Move
                    {
                        Name = "Walk",
                        Owner_Character = Owner_Character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        Direction = Owner_Character.Direction
                    });
                }
            }
            else if (bottom_tiles != null)
            {
                int? distance = WorldUtil.GetDistance(Owner_Character.Location, Location) * 8;
                List<ALocation> path = Pathing.GetPath(bottom_tiles, middle_tiles, Owner_Character, Location, distance, Owner_Character.Gender == "Male");

                if (path.Count > 0)
                {
                    if (path[0].X == Owner_Character.Location.X &&
                        path[0].Y == Owner_Character.Location.Y)
                    {
                        path.Remove(path[0]);
                    }

                    Owner_Character.Path.AddRange(path);

                    ContinuePathing();
                }
                else
                {
                    Owner_Character.Job.Tasks.Add(new Wander
                    {
                        Name = "Wander",
                        Owner_Character = Owner_Character,
                        Priority = Priority + 1,
                        StartTime = new TimeHandler(TimeManager.Now),
                    });
                }
            }
        }

        private void ContinuePathing()
        {
            if (Owner_Character?.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            ALocation next_path = Owner_Character.Path[0];
            Location location = new(next_path.X, next_path.Y, 0);

            Direction direction = WorldUtil.GetDirection(Owner_Character.Location, location);
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

            if (Owner_Character.Stats.Bladder >= 60)
            {
                SubTasks.Add(new Move
                {
                    Name = "Run",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Location = Owner_Character.Destination,
                    Direction = direction
                });
            }
            else
            {
                SubTasks.Add(new Move
                {
                    Name = "Walk",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Location = Owner_Character.Destination,
                    Direction = direction
                });
            }
        }
    }
}
