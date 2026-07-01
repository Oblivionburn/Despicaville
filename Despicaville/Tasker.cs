using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Inventories;
using OP_Engine.Inputs;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using Despicaville.Util;
using Despicaville.JobTasks;

namespace Despicaville
{
    public class Tasker : GameComponent
    {
        #region Variables



        #endregion

        #region Constructor

        public Tasker(Game game) : base(game)
        {

        }

        #endregion

        #region Methods

        public static void GetTask(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            if (character.InCombat)
            {
                Attacking(character);
                return;
            }

            if (character.Stats.Bladder >= 60)
            {
                FindToilet(character, true);
                return;
            }

            Appointment? appointment = character.Job.GetAppointment(TimeManager.Now);
            if (appointment?.Name == null)
            {
                return;
            }

            if (appointment.Name.Contains("Sleep"))
            {
                FindBed(character, appointment);
                return;
            }
            else if (appointment.Name.Contains("Work") &&
                     character.Job.Name != null)
            {
                if ((appointment.Name.Contains("1st-Shift") &&
                    character.Job.Name.Contains("1st-Shift")) ||
                    (appointment.Name.Contains("2nd-Shift") &&
                    character.Job.Name.Contains("2nd-Shift")))
                {
                    WorkJob(character);
                    return;
                }
            }
            else if (appointment.Name.Contains("FreeTime"))
            {
                if (character.Stats.Thirst >= 60)
                {
                    bool hasDrink = HasDrink(character);
                    if (hasDrink)
                    {
                        bool foundComfort = FindComfort(character, true);
                        if (foundComfort)
                        {
                            ConsumeDrink(character);
                        }
                    }
                    else
                    {
                        bool foundDrink = FindDrink(character, true);
                        if (!foundDrink)
                        {
                            FindSink(character, true);
                        }
                    }
                    return;
                }
                else if (character.Stats.Hunger >= 60)
                {
                    bool hasFood = HasFood(character);
                    if (hasFood)
                    {
                        bool foundComfort = FindComfort(character, true);
                        if (foundComfort)
                        {
                            ConsumeFood(character);
                        }
                    }
                    else
                    {
                        FindFood(character, true);
                    }
                    return;
                }

                if (character.Stats.Bladder >= 30)
                {
                    FindToilet(character, false);
                    return;
                }
                else if (character.Stats.Thirst >= 30)
                {
                    bool hasDrink = HasDrink(character);
                    if (hasDrink)
                    {
                        bool foundComfort = FindComfort(character, false);
                        if (foundComfort)
                        {
                            ConsumeDrink(character);
                        }
                    }
                    else
                    {
                        bool foundDrink = FindDrink(character, false);
                        if (!foundDrink)
                        {
                            FindSink(character, false);
                        }
                    }
                    return;
                }
                else if (character.Stats.Hunger >= 30)
                {
                    bool hasFood = HasFood(character);
                    if (hasFood)
                    {
                        bool foundComfort = FindComfort(character, false);
                        if (foundComfort)
                        {
                            ConsumeFood(character);
                        }
                    }
                    else
                    {
                        FindFood(character, false);
                    }
                    return;
                }

                FindEntertainment(character);
                return;
            }

            FindComfort(character, false);
            return;
        }

        private static void Attacking(Character character)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return;
            }

