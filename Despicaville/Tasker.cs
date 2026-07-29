using System;
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
using Despicaville.MajorTasks;
using Despicaville.SubTasks;

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

        public static void UpdateNeeds(Character character)
        {
            if (character.InCombat)
            {
                HandleCombat(character);
            }

            if (character.Stats.Bladder >= 30)
            {
                HandleBladder(character);
            }

            if (character.Stats.Thirst >= 30)
            {
                HandleThirst(character);
            }

            if (character.Stats.Hunger >= 30)
            {
                HandleHunger(character);
            }

            character.Job.Sort_ByPriority(false);
        }

        public static void GetTask(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            Appointment? appointment = character.Job.GetAppointment(TimeManager.Now);
            if (appointment?.Name == null)
            {
                return;
            }

            if (appointment.Name.Contains("Sleep"))
            {
                JobTask? findBed = character.Job.GetTask("FindBed");
                if (findBed == null)
                {
                    if (appointment.StartTime == null ||
                        appointment.EndTime == null)
                    {
                        return;
                    }

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

                    character.Job.Tasks.Add(new FindBed
                    {
                        Name = "FindBed",
                        Owner_Character = character,
                        Priority = 80,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, duration)
                    });
                }
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
                    JobTask? workJob = character.Job.GetTask("WorkJob");
                    if (workJob == null)
                    {
                        character.Job.Tasks.Add(new WorkJob
                        {
                            Name = "WorkJob",
                            Owner_Character = character,
                            Priority = 30,
                            StartTime = new TimeHandler(TimeManager.Now)
                        });
                    }
                    return;
                }
            }

            JobTask? findEntertainment = character.Job.GetTask("FindEntertainment");
            if (findEntertainment == null)
            {
                character.Job.Tasks.Add(new FindEntertainment
                {
                    Name = "FindEntertainment",
                    Owner_Character = character,
                    Priority = 10,
                    StartTime = new TimeHandler(TimeManager.Now)
                });
            }
        }

        private static void HandleCombat(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            JobTask? attacking = character.Job.GetTask("Attacking");
            if (attacking == null)
            {
                character.Job.Tasks.Add(new Attacking
                {
                    Name = "Attacking",
                    Owner_Character = character,
                    Priority = 101,
                    StartTime = new TimeHandler(TimeManager.Now)
                });
            }
        }

        private static void HandleBladder(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            JobTask? findToilet = character.Job.GetTask("FindToilet");
            if (findToilet != null)
            {
                findToilet.Priority = (int)character.Stats.Bladder;
            }
            else
            {
                character.Job.Tasks.Add(new FindToilet
                {
                    Name = "FindToilet",
                    Owner_Character = character,
                    Priority = (int)character.Stats.Bladder,
                    StartTime = new TimeHandler(TimeManager.Now)
                });
            }
        }

        private static void HandleThirst(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            bool hasDrink = HasDrink(character);
            if (hasDrink)
            {
                bool stillBusy = false;

                JobTask? findDrink = character.Job.GetTask("FindDrink");
                if (findDrink != null)
                {
                    stillBusy = true;
                }

                if (!stillBusy)
                {
                    bool seated = Seated(character);
                    if (seated)
                    {
                        ConsumeDrink(character);
                    }
                    else
                    {
                        JobTask? findComfort = character.Job.GetTask("FindComfort");
                        if (findComfort != null)
                        {
                            findComfort.Description = character.Stats.Thirst >= 60 ? "Desperate" : "";
                        }
                        else
                        {
                            character.Job.Tasks.Add(new FindComfort
                            {
                                Name = "FindComfort",
                                Description = character.Stats.Thirst >= 60 ? "Desperate" : "",
                                Owner_Character = character,
                                Priority = 101,
                                StartTime = new TimeHandler(TimeManager.Now)
                            });
                        }
                    }
                }
            }
            else
            {
                JobTask? findDrink = character.Job.GetTask("FindDrink");
                if (findDrink != null)
                {
                    findDrink.Priority = (int)character.Stats.Thirst;
                }
                else
                {
                    character.Job.Tasks.Add(new FindDrink
                    {
                        Name = "FindDrink",
                        Owner_Character = character,
                        Priority = (int)character.Stats.Thirst,
                        StartTime = new TimeHandler(TimeManager.Now)
                    });
                }
            }
        }

        private static bool HasDrink(Character character)
        {
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

        private static void ConsumeDrink(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            JobTask? useItem = null;

            int taskCount = character.Job.Tasks.Count;
            for (int i = 0; i < taskCount; i++)
            {
                JobTask task = character.Job.Tasks[i];
                if (!string.IsNullOrEmpty(task.Name) &&
                    task.Name.Contains("UseItem"))
                {
                    useItem = task;
                }
            }

            if (useItem == null)
            {
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
                            Priority = 101,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                        });
                        break;
                    }
                }
            }
        }

        private static void HandleHunger(Character character)
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            bool hasFood = HasFood(character);
            if (hasFood)
            {
                bool stillBusy = false;

                JobTask? findFood = character.Job.GetTask("FindFood");
                if (findFood != null)
                {
                    stillBusy = true;
                }

                if (!stillBusy)
                {
                    bool seated = Seated(character);
                    if (seated)
                    {
                        ConsumeFood(character);
                    }
                    else
                    {
                        JobTask? findComfort = character.Job.GetTask("FindComfort");
                        if (findComfort != null)
                        {
                            findComfort.Description = character.Stats.Hunger >= 60 ? "Desperate" : "";
                        }
                        else
                        {
                            character.Job.Tasks.Add(new FindComfort
                            {
                                Name = "FindComfort",
                                Description = character.Stats.Hunger >= 60 ? "Desperate" : "",
                                Owner_Character = character,
                                Priority = 101,
                                StartTime = new TimeHandler(TimeManager.Now)
                            });
                        }
                    }
                }
            }
            else
            {
                JobTask? findFood = character.Job.GetTask("FindFood");
                if (findFood != null)
                {
                    findFood.Priority = (int)character.Stats.Hunger;
                }
                else
                {
                    character.Job.Tasks.Add(new FindFood
                    {
                        Name = "FindFood",
                        Owner_Character = character,
                        Priority = (int)character.Stats.Hunger,
                        StartTime = new TimeHandler(TimeManager.Now)
                    });
                }
            }
        }

        private static bool HasFood(Character character)
        {
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

            JobTask? useItem = null;

            int taskCount = character.Job.Tasks.Count;
            for (int i = 0; i < taskCount; i++)
            {
                JobTask task = character.Job.Tasks[i];
                if (!string.IsNullOrEmpty(task.Name) &&
                    task.Name.Contains("UseItem"))
                {
                    useItem = task;
                }
            }

            if (useItem == null)
            {
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
                            Priority = 101,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                        });
                        break;
                    }
                }
            }
        }

        private static bool Seated(Character character)
        {
            if (character.Location == null)
            {
                return false;
            }

            List<Tile> comfortSpots = WorldUtil.GetComfortSpots(character);
            if (comfortSpots.Count > 0)
            {
                Tile? comfortSpot = WorldUtil.GetClosestTile(comfortSpots, character);
                if (comfortSpot?.Location != null)
                {
                    if (character.Location.X == comfortSpot.Location.X &&
                        character.Location.Y == comfortSpot.Location.Y)
                    {
                        if (character.Direction == comfortSpot.Direction)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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
                JobTask? majorTask = character.Job.Get_CurrentTask();
                majorTask?.SubTasks.Add(new CloseDoor
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
                JobTask? majorTask = character.Job.Get_CurrentTask();
                majorTask?.SubTasks.Add(new CloseWindow
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
