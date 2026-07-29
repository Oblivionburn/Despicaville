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
    public class FindEntertainment : JobTask
    {
        Tile? tv = null;

        public override void Action_Start()
        {
            if (TimeManager.Now == null ||
                Owner_Character?.Location == null)
            {
                return;
            }

            //Is there a TV nearby?
            List<Tile> list = WorldUtil.GetFurniture_Owned(Owner_Character, "TV");
            if (list.Count > 0)
            {
                tv = WorldUtil.GetClosestTile(list, Owner_Character);
            }
        }

        public override void Action()
        {
            if (TimeManager.Now == null ||
                Owner_Character?.Location == null ||
                tv?.Location == null)
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

            //Is the TV on?
            if (tv.IsLightSource)
            {
                //Is there somewhere to sit near the TV?
                List<Tile> comfortSpots = WorldUtil.GetComfortSpots(Owner_Character);
                if (comfortSpots.Count > 0)
                {
                    Tile? nearbySpot = WorldUtil.GetClosestTile(comfortSpots, tv.Location);
                    if (nearbySpot?.Location != null)
                    {
                        if (Owner_Character.Location.X == nearbySpot.Location.X &&
                            Owner_Character.Location.Y == nearbySpot.Location.Y)
                        {
                            Direction direction = WorldUtil.GetDirection(Owner_Character.Location, tv.Location);
                            if (Owner_Character.Direction != direction)
                            {
                                SubTasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    Owner_Character = Owner_Character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                                    Direction = direction
                                });
                            }
                            else
                            {
                                SubTasks.Add(new Wait
                                {
                                    Name = "Wait",
                                    Owner_Character = Owner_Character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMinutes(1))
                                });

                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMinutes(1));
                            }
                        }
                        else
                        {
                            Map? map = WorldUtil.GetMap();
                            Layer? bottom_tiles = map?.GetLayer("BottomTiles");
                            Layer? middle_tiles = map?.GetLayer("MiddleTiles");

                            if (bottom_tiles != null &&
                                middle_tiles != null)
                            {
                                PathTo(nearbySpot.Location, false);
                            }
                        }
                    }
                }
            }
            else
            {
                if (WorldUtil.NextTo(tv.Location, Owner_Character.Location))
                {
                    Direction direction = WorldUtil.GetDirection(Owner_Character.Location, tv.Location);
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
                    }
                    else
                    {
                        SubTasks.Add(new ToggleTV
                        {
                            Name = "ToggleTV",
                            Owner_Character = Owner_Character,
                            Location = tv.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                            TaskBar = CharacterUtil.GenTaskbar(Owner_Character, 1000)
                        });
                    }
                }
                else
                {
                    Map? map = WorldUtil.GetMap();
                    Layer? bottom_tiles = map?.GetLayer("BottomTiles");
                    Layer? middle_tiles = map?.GetLayer("MiddleTiles");

                    if (bottom_tiles != null &&
                        middle_tiles != null)
                    {
                        PathTo(tv.Location, true);
                    }
                }
            }
        }

        private void PathTo(Location target, bool stop_next_to_tile)
        {
            if (Owner_Character?.Location == null ||
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
                    Name = "Walk",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Direction = Owner_Character.Direction
                });
            }
            else if (bottom_tiles != null)
            {
                int? distance = WorldUtil.GetDistance(Owner_Character.Location, target) * 8;
                List<ALocation> path = Pathing.GetPath(bottom_tiles, middle_tiles, Owner_Character, target, distance, stop_next_to_tile);

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
                Name = "Walk",
                Owner_Character = Owner_Character,
                StartTime = new TimeHandler(TimeManager.Now),
                Location = Owner_Character.Destination,
                Direction = direction
            });
        }
    }
}
