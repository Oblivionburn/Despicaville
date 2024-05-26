using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Jobs;
using OP_Engine.Menus;
using OP_Engine.Time;
using OP_Engine.Inventories;
using OP_Engine.Sounds;
using OP_Engine.Inputs;

using Despicaville.Util;

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

        public static void GiveTask_Citizen(World world, Character character)
        {
            if (character.GetStat("Thirst").Value >= 60)
            {
                FindWater(world, character, true);
            }
            else if (character.GetStat("Thirst").Value >= 30)
            {
                FindWater(world, character, false);
            }
            else if (character.GetStat("Hunger").Value >= 60)
            {
                FindFood(world, character, true);
            }
            else if (character.GetStat("Hunger").Value >= 30)
            {
                FindFood(world, character, false);
            }
            else if (character.GetStat("Stamina").Value <= 30)
            {
                GoToSleep(world, character);
            }
            else if (character.GetStat("Boredom").Value >= 30)
            {
                FindEntertainment(world, character);
            }
            else if (character.GetStat("Stamina").Value <= 60)
            {
                FindRest(world, character);
            }
            else if (WorldUtil.PassedOpenDoor(world.Maps[0].GetLayer("MiddleTiles"), character))
            {
                CloseDoor_Behind(character);
            }
            else if (WorldUtil.PassedOpenWindow(world.Maps[0].GetLayer("MiddleTiles"), character))
            {
                CloseWindow_Behind(character);
            }
            else
            {
                Wander(character);
            }
        }

        public static void Character_StartAction(World world, Character character)
        {
            if (character.Job.Tasks.Count > 0)
            {
                Task task = character.Job.CurrentTask;
                if (task != null)
                {
                    if (task.Started &&
                        !character.Moving)
                    {
                        if (task.Name == "Sneak" || 
                            task.Name == "Walk" || 
                            task.Name == "Run")
                        {
                            Move(world, character);
                        }
                        else if (task.Name.Contains("GoTo"))
                        {
                            string[] task_parts = task.Name.Split('_');
                            GoTo(world, character, task_parts[1]);
                        }
                        else if (task.Name == "Attacking")
                        {
                            Attacking(character);
                        }
                        else if (task.Name.Contains("UseSink_Start"))
                        {
                            if (task.Name.Contains("Drink"))
                            {
                                UseSink_Start(character, false);
                            }
                            else
                            {
                                UseSink_Start(character, true);
                            }
                        }
                    }
                }
            }
        }

        public static void Character_DoAction(World world, Character character)
        {
            if (character.Job.Tasks.Count > 0)
            {
                Task task = character.Job.CurrentTask;
                if (task != null)
                {
                    if (task.Started)
                    {
                        if (!task.Completed)
                        {
                            if (task.Name == "Wait")
                            {
                                Wait(character);
                            }
                            else if (task.Name == "Sleep")
                            {
                                Sleep(character);
                            }
                        }
                    }
                }
            }
        }

        public static void Character_EndAction(World world, Character character)
        {
            if (character.Job.Tasks.Count > 0)
            {
                Task task = character.Job.CurrentTask;
                if (task != null)
                {
                    if (task.Completed &&
                        task.Keep_On_Completed)
                    {
                        task.Keep_On_Completed = false;

                        int loudness = 2;
                        if (task.Name.Contains("Quiet"))
                        {
                            loudness = 1;
                        }
                        else if (task.Name.Contains("Loud"))
                        {
                            loudness = 3;
                        }

                        if (task.Name == "Turn")
                        {
                            Turn(character);
                        }
                        else if (CombatUtil.IsAttack(task.Name))
                        {
                            Attack(character);
                        }
                        else if (task.Name.Contains("OpenDoor"))
                        {
                            OpenDoor(world, character, loudness);
                        }
                        else if (task.Name.Contains("CloseDoor"))
                        {
                            CloseDoor(world, character, loudness);
                        }
                        else if (task.Name.Contains("OpenWindow"))
                        {
                            OpenWindow(world, character, loudness);
                        }
                        else if (task.Name.Contains("CloseWindow"))
                        {
                            CloseWindow(world, character, loudness);
                        }
                        else if (task.Name.Contains("OpenFridge"))
                        {
                            OpenFridge(world, character, loudness);
                        }
                        else if (task.Name.Contains("CloseFridge"))
                        {
                            CloseFridge(world, character, loudness);
                        }
                        else if (task.Name == "ToggleLight")
                        {
                            ToggleLight(world, character);
                        }
                        else if (task.Name == "ToggleTV")
                        {
                            ToggleTV(world, character);
                        }
                        else if (task.Name.Contains("Search"))
                        {
                            EndSearch(world, character, loudness);
                        }
                        else if (task.Name.Contains("UseSink_End"))
                        {
                            if (task.Name.Contains("Drink"))
                            {
                                UseSink_End(world, character, false);
                            }
                            else
                            {
                                UseSink_End(world, character, true);
                            }
                        }
                        else if (task.Name.Contains("UseItem"))
                        {
                            UseItem(world, character);
                        }
                        else if (task.Name.Contains("Examine"))
                        {
                            Examine(character);
                        }
                    }
                }
            }
        }

        #region New Task

        public static void FindWater(World world, Character character, bool desperate)
        {
            bool found = false;

            Inventory inventory = character.Inventory;

            //Do we already have something to drink?
            foreach (Item existing in inventory.Items)
            {
                Something thirst = existing.GetProperty("Thirst");
                if (thirst != null)
                {
                    found = true;
                    AddTask(character, "UseItem_" + existing.ID, false, true, TimeSpan.FromSeconds(thirst.Value * -1), default, 0);
                    break;
                }
            }

            //Does a nearby fridge have something to drink?
            if (!found)
            {
                Map map = world.Maps[0];
                Layer bottom_tiles = map.GetLayer("BottomTiles");
                Layer middle_tiles = map.GetLayer("MiddleTiles");

                List<Tile> fridges = WorldUtil.GetOwned_Furniture(character, "Fridge");
                if (fridges.Count > 0)
                {
                    Tile fridge = WorldUtil.GetClosestTile(fridges, character);
                    if (fridge != null)
                    {
                        Item item = null;
                        foreach (Item existing in fridge.Inventory.Items)
                        {
                            Something thirst = existing.GetProperty("Thirst");
                            if (thirst != null)
                            {
                                found = true;
                                item = existing;
                                break;
                            }
                        }

                        if (found)
                        {
                            if (WorldUtil.NextTo(fridge.Location, character.Location))
                            {
                                Direction direction = WorldUtil.GetFurnitureDirection(fridge, character);
                                if (direction != character.Direction)
                                {
                                    AddTask(character, "Turn", false, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), default, direction);
                                }
                                else
                                {
                                    if (fridge.Texture.Name.Contains("Used"))
                                    {
                                        InventoryUtil.TransferItem(fridge.Inventory, character.Inventory, item);
                                        AddTask(character, "CloseFridge", false, true, TimeSpan.FromMilliseconds(200), fridge.Location, direction);
                                    }
                                    else
                                    {
                                        AddTask(character, "OpenFridge", false, true, TimeSpan.FromMilliseconds(200), fridge.Location, direction);
                                    }
                                }
                            }
                            else
                            {
                                character.Path = new List<ALocation>();

                                int distance = WorldUtil.GetDistance(fridge.Location, character.Location) * 2;
                                List<ALocation> path = DPathing.GetPath(bottom_tiles, middle_tiles, character, fridge, distance, true);
                                if (path != null)
                                {
                                    character.Path.AddRange(path);

                                    if (character.Path.Count > 0)
                                    {
                                        if (desperate)
                                        {
                                            AddTask(character, "GoTo_Run", true, false, null, default, 0);
                                        }
                                        else
                                        {
                                            AddTask(character, "GoTo_Walk", true, false, null, default, 0);
                                        }
                                    }
                                    else
                                    {
                                        found = false;
                                        DPathing.AddAttemptedTile(character, fridge);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Is there a sink nearby to drink from?
            if (!found)
            {
                Map map = world.Maps[0];
                Layer bottom_tiles = map.GetLayer("BottomTiles");
                Layer middle_tiles = map.GetLayer("MiddleTiles");

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
                                AddTask(character, "Turn", false, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), default, direction);
                            }
                            else
                            {
                                AddTask(character, "UseSink_Start_Drink", true, false, null, default, 0);
                            }
                        }
                        else
                        {
                            character.Path = new List<ALocation>();

                            int distance = WorldUtil.GetDistance(sink.Location, character.Location) * 2;
                            List<ALocation> path = DPathing.GetPath(bottom_tiles, middle_tiles, character, sink, distance, true);
                            if (path != null)
                            {
                                character.Path.AddRange(path);

                                if (character.Path.Count > 0)
                                {
                                    if (desperate)
                                    {
                                        AddTask(character, "GoTo_Run", true, false, null, default, 0);
                                    }
                                    else
                                    {
                                        AddTask(character, "GoTo_Walk", true, false, null, default, 0);
                                    }
                                }
                                else
                                {
                                    found = false;
                                    DPathing.AddAttemptedTile(character, sink);
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                Wander(character);
            }
        }

        public static void FindFood(World world, Character character, bool desperate)
        {
            bool found = false;

            Inventory inventory = character.Inventory;

            //Do we already have something to eat?
            foreach (Item existing in inventory.Items)
            {
                Something hunger = existing.GetProperty("Hunger");
                if (hunger != null)
                {
                    found = true;
                    AddTask(character, "UseItem_" + existing.ID, false, true, TimeSpan.FromSeconds(hunger.Value * -1), default, 0);
                    break;
                }
            }

            //Does a nearby fridge have something to eat?
            if (!found)
            {
                Map map = world.Maps[0];
                Layer bottom_tiles = map.GetLayer("BottomTiles");
                Layer middle_tiles = map.GetLayer("MiddleTiles");

                List<Tile> fridges = WorldUtil.GetOwned_Furniture(character, "Fridge");
                if (fridges.Count > 0)
                {
                    Tile fridge = WorldUtil.GetClosestTile(fridges, character);
                    if (fridge != null)
                    {
                        Item item = null;
                        foreach (Item existing in fridge.Inventory.Items)
                        {
                            Something hunger = existing.GetProperty("Hunger");
                            if (hunger != null)
                            {
                                found = true;
                                item = existing;
                                break;
                            }
                        }

                        if (found)
                        {
                            if (WorldUtil.NextTo(fridge.Location, character.Location))
                            {
                                Direction direction = WorldUtil.GetFurnitureDirection(fridge, character);
                                if (direction != character.Direction)
                                {
                                    AddTask(character, "Turn", false, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), default, direction);
                                }
                                else
                                {
                                    if (fridge.Texture.Name.Contains("Used"))
                                    {
                                        InventoryUtil.TransferItem(fridge.Inventory, character.Inventory, item);
                                        AddTask(character, "CloseFridge", false, true, TimeSpan.FromMilliseconds(200), fridge.Location, direction);
                                    }
                                    else
                                    {
                                        AddTask(character, "OpenFridge", false, true, TimeSpan.FromMilliseconds(200), fridge.Location, direction);
                                    }
                                }
                            }
                            else
                            {
                                character.Path = new List<ALocation>();

                                int distance = WorldUtil.GetDistance(fridge.Location, character.Location) * 2;
                                List<ALocation> path = DPathing.GetPath(bottom_tiles, middle_tiles, character, fridge, distance, true);
                                if (path != null)
                                {
                                    character.Path.AddRange(path);

                                    if (character.Path.Count > 0)
                                    {
                                        if (desperate)
                                        {
                                            AddTask(character, "GoTo_Run", true, false, null, default, 0);
                                        }
                                        else
                                        {
                                            AddTask(character, "GoTo_Walk", true, false, null, default, 0);
                                        }
                                    }
                                    else
                                    {
                                        found = false;
                                        DPathing.AddAttemptedTile(character, fridge);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                Wander(character);
            }
        }

        public static void FindEntertainment(World world, Character character)
        {

        }

        public static void FindRest(World world, Character character)
        {

        }

        public static void GoToSleep(World world, Character character)
        {

        }

        public static void CloseDoor_Behind(Character character)
        {
            Direction direction = Direction.Nowhere;
            Vector3 location = default;

            if (character.Direction == Direction.Up)
            {
                direction = Direction.Down;
                location = new Vector3(character.Location.X, character.Location.Y + 1, 0);
            }
            else if (character.Direction == Direction.Right)
            {
                direction = Direction.Left;
                location = new Vector3(character.Location.X - 1, character.Location.Y, 0);
            }
            else if (character.Direction == Direction.Down)
            {
                direction = Direction.Up;
                location = new Vector3(character.Location.X, character.Location.Y - 1, 0);
            }
            else if (character.Direction == Direction.Left)
            {
                direction = Direction.Right;
                location = new Vector3(character.Location.X + 1, character.Location.Y, 0);
            }

            AddTask(character, "CloseDoor", true, true, TimeSpan.FromSeconds(1), location, direction);
        }

        public static void CloseWindow_Behind(Character character)
        {
            Direction direction = Direction.Nowhere;
            Vector3 location = default;

            if (character.Direction == Direction.Up)
            {
                direction = Direction.Down;
                location = new Vector3(character.Location.X, character.Location.Y + 1, 0);
            }
            else if (character.Direction == Direction.Right)
            {
                direction = Direction.Left;
                location = new Vector3(character.Location.X - 1, character.Location.Y, 0);
            }
            else if (character.Direction == Direction.Down)
            {
                direction = Direction.Up;
                location = new Vector3(character.Location.X, character.Location.Y - 1, 0);
            }
            else if (character.Direction == Direction.Left)
            {
                direction = Direction.Right;
                location = new Vector3(character.Location.X + 1, character.Location.Y, 0);
            }

            AddTask(character, "CloseWindow", true, true, TimeSpan.FromSeconds(1), location, direction);
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
                AddTask(character, "Wait", true, false, TimeSpan.FromMilliseconds(1000), default, direction);
            }
            else if (choice > 5 &&
                     choice <= 8)
            {
                AddTask(character, "Walk", true, false, null, default, direction);
            }
            else if (choice > 8)
            {
                AddTask(character, "Turn", true, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), default, direction);
            }
        }

        #endregion

        #region Start Task

        public static void Move(World world, Character character)
        {
            Task task = character.Job.CurrentTask;

            if (task.Name == "Sneak")
            {
                character.Speed = 0.5f;
            }
            else if (task.Name == "Walk")
            {
                character.Speed = 1;
            }
            else if (task.Name == "Run")
            {
                character.Speed = 2;
            }

            if (task.Direction == Direction.Up)
            {
                character.Destination = new Vector3(character.Location.X, character.Location.Y - 1, character.Location.Z);
            }
            else if (task.Direction == Direction.Right)
            {
                character.Destination = new Vector3(character.Location.X + 1, character.Location.Y, character.Location.Z);
            }
            else if (task.Direction == Direction.Down)
            {
                character.Destination = new Vector3(character.Location.X, character.Location.Y + 1, character.Location.Z);
            }
            else if (task.Direction == Direction.Left)
            {
                character.Destination = new Vector3(character.Location.X - 1, character.Location.Y, character.Location.Z);
            }

            Map map = world.Maps[0];
            if (AI.CanMove(character, map, character.Destination))
            {
                if (character.Type != "Player")
                {
                    if (task.Direction != character.Direction)
                    {
                        if (task.Direction == Direction.Up)
                        {
                            character.Animator.FaceNorth(character);
                        }
                        else if (task.Direction == Direction.Right)
                        {
                            character.Animator.FaceEast(character);
                        }
                        else if (task.Direction == Direction.Down)
                        {
                            character.Animator.FaceSouth(character);
                        }
                        else if (task.Direction == Direction.Left)
                        {
                            character.Animator.FaceWest(character);
                        }
                    }
                }

                character.Moving = true;

                Layer effect_tiles = map.GetLayer("EffectTiles");
                Layer middle_tiles = map.GetLayer("MiddleTiles");
                Tile tile = middle_tiles.GetTile(new Vector2(character.Destination.X, character.Destination.Y));
                if (tile != null)
                {
                    if (tile.Name.Contains("Window") &&
                        tile.Name.Contains("Closed"))
                    {
                        BreakWindow(effect_tiles, tile, character);
                    }
                }
            }
            else
            {
                task.EndTime = new TimeHandler(TimeManager.Now);

                if (task.Direction != character.Direction)
                {
                    AddTask(character, "Turn", false, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), character.Destination, task.Direction);
                
                    if (character.Type == "Player")
                    {
                        TimeTracker.Tick(200);
                    }
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
                            AddTask(character, "OpenWindow", false, true, TimeSpan.FromSeconds(1), character.Destination, task.Direction);
                            AddTask(character, task.Name, false, false, TimeSpan.FromSeconds(1), default, task.Direction);
                            AddTask(character, task.Name, false, false, TimeSpan.FromSeconds(1), default, task.Direction);
                        }
                        else if (tile.Name.Contains("Door") &&
                                 tile.Name.Contains("Closed"))
                        {
                            AddTask(character, "OpenDoor", false, true, TimeSpan.FromSeconds(1), character.Destination, task.Direction);
                            AddTask(character, task.Name, false, false, TimeSpan.FromSeconds(1), default, task.Direction);
                            AddTask(character, task.Name, false, false, TimeSpan.FromSeconds(1), default, task.Direction);
                        }
                    }
                }
            }
        }

        public static void GoTo(World world, Character character, string speed)
        {
            Task task = character.Job.CurrentTask;
            task.Completed = true;
            character.Job.Update(TimeManager.Now);

            if (character.Path.Count > 0)
            {
                ALocation last_path = character.Path[character.Path.Count - 1];
                Vector3 location = new Vector3(last_path.X, last_path.Y, 0);

                bool reached_destination = false;

                while (location.X == character.Location.X &&
                       location.Y == character.Location.Y)
                {
                    character.Path.Remove(last_path);
                    if (character.Path.Count > 0)
                    {
                        last_path = character.Path[character.Path.Count - 1];
                        location = new Vector3(last_path.X, last_path.Y, 0);
                    }
                    else
                    {
                        reached_destination = true;
                        break;
                    }
                }

                if (!reached_destination)
                {
                    Direction direction = WorldUtil.GetDirection(location, character.Location, false);
                    if (direction == character.Direction)
                    {
                        if (direction == Direction.Up)
                        {
                            character.Destination = new Vector3(character.Location.X, character.Location.Y - 1, character.Location.Z);
                        }
                        else if (direction == Direction.Right)
                        {
                            character.Destination = new Vector3(character.Location.X + 1, character.Location.Y, character.Location.Z);
                        }
                        else if (direction == Direction.Down)
                        {
                            character.Destination = new Vector3(character.Location.X, character.Location.Y + 1, character.Location.Z);
                        }
                        else if (direction == Direction.Left)
                        {
                            character.Destination = new Vector3(character.Location.X - 1, character.Location.Y, character.Location.Z);
                        }

                        if (WorldUtil.PassedOpenDoor(world.Maps[0].GetLayer("MiddleTiles"), character))
                        {
                            CloseDoor_Behind(character);
                        }
                        else if (WorldUtil.PassedOpenWindow(world.Maps[0].GetLayer("MiddleTiles"), character))
                        {
                            CloseWindow_Behind(character);
                        }
                        else
                        {
                            AddTask(character, speed, true, false, null, character.Destination, direction);
                            Character_StartAction(world, character);
                        }
                    }
                    else
                    {
                        AddTask(character, "Turn", false, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), character.Destination, direction);
                    }
                }
            }
        }

        public static void UseSink_Start(Character character, bool fill)
        {
            Task task = character.Job.CurrentTask;
            task.Completed = true;
            character.Job.Update(TimeManager.Now);

            Character player = Handler.GetPlayer();
            Vector2 player_location = new Vector2(player.Location.X, player.Location.Y);

            Map block_map = WorldUtil.GetCurrentMap(character);
            Layer top_tiles = block_map.GetLayer("TopTiles");

            Tile sink = WorldUtil.StandingByFurniture(top_tiles, character.Location, "Sink");
            if (sink != null)
            {
                Direction furniture_direction = WorldUtil.GetDirection(sink.Location, character.Location, false);
                if (character.Direction != furniture_direction)
                {
                    AddTask(character, "Turn", false, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(character)), character.Destination, furniture_direction);
                }
                else
                {
                    if (!sink.Texture.Name.Contains("Used"))
                    {
                        sink.Texture = AssetManager.Textures[sink.Texture.Name + "_Used"];
                        AssetManager.PlaySound_Random_AtDistance("WaterRunning", player_location, new Vector2(sink.Location.X, sink.Location.Y), 5);

                        if (fill)
                        {
                            string[] task_parts = task.Name.Split('_');
                            long item_id = long.Parse(task_parts[3]);

                            AddTask(character, "UseSink_End_" + item_id, true, true, TimeSpan.FromSeconds(3), sink.Location, furniture_direction);
                        }
                        else
                        {
                            AddTask(character, "UseSink_End_Drink", true, true, TimeSpan.FromSeconds(30), sink.Location, furniture_direction);
                        }

                        if (character.Type == "Player")
                        {
                            if (fill)
                            {
                                string[] task_parts = task.Name.Split('_');
                                long item_id = long.Parse(task_parts[3]);

                                Item container = character.Inventory.GetItem(item_id);
                                if (container != null)
                                {
                                    GameUtil.AddMessage("You fill a " + container.Name + " from a sink.");
                                }
                            }
                            else
                            {
                                GameUtil.AddMessage("You drink water from a sink.");
                            }
                        }
                        else
                        {
                            Direction direction = WorldUtil.GetDirection(sink.Location, player.Location, true);
                            if (WorldUtil.InRange(player.Location, sink.Location, 5))
                            {
                                GameUtil.AddMessage("You hear a sink running to the " + direction.ToString() + ".");
                            }
                        }
                    }
                }
            }
        }

        public static void Attacking(Character attacker)
        {
            Task task = attacker.Job.CurrentTask;
            task.Completed = true;
            attacker.Job.Update(TimeManager.Now);

            Character target = WorldUtil.GetCharacter_Target(attacker);
            if (target != null)
            {
                Dictionary<string, string> attackChoice = AI.AttackChoice(attacker);
                string attackType = attackChoice.ElementAt(0).Value;

                if (CombatUtil.InRange(attacker, target, attackType))
                {
                    AddAttack(attacker, target, attackChoice);
                }
                else
                {
                    Direction direction = WorldUtil.GetDirection(target.Location, attacker.Location, false);
                    if (attacker.Direction != direction)
                    {
                        AddTask(attacker, "Turn", true, true, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(attacker)), default, direction);
                    }
                    else
                    {
                        AddTask(attacker, "Walk", true, false, null, default, direction);
                    }
                }
            }
        }

        #endregion

        #region Do Task

        public static void Wait(Character character)
        {
            CharacterUtil.Rest(character);
        }

        public static void Sleep(Character character)
        {
            CharacterUtil.Sleep(character);
        }

        #endregion

        #region End Task

        public static void Turn(Character character)
        {
            Task task = character.Job.Get_CurrentTask();

            if (task.Direction != character.Direction)
            {
                if (task.Direction == Direction.Up)
                {
                    character.Animator.FaceNorth(character);
                }
                else if (task.Direction == Direction.Right)
                {
                    character.Animator.FaceEast(character);
                }
                else if (task.Direction == Direction.Down)
                {
                    character.Animator.FaceSouth(character);
                }
                else if (task.Direction == Direction.Left)
                {
                    character.Animator.FaceWest(character);
                }
            }
        }

        public static void ToggleLight(World world, Character character)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();
            AssetManager.PlaySound_Random_AtDistance("Click", new Vector2(player.Location.X, player.Location.Y), location, 2);

            Map map = world.Maps[0];
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);
            tile.IsLightSource = !tile.IsLightSource;

            if (character.Type == "Player")
            {
                if (tile.IsLightSource)
                {
                    GameUtil.AddMessage("You turned on a light.");
                }
                else
                {
                    GameUtil.AddMessage("You turned off a light.");
                }
            }
        }

        public static void ToggleTV(World world, Character character)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();
            AssetManager.PlaySound_Random_AtDistance("Click", new Vector2(player.Location.X, player.Location.Y), location, 2);

            Map map = world.Maps[0];
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);
            tile.IsLightSource = !tile.IsLightSource;

            if (character.Type == "Player")
            {
                if (tile.IsLightSource)
                {
                    GameUtil.AddMessage("You turned on a TV.");
                }
                else
                {
                    GameUtil.AddMessage("You turned off a TV.");
                }
            }
        }

        public static void OpenDoor(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            Map map = world.Maps[0];
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);

            if (character.Direction == Direction.Up)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (character.Direction == Direction.Right)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }
            else if (character.Direction == Direction.Down)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y + (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_WestEast_Open";
            }
            else if (character.Direction == Direction.Left)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Region = new Region(tile.Region.X + (tile.Region.Width / 2), tile.Region.Y - (tile.Region.Height / 2), tile.Region.Width, tile.Region.Height);
                tile.Name = "Door_NorthSouth_Open";
            }

            tile.BlocksMovement = false;

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You quietly opened a door.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You opened a door.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You loudly opened a door.");
                }
            }
            else
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, task.Location, 2))
                {
                    GameUtil.AddMessage("You hear a door quietly open to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, task.Location, 4))
                {
                    GameUtil.AddMessage("You hear a door open to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, task.Location, 8))
                {
                    GameUtil.AddMessage("You hear a door loudly open to the " + direction.ToString() + ".");
                }
            }
        }

        public static void CloseDoor(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            Map map = world.Maps[0];
            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);

            Tile bottom_tile = bottom_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
            tile.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);

            if (character.Direction == Direction.Up)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Name = "Door_WestEast_Closed";
            }
            else if (character.Direction == Direction.Right)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Name = "Door_NorthSouth_Closed";
            }
            else if (character.Direction == Direction.Down)
            {
                tile.Texture = AssetManager.Textures["Door_WestEast"];
                tile.Name = "Door_WestEast_Closed";
            }
            else if (character.Direction == Direction.Left)
            {
                tile.Texture = AssetManager.Textures["Door_NorthSouth"];
                tile.Name = "Door_NorthSouth_Closed";
            }

            tile.BlocksMovement = true;

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You softly closed a door.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You closed a door.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You slammed a door shut.");
                }
            }
            else
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, task.Location, 2))
                {
                    GameUtil.AddMessage("You hear a door softly closed to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, task.Location, 4))
                {
                    GameUtil.AddMessage("You hear a door close to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, task.Location, 8))
                {
                    GameUtil.AddMessage("You hear a door slammed shut to the " + direction.ToString() + ".");
                }
            }
        }

        public static void OpenWindow(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowOpen", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowOpen", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowOpen", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            Map map = world.Maps[0];
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);

            if (character.Direction == Direction.Up ||
                character.Direction == Direction.Down)
            {
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width / 8, tile.Region.Height);
                tile.Name = "Window_WestEast_Open";
            }
            else if (character.Direction == Direction.Right ||
                     character.Direction == Direction.Left)
            {
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height / 8);
                tile.Name = "Window_NorthSouth_Open";
            }

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You quietly opened a window.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You opened a window.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You loudly opened a window.");
                }
            }
            else
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, task.Location, 2))
                {
                    GameUtil.AddMessage("You hear a window quietly open to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, task.Location, 4))
                {
                    GameUtil.AddMessage("You hear a window open to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, task.Location, 8))
                {
                    GameUtil.AddMessage("You hear a window loudly open to the " + direction.ToString() + ".");
                }
            }
        }

        public static void CloseWindow(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowClose", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowClose", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("WindowClose", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            Map map = world.Maps[0];
            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);

            Tile bottom_tile = bottom_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
            tile.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);

            if (character.Direction == Direction.Up ||
                character.Direction == Direction.Down)
            {
                tile.Name = "Window_WestEast_Closed";
            }
            else if (character.Direction == Direction.Right ||
                     character.Direction == Direction.Left)
            {
                tile.Name = "Window_NorthSouth_Closed";
            }

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You softly closed a window.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You closed a window.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You slammed a window shut.");
                }
            }
            else
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, task.Location, 2))
                {
                    GameUtil.AddMessage("You hear a window softly closed to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, task.Location, 4))
                {
                    GameUtil.AddMessage("You hear a window close to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, task.Location, 8))
                {
                    GameUtil.AddMessage("You hear a window slammed shut to the " + direction.ToString() + ".");
                }
            }
        }

        public static void OpenFridge(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorOpen", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            Map map = world.Maps[0];
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);

            if (character.Direction == Direction.Up &&
                tile.Direction == Direction.Down)
            {
                tile.Texture = AssetManager.Textures["Fridge_South_Used"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, Main.Game.TileSize.Y * 2);
            }
            else if (character.Direction == Direction.Right &&
                     tile.Direction == Direction.Left)
            {
                tile.Texture = AssetManager.Textures["Fridge_West_Used"];
                tile.Region = new Region(tile.Region.X - Main.Game.TileSize.X, tile.Region.Y, Main.Game.TileSize.X * 2, tile.Region.Height);
            }
            else if (character.Direction == Direction.Down &&
                     tile.Direction == Direction.Up)
            {
                tile.Texture = AssetManager.Textures["Fridge_North_Used"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y - Main.Game.TileSize.Y, tile.Region.Width, Main.Game.TileSize.Y * 2);
            }
            else if (character.Direction == Direction.Left &&
                     tile.Direction == Direction.Right)
            {
                tile.Texture = AssetManager.Textures["Fridge_East_Used"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y, Main.Game.TileSize.X * 2, tile.Region.Height);
            }
            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You quietly opened a fridge.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You opened a fridge.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You loudly opened a fridge.");
                }
            }
            else
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, task.Location, 2))
                {
                    GameUtil.AddMessage("You hear a fridge quietly open to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, task.Location, 4))
                {
                    GameUtil.AddMessage("You hear a fridge open to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, task.Location, 8))
                {
                    GameUtil.AddMessage("You hear a fridge loudly open to the " + direction.ToString() + ".");
                }
            }
        }

        public static void CloseFridge(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Character player = Handler.GetPlayer();

            if (loudness == 1)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", new Vector2(player.Location.X, player.Location.Y), location, 2);
            }
            else if (loudness == 2)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", new Vector2(player.Location.X, player.Location.Y), location, 4);
            }
            else if (loudness == 3)
            {
                AssetManager.PlaySound_Random_AtDistance("DoorClose", new Vector2(player.Location.X, player.Location.Y), location, 8);
            }

            Map map = world.Maps[0];
            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(location);

            if (character.Direction == Direction.Up &&
                tile.Direction == Direction.Down)
            {
                tile.Texture = AssetManager.Textures["Fridge_South"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, Main.Game.TileSize.Y);
            }
            else if (character.Direction == Direction.Right &&
                     tile.Direction == Direction.Left)
            {
                tile.Texture = AssetManager.Textures["Fridge_West"];
                tile.Region = new Region(tile.Region.X + Main.Game.TileSize.X, tile.Region.Y, Main.Game.TileSize.X, tile.Region.Height);
            }
            else if (character.Direction == Direction.Down &&
                     tile.Direction == Direction.Up)
            {
                tile.Texture = AssetManager.Textures["Fridge_North"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y + Main.Game.TileSize.Y, tile.Region.Width, Main.Game.TileSize.Y);
            }
            else if (character.Direction == Direction.Left &&
                     tile.Direction == Direction.Right)
            {
                tile.Texture = AssetManager.Textures["Fridge_East"];
                tile.Region = new Region(tile.Region.X, tile.Region.Y, Main.Game.TileSize.X, tile.Region.Height);
            }
            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);

            tile.BlocksMovement = true;

            if (character.Type == "Player")
            {
                if (loudness == 1)
                {
                    GameUtil.AddMessage("You softly closed a fridge.");
                }
                else if (loudness == 2)
                {
                    GameUtil.AddMessage("You closed a fridge.");
                }
                else if (loudness == 3)
                {
                    GameUtil.AddMessage("You slammed a fridge shut.");
                }
            }
            else
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);

                if (loudness == 1 &&
                    WorldUtil.InRange(player.Location, task.Location, 2))
                {
                    GameUtil.AddMessage("You hear a fridge softly closed to the " + direction.ToString() + ".");
                }
                else if (loudness == 2 &&
                         WorldUtil.InRange(player.Location, task.Location, 4))
                {
                    GameUtil.AddMessage("You hear a fridge close to the " + direction.ToString() + ".");
                }
                else if (loudness == 3 &&
                         WorldUtil.InRange(player.Location, task.Location, 8))
                {
                    GameUtil.AddMessage("You hear a fridge slammed shut to the " + direction.ToString() + ".");
                }
            }
        }

        public static void BreakWindow(Layer effect_tiles, Tile tile, Character character)
        {
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

            Vector2 location = new Vector2(character.Destination.X, character.Destination.Y);
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

            Character player = Handler.GetPlayer();
            AssetManager.PlaySound_Random_AtDistance("GlassBreak", new Vector2(player.Location.X, player.Location.Y), location, 10);

            Tile new_tile = effect_tiles.GetTile(location);
            if (new_tile != null)
            {
                new_tile.Name = "BrokenGlass_" + character.Direction.ToString();
                new_tile.Texture = AssetManager.Textures[new_tile.Name];
                new_tile.Image = new Rectangle(0, 0, new_tile.Texture.Width, new_tile.Texture.Height);
                new_tile.Visible = true;
            }

            if (character.Type == "Player")
            {
                TimeTracker.Tick(1000);
                GameUtil.AddMessage("You broke a window.");
            }
        }

        public static void EndSearch(World world, Character character, int loudness)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Map map = world.Maps[0];
            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Layer middle_tiles = map.GetLayer("MiddleTiles");

            Tile tile = WorldUtil.GetFurniture(middle_tiles, new Vector3(location.X, location.Y, 0));
            if ((tile != null && tile.Texture == null) ||
                tile == null)
            {
                tile = bottom_tiles.GetTile(location);
            }

            if (tile.Texture != null)
            {
                if (tile.Inventory.Items.Count > 0)
                {
                    TimeManager.Paused = true;

                    Handler.Trading = true;
                    Handler.Trading_InventoryID.Add(tile.Inventory.ID);

                    Menu main = MenuManager.GetMenu("Inventory");
                    main.Load();
                    main.Active = true;
                    main.Visible = true;
                }
                else
                {
                    if (loudness == 1)
                    {
                        GameUtil.AddMessage("You quietly searched the " + WorldUtil.GetTile_Name(tile) + ", but found nothing.");
                    }
                    else if (loudness == 2)
                    {
                        GameUtil.AddMessage("You searched the " + WorldUtil.GetTile_Name(tile) + ", but found nothing.");
                    }
                    else if (loudness == 3)
                    {
                        GameUtil.AddMessage("You loudly searched the " + WorldUtil.GetTile_Name(tile) + ", but found nothing.");
                    }
                }
            }
        }

        public static void UseItem(World world, Character character)
        {
            Task task = character.Job.Get_CurrentTask();

            Inventory inventory = null;
            Item item = null;

            foreach (Item existing in character.Inventory.Items)
            {
                if (InventoryUtil.IsContainer(existing))
                {
                    foreach (Item container_item in existing.Inventory.Items)
                    {
                        if (task.Name == "UseItem_" + container_item.ID)
                        {
                            item = container_item;
                            inventory = existing.Inventory;
                            break;
                        }
                    }

                    if (item != null)
                    {
                        break;
                    }
                }
                else if (task.Name == "UseItem_" + existing.ID)
                {
                    item = existing;
                    inventory = character.Inventory;
                    break;
                }
            }

            if (item == null)
            {
                if (Handler.Trading)
                {
                    Inventory other_inventory = InventoryManager.GetInventory(Handler.Trading_InventoryID[Handler.Trading_InventoryID.Count - 1]);
                    foreach (Item existing in other_inventory.Items)
                    {
                        if (InventoryUtil.IsContainer(existing))
                        {
                            foreach (Item container_item in existing.Inventory.Items)
                            {
                                if (task.Name == "UseItem_" + container_item.ID)
                                {
                                    item = container_item;
                                    inventory = existing.Inventory;
                                    break;
                                }
                            }

                            if (item != null)
                            {
                                break;
                            }
                        }
                        else if (task.Name == "UseItem_" + existing.ID)
                        {
                            item = existing;
                            inventory = other_inventory;
                            break;
                        }
                    }
                }
            }

            if (item != null)
            {
                int eat = 0;
                int drink = 0;

                if (InventoryUtil.IsFood(item) ||
                    item.Task == "Inject")
                {
                    Something hunger = item.GetProperty("Hunger");
                    if (hunger != null)
                    {
                        eat++;
                        character.GetStat("Hunger").IncreaseValue(hunger.Value);
                    }

                    Something thirst = item.GetProperty("Thirst");
                    if (thirst != null)
                    {
                        drink++;
                        character.GetStat("Thirst").IncreaseValue(thirst.Value);
                    }

                    Something stamina = item.GetProperty("Stamina");
                    if (stamina != null)
                    {
                        eat++;
                        drink++;
                        character.GetStat("Stamina").IncreaseValue(stamina.Value);
                    }

                    Something consciousness = item.GetProperty("Consciousness");
                    if (consciousness != null)
                    {
                        eat++;
                        character.GetStat("Consciousness").IncreaseValue(consciousness.Value);
                    }

                    Something paranoia = item.GetProperty("Paranoia");
                    if (paranoia != null)
                    {
                        eat++;
                        character.GetStat("Paranoia").IncreaseValue(paranoia.Value);
                    }

                    Something bladder = item.GetProperty("Bladder");
                    if (bladder != null)
                    {
                        drink++;
                        character.GetStat("Bladder").IncreaseValue(bladder.Value);
                    }

                    Something blood = item.GetProperty("Blood");
                    if (blood != null)
                    {
                        drink++;
                        character.GetStat("Blood").IncreaseValue(blood.Value);
                    }

                    Something pain = item.GetProperty("Pain");
                    if (pain != null)
                    {
                        eat++;

                        Something painKiller = character.GetStatusEffect("Painkiller");
                        if (painKiller != null)
                        {
                            painKiller.IncreaseValue(pain.Value);
                        }
                        else
                        {
                            character.StatusEffects.Add(new Something
                            {
                                Name = "Painkiller",
                                Value = pain.Value
                            });
                        }
                    }

                    Something poison = item.GetProperty("Poison");
                    if (poison != null)
                    {
                        drink++;

                        Something poisoned = character.GetStatusEffect("Poisoned");
                        if (poisoned != null)
                        {
                            poisoned.IncreaseValue(poison.Value);
                        }
                        else
                        {
                            character.StatusEffects.Add(new Something
                            {
                                Name = "Poisoned",
                                Value = poison.Value
                            });
                        }
                    }

                    inventory.Items.Remove(item);

                    Inventory assets = InventoryManager.GetInventory("Assets");
                    Item new_item = null;

                    if (item.Name.Contains("Bottle"))
                    {
                        new_item = assets.GetItem("Bottle");
                    }
                    else if (item.Name.Contains("Bucket"))
                    {
                        new_item = assets.GetItem("Bucket");
                    }
                    else if (item.Name.Contains("Cannister"))
                    {
                        new_item = assets.GetItem("Cannister");
                    }
                    else if (item.Name.Contains("Syringe"))
                    {
                        new_item = assets.GetItem("Syringe");
                    }

                    if (new_item != null)
                    {
                        Item copy = InventoryUtil.CopyItem(new_item, true);
                        character.Inventory.Items.Add(copy);
                    }
                }

                if (character.Type == "Player")
                {
                    if (InventoryUtil.IsFood(item))
                    {
                        if (item.Task == "Inject")
                        {
                            GameUtil.AddMessage("You injected a " + item.Name + ".");
                        }
                        else if (drink >= eat)
                        {
                            if (GameUtil.NameStartsWithVowel(item.Name))
                            {
                                GameUtil.AddMessage("You drank an " + item.Name + ".");
                            }
                            else
                            {
                                GameUtil.AddMessage("You drank a " + item.Name + ".");
                            }
                        }
                        else if (eat > drink)
                        {
                            if (GameUtil.NameStartsWithVowel(item.Name))
                            {
                                GameUtil.AddMessage("You ate an " + item.Name + ".");
                            }
                            else
                            {
                                GameUtil.AddMessage("You ate a " + item.Name + ".");
                            }
                        }
                    }
                }
            }
            else
            {
                AbortTask(character);
            }
        }

        public static void UseSink_End(World world, Character character, bool fill)
        {
            Task task = character.Job.Get_CurrentTask();

            Vector2 location = new Vector2(task.Location.X, task.Location.Y);

            Map block_map = WorldUtil.GetCurrentMap(character);
            Layer top_tiles = block_map.GetLayer("TopTiles");

            Tile sink = WorldUtil.GetFurniture(top_tiles, new Vector3(location.X, location.Y, 0));
            if (sink != null)
            {
                if (sink.Name.Contains("Sink"))
                {
                    string[] name_parts = sink.Texture.Name.Split('_');
                    sink.Texture = AssetManager.Textures[name_parts[0] + "_" + name_parts[1]];
                }
            }

            if (fill)
            {
                string[] task_parts = task.Name.Split('_');
                long item_id = long.Parse(task_parts[3]);

                Item container = character.Inventory.GetItem(item_id);
                if (container != null)
                {
                    string name = container.Name + " of Water";

                    Inventory assets = InventoryManager.GetInventory("Assets");
                    Item asset_container = assets.GetItem(name);
                    if (asset_container != null)
                    {
                        Item new_container = InventoryUtil.CopyItem(asset_container, true);
                        character.Inventory.Items.Remove(container);
                        character.Inventory.Items.Add(new_container);
                    }
                }
            }
            else
            {
                Something thirst = character.GetStat("Thirst");
                thirst.DecreaseValue(30);
            }

            SoundManager.StopSound("WaterRunning");
        }

        public static void Attack(Character attacker)
        {
            Task task = attacker.Job.CurrentTask;

            Character player = Handler.GetPlayer();
            Character defender = WorldUtil.GetCharacter_Target(attacker);

            bool attacker_visible = WorldUtil.Location_IsVisible(defender.ID, attacker.Location);
            bool attacker_visible_to_player = WorldUtil.Location_IsVisible(player.ID, attacker.Location);

            bool hit = false;

            bool CharacterAtLocation = WorldUtil.IsCharacter_AtLocation(defender.ID, task.Location);
            if (CharacterAtLocation)
            {
                float chance = CombatUtil.ChanceToHitBodyPart(attacker, defender, task.Assignment, task.Name);
                if (task.Location.X == defender.Location.X &&
                    task.Location.Y == defender.Location.Y)
                {
                    if (Utility.RandomPercent(chance))
                    {
                        hit = true;
                    }
                }
            }

            int sound_distance;
            if (hit)
            {
                sound_distance = CombatUtil.AttackSound_Hit(attacker, task.Name);
            }
            else
            {
                sound_distance = CombatUtil.AttackSound_Miss(attacker, task.Name);
            }

            if (attacker.Type != "Player" &&
                !attacker_visible_to_player &&
                sound_distance > 0)
            {
                Direction direction = WorldUtil.GetDirection(task.Location, player.Location, true);
                if (WorldUtil.InRange(player.Location, attacker.Location, sound_distance))
                {
                    if (task.Name == "Shoot")
                    {
                        GameUtil.AddMessage("You hear a gunshot to the " + direction.ToString() + ".");
                    }
                }
            }

            bool react = false;
            if (hit)
            {
                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage(attacker.Name + " hit your " + task.Assignment.ToLower() + " with their " + task.Type.ToLower() + ".");
                    CombatUtil.Update_Player_BodyStat(player, CharacterUtil.BodyPartFromName(task.Assignment));
                }
                else if (attacker.Type == "Player")
                {
                    GameUtil.AddMessage("You hit " + defender.Name + " in the " + task.Assignment.ToLower() + " with your " + task.Type.ToLower() + ".");
                }

                CombatUtil.DoDamage(attacker, defender, task.Type, task.Name, task.Assignment);

                if (defender.Type != "Player")
                {
                    react = true;
                }
            }
            else
            {
                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage(attacker.Name + " missed your " + task.Assignment.ToLower() + " with their " + task.Type.ToLower() + ".");
                }
                else if (attacker.Type == "Player")
                {
                    GameUtil.AddMessage("You missed hitting " + defender.Name + " in the " + task.Assignment.ToLower() + " with your " + task.Type.ToLower() + ".");
                }

                if (attacker_visible ||
                    WorldUtil.InRange(defender.Location, attacker.Location, sound_distance))
                {
                    if (defender.Type != "Player")
                    {
                        react = true;
                    }
                }
            }

            if (react)
            {
                string reaction = AI.ReactToAttack(attacker, defender);
                AddTask(defender, reaction, true, false, null, default, 0);
            }
        }

        public static void Examine(Character character)
        {
            if (character.Type == "Player")
            {
                WorldUtil.GenDescription();
            }
        }

        #endregion

        public static void AddAttack(Character attacker, Character defender, Dictionary<string, string> attackChoice)
        {
            Character player = Handler.GetPlayer();
            attacker.Target_ID = defender.ID;

            CryptoRandom random = new CryptoRandom();
            string part_target = CharacterUtil.BodyPartToName(Handler.BodyParts[random.Next(0, Handler.BodyParts.Length)]);

            string attackingWith = attackChoice.ElementAt(0).Key;
            string attackType = attackChoice.ElementAt(0).Value;

            Task task = new Task();
            task.Name = attackType;
            task.Assignment = part_target;
            task.Type = attackingWith;
            task.OwnerIDs.Add(attacker.ID);
            task.Keep_On_Completed = true;
            task.StartTime = new TimeHandler(TimeManager.Now);
            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CombatUtil.AttackTime(attacker, attackType)));
            task.Location = new Vector3(defender.Location.X, defender.Location.Y, 0);

            bool attacker_visible_to_player = WorldUtil.Location_IsVisible(player.ID, attacker.Location);
            bool defender_visible_to_player = WorldUtil.Location_IsVisible(player.ID, defender.Location);

            if (attacker_visible_to_player)
            {
                if (defender.Type != "Player" &&
                    defender_visible_to_player)
                {
                    if (GameUtil.NameStartsWithVowel(task.Type))
                    {
                        GameUtil.AddMessage("You see " + attacker.Name + " " + task.Name.ToLower() + " an " + task.Type.ToLower() + " at " + defender.Name + ".");
                    }
                    else
                    {
                        GameUtil.AddMessage("You see " + attacker.Name + " " + task.Name.ToLower() + " a " + task.Type.ToLower() + " at " + defender.Name + ".");
                    }
                }
                else if (defender.Type == "Player")
                {
                    if (GameUtil.NameStartsWithVowel(task.Type))
                    {
                        GameUtil.AddMessage("You see " + attacker.Name + " " + task.Name.ToLower() + " an " + task.Type.ToLower() + " at your " + task.Assignment.ToLower() + ".");
                    }
                    else
                    {
                        GameUtil.AddMessage("You see " + attacker.Name + " " + task.Name.ToLower() + " a " + task.Type.ToLower() + " at your " + task.Assignment.ToLower() + ".");
                    }
                }
            }

            attacker.Job.Tasks.Add(task);
        }

        public static void AddTask(Character character, string name, bool started, bool keep_on_completed, TimeSpan? time_span, Vector3 location, Direction direction)
        {
            Task task = new Task();
            task.Name = name;
            task.OwnerIDs = GameUtil.OwnerIDs(character);
            task.Started = started;
            task.Keep_On_Completed = keep_on_completed;
            task.StartTime = new TimeHandler(TimeManager.Now);
            task.Location = location;
            task.Direction = direction;

            if (time_span.HasValue)
            {
                task.EndTime = new TimeHandler(TimeManager.Now, (TimeSpan)time_span);

                if (name != "Wait" &&
                    name != "Turn")
                {
                    task.TaskBar = CharacterUtil.GenTaskbar(character, (int)time_span.Value.TotalMilliseconds);
                }
            }

            character.Job.Tasks.Add(task);
        }

        public static void AbortTask(Character character)
        {
            character.Animator.Reset(character);
            character.Path.Clear();
            character.Job.Tasks.Clear();

            character.InCombat = false;
            character.Interacting = false;

            if (character.Type == "Player")
            {
                //MenuManager.GetMenu("UI").GetPicture("Interact").Visible = false;
            }

            character.Target_ID = -1;
        }

        public static void Interact(Tile tile, Character player)
        {
            Task task = new Task();
            task.OwnerIDs.Add(player.ID);
            task.Keep_On_Completed = true;
            task.StartTime = new TimeHandler(TimeManager.Now);

            if (player.Direction == Direction.Up)
            {
                task.Location = new Vector3(player.Location.X, player.Location.Y - 1, 0);
            }
            else if (player.Direction == Direction.Right)
            {
                task.Location = new Vector3(player.Location.X + 1, player.Location.Y, 0);
            }
            else if (player.Direction == Direction.Down)
            {
                task.Location = new Vector3(player.Location.X, player.Location.Y + 1, 0);
            }
            else if (player.Direction == Direction.Left)
            {
                task.Location = new Vector3(player.Location.X - 1, player.Location.Y, 0);
            }

            bool found = false;

            if (tile != null)
            {
                if (tile.Name.Contains("Sink"))
                {
                    found = true;
                    task.Name = "UseSink_Start_Drink";
                    task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(1));
                }
                else if (tile.Name.Contains("Lamp"))
                {
                    found = true;
                    task.Name = "ToggleLight";
                    task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                }
                else if (tile.Name.Contains("TV"))
                {
                    found = true;
                    task.Name = "ToggleTV";
                    task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                }
                else if (tile.Name.Contains("Door"))
                {
                    if (tile.Name.Contains("Closed"))
                    {
                        found = true;

                        if (InputManager.KeyDown("Crouch"))
                        {
                            task.Name = "Quiet_OpenDoor";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 4000);
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            task.Name = "Loud_OpenDoor";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(250));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 250);
                        }
                        else
                        {
                            task.Name = "OpenDoor";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                        }
                    }
                    else if (tile.Name.Contains("Open"))
                    {
                        found = true;

                        if (InputManager.KeyDown("Crouch"))
                        {
                            task.Name = "Quiet_CloseDoor";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 4000);
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            task.Name = "Loud_CloseDoor";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(250));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 250);
                        }
                        else
                        {
                            task.Name = "CloseDoor";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                        }
                    }
                }
                else if (tile.Name.Contains("Window") &&
                         !tile.Name.Contains("Broken"))
                {
                    if (tile.Name.Contains("Closed"))
                    {
                        found = true;

                        if (InputManager.KeyDown("Crouch"))
                        {
                            task.Name = "Quiet_OpenWindow";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 4000);
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            task.Name = "Loud_OpenWindow";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(250));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 250);
                        }
                        else
                        {
                            task.Name = "OpenWindow";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                        }
                    }
                    else if (tile.Name.Contains("Open"))
                    {
                        found = true;

                        if (InputManager.KeyDown("Crouch"))
                        {
                            task.Name = "Quiet_CloseWindow";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(4));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 4000);
                        }
                        else if (InputManager.KeyDown("Run"))
                        {
                            task.Name = "Loud_CloseWindow";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(250));
                            task.TaskBar = CharacterUtil.GenTaskbar(player, 250);
                        }
                        else
                        {
                            task.Name = "CloseWindow";
                            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                        }
                    }
                }
                else if (WorldUtil.CanSearch(tile.Name))
                {
                    found = true;

                    if (InputManager.KeyDown("Crouch"))
                    {
                        task.Name = "Quiet_Search";
                        task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(100));
                        task.TaskBar = CharacterUtil.GenTaskbar(player, 100000);
                        GameUtil.AddMessage("You started quietly searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else if (InputManager.KeyDown("Run"))
                    {
                        task.Name = "Loud_Search";
                        task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(1));
                        task.TaskBar = CharacterUtil.GenTaskbar(player, 1000);
                        GameUtil.AddMessage("You started quickly searching the " + WorldUtil.GetTile_Name(tile) + ".");
                    }
                    else
                    {
                        task.Name = "Search";
                        task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10));
                        task.TaskBar = CharacterUtil.GenTaskbar(player, 10000);
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

            if (!string.IsNullOrEmpty(task.Name) &&
                found)
            {
                player.Job.Tasks.Add(task);
            }
            else
            {
                task.Dispose();
            }
        }

        #endregion
    }
}
