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
    public class WorkJob : JobTask
    {
        private Job? work = null;

        public override void Action_Start()
        {
            if (TimeManager.Now == null ||
                Owner_Character == null)
            {
                return;
            }

            int jobCount = Handler.Jobs.Count;
            for (int j = 0; j < jobCount; j++)
            {
                Job job = Handler.Jobs[j];
                if (Owner_Character.Job.ID == job.ID)
                {
                    work = job;
                    break;
                }
            }

            if (work == null)
            {
                return;
            }
        }

        public override void Action()
        {
            if (TimeManager.Now == null ||
                Owner_Character?.Location == null ||
                work == null)
            {
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

            JobTask? workTask = work?.GetTask(TimeManager.Now);
            if (workTask == null)
            {
                //There's no work left to do
                EndTime = new TimeHandler(TimeManager.Now);
                return;
            }

            if (workTask.Location == null)
            {
                return;
            }

            Location = workTask.Location;
            Direction = workTask.Direction;

            //Are we at the task location?
            if (Owner_Character.Location.X == Location.X &&
                Owner_Character.Location.Y == Location.Y)
            {
                //Are we facing the right direction?
                if (Owner_Character.Direction != Direction)
                {
                    SubTasks.Add(new Turn
                    {
                        Name = "Turn",
                        Owner_Character = Owner_Character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                        Direction = Direction
                    });
                }
                else if (workTask != null)
                {
                    if (workTask.StartTime == null ||
                        workTask.EndTime == null)
                    {
                        return;
                    }

                    //We're ready, so do the current task
                    SubTasks.Add(new JobTask
                    {
                        ID = workTask.ID,
                        Name = workTask.Name,
                        Type = workTask.Type,
                        Assignment = workTask.Assignment,
                        StartTime = new TimeHandler((long)workTask.StartTime.Hours, 0, 0, 0),
                        EndTime = new TimeHandler((long)workTask.EndTime.Hours, 0, 0, 0),
                        Location = Location,
                        Direction = Direction
                    });
                }
            }
            else
            {
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
                SubTasks.Add(new Move
                {
                    Name = "Run",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Direction = Owner_Character.Direction
                });
            }
            else if (bottom_tiles != null)
            {
                int? distance = WorldUtil.GetDistance(Owner_Character.Location, Location) * 8;
                List<ALocation> path = Pathing.GetPath(bottom_tiles, middle_tiles, Owner_Character, Location, distance, false);

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

            SubTasks.Add(new Move
            {
                Name = "Run",
                Owner_Character = Owner_Character,
                StartTime = new TimeHandler(TimeManager.Now),
                Location = Owner_Character.Destination,
                Direction = direction
            });
        }
    }
}
