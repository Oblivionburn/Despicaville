using System;
using System.Collections.Generic;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Inventories;
using Despicaville.Util;
using Despicaville.SubTasks;

namespace Despicaville.MajorTasks
{
    public class FindFood : JobTask
    {
        Tile? fridge = null;

        public override void Action_Start()
        {
            if (TimeManager.Now == null ||
                Owner_Character?.Location == null)
            {
                return;
            }

            List<Tile> fridges = WorldUtil.GetFurniture_Owned(Owner_Character, "Fridge");
            if (fridges.Count > 0)
            {
                fridge = WorldUtil.GetClosestTile(fridges, Owner_Character.Location);
            }
        }

        public override void Action()
        {
            if (Owner_Character?.Location == null ||
                TimeManager.Now == null ||
                fridge?.Location == null)
            {
                return;
            }

            if (SubTasks.Count > 0)
            {
                //We're still busy doing stuff
                return;
            }

            if (HasFood())
            {
                EndTime = new TimeHandler(TimeManager.Now);
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

            Item? item = null;

            if (fridge.Inventory != null)
            {
                int fridgeCount = fridge.Inventory.Items.Count;
                for (int i = 0; i < fridgeCount; i++)
                {
                    Item existing = fridge.Inventory.Items[i];

                    Property? hunger = existing.GetProperty("Hunger");
                    if (hunger != null)
                    {
                        item = existing;
                        break;
                    }
                }
            }

            if (item != null)
            {
                if (WorldUtil.NextTo(fridge.Location, Owner_Character.Location))
                {
                    Direction direction = WorldUtil.GetDirection(Owner_Character.Location, fridge.Location);
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
                        if (fridge.Texture != null &&
                            fridge.Texture.Name.Contains("Used"))
                        {
                            InventoryUtil.TransferItem(fridge.Inventory, Owner_Character.Inventory, item);

                            int seconds = Owner_Character.Stats.Hunger >= 60 ? 10 : 20;
                            int milliseconds = seconds * 1000;

                            SubTasks.Add(new Search
                            {
                                Name = "Search",
                                Owner_Character = Owner_Character,
                                Location = fridge.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                TaskBar = CharacterUtil.GenTaskbar(Owner_Character, milliseconds)
                            });

                            seconds = Owner_Character.Stats.Hunger >= 60 ? 1 : 2;

                            SubTasks.Add(new CloseFridge
                            {
                                Name = "CloseFridge",
                                Owner_Character = Owner_Character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                Location = fridge.Location,
                                Direction = direction
                            });
                        }
                        else
                        {
                            int seconds = Owner_Character.Stats.Hunger >= 60 ? 1 : 2;

                            SubTasks.Add(new OpenFridge
                            {
                                Name = "OpenFridge",
                                Owner_Character = Owner_Character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                Location = fridge.Location,
                                Direction = direction
                            });
                        }
                    }
                }
                else
                {
                    PathTo();
                }
            }
        }

        private bool HasFood()
        {
            if (Owner_Character == null)
            {
                return false;
            }

            Inventory inventory = Owner_Character.Inventory;

            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item existing = inventory.Items[i];

                Property? hunger = existing.GetProperty("Hunger");
                if (hunger != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void PathTo()
        {
            if (fridge?.Location == null ||
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
                if (Owner_Character.Stats.Hunger >= 60)
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
                int? distance = WorldUtil.GetDistance(Owner_Character.Location, fridge.Location) * 8;
                List<ALocation> path = Pathing.GetPath(bottom_tiles, middle_tiles, Owner_Character, fridge.Location, distance, true);

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

            if (Owner_Character.Stats.Hunger >= 60)
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