            Character? target = CharacterUtil.GetCharacter_Target(character);
            if (target?.Location != null)
            {
                Direction direction = WorldUtil.GetDirection(character.Location, target.Location);

                if (direction != character.Direction)
                {
                    character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        Owner_Character = character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                        Direction = direction
                    });
                }
                else if (!WorldUtil.InRange(target.Location, character.Location, 1))
                {
                    character.Job.Tasks.Add(new Move
                    {
                        Name = "Walk",
                        Owner_Character = character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        Direction = character.Direction
                    });
                }
                else if (character.Location != null)
                {
                    Location location = new();
                    if (character.Direction == Direction.North)
                    {
                        location = new Location(character.Location.X, character.Location.Y - 1, 1);
                    }
                    else if (character.Direction == Direction.East)
                    {
                        location = new Location(character.Location.X + 1, character.Location.Y, 1);
                    }
                    else if (character.Direction == Direction.South)
                    {
                        location = new Location(character.Location.X, character.Location.Y + 1, 1);
                    }
                    else if (character.Direction == Direction.West)
                    {
                        location = new Location(character.Location.X - 1, character.Location.Y, 1);
                    }

                    Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(character);
                    string action = AttackingWith.ElementAt(0).Value;

                    int attackTime = CombatUtil.AttackTime(character, action);
                    character.Job.Tasks.Add(new Attack
                    {
                        Name = "Attack",
                        Owner_Character = character,
                        Location = location,
                        Direction = character.Direction,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(attackTime)),
                        TaskBar = CharacterUtil.GenTaskbar(character, attackTime)
                    });
                }
            }
            else
            {
                AbortTask(character);
            }
        }

        private static bool FindDrink(Character character, bool desperate)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return false;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return false;
            }

            //Does a nearby fridge have something to drink?
            List<Tile> fridges = WorldUtil.GetFurniture_Owned(character, "Fridge");
            if (fridges.Count > 0)
            {
                Tile? fridge = WorldUtil.GetClosestTile(fridges, character);
                if (fridge?.Location != null)
                {
                    Item? item = null;

                    if (fridge.Inventory != null)
                    {
                        int fridgeCount = fridge.Inventory.Items.Count;
                        for (int i = 0; i < fridgeCount; i++)
                        {
                            Item existing = fridge.Inventory.Items[i];

                            Property? thirst = existing.GetProperty("Thirst");
                            if (thirst != null)
                            {
                                item = existing;
                                break;
                            }
                        }
                    }

                    if (item != null)
                    {
                        if (WorldUtil.NextTo(fridge.Location, character.Location))
                        {
                            Direction direction = WorldUtil.GetDirection(character.Location, fridge.Location);
                            if (direction != character.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    Owner_Character = character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = direction
                                });
                                return false;
                            }
                            else
                            {
                                if (fridge.Texture != null &&
                                    fridge.Texture.Name.Contains("Used"))
                                {
                                    InventoryUtil.TransferItem(fridge.Inventory, character.Inventory, item);

                                    int seconds = desperate ? 10 : 20;
                                    int milliseconds = seconds * 1000;

                                    character.Job.Tasks.Add(new Search
                                    {
                                        Name = "Search",
                                        Owner_Character = character,
                                        Location = fridge.Location,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                        TaskBar = CharacterUtil.GenTaskbar(character, milliseconds)
                                    });

                                    seconds = desperate ? 1 : 2;

                                    character.Job.Tasks.Add(new CloseFridge
                                    {
                                        Name = "CloseFridge",
                                        Owner_Character = character,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });

                                    return true;
                                }
                                else
                                {
                                    int seconds = desperate ? 1 : 2;

                                    character.Job.Tasks.Add(new OpenFridge
                                    {
                                        Name = "OpenFridge",
                                        Owner_Character = character,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });

                                    return false;
                                }
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
                                PathTo(bottom_tiles, middle_tiles, fridge.Location, character, desperate, true);
                                return false;
                            }
                        }
                    }
                }
            }

            Wander(character);
            return false;
        }

        private static void ConsumeDrink(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            Inventory inventory = character.Inventory;

            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item existing = inventory.Items[i];

                Property? thirst = existing.GetProperty("Thirst");
                if (thirst != null)
                {
                    TimeSpan duration = TimeSpan.FromMilliseconds(thirst.Value * -10 * 1000);

                    character.Job.Tasks.Add(new UseItem
                    {
                        Name = "UseItem_" + existing.ID,
                        Owner_Character = character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, duration),
                        TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                    });

                    return;
                }
            }
        }

        private static bool HasDrink(Character character)
        {
            if (TimeManager.Now == null)
            {
                return false;
            }

            Inventory inventory = character.Inventory;

            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item existing = inventory.Items[i];

                Property? thirst = existing.GetProperty("Thirst");
                if (thirst != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void FindSink(Character character, bool desperate)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return;
            }

            List<Tile> sinks = WorldUtil.GetFurniture_Owned(character, "Sink");
            if (sinks.Count > 0)
            {
                Tile? sink = WorldUtil.GetClosestTile(sinks, character);
                if (sink?.Location != null)
                {
                    if (WorldUtil.NextTo(sink.Location, character.Location))
                    {
                        Direction direction = WorldUtil.GetDirection(character.Location, sink.Location);
                        if (direction != character.Direction)
                        {
                            character.Job.Tasks.Add(new Turn
                            {
                                Name = "Turn",
                                Owner_Character = character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                Direction = direction
                            });
                            return;
                        }
                        else if (sink.Texture != null &&
                                 !sink.Texture.Name.Contains("Used"))
                        {
                            TimeSpan duration = TimeSpan.FromMilliseconds(character.Stats.Thirst * 1000);

                            character.Job.Tasks.Add(new UseSink
                            {
                                Name = "UseSink",
                                Owner_Character = character,
                                Location = sink.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, duration),
                                TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                            });
                            return;
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
                            PathTo(bottom_tiles, middle_tiles, sink.Location, character, desperate, true);
                            return;
                        }
                    }
                }
            }

            Wander(character);
        }

        private static void FindFood(Character character, bool desperate)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return;
            }

            //Does a nearby fridge have something to eat?
            List<Tile> fridges = WorldUtil.GetFurniture_Owned(character, "Fridge");
            if (fridges.Count > 0)
            {
                Tile? fridge = WorldUtil.GetClosestTile(fridges, character);
                if (fridge?.Location != null)
                {
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
                        if (WorldUtil.NextTo(fridge.Location, character.Location))
                        {
                            Direction direction = WorldUtil.GetDirection(character.Location, fridge.Location);
                            if (direction != character.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    Owner_Character = character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = direction
                                });
                                return;
                            }
                            else
                            {
                                if (fridge.Texture != null &&
                                    fridge.Texture.Name.Contains("Used"))
                                {
                                    InventoryUtil.TransferItem(fridge.Inventory, character.Inventory, item);

                                    int seconds = desperate ? 10 : 20;
                                    int milliseconds = seconds * 1000;

                                    character.Job.Tasks.Add(new Search
                                    {
                                        Name = "Search",
                                        Owner_Character = character,
                                        Location = fridge.Location,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                        TaskBar = CharacterUtil.GenTaskbar(character, milliseconds)
                                    });

                                    seconds = desperate ? 1 : 2;

                                    character.Job.Tasks.Add(new CloseFridge
                                    {
                                        Name = "CloseFridge",
                                        Owner_Character = character,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });

                                    return;
                                }
                                else
                                {
                                    int seconds = desperate ? 1 : 2;

                                    character.Job.Tasks.Add(new OpenFridge
                                    {
                                        Name = "OpenFridge",
                                        Owner_Character = character,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(seconds)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });

                                    return;
                                }
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
                                PathTo(bottom_tiles, middle_tiles, fridge.Location, character, desperate, true);
                                return;
                            }
                        }
                    }
                }
            }

            Wander(character);
        }

        private static bool HasFood(Character character)
        {
            if (TimeManager.Now == null)
            {
                return false;
            }

            Inventory inventory = character.Inventory;

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

        private static void ConsumeFood(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            Inventory inventory = character.Inventory;

            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item existing = inventory.Items[i];

                Property? hunger = existing.GetProperty("Hunger");
                if (hunger != null)
                {
                    TimeSpan duration = TimeSpan.FromMilliseconds(hunger.Value * -10 * 1000);

                    character.Job.Tasks.Add(new UseItem
                    {
                        Name = "UseItem_" + existing.ID,
                        Owner_Character = character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, duration),
                        TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                    });
                }
            }
        }

        private static void FindToilet(Character character, bool desperate)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return;
            }

            //Where is the nearest toilet?
            List<Tile> toilets = WorldUtil.GetFurniture_Unused(Handler.MiddleFurniture, "Toilet");
            if (toilets.Count > 0)
            {
                Tile? toilet = WorldUtil.GetClosestTile(toilets, character);
                if (toilet?.Location != null)
                {
                    bool nextTo = false;
                    bool okay = false;

                    if (WorldUtil.NextTo(toilet.Location, character.Location) &&
                        character.Gender == "Male")
                    {
                        nextTo = true;
                        okay = true;
                    }
                    else if (toilet.Location.X == character.Location.X &&
                             toilet.Location.Y == character.Location.Y)
                    {
                        okay = true;
                    }

                    if (okay)
                    {
                        if (nextTo)
                        {
                            Direction direction = WorldUtil.GetDirection(character.Location, toilet.Location);
                            if (direction != character.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    Owner_Character = character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = direction
                                });
                                return;
                            }
                        }
                        else if (toilet.Direction != character.Direction)
                        {
                            character.Job.Tasks.Add(new Turn
                            {
                                Name = "Turn",
                                Owner_Character = character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                Direction = toilet.Direction
                            });
                            return;
                        }

                        TimeSpan duration = TimeSpan.FromSeconds(character.Stats.Bladder);

                        character.Job.Tasks.Add(new UseToilet
                        {
                            Name = "UseToilet",
                            Owner_Character = character,
                            Location = toilet.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                        });
                        return;
                    }
                    else
                    {
                        Map? map = WorldUtil.GetMap();
                        Layer? bottom_tiles = map?.GetLayer("BottomTiles");
                        Layer? middle_tiles = map?.GetLayer("MiddleTiles");

                        if (bottom_tiles != null &&
                            middle_tiles != null)
                        {
                            PathTo(bottom_tiles, middle_tiles, toilet.Location, character, desperate, character.Gender == "Male");
                            return;
                        }
                    }
                }
            }

            Wander(character);
        }

        private static void FindBed(Character character, Appointment appointment)
        {
            if (TimeManager.Now == null ||
                appointment.StartTime == null ||
                appointment.EndTime == null ||
                character.Location == null)
            {
                return;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, true);
                return;
            }

            //Where is our bed?
            Handler.OwnedFurniture.TryGetValue(character.ID, out List<Tile>? list);
            if (list?.Count > 0)
            {
                Tile? bed = null;

                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile furniture = list[i];
                    if (furniture.Name != null &&
                        furniture.Name.Contains("Bed"))
                    {
                        bed = furniture;
                        break;
                    }
                }

                if (bed?.Location != null)
                {
                    Location? sleepSpot;

                    if (bed.Direction == Direction.North)
                    {
                        sleepSpot = new Location(bed.Location.X, bed.Location.Y + 1);
                    }
                    else if (bed.Direction == Direction.West)
                    {
                        sleepSpot = new Location(bed.Location.X + 1, bed.Location.Y);
                    }
                    else
                    {
                        sleepSpot = new Location(bed.Location.X, bed.Location.Y);
                    }

                    if (sleepSpot != null)
                    {
                        if (character.Location.X == sleepSpot.X &&
                            character.Location.Y == sleepSpot.Y)
                        {
                            if (character.Direction != bed.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    Owner_Character = character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = bed.Direction
                                });
                                return;
                            }
                            else
                            {
                                int days_Now = (int)TimeManager.Now.TotalDays;
                                int hours_Now = TimeManager.Now.Hours;
                                int minutes_Now = TimeManager.Now.Minutes;
                                int seconds_Now = TimeManager.Now.Seconds;
                                int milliseconds_Now = TimeManager.Now.Milliseconds;

                                int days_EndTime = 0;
                                if (TimeManager.Now.Hours >= appointment.StartTime.Hours)
                                {
                                    days_EndTime = (int)TimeManager.Now.TotalDays + 1;
                                }
                                else
                                {
                                    days_EndTime = (int)TimeManager.Now.TotalDays;
                                }

                                int hours_EndTime = appointment.EndTime.Hours;
                                int minutes_EndTime = appointment.EndTime.Minutes;
                                int seconds_EndTime = appointment.EndTime.Seconds;
                                int milliseconds_EndTime = appointment.EndTime.Milliseconds;

                                TimeSpan duration = new TimeSpan(days_EndTime, hours_EndTime, minutes_EndTime, seconds_EndTime, milliseconds_EndTime) -
                                    new TimeSpan(days_Now, hours_Now, minutes_Now, seconds_Now, milliseconds_Now);

                                character.Job.Tasks.Add(new Sleep
                                {
                                    Name = "Sleep",
                                    Owner_Character = character,
                                    Location = bed.Location,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, duration)
                                });
                                return;
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
                                PathTo(bottom_tiles, middle_tiles, bed.Location, character, true, false);
                                return;
                            }
                        }
                    }
                }
            }

            Wander(character);
        }

        private static bool FindComfort(Character character, bool desperate)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return false;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return false;
            }

            List<Tile> comfortSpots = WorldUtil.GetComfortSpots(character);
            if (comfortSpots.Count > 0)
            {
                Tile? tile = WorldUtil.GetClosestTile(comfortSpots, character);
                if (tile?.Location != null)
                {
                    if (character.Location.X == tile.Location.X &&
                        character.Location.Y == tile.Location.Y)
                    {
                        Tile? furniture = WorldUtil.GetFurniture(Handler.MiddleFurniture, tile.Location);
                        if (furniture == null)
                        {
                            return false;
                        }

                        if (character.Direction != furniture.Direction)
                        {
                            character.Job.Tasks.Add(new Turn
                            {
                                Name = "Turn",
                                Owner_Character = character,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                Direction = furniture.Direction
                            });
                            return false;
                        }
                        else
                        {
                            CharacterUtil.Rest(character);
                            return true;
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
                            PathTo(bottom_tiles, middle_tiles, tile.Location, character, desperate, false);
                            return false;
                        }
                    }
                }
            }

            Wander(character);
            return false;
        }

        private static bool FindEntertainment(Character character)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return false;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, false);
                return false;
            }

            //Is there a TV nearby?
            List<Tile> list = WorldUtil.GetFurniture_Owned(character, "TV");
            if (list.Count > 0)
            {
                Tile? tv = WorldUtil.GetClosestTile(list, character);
                if (tv?.Location != null)
                {
                    //Is the TV on?
                    if (tv.IsLightSource)
                    {
                        //Is there somewhere to sit near the TV?
                        List<Tile> comfortSpots = WorldUtil.GetComfortSpots(character);
                        if (comfortSpots.Count > 0)
                        {
                            List<Tile> nearbySpots = [];
                            for (int i = 0; i < comfortSpots.Count; i++)
                            {
                                Tile comfortSpot = comfortSpots[i];
                                if (comfortSpot.Location == null)
                                {
                                    continue;
                                }

                                int? distance = WorldUtil.GetDistance(comfortSpot.Location, tv.Location);
                                if (distance <= 5)
                                {
                                    if (character.Location.X == comfortSpot.Location.X &&
                                        character.Location.Y == comfortSpot.Location.Y)
                                    {
                                        nearbySpots = [comfortSpot];
                                        break;
                                    }
                                    else
                                    {
                                        nearbySpots.Add(comfortSpot);
                                    }
                                }
                            }

                            Tile? nearbySpot = WorldUtil.GetClosestTile(nearbySpots, tv.Location, false);
                            if (nearbySpot?.Location != null)
                            {
                                if (character.Location.X == nearbySpot.Location.X &&
                                    character.Location.Y == nearbySpot.Location.Y)
                                {
                                    Direction direction = WorldUtil.GetDirection(character.Location, tv.Location);
                                    if (character.Direction != direction)
                                    {
                                        character.Job.Tasks.Add(new Turn
                                        {
                                            Name = "Turn",
                                            Owner_Character = character,
                                            StartTime = new TimeHandler(TimeManager.Now),
                                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                            Direction = direction
                                        });
                                        return false;
                                    }
                                    else
                                    {
                                        character.Job.Tasks.Add(new Wait
                                        {
                                            Name = "Wait",
                                            Owner_Character = character,
                                            StartTime = new TimeHandler(TimeManager.Now),
                                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMinutes(1))
                                        });
                                        return true;
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
                                        PathTo(bottom_tiles, middle_tiles, nearbySpot.Location, character, false, false);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (WorldUtil.NextTo(tv.Location, character.Location))
                        {
                            Direction direction = WorldUtil.GetDirection(character.Location, tv.Location);
                            if (direction != character.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    Owner_Character = character,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = direction
                                });
                                return false;
                            }
                            else
                            {
                                character.Job.Tasks.Add(new ToggleTV
                                {
                                    Name = "ToggleTV",
                                    Owner_Character = character,
                                    Location = tv.Location,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                    TaskBar = CharacterUtil.GenTaskbar(character, 1000)
                                });
                                return false;
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
                                PathTo(bottom_tiles, middle_tiles, tv.Location, character, false, true);
                                return false;
                            }
                        }
                    }
                }
            }

            Wander(character);
            return false;
        }

        private static void WorkJob(Character character)
        {
            if (TimeManager.Now == null ||
                character.Location == null)
            {
                return;
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, false);
                return;
            }

            Job? work = null;

            int jobCount = Handler.Jobs.Count;
            for (int j = 0; j < jobCount; j++)
            {
                Job job = Handler.Jobs[j];
                if (character.Job.ID == job.ID)
                {
                    work = job;
                    break;
                }
            }

            if (work == null)
            {
                return;
            }

            //Get the task we should be working right now
            JobTask? task = work.GetTask(TimeManager.Now);
            if (task?.Location != null &&
                task.StartTime != null &&
                task.EndTime != null)
            {
                //Are we at the task location?
                if (character.Location.X == task.Location.X &&
                    character.Location.Y == task.Location.Y)
                {
                    //Are we facing the right direction?
                    if (character.Direction != task.Direction)
                    {
                        character.Job.Tasks.Add(new Turn
                        {
                            Name = "Turn",
                            Owner_Character = character,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                            Direction = task.Direction
                        });
                    }
                    else
                    {
                        //We're ready, so do the current task
                        character.Job.Tasks.Add(new JobTask
                        {
                            ID = task.ID,
                            Name = task.Name,
                            Type = task.Type,
                            Assignment = task.Assignment,
                            StartTime = new TimeHandler((long)task.StartTime.Hours, 0, 0, 0),
                            EndTime = new TimeHandler((long)task.EndTime.Hours, 0, 0, 0),
                            Location = task.Location,
                            Direction = task.Direction
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
                        PathTo(bottom_tiles, middle_tiles, task.Owner_Tile?.Location, character, true, true);
                    }
                }
            }
        }

        private static void PathTo(Layer bottom_tiles, Layer middle_tiles, Location? target, Character? character, bool desperate, bool stop_next_to_tile)
        {
            if (target == null ||
                character?.Location == null)
            {
                return;
            }

            Tile? middle_tile = middle_tiles.GetTile(character.Location.ToVector2);
            if (middle_tile?.Name != null &&
                middle_tile.Name.Contains("Door") &&
                TimeManager.Now != null)
            {
                if (desperate)
                {
                    character.Job.Tasks.Add(new Move
                    {
                        Name = "Run",
                        Owner_Character = character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        Direction = character.Direction
                    });
                }
                else
                {
                    character.Job.Tasks.Add(new Move
                    {
                        Name = "Walk",
                        Owner_Character = character,
                        StartTime = new TimeHandler(TimeManager.Now),
                        Direction = character.Direction
                    });
                }
            }
            else
            {
                int? distance = WorldUtil.GetDistance(character.Location, target) * 8;
                List<ALocation> path = Pathing.GetPath(bottom_tiles, middle_tiles, character, target, distance, stop_next_to_tile);

                if (path.Count > 0)
                {
                    if (path[0].X == character.Location.X &&
                        path[0].Y == character.Location.Y)
                    {
                        path.Remove(path[0]);
                    }

                    character.Path.AddRange(path);

                    ContinuePathing(character, desperate);
                }
                else
                {
                    Wander(character);
                }
            }
        }

        private static void ContinuePathing(Character? character, bool desperate)
        {
            if (character?.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            ALocation next_path = character.Path[0];
            Location location = new(next_path.X, next_path.Y, 0);

            Direction direction = WorldUtil.GetDirection(character.Location, location);
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

            if (desperate)
            {
                character.Job.Tasks.Add(new Move
                {
                    Name = "Run",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Location = character.Destination,
                    Direction = direction
                });
            }
            else
            {
                character.Job.Tasks.Add(new Move
                {
                    Name = "Walk",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Location = character.Destination,
                    Direction = direction
                });
            }
        }

        public static void Wander(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            Direction direction = Direction.Nowhere;

            CryptoRandom random = new();
            int choice = random.Next(1, 101);
            if (choice <= 28)
            {
                direction = Direction.North;
            }
            else if (choice <= 50)
            {
                direction = Direction.East;
            }
            else if (choice <= 72)
            {
                direction = Direction.South;
            }
            else if (choice <= 100)
            {
                direction = Direction.West;
            }

            random = new CryptoRandom();
            choice = random.Next(1, 11);
            if (choice <= 5)
            {
                character.Job.Tasks.Add(new Wait
                {
                    Name = "Wait",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(30))
                });
            }
            else if (choice > 5 &&
                     choice <= 8)
            {
                character.Job.Tasks.Add(new Move
                {
                    Name = "Walk",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Direction = direction
                });
            }
            else if (choice > 8)
            {
                character.Job.Tasks.Add(new Turn
                {
                    Name = "Turn",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                    Direction = direction
                });

                character.Job.Tasks.Add(new Wait
                {
                    Name = "Wait",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10))
                });
            }
        }

        public static void CloseDoor_Behind(Character character)
        {
            if (character.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            Direction direction = Direction.Nowhere;
            Location? location = null;

            if (character.Direction == Direction.North)
            {
                direction = Direction.South;
                location = new Location(character.Location.X, character.Location.Y + 1, 0);
            }
            else if (character.Direction == Direction.East)
            {
                direction = Direction.West;
                location = new Location(character.Location.X - 1, character.Location.Y, 0);
            }
            else if (character.Direction == Direction.South)
            {
                direction = Direction.North;
                location = new Location(character.Location.X, character.Location.Y - 1, 0);
            }
            else if (character.Direction == Direction.West)
            {
                direction = Direction.East;
                location = new Location(character.Location.X + 1, character.Location.Y, 0);
            }

            if (location != null)
            {
                character.Job.Tasks.Add(new CloseDoor
                {
                    Name = "CloseDoor",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                    Location = location,
                    Direction = direction
                });
            }
        }

        public static void CloseWindow_Behind(Character character)
        {
            if (character.Location == null ||
                TimeManager.Now == null)
            {
                return;
            }

            Direction direction = Direction.Nowhere;
            Location? location = null;

            if (character.Direction == Direction.North)
            {
                direction = Direction.South;
                location = new Location(character.Location.X, character.Location.Y + 1, 0);
            }
            else if (character.Direction == Direction.East)
            {
                direction = Direction.West;
                location = new Location(character.Location.X - 1, character.Location.Y, 0);
            }
            else if (character.Direction == Direction.South)
            {
                direction = Direction.North;
                location = new Location(character.Location.X, character.Location.Y - 1, 0);
            }
            else if (character.Direction == Direction.West)
            {
                direction = Direction.East;
                location = new Location(character.Location.X + 1, character.Location.Y, 0);
            }

            if (location != null)
            {
                character.Job.Tasks.Add(new CloseWindow
                {
                    Name = "CloseWindow",
                    Owner_Character = character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                    Location = location,
                    Direction = direction
                });
            }
        }

        public static void BreakWindow(Vector2 destination, Direction direction)
        {
            if (Handler.Player == null)
            {
                return;
            }

            Map? map = WorldUtil.GetMap();

            Layer? middle_tiles = map?.GetLayer("MiddleTiles");
            Tile? tile = middle_tiles?.GetTile(destination);

            if (tile == null)
            {
                return;
            }

            if (tile.Direction == Direction.North)
            {
                tile.Name = "Window_NorthSouth_Broken";
            }
            else if (tile.Direction == Direction.East)
            {
                tile.Name = "Window_WestEast_Broken";
            }

            if (tile.Name != null)
            {
                tile.Texture = Handler.GetTexture(tile.Name);
                if (tile.Texture != null)
                {
                    tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                }
            }

            Vector2 location = new(destination.X, destination.Y);
            if (direction == Direction.North)
            {
                location.Y--;
            }
            else if (direction == Direction.East)
            {
                location.X++;
            }
            else if (direction == Direction.South)
            {
                location.Y++;
            }
            else if (direction == Direction.West)
            {
                location.X--;
            }

            if (!Handler.Player.Unconscious &&
                Handler.Player.Location != null)
            {
                AssetManager.PlaySound_Random_AtDistance("GlassBreak", Handler.Player.Location.ToVector2, location, 10);
            }

            string name = "BrokenGlass_" + direction.ToString();
            WorldUtil.AddEffect(new Vector3(location.X, location.Y, 0), name, name);
        }

        public static void AbortTask(Character character)
        {
            character.ResetAnimation();
            character.Path.Clear();
            character.Job.Tasks.Clear();

            character.InCombat = false;
        }

        public static void Interact(Tile tile)
        {
            if (Handler.Player == null ||
                TimeManager.Now == null)
            {
                return;
            }

            if (tile != null &&
                tile.Name != null)
            {
                if (tile.Name.Contains("Sink"))
                {
                    if (Handler.Player.Stats.Thirst > 0)
                    {
                        TimeSpan duration = TimeSpan.FromSeconds(Handler.Player.Stats.Thirst);

                        Handler.Player.Job.Tasks.Add(new UseSink
                        {
                            Name = "UseSink",
                            Owner_Character = Handler.Player,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(Handler.Player, (int)duration.TotalMilliseconds)
                        });
                    }
                    else
                    {
                        GameUtil.AddMessage("You're not thirsty enough to drink from a sink.");
                    }
                }
                else if (tile.Name.Contains("Toilet"))
                {
                    if (Handler.Player.Stats.Bladder > 0)
                    {
                        TimeSpan duration = TimeSpan.FromSeconds(Handler.Player.Stats.Bladder);

                        Handler.Player.Job.Tasks.Add(new UseToilet
                        {
                            Name = "UseToilet",
                            Owner_Character = Handler.Player,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(Handler.Player, (int)duration.TotalMilliseconds)
                        });
                    }
                    else
                    {
                        GameUtil.AddMessage("You don't need to use a toilet right now.");
                    }
                }
                else if (tile.Name.Contains("Lamp"))
                {
                    Handler.Player.Job.Tasks.Add(new ToggleLight
                    {
                        Name = "ToggleLight",
                        Owner_Character = Handler.Player,
                        Location = tile.Location,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                        TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 1000)
                    });
                }
                else if (tile.Name.Contains("TV"))
                {
                    Handler.Player.Job.Tasks.Add(new ToggleTV
                    {
                        Name = "ToggleTV",
                        Owner_Character = Handler.Player,
                        Location = tile.Location,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                        TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 1000)
                    });
                }
                else if (tile.Name.Contains("Door"))
                {
                    if (tile.Name.Contains("Closed"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            Handler.Player.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "Quiet_OpenDoor",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            Handler.Player.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "Loud_OpenDoor",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 1000)
                            });
                        }
                        else
                        {
                            Handler.Player.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "OpenDoor",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 2000)
                            });
                        }
                    }
                    else if (tile.Name.Contains("Open"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            Handler.Player.Job.Tasks.Add(new CloseDoor
                            {
                                Name = "Quiet_CloseDoor",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            Handler.Player.Job.Tasks.Add(new CloseDoor
                            {
                                Name = "Loud_CloseDoor",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 1000)
                            });
                        }
                        else
                        {
                            Handler.Player.Job.Tasks.Add(new CloseDoor
                            {
                                Name = "CloseDoor",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 2000)
                            });
                        }
                    }
                }
                else if (tile.Name.Contains("Window") &&
                         !tile.Name.Contains("Broken"))
                {
                    if (tile.Name.Contains("Closed"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            Handler.Player.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "Quiet_OpenWindow",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            Handler.Player.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "Loud_OpenWindow",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 1000)
                            });
                        }
                        else
                        {
                            Handler.Player.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "OpenWindow",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 2000)
                            });
                        }
                    }
                    else if (tile.Name.Contains("Open"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            Handler.Player.Job.Tasks.Add(new CloseWindow
                            {
                                Name = "Quiet_CloseWindow",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            Handler.Player.Job.Tasks.Add(new CloseWindow
                            {
                                Name = "Loud_CloseWindow",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 1000)
                            });
                        }
                        else
                        {
                            Handler.Player.Job.Tasks.Add(new CloseWindow
                            {
                                Name = "CloseWindow",
                                Owner_Character = Handler.Player,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 2000)
                            });
                        }
                    }
                }
                else if (tile.CanUse)
                {
                    if (InputManager.KeyDown("Crouch"))
                    {
                        Handler.Player.Job.Tasks.Add(new Search
                        {
                            Name = "Quiet_Search",
                            Owner_Character = Handler.Player,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(20)),
                            TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 20000)
                        });

                        GameUtil.AddMessage("You started quietly searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else if (InputManager.KeyDown("Run"))
                    {
                        Handler.Player.Job.Tasks.Add(new Search
                        {
                            Name = "Loud_Search",
                            Owner_Character = Handler.Player,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(5)),
                            TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 5000)
                        });

                        GameUtil.AddMessage("You started quickly searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else
                    {
                        Handler.Player.Job.Tasks.Add(new Search
                        {
                            Name = "Search",
                            Owner_Character = Handler.Player,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10)),
                            TaskBar = CharacterUtil.GenTaskbar(Handler.Player, 10000)
                        });

                        GameUtil.AddMessage("You started searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                }
                else
                {
                    WorldUtil.GenDescription(tile);
                }
            }
        }

        #endregion
    }
}
