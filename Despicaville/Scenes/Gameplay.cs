using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Characters;
using OP_Engine.Inputs;
using OP_Engine.Tiles;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Jobs;
using OP_Engine.Controls;
using OP_Engine.Time;
using OP_Engine.Inventories;
using OP_Engine.Rendering;

using Despicaville.Util;

namespace Despicaville.Scenes
{
    public class Gameplay : Scene
    {
        #region Variables



        #endregion

        #region Constructor

        public Gameplay(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Gameplay";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                if (SoundManager.NeedMusic)
                {
                    if (TimeManager.Now.Hours >= 5 && TimeManager.Now.Hours <= 21)
                    {
                        AssetManager.PlayMusic_Random("Day", false);
                    }
                    else
                    {
                        AssetManager.PlayMusic_Random("Night", false);
                    }
                }

                UpdateMenuControls();

                if (!TimeManager.Paused)
                {
                    Character player = Handler.GetPlayer();
                    if (player != null)
                    {
                        WorldUtil.UpdateWorldMap(player);

                        if (InputManager.KeyPressed("Map"))
                        {
                            Handler.WorldMap_Visible = !Handler.WorldMap_Visible;
                        }

                        if (!player.Dead)
                        {
                            if (!player.Moving)
                            {
                                if (!player.Unconscious)
                                {
                                    UpdatePlayerControls(player);
                                }
                                else
                                {
                                    TimeTracker.Tick(Handler.ActionRate);
                                    CharacterUtil.Sleep(player);
                                }
                            }

                            UpdatePlayer(player);
                            UpdateCitizens();
                        }
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                if (World.Visible)
                {
                    Army characters = CharacterManager.GetArmy("Characters");
                    Character player = Handler.GetPlayer();

                    List<Tile> trees = new List<Tile>();

                    Map map = World.Maps[0];

                    Layer bottom_tiles = map.GetLayer("BottomTiles");
                    Layer room_tiles = map.GetLayer("RoomTiles");
                    Layer middle_tiles = map.GetLayer("MiddleTiles");
                    Layer top_tiles = map.GetLayer("TopTiles");
                    Layer effect_tiles = map.GetLayer("EffectTiles");

                    int bottom_count = bottom_tiles.Tiles.Count;
                    for (int i = 0; i < bottom_count; i++)
                    {
                        Tile bottom_tile = bottom_tiles.Tiles[i];
                        Vector2 location = new Vector2(bottom_tile.Location.X, bottom_tile.Location.Y);

                        bottom_tile.Update(resolution);

                        if (bottom_tile.InView)
                        {
                            if (bottom_tile.InSight)
                            {
                                bottom_tile.Draw(spriteBatch, resolution, color);
                            }
                            else
                            {
                                bottom_tile.Draw(spriteBatch, resolution, color * 0.95f);
                            }

                            if (Main.Game.Debugging)
                            {
                                Tile room_tile = room_tiles.GetTile(location);
                                if (room_tile != null)
                                {
                                    if (room_tile.Texture != null)
                                    {
                                        room_tile.Update(resolution);

                                        if (room_tile.InView)
                                        {
                                            room_tile.Draw(spriteBatch, resolution, color);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    int middle_count = middle_tiles.Tiles.Count;
                    for (int i = 0; i < middle_count; i++)
                    {
                        Tile middle_tile = middle_tiles.Tiles[i];
                        if (middle_tile.Texture != null)
                        {
                            middle_tile.Update(resolution);

                            if (middle_tile.InView)
                            {
                                if (middle_tile.Name.Contains("Tree"))
                                {
                                    trees.Add(middle_tile);
                                }
                                else
                                {
                                    if (middle_tile.InSight)
                                    {
                                        middle_tile.Draw(spriteBatch, resolution, color);
                                    }
                                    else
                                    {
                                        middle_tile.Draw(spriteBatch, resolution, color * 0.95f);
                                    }
                                }
                            }
                        }
                    }

                    int top_count = top_tiles.Tiles.Count;
                    for (int i = 0; i < top_count; i++)
                    {
                        Tile top_tile = top_tiles.Tiles[i];
                        if (top_tile.Texture != null)
                        {
                            top_tile.Update(resolution);

                            if (top_tile.InView)
                            {
                                if (top_tile.InSight)
                                {
                                    top_tile.Draw(spriteBatch, resolution, color);
                                }
                                else
                                {
                                    top_tile.Draw(spriteBatch, resolution, color * 0.95f);
                                }
                            }
                        }
                    }

                    int effect_count = effect_tiles.Tiles.Count;
                    for (int i = 0; i < effect_count; i++)
                    {
                        Tile effect_tile = effect_tiles.Tiles[i];
                        if (effect_tile.Texture != null)
                        {
                            effect_tile.Update(resolution);

                            if (effect_tile.InView)
                            {
                                if (effect_tile.InSight)
                                {
                                    effect_tile.Draw(spriteBatch, resolution, color);
                                }
                                else
                                {
                                    effect_tile.Draw(spriteBatch, resolution, color * 0.95f);
                                }
                            }
                        }
                    }

                    for (float i = -(Main.Game.TileSize.X * 3); i < resolution.Y + (Main.Game.TileSize.X * 3); i += 0.5f)
                    {
                        foreach (Squad squad in characters.Squads)
                        {
                            foreach (Character character in squad.Characters)
                            {
                                if (character.Region.Y == i)
                                {
                                    if (character.Type == "Player" ||
                                        character.InSight)
                                    {
                                        if (character.Unconscious ||
                                            character.Dead)
                                        {
                                            Texture2D body_texture = AssetManager.Textures["Down_" + character.Texture.Name];
                                            if (body_texture != null)
                                            {
                                                Rectangle image = new Rectangle();

                                                if (character.Direction == Direction.Up)
                                                {
                                                    image = new Rectangle(0, 0, 128, 128);
                                                }
                                                else if (character.Direction == Direction.Right)
                                                {
                                                    image = new Rectangle(128, 0, 128, 128);
                                                }
                                                else if (character.Direction == Direction.Down)
                                                {
                                                    image = new Rectangle(256, 0, 128, 128);
                                                }
                                                else if (character.Direction == Direction.Left)
                                                {
                                                    image = new Rectangle(384, 0, 128, 128);
                                                }

                                                Rectangle region = character.Region.ToRectangle;
                                                spriteBatch.Draw(AssetManager.Textures["Down_" + character.Texture.Name], region, image, Color.White);

                                                foreach (Item item in character.Inventory.Items)
                                                {
                                                    Texture2D texture = AssetManager.Textures["Down_" + item.Type];
                                                    if (texture != null)
                                                    {
                                                        spriteBatch.Draw(texture, region, image, item.DrawColor);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            character.Draw(spriteBatch, resolution, color);

                                            Task task = character.Job.CurrentTask;
                                            if (task != null)
                                            {
                                                ProgressBar taskbar = task.TaskBar;
                                                if (taskbar != null)
                                                {
                                                    taskbar.Draw(spriteBatch);
                                                }
                                            }
                                        }
                                    }
                                    else if (WorldUtil.InRange(character.Location, player.Location, Handler.HearingDistance) &&
                                             character.Moving)
                                    {
                                        Rectangle region = character.Region.ToRectangle;
                                        spriteBatch.Draw(character.Texture, region, character.Image, Color.Black * 0.25f);

                                        foreach (Item item in character.Inventory.Items)
                                        {
                                            if (item.Visible &&
                                                item.Texture != null)
                                            {
                                                spriteBatch.Draw(item.Texture, region, character.Image, Color.Black * 0.25f);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    int trees_count = trees.Count;
                    for (int i = 0; i < trees_count; i++)
                    {
                        Tile tile = trees[i];
                        tile.Update(resolution);

                        if (tile.InView)
                        {
                            if (tile.InSight)
                            {
                                tile.Draw(spriteBatch, resolution, color);
                            }
                            else
                            {
                                tile.Draw(spriteBatch, resolution, color * 0.95f);
                            }
                        }
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution)
        {
            if (Visible)
            {
                if (World.Maps.Count > 0)
                {
                    Map map = World.Maps[0];
                    Layer roof_tiles = map.GetLayer("RoofTiles");

                    if (!Main.Game.Debugging)
                    {
                        int roof_count = roof_tiles.Tiles.Count;
                        for (int i = 0; i < roof_count; i++)
                        {
                            Tile tile = roof_tiles.Tiles[i];
                            tile.Update(resolution);

                            if (tile.InView)
                            {
                                if (!tile.InSight)
                                {
                                    tile.Draw(spriteBatch, resolution, RenderingManager.Lighting.DrawColor);
                                }
                            }
                        }
                    }
                }

                List<Tile> visible = new List<Tile>();

                Character player = Handler.GetPlayer();
                if (player != null)
                {
                    if (Handler.VisibleTiles.ContainsKey(player.ID))
                    {
                        visible = Handler.VisibleTiles[player.ID];
                    }
                }

                foreach (Tile tile in visible)
                {
                    tile.Draw(spriteBatch, resolution);
                }

                if (Handler.WorldMap_Visible)
                {
                    foreach (Tile tile in WorldGen.Worldmap)
                    {
                        tile.Draw(spriteBatch, resolution, Color.White);
                    }

                    Handler.GetPlayer().Draw(spriteBatch, resolution, Color.White);
                }
            }
        }

        private void UpdateMenuControls()
        {
            if (InputManager.KeyPressed("Cancel"))
            {
                Menu main_menu = MenuManager.GetMenu("Main");
                if (!main_menu.Visible)
                {
                    main_menu.Open();
                }
                else
                {
                    main_menu.Close();
                }
            }

            if (InputManager.KeyPressed("Inventory"))
            {
                Menu inv_menu = MenuManager.GetMenu("Inventory");
                if (!inv_menu.Visible)
                {
                    Handler.Trading = false;
                    Handler.Trading_InventoryID.Clear();
                    inv_menu.Open();
                }
                else
                {
                    inv_menu.Close();
                }
            }

            if (InputManager.KeyPressed("Debug"))
            {
                if (!Main.Game.Debugging)
                {
                    Main.Game.Debugging = true;
                }
                else if (Main.Game.Debugging)
                {
                    Main.Game.Debugging = false;
                }
            }
        }

        private void UpdatePlayerControls(Character player)
        {
            bool mouse_in_view = false;

            Menu ui = MenuManager.GetMenu("UI");
            Picture world_view = ui.GetPicture("Panel_Upper_Center");
            if (InputManager.MouseWithin(world_view.Region.ToRectangle))
            {
                mouse_in_view = true;
            }

            if (player.Job.Tasks.Count == 0)
            {
                #region Move

                Task task = new Task();
                task.OwnerIDs.Add(player.ID);
                task.Started = true;
                task.StartTime = TimeManager.Now;

                task.Name = "Walk";
                if (InputManager.KeyDown("Run"))
                {
                    task.Name = "Run";
                }
                else if (InputManager.KeyDown("Crouch"))
                {
                    task.Name = "Sneak";
                }

                if (InputManager.KeyDown("Up"))
                {
                    task.Direction = Direction.Up;
                }
                else if (InputManager.KeyDown("Right"))
                {
                    task.Direction = Direction.Right;
                }
                else if (InputManager.KeyDown("Down"))
                {
                    task.Direction = Direction.Down;
                }
                else if (InputManager.KeyDown("Left"))
                {
                    task.Direction = Direction.Left;
                }
                else if (InputManager.Mouse_LB_Held &&
                         mouse_in_view)
                {
                    task.Direction = player.Direction;
                }

                if (task.Direction != Direction.Nowhere)
                {
                    player.Job.Tasks.Add(task);
                }

                #endregion
            }

            if (player.Job.Tasks.Count == 0 &&
                mouse_in_view)
            {
                #region Turn

                Task task = new Task();
                task.Name = "Turn";
                task.OwnerIDs.Add(player.ID);
                task.Keep_On_Completed = true;
                task.StartTime = new TimeHandler(TimeManager.Now);
                task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(player)));

                int x_diff = (int)Math.Abs(InputManager.Mouse.X - (player.Region.X + (player.Region.Width / 2)));
                int y_diff = (int)Math.Abs(InputManager.Mouse.Y - (player.Region.Y + (player.Region.Height / 2)));

                if (x_diff > y_diff)
                {
                    if (InputManager.Mouse.X <= player.Region.X + (player.Region.Width / 2))
                    {
                        if (player.Direction != Direction.Left)
                        {
                            task.Direction = Direction.Left;
                        }
                    }
                    else if (InputManager.Mouse.X > player.Region.X + (player.Region.Width / 2))
                    {
                        if (player.Direction != Direction.Right)
                        {
                            task.Direction = Direction.Right;
                        }
                    }
                }
                else
                {
                    if (InputManager.Mouse.Y <= player.Region.Y + (player.Region.Height / 2))
                    {
                        if (player.Direction != Direction.Up)
                        {
                            task.Direction = Direction.Up;
                        }
                    }
                    else if (InputManager.Mouse.Y > player.Region.Y + (player.Region.Height / 2))
                    {
                        if (player.Direction != Direction.Down)
                        {
                            task.Direction = Direction.Down;
                        }
                    }
                }

                if (task.Direction != Direction.Nowhere)
                {
                    player.Job.Tasks.Add(task);
                }

                #endregion
            }

            if (player.Job.Tasks.Count == 0)
            {
                #region Wait

                if (InputManager.KeyDown("Wait"))
                {
                    if (InputManager.KeyDown("Run"))
                    {
                        Tasker.AddTask(player, "Wait", true, false, TimeSpan.FromMilliseconds(Handler.ActionRate * 60), default, 0);
                        TimeTracker.Tick(Handler.ActionRate * 60);
                    }
                    else if (InputManager.KeyDown("Crouch"))
                    {
                        Tasker.AddTask(player, "Wait", true, false, TimeSpan.FromMilliseconds(1), default, 0);
                        TimeTracker.Tick(1);
                    }
                    else
                    {
                        Tasker.AddTask(player, "Wait", true, false, TimeSpan.FromMilliseconds(Handler.ActionRate), default, 0);
                        TimeTracker.Tick(Handler.ActionRate);
                    }
                }

                #endregion
            }

            if (InputManager.KeyPressed("Interact") &&
                player.Job.Tasks.Count == 0)
            {
                #region Interact

                Vector3 location = default;
                if (player.Direction == Direction.Up)
                {
                    location = new Vector3(player.Location.X, player.Location.Y - 1, 0);
                }
                else if (player.Direction == Direction.Right)
                {
                    location = new Vector3(player.Location.X + 1, player.Location.Y, 0);
                }
                else if (player.Direction == Direction.Down)
                {
                    location = new Vector3(player.Location.X, player.Location.Y + 1, 0);
                }
                else if (player.Direction == Direction.Left)
                {
                    location = new Vector3(player.Location.X - 1, player.Location.Y, 0);
                }

                Map block_map = WorldUtil.GetCurrentMap(player);

                Map map = World.Maps[0];
                Layer bottom_tiles = map.GetLayer("BottomTiles");
                Layer middle_tiles = map.GetLayer("MiddleTiles");
                Layer top_tiles = block_map.GetLayer("TopTiles");
                Layer effect_tiles = map.GetLayer("EffectTiles");

                Tile bottom_tile = bottom_tiles.GetTile(location);
                Tile middle_tile = WorldUtil.GetFurniture(middle_tiles, new Vector3(location.X, location.Y, 0));
                Tile top_tile = WorldUtil.GetFurniture(top_tiles, new Vector3(location.X, location.Y, 0));
                Tile effect_tile = effect_tiles.GetTile(location);

                Tile interaction_tile = null;
                if (effect_tile?.Texture != null)
                {
                    interaction_tile = effect_tile;
                }
                else if (top_tile?.Texture != null)
                {
                    interaction_tile = top_tile;
                }
                else if (middle_tile?.Texture != null)
                {
                    interaction_tile = middle_tile;
                }
                else if (bottom_tile != null)
                {
                    interaction_tile = bottom_tile;
                }

                Tasker.Interact(interaction_tile, player);

                #endregion
            }

            if (InputManager.Mouse_RB_Pressed &&
                player.Job.Tasks.Count == 0)
            {
                #region Interaction Menu

                Handler.Interaction_Character = null;
                Handler.Interaction_Tile = null;

                if (Handler.VisibleTiles.ContainsKey(player.ID))
                {
                    List<Tile> visible = Handler.VisibleTiles[player.ID];
                    foreach (Tile tile in visible)
                    {
                        Vector2 location = new Vector2(tile.Location.X, tile.Location.Y);

                        if (InputManager.MouseWithin(tile.Region.ToRectangle))
                        {
                            Army army = CharacterManager.GetArmy("Characters");
                            Squad citizens = army.GetSquad("Citizens");

                            Character character = WorldUtil.MouseGetCharacter(citizens.Characters, tile.Location);

                            Map map = World.Maps[0];

                            Layer bottom_tiles = map.GetLayer("BottomTiles");
                            Layer middle_tiles = map.GetLayer("MiddleTiles");
                            Layer top_tiles = map.GetLayer("TopTiles");
                            Layer effect_tiles = map.GetLayer("EffectTiles");

                            Tile bottom_tile = bottom_tiles.GetTile(location);
                            Tile middle_tile = WorldUtil.GetFurniture(middle_tiles, tile.Location);
                            Tile top_tile = top_tiles.GetTile(location);
                            Tile effect_tile = effect_tiles.GetTile(location);

                            if (InputManager.KeyDown("Crouch"))
                            {
                                if (character != null)
                                {
                                    Handler.Interaction_Character = character;
                                }
                                else if (effect_tile?.Texture != null)
                                {
                                    Handler.Interaction_Tile = effect_tile;
                                }
                                else if (top_tile?.Texture != null)
                                {
                                    //Get what's underneath
                                    if (middle_tile?.Texture != null)
                                    {
                                        Handler.Interaction_Tile = middle_tile;
                                    }
                                }
                                else if (middle_tile?.Texture != null)
                                {
                                    Handler.Interaction_Tile = middle_tile;
                                }
                                else if (bottom_tile != null)
                                {
                                    Handler.Interaction_Tile = bottom_tile;
                                }
                            }
                            else
                            {
                                if (character != null)
                                {
                                    Handler.Interaction_Character = character;
                                }
                                else if (effect_tile?.Texture != null)
                                {
                                    Handler.Interaction_Tile = effect_tile;
                                }
                                else if (top_tile?.Texture != null)
                                {
                                    Handler.Interaction_Tile = top_tile;
                                }
                                else if (middle_tile?.Texture != null)
                                {
                                    Handler.Interaction_Tile = middle_tile;
                                }
                                else if (bottom_tile != null)
                                {
                                    Handler.Interaction_Tile = bottom_tile;
                                }
                            }

                            Menu interact = MenuManager.GetMenu("Interact");
                            interact.Load();
                            interact.Open();

                            break;
                        }
                    }
                }

                #endregion
            }
        }

        private void UpdatePlayer(Character player)
        {
            if (player != null)
            {
                Something blood = player.GetStat("Blood");
                if (blood.Value <= 0)
                {
                    player.Dead = true;
                    JobManager.Jobs.Remove(player.Job);

                    if (player.Type == "Player")
                    {
                        //Game Over
                    }
                }

                CharacterUtil.UpdatePain(player);

                if (!player.Dead)
                {
                    Tasker.Character_StartAction(World, player);
                    Tasker.Character_EndAction(World, player);

                    player.Move_TotalDistance = Main.Game.TileSize.X;
                    player.Update();

                    CharacterUtil.UpdateGear(player);

                    if (!player.Moving)
                    {
                        Task task = player.Job.CurrentTask;
                        if (task != null)
                        {
                            if (task.Name == "Sneak" ||
                                task.Name == "Walk" ||
                                task.Name == "Run")
                            {
                                task.EndTime = new TimeHandler(TimeManager.Now);

                                if (player.Location == player.Destination)
                                {
                                    if (task.Name == "Sneak")
                                    {
                                        player.GetStat("Stamina").DecreaseValue(0.0385f / player.GetStat("Endurance").Value);
                                    }
                                    else if (task.Name == "Walk")
                                    {
                                        player.GetStat("Stamina").DecreaseValue(0.077f / player.GetStat("Endurance").Value);
                                    }
                                    else if (task.Name == "Run")
                                    {
                                        player.GetStat("Stamina").DecreaseValue(0.154f / player.GetStat("Endurance").Value);
                                    }
                                }

                                WorldUtil.SetCurrentMap(player);

                                player.Job.Update(TimeManager.Now);
                            }
                            else if (!task.Completed)
                            {
                                long milliseconds = task.EndTime.TotalMilliseconds - TimeManager.Now.TotalMilliseconds;

                                if (milliseconds > 60000)
                                {
                                    //1m
                                    TimeTracker.Tick(60000);
                                }
                                else if (milliseconds > 10000)
                                {
                                    //10s
                                    TimeTracker.Tick(10000);
                                }
                                else if (milliseconds > 1000)
                                {
                                    //1s
                                    TimeTracker.Tick(1000);
                                }
                                else if (milliseconds > 100)
                                {
                                    //100ms
                                    TimeTracker.Tick(100);
                                }
                                else if (milliseconds > 10)
                                {
                                    //10ms
                                    TimeTracker.Tick(10);
                                }
                                else if (milliseconds > 0)
                                {
                                    //1ms
                                    TimeTracker.Tick(1);
                                }
                            }
                            else
                            {
                                player.Job.Update(TimeManager.Now);
                            }
                        }

                        if (player.GetStat("Consciousness").Value <= 0)
                        {
                            player.Unconscious = true;
                        }
                    }
                    else
                    {
                        TimeTracker.Tick(Handler.ActionRate);
                        GameUtil.CenterToPlayer_OnFrame();
                    }

                    CharacterUtil.UpdateSight(player);
                    GameUtil.UpdateWorld(World, player);
                }
            }
        }

        private void UpdateCitizens()
        {
            Squad citizens = CharacterManager.GetArmy("Characters").GetSquad("Citizens");
            for (int i = 0; i < citizens.Characters.Count; i++)
            {
                Character character = citizens.Characters[i];
                if (!character.Dead)
                {
                    CharacterUtil.UpdatePain(character);
                }

                CharacterUtil.UpdateGear(character);
                CharacterUtil.UpdateSight(character);
            }
        }

        public override void Load(ContentManager content)
        {
            
        }

        public override void Resize(Point point)
        {
            GameUtil.CenterToPlayer_OnStart();
        }

        #endregion
    }
}
