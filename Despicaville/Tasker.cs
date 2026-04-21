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
using Despicaville.Util;
using Despicaville.Tasks;

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

        public static void GiveTask_Citizen(Character character)
        {
            if (character.InCombat)
            {
                Attacking(character);
                return;
            }

            Property thirst = character.GetStat("Thirst");
            if (thirst.Value >= 60)
            {
                FindWater(character, true);
                return;
            }
            else if (thirst.Value >= 30)
            {
                FindWater(character, false);
                return;
            }

            Property hunger = character.GetStat("Hunger");
            if (hunger.Value >= 60)
            {
                FindFood(character, true);
                return;
            }
            else if (hunger.Value >= 30)
            {
                FindFood(character, false);
                return;
            }

            if (thirst.Value < 30 &&
                hunger.Value < 30)
            {
                Wander(character);
            }
        }

        public static void Attacking(Character character)
        {
            Character target = WorldUtil.GetCharacter_Target(character);
            if (target != null)
            {
                Direction direction = WorldUtil.GetDirection(target.Location, character.Location, false);

                if (direction != character.Direction)
                {
                    character.Job.Tasks.Add(new Turn
                    {
                        Name = "Turn",
                        OwnerID = character.ID,
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
                        OwnerID = character.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        Direction = character.Direction
                    });
                }
                else
                {
                    Location location = new Location();
                    if (character.Direction == Direction.Up)
                    {
                        location = new Location(character.Location.X, character.Location.Y - 1, 1);
                    }
                    else if (character.Direction == Direction.Right)
                    {
                        location = new Location(character.Location.X + 1, character.Location.Y, 1);
                    }
                    else if (character.Direction == Direction.Down)
                    {
                        location = new Location(character.Location.X, character.Location.Y + 1, 1);
                    }
                    else if (character.Direction == Direction.Left)
                    {
                        location = new Location(character.Location.X - 1, character.Location.Y, 1);
                    }

                    Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(character);
                    string action = AttackingWith.ElementAt(0).Value;

                    int attackTime = CombatUtil.AttackTime(character, action);
                    character.Job.Tasks.Add(new Attack
                    {
                        Name = "Attack",
                        OwnerID = character.ID,
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

        public static void FindWater(Character character, bool desperate)
        {
            Inventory inventory = character.Inventory;

            //Do we already have something to drink?
            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item existing = inventory.Items[i];

                Property thirst = existing.GetProperty("Thirst");
                if (thirst != null)
                {
                    character.Job.Tasks.Add(new UseItem
                    {
                        Name = "UseItem_" + existing.ID,
                        OwnerID = character.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(thirst.Value * -10)),
                    });

                    return;
                }
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return;
            }

            //Is there a sink nearby to drink from?
            List<Tile> sinks = WorldUtil.GetOwned_Furniture(character, "Sink");
            if (sinks.Count > 0)
            {
                Tile sink = WorldUtil.GetClosestTile(sinks, character);
                if (sink != null)
                {
                    if (WorldUtil.NextTo(sink.Location, character.Location))
                    {
                        Direction direction = WorldUtil.GetFurnitureDirection(sink, character);
                        if (direction != character.Direction)
                        {
                            character.Job.Tasks.Add(new Turn
                            {
                                Name = "Turn",
                                OwnerID = character.ID,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                Direction = direction
                            });
                        }
                        else if (!sink.Texture.Name.Contains("Used"))
                        {
                            Property thirst = character.GetStat("Thirst");
                            TimeSpan duration = TimeSpan.FromSeconds(thirst.Value);

                            character.Job.Tasks.Add(new UseSink
                            {
                                Name = "UseSink",
                                OwnerID = character.ID,
                                Location = sink.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, duration),
                                TaskBar = CharacterUtil.GenTaskbar(character, (int)duration.TotalMilliseconds)
                            });
                        }
                    }
                    else
                    {
                        Map map = WorldUtil.GetMap();
                        Layer bottom_tiles = map.GetLayer("BottomTiles");
                        Layer middle_tiles = map.GetLayer("MiddleTiles");
                        Layer room_tiles = map.GetLayer("RoomTiles");

                        PathTo(bottom_tiles, middle_tiles, room_tiles, sink, character, desperate);
                    }

                    return;
                }
            }

            //Does a nearby fridge have something to drink?
            List<Tile> fridges = WorldUtil.GetOwned_Furniture(character, "Fridge");
            if (fridges.Count > 0)
            {
                Tile fridge = WorldUtil.GetClosestTile(fridges, character);
                if (fridge != null)
                {
                    Item item = null;

                    int fridgeCount = fridge.Inventory.Items.Count;
                    for (int i = 0; i < fridgeCount; i++)
                    {
                        Item existing = fridge.Inventory.Items[i];

                        Property thirst = existing.GetProperty("Thirst");
                        if (thirst != null)
                        {
                            item = existing;
                            break;
                        }
                    }

                    if (item != null)
                    {
                        if (WorldUtil.NextTo(fridge.Location, character.Location))
                        {
                            Direction direction = WorldUtil.GetFurnitureDirection(fridge, character);
                            if (direction != character.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    OwnerID = character.ID,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = direction
                                });
                            }
                            else
                            {
                                if (fridge.Texture.Name.Contains("Used"))
                                {
                                    InventoryUtil.TransferItem(fridge.Inventory, character.Inventory, item);

                                    character.Job.Tasks.Add(new CloseFridge
                                    {
                                        Name = "CloseFridge",
                                        OwnerID = character.ID,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });
                                }
                                else
                                {
                                    character.Job.Tasks.Add(new OpenFridge
                                    {
                                        Name = "OpenFridge",
                                        OwnerID = character.ID,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });
                                }
                            }
                        }
                        else
                        {
                            Map map = WorldUtil.GetMap();
                            Layer bottom_tiles = map.GetLayer("BottomTiles");
                            Layer middle_tiles = map.GetLayer("MiddleTiles");
                            Layer room_tiles = map.GetLayer("RoomTiles");

                            PathTo(bottom_tiles, middle_tiles, room_tiles, fridge, character, desperate);
                        }

                        return;
                    }
                }
            }

            Wander(character);
        }

        public static void FindFood(Character character, bool desperate)
        {
            Inventory inventory = character.Inventory;

            //Do we already have something to eat?
            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item existing = inventory.Items[i];

                Property hunger = existing.GetProperty("Hunger");
                if (hunger != null)
                {
                    character.Job.Tasks.Add(new UseItem
                    {
                        Name = "UseItem_" + existing.ID,
                        OwnerID = character.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(hunger.Value * -10)),
                    });

                    return;
                }
            }

            //Are we already pathing to something?
            if (character.Path.Count > 0)
            {
                ContinuePathing(character, desperate);
                return;
            }

            //Does a nearby fridge have something to eat?
            List<Tile> fridges = WorldUtil.GetOwned_Furniture(character, "Fridge");
            if (fridges.Count > 0)
            {
                Tile fridge = WorldUtil.GetClosestTile(fridges, character);
                if (fridge != null)
                {
                    Item item = null;

                    int fridgeCount = fridge.Inventory.Items.Count;
                    for (int i = 0; i < fridgeCount; i++)
                    {
                        Item existing = fridge.Inventory.Items[i];

                        Property hunger = existing.GetProperty("Hunger");
                        if (hunger != null)
                        {
                            item = existing;
                            break;
                        }
                    }

                    if (item != null)
                    {
                        if (WorldUtil.NextTo(fridge.Location, character.Location))
                        {
                            Direction direction = WorldUtil.GetFurnitureDirection(fridge, character);
                            if (direction != character.Direction)
                            {
                                character.Job.Tasks.Add(new Turn
                                {
                                    Name = "Turn",
                                    OwnerID = character.ID,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                                    Direction = direction
                                });
                            }
                            else
                            {
                                if (fridge.Texture.Name.Contains("Used"))
                                {
                                    InventoryUtil.TransferItem(fridge.Inventory, character.Inventory, item);

                                    character.Job.Tasks.Add(new CloseFridge
                                    {
                                        Name = "CloseFridge",
                                        OwnerID = character.ID,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });
                                }
                                else
                                {
                                    character.Job.Tasks.Add(new OpenFridge
                                    {
                                        Name = "OpenFridge",
                                        OwnerID = character.ID,
                                        StartTime = new TimeHandler(TimeManager.Now),
                                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                        Location = fridge.Location,
                                        Direction = direction
                                    });
                                }
                            }
                        }
                        else
                        {
                            Map map = WorldUtil.GetMap();
                            Layer bottom_tiles = map.GetLayer("BottomTiles");
                            Layer middle_tiles = map.GetLayer("MiddleTiles");
                            Layer room_tiles = map.GetLayer("RoomTiles");

                            PathTo(bottom_tiles, middle_tiles, room_tiles, fridge, character, desperate);
                        }

                        return;
                    }
                }
            }

            Wander(character);
        }

        public static void CloseDoor_Behind(Character character)
        {
            Direction direction = Direction.Nowhere;
            Location location = null;

            if (character.Direction == Direction.Up)
            {
                direction = Direction.Down;
                location = new Location(character.Location.X, character.Location.Y + 1, 0);
            }
            else if (character.Direction == Direction.Right)
            {
                direction = Direction.Left;
                location = new Location(character.Location.X - 1, character.Location.Y, 0);
            }
            else if (character.Direction == Direction.Down)
            {
                direction = Direction.Up;
                location = new Location(character.Location.X, character.Location.Y - 1, 0);
            }
            else if (character.Direction == Direction.Left)
            {
                direction = Direction.Right;
                location = new Location(character.Location.X + 1, character.Location.Y, 0);
            }

            character.Job.Tasks.Add(new CloseDoor
            {
                Name = "CloseDoor",
                OwnerID = character.ID,
                StartTime = new TimeHandler(TimeManager.Now),
                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                Location = location,
                Direction = direction
            });
        }

        public static void CloseWindow_Behind(Character character)
        {
            Direction direction = Direction.Nowhere;
            Location location = null;

            if (character.Direction == Direction.Up)
            {
                direction = Direction.Down;
                location = new Location(character.Location.X, character.Location.Y + 1, 0);
            }
            else if (character.Direction == Direction.Right)
            {
                direction = Direction.Left;
                location = new Location(character.Location.X - 1, character.Location.Y, 0);
            }
            else if (character.Direction == Direction.Down)
            {
                direction = Direction.Up;
                location = new Location(character.Location.X, character.Location.Y - 1, 0);
            }
            else if (character.Direction == Direction.Left)
            {
                direction = Direction.Right;
                location = new Location(character.Location.X + 1, character.Location.Y, 0);
            }

            character.Job.Tasks.Add(new CloseWindow
            {
                Name = "CloseWindow",
                OwnerID = character.ID,
                StartTime = new TimeHandler(TimeManager.Now),
                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                Location = location,
                Direction = direction
            });
        }

        public static void Wander(Character character)
        {
            Direction direction = Direction.Nowhere;

            CryptoRandom random = new CryptoRandom();
            int choice = random.Next(1, 101);
            if (choice <= 28)
            {
                direction = Direction.Up;
            }
            else if (choice <= 50)
            {
                direction = Direction.Right;
            }
            else if (choice <= 72)
            {
                direction = Direction.Down;
            }
            else if (choice <= 100)
            {
                direction = Direction.Left;
            }

            random = new CryptoRandom();
            choice = random.Next(1, 11);
            if (choice <= 5)
            {
                character.Job.Tasks.Add(new Wait
                {
                    Name = "Wait",
                    OwnerID = character.ID,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10))
                });
            }
            else if (choice > 5 &&
                     choice <= 8)
            {
                character.Job.Tasks.Add(new Move
                {
                    Name = "Walk",
                    OwnerID = character.ID,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Direction = direction
                });
            }
            else if (choice > 8)
            {
                character.Job.Tasks.Add(new Turn
                {
                    Name = "Turn",
                    OwnerID = character.ID,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character))),
                    Direction = direction
                });
            }
        }

        public static void BreakWindow(Character character)
        {
            Map map = WorldUtil.GetMap();

            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(character.Destination.ToVector2);

            if (tile.Direction == Direction.Up)
            {
                tile.Name = "Window_NorthSouth_Broken";
            }
            else if (tile.Direction == Direction.Right)
            {
                tile.Name = "Window_WestEast_Broken";
            }

            tile.Texture = AssetManager.Textures[tile.Name];
            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);

            Vector2 location = character.Destination.ToVector2;
            if (character.Direction == Direction.Up)
            {
                location.Y--;
            }
            else if (character.Direction == Direction.Right)
            {
                location.X++;
            }
            else if (character.Direction == Direction.Down)
            {
                location.Y++;
            }
            else if (character.Direction == Direction.Left)
            {
                location.X--;
            }

            if (!Handler.Player.Unconscious)
            {
                AssetManager.PlaySound_Random_AtDistance("GlassBreak", Handler.Player.Location.ToVector2, location, 10);
            }

            string name = "BrokenGlass_" + character.Direction.ToString();
            WorldUtil.AddEffect(new Vector3(location.X, location.Y, 0), name, name);
        }

        public static void AbortTask(Character character)
        {
            character.ResetAnimation();
            character.Path.Clear();
            character.Job.Tasks.Clear();

            character.InCombat = false;
        }

        public static void Interact(Tile tile, Character player)
        {
            if (tile != null)
            {
                if (tile.Name.Contains("Sink"))
                {
                    Property thirst = player.GetStat("Thirst");
                    if (thirst.Value > 0)
                    {
                        TimeSpan duration = TimeSpan.FromSeconds(thirst.Value);

                        player.Job.Tasks.Add(new UseSink
                        {
                            Name = "UseSink",
                            OwnerID = player.ID,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(player, (int)duration.TotalMilliseconds)
                        });
                    }
                    else
                    {
                        GameUtil.AddMessage("You're not thirsty enough to drink from a sink.");
                    }
                }
                else if (tile.Name.Contains("Toilet"))
                {
                    Property bladder = player.GetStat("Bladder");
                    if (bladder.Value > 0)
                    {
                        TimeSpan duration = TimeSpan.FromSeconds(bladder.Value);

                        player.Job.Tasks.Add(new UseToilet
                        {
                            Name = "UseToilet",
                            OwnerID = player.ID,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, duration),
                            TaskBar = CharacterUtil.GenTaskbar(player, (int)duration.TotalMilliseconds)
                        });
                    }
                    else
                    {
                        GameUtil.AddMessage("You don't need to use a toilet right now.");
                    }
                }
                else if (tile.Name.Contains("Lamp"))
                {
                    player.Job.Tasks.Add(new ToggleLight
                    {
                        Name = "ToggleLight",
                        OwnerID = player.ID,
                        Location = tile.Location,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                        TaskBar = CharacterUtil.GenTaskbar(player, 1000)
                    });
                }
                else if (tile.Name.Contains("TV"))
                {
                    player.Job.Tasks.Add(new ToggleTV
                    {
                        Name = "ToggleTV",
                        OwnerID = player.ID,
                        Location = tile.Location,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                        TaskBar = CharacterUtil.GenTaskbar(player, 1000)
                    });
                }
                else if (tile.Name.Contains("Door"))
                {
                    if (tile.Name.Contains("Closed"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            player.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "Quiet_OpenDoor",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            player.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "Loud_OpenDoor",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 1000)
                            });
                        }
                        else
                        {
                            player.Job.Tasks.Add(new OpenDoor
                            {
                                Name = "OpenDoor",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 2000)
                            });
                        }
                    }
                    else if (tile.Name.Contains("Open"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            player.Job.Tasks.Add(new CloseDoor
                            {
                                Name = "Quiet_CloseDoor",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            player.Job.Tasks.Add(new CloseDoor
                            {
                                Name = "Loud_CloseDoor",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 1000)
                            });
                        }
                        else
                        {
                            player.Job.Tasks.Add(new CloseDoor
                            {
                                Name = "CloseDoor",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 2000)
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
                            player.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "Quiet_OpenWindow",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            player.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "Loud_OpenWindow",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 1000)
                            });
                        }
                        else
                        {
                            player.Job.Tasks.Add(new OpenWindow
                            {
                                Name = "OpenWindow",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 2000)
                            });
                        }
                    }
                    else if (tile.Name.Contains("Open"))
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            player.Job.Tasks.Add(new CloseWindow
                            {
                                Name = "Quiet_CloseWindow",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 4000)
                            });
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            player.Job.Tasks.Add(new CloseWindow
                            {
                                Name = "Loud_CloseWindow",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 1000)
                            });
                        }
                        else
                        {
                            player.Job.Tasks.Add(new CloseWindow
                            {
                                Name = "CloseWindow",
                                OwnerID = player.ID,
                                Location = tile.Location,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(2)),
                                TaskBar = CharacterUtil.GenTaskbar(player, 2000)
                            });
                        }
                    }
                }
                else if (WorldUtil.CanSearch(tile.Name))
                {
                    if (InputManager.KeyDown("Crouch"))
                    {
                        player.Job.Tasks.Add(new Search
                        {
                            Name = "Quiet_Search",
                            OwnerID = player.ID,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(20)),
                            TaskBar = CharacterUtil.GenTaskbar(player, 20000)
                        });

                        GameUtil.AddMessage("You started quietly searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else if (InputManager.KeyDown("Run"))
                    {
                        player.Job.Tasks.Add(new Search
                        {
                            Name = "Loud_Search",
                            OwnerID = player.ID,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(5)),
                            TaskBar = CharacterUtil.GenTaskbar(player, 5000)
                        });

                        GameUtil.AddMessage("You started quickly searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else
                    {
                        player.Job.Tasks.Add(new Search
                        {
                            Name = "Search",
                            OwnerID = player.ID,
                            Location = tile.Location,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10)),
                            TaskBar = CharacterUtil.GenTaskbar(player, 10000)
                        });

                        GameUtil.AddMessage("You started searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                }
                else
                {
                    bool plural = false;

                    string name = WorldUtil.GetTile_Name(tile);
                    if (name.Contains(" "))
                    {
                        if (name.Split(' ')[0] == "some")
                        {
                            plural = true;
                        }
                    }

                    if (plural)
                    {
                        GameUtil.AddMessage("You can't do anything with " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else
                    {
                        GameUtil.AddMessage("You can't do anything with the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                }
            }
        }

        private static void PathTo(Layer bottom_tiles, Layer middle_tiles, Layer room_tiles, Tile furniture, Character character, bool desperate)
        {
            bool in_room = false;

            Tile room_tile = room_tiles.GetTile(character.Location.ToVector2);
            if (room_tile != null &&
                room_tile.Texture != null)
            {
                in_room = true;
            }

            List<ALocation> path = new List<ALocation>();
            character.Path = new List<ALocation>();

            if (in_room)
            {
                if (WorldUtil.Furniture_InRoom(furniture, character))
                {
                    int distance = WorldUtil.GetDistance(furniture.Location, character.Location) * 4;
                    path = DPathing.GetPath(bottom_tiles, middle_tiles, character, furniture, distance, true);
                }
                else
                {
                    Tile exit = WorldUtil.GetNearestExit_ToFurniture(character, middle_tiles, furniture);
                    if (exit != null)
                    {
                        Tile tile = null;

                        if (!string.IsNullOrEmpty(exit.Name))
                        {
                            if (exit.Name.Contains("NorthSouth"))
                            {
                                if (exit.Location.X < character.Location.X)
                                {
                                    tile = bottom_tiles.GetTile(new Vector2(exit.Location.X - 1, exit.Location.Y));
                                }
                                else if (exit.Location.X > character.Location.X)
                                {
                                    tile = bottom_tiles.GetTile(new Vector2(exit.Location.X + 1, exit.Location.Y));
                                }
                            }
                            else if (exit.Name.Contains("WestEast"))
                            {
                                if (exit.Location.Y < character.Location.Y)
                                {
                                    tile = bottom_tiles.GetTile(new Vector2(exit.Location.X, exit.Location.Y - 1));
                                }
                                else if (exit.Location.Y > character.Location.Y)
                                {
                                    tile = bottom_tiles.GetTile(new Vector2(exit.Location.X, exit.Location.Y + 1));
                                }
                            }
                        }
                        else
                        {
                            tile = bottom_tiles.GetTile(exit.Location.ToVector2);
                        }

                        if (tile != null)
                        {
                            int distance = WorldUtil.GetDistance(tile.Location, character.Location) * 4;
                            path = DPathing.GetPath(bottom_tiles, middle_tiles, character, tile, distance, false);
                        }
                    }
                }
            }
            else
            {
                Tile middle_tile = middle_tiles.GetTile(character.Location.ToVector2);
                if (middle_tile != null &&
                    middle_tile.Name.Contains("Door"))
                {
                    if (desperate)
                    {
                        character.Job.Tasks.Add(new Move
                        {
                            Name = "Run",
                            OwnerID = character.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = character.Direction
                        });
                    }
                    else
                    {
                        character.Job.Tasks.Add(new Move
                        {
                            Name = "Walk",
                            OwnerID = character.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = character.Direction
                        });
                    }
                }
                else
                {
                    int distance = WorldUtil.GetDistance(furniture.Location, character.Location) * 4;
                    path = DPathing.GetPath(bottom_tiles, middle_tiles, character, furniture, distance, true);
                }
            }

            if (path != null &&
                path.Count > 0)
            {
                path.Reverse();
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

        private static void ContinuePathing(Character character, bool desperate)
        {
            Direction direction = WorldUtil.GetDirection(new Location(character.Path[0].X, character.Path[0].Y, 0), character.Location, false);
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

            if (desperate)
            {
                character.Job.Tasks.Add(new Move
                {
                    Name = "Run",
                    OwnerID = character.ID,
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
                    OwnerID = character.ID,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Location = character.Destination,
                    Direction = direction
                });
            }
        }

        #endregion
    }
}
