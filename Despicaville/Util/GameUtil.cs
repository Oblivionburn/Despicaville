using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Jobs;

namespace Despicaville.Util
{
    public static class GameUtil
    {
        public static void Start()
        {
            CenterToPlayer_OnStart();

            TimeTracker.Reset();

            Main.Game.GameStarted = true;
            Toggle_MainMenu();

            Menu ui = MenuManager.GetMenu("UI");
            ui.Active = true;
            ui.Visible = true;

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;

            AddMessage("You awake.");

            Scene scene = SceneManager.GetScene("Gameplay");
            scene.Resize(Main.Game.Resolution);
            SceneManager.ChangeScene(scene);

            CenterToPlayer_OnFrame();
        }

        public static void ReturnToTitle()
        {
            Main.Game.GameStarted = false;
            TimeManager.Paused = false;

            Handler.Messages.Clear();
            Handler.VisibleTiles.Clear();
            Handler.light_maps.Clear();
            Handler.light_sources.Clear();
            ResetArmy();

            Handler.CharGen_Stage = 0;
            Handler.Loading_Stage = 4;
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "";            

            Toggle_MainMenu();

            Menu ui = MenuManager.GetMenu("UI");
            ui.Visible = false;
            ui.Active = false;

            Menu main = MenuManager.GetMenu("Main");
            main.Visible = true;
            main.Active = true;

            SceneManager.ChangeScene("Title");

            SoundManager.StopAll();
            SoundManager.NeedMusic = true;
        }

        public static void ResetArmy()
        {
            CharacterManager.Armies.Clear();

            for (int i = 0; i < InventoryManager.Inventories.Count; i++)
            {
                foreach (Inventory existing in InventoryManager.Inventories)
                {
                    if (existing.Name != "Assets")
                    {
                        InventoryManager.Inventories.Remove(existing);
                        break;
                    }
                }
            }

            Army characters = new Army();
            characters.ID = Handler.GetID();
            characters.Name = "Characters";
            CharacterManager.Armies.Add(characters);

            Squad players = new Squad();
            players.ID = Handler.GetID();
            players.Name = "Players";
            characters.Squads.Add(players);

            Squad citizens = new Squad();
            citizens.ID = Handler.GetID();
            citizens.Name = "Citizens";
            characters.Squads.Add(citizens);

            Squad animals = new Squad();
            animals.ID = Handler.GetID();
            animals.Name = "Animals";
            characters.Squads.Add(animals);
        }

        public static void CenterToPlayer_OnStart()
        {
            Army army = CharacterManager.GetArmy("Characters");

            Character player = Handler.GetPlayer();
            if (player != null)
            {
                player.Speed = (Main.Game.TileSize.X / 32);
                player.Move_TotalDistance = Main.Game.TileSize.X;

                player.Region.X = (Main.Game.ScreenWidth / 2) - (Main.Game.TileSize.X / 2);
                player.Region.Y = (Main.Game.ScreenHeight / 2) - (Main.Game.TileSize.Y / 2) - (Main.Game.TileSize.Y * 2);
                player.Region.Width = Main.Game.TileSize.X;
                player.Region.Height = Main.Game.TileSize.Y;

                World world = SceneManager.GetScene("Gameplay").World;
                if (world != null)
                {
                    Map map = world.Maps[0];
                    if (map != null)
                    {
                        Layer bottom_tiles = map.GetLayer("BottomTiles");
                        Layer room_tiles = map.GetLayer("RoomTiles");
                        Layer middle_tiles = map.GetLayer("MiddleTiles");
                        Layer top_tiles = map.GetLayer("TopTiles");
                        Layer effect_tiles = map.GetLayer("EffectTiles");
                        Layer roof_tiles = map.GetLayer("RoofTiles");
                        
                        if (bottom_tiles != null)
                        {
                            Tile current = bottom_tiles.GetTile(new Vector2(player.Location.X, player.Location.Y));
                            if (current != null)
                            {
                                current.Region.X = player.Region.X;
                                current.Region.Y = player.Region.Y;
                                current.Region.Width = Main.Game.TileSize.X;
                                current.Region.Height = Main.Game.TileSize.Y;

                                foreach (Tile tile in bottom_tiles.Tiles)
                                {
                                    int x_diff = (int)tile.Location.X - (int)current.Location.X;
                                    if (x_diff < 0)
                                    {
                                        x_diff *= -1;
                                    }

                                    int y_diff = (int)tile.Location.Y - (int)current.Location.Y;
                                    if (y_diff < 0)
                                    {
                                        y_diff *= -1;
                                    }

                                    if (tile.Location.X < current.Location.X)
                                    {
                                        tile.Region.X = current.Region.X - (x_diff * Main.Game.TileSize.X);
                                    }
                                    else if (tile.Location.X > current.Location.X)
                                    {
                                        tile.Region.X = current.Region.X + (x_diff * Main.Game.TileSize.X);
                                    }
                                    else if (tile.Location.X == current.Location.X)
                                    {
                                        tile.Region.X = current.Region.X;
                                    }

                                    if (tile.Location.Y < current.Location.Y)
                                    {
                                        tile.Region.Y = current.Region.Y - (y_diff * Main.Game.TileSize.Y);
                                    }
                                    else if (tile.Location.Y > current.Location.Y)
                                    {
                                        tile.Region.Y = current.Region.Y + (y_diff * Main.Game.TileSize.Y);
                                    }
                                    else if (tile.Location.Y == current.Location.Y)
                                    {
                                        tile.Region.Y = current.Region.Y;
                                    }

                                    tile.Region.Width = Main.Game.TileSize.X;
                                    tile.Region.Height = Main.Game.TileSize.Y;

                                    Tile middle_tile = middle_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                                    if (middle_tile != null)
                                    {
                                        middle_tile.Region.X = tile.Region.X;
                                        middle_tile.Region.Y = tile.Region.Y;

                                        if (!string.IsNullOrEmpty(middle_tile.Name))
                                        {
                                            if (middle_tile.Name.Contains("Tree"))
                                            {
                                                middle_tile.Region.X = tile.Region.X - Main.Game.TileSize.X;
                                                middle_tile.Region.Y = tile.Region.Y - Main.Game.TileSize.Y;
                                                middle_tile.Region.Width = Main.Game.TileSize.X * 3;
                                                middle_tile.Region.Height = Main.Game.TileSize.Y * 3;
                                            }
                                            else if (middle_tile.Name.Contains("Bench") ||
                                                     middle_tile.Name.Contains("Couch"))
                                            {
                                                if (middle_tile.Direction == Direction.Up ||
                                                    middle_tile.Direction == Direction.Down)
                                                {
                                                    middle_tile.Region.Width = Main.Game.TileSize.X * 3;
                                                    middle_tile.Region.Height = Main.Game.TileSize.Y;
                                                }
                                                else if (middle_tile.Direction == Direction.Left ||
                                                         middle_tile.Direction == Direction.Right)
                                                {
                                                    middle_tile.Region.Width = Main.Game.TileSize.X;
                                                    middle_tile.Region.Height = Main.Game.TileSize.Y * 3;
                                                }
                                            }
                                            else if (middle_tile.Name.Contains("Dresser") ||
                                                     middle_tile.Name.Contains("Loveseat"))
                                            {
                                                if (middle_tile.Direction == Direction.Up ||
                                                    middle_tile.Direction == Direction.Down)
                                                {
                                                    middle_tile.Region.Width = Main.Game.TileSize.X * 2;
                                                    middle_tile.Region.Height = Main.Game.TileSize.Y;
                                                }
                                                else if (middle_tile.Direction == Direction.Left ||
                                                         middle_tile.Direction == Direction.Right)
                                                {
                                                    middle_tile.Region.Width = Main.Game.TileSize.X;
                                                    middle_tile.Region.Height = Main.Game.TileSize.Y * 2;
                                                }
                                            }
                                            else if (middle_tile.Name.Contains("Bath") ||
                                                     middle_tile.Name.Contains("Bed") ||
                                                     middle_tile.Name.Contains("ComputerDesk"))
                                            {
                                                if (middle_tile.Name.Contains("DoubleBed"))
                                                {
                                                    middle_tile.Region.Width = Main.Game.TileSize.X * 2;
                                                    middle_tile.Region.Height = Main.Game.TileSize.Y * 2;
                                                }
                                                else if (middle_tile.Name.Contains("ComputerDesk"))
                                                {
                                                    if (middle_tile.Direction == Direction.Up ||
                                                        middle_tile.Direction == Direction.Down)
                                                    {
                                                        middle_tile.Region.Width = Main.Game.TileSize.X * 2;
                                                        middle_tile.Region.Height = Main.Game.TileSize.Y;
                                                    }
                                                    else if (middle_tile.Direction == Direction.Right ||
                                                             middle_tile.Direction == Direction.Left)
                                                    {
                                                        middle_tile.Region.Width = Main.Game.TileSize.X;
                                                        middle_tile.Region.Height = Main.Game.TileSize.Y * 2;
                                                    }
                                                }
                                                else
                                                {
                                                    if (middle_tile.Direction == Direction.Up ||
                                                        middle_tile.Direction == Direction.Down)
                                                    {
                                                        middle_tile.Region.Width = Main.Game.TileSize.X;
                                                        middle_tile.Region.Height = Main.Game.TileSize.Y * 2;
                                                    }
                                                    else if (middle_tile.Direction == Direction.Right ||
                                                             middle_tile.Direction == Direction.Left)
                                                    {
                                                        middle_tile.Region.Width = Main.Game.TileSize.X * 2;
                                                        middle_tile.Region.Height = Main.Game.TileSize.Y;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                middle_tile.Region.Width = Main.Game.TileSize.X;
                                                middle_tile.Region.Height = Main.Game.TileSize.Y;
                                            }
                                        }
                                        else
                                        {
                                            middle_tile.Region.Width = Main.Game.TileSize.X;
                                            middle_tile.Region.Height = Main.Game.TileSize.Y;
                                        }
                                    }

                                    Tile top_tile = top_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                                    if (top_tile != null)
                                    {
                                        top_tile.Region = tile.Region;
                                    }

                                    Tile room_tile = room_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                                    if (room_tile != null)
                                    {
                                        room_tile.Region = tile.Region;
                                    }

                                    Tile effect_tile = effect_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                                    if (effect_tile != null)
                                    {
                                        effect_tile.Region = tile.Region;
                                    }

                                    Tile roof_tile = roof_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                                    if (roof_tile != null)
                                    {
                                        roof_tile.Region = tile.Region;
                                    }

                                    foreach (Squad squad in army.Squads)
                                    {
                                        if (squad.Name != "Players")
                                        {
                                            foreach (Character character in squad.Characters)
                                            {
                                                if (character.Location.X == tile.Location.X &&
                                                    character.Location.Y == tile.Location.Y)
                                                {
                                                    character.Speed = 1;
                                                    character.Move_TotalDistance = Main.Game.TileSize.X;

                                                    character.Region.X = tile.Region.X;
                                                    character.Region.Y = tile.Region.Y;
                                                    character.Region.Width = Main.Game.TileSize.X;
                                                    character.Region.Height = Main.Game.TileSize.Y;

                                                    if (character.Moving)
                                                    {
                                                        if (character.Destination.Y < character.Location.Y)
                                                        {
                                                            character.Region.Y -= (int)character.Moved;
                                                        }
                                                        else if (character.Destination.X > character.Location.X)
                                                        {
                                                            character.Region.X += (int)character.Moved;
                                                        }
                                                        else if (character.Destination.Y > character.Location.Y)
                                                        {
                                                            character.Region.Y += (int)character.Moved;
                                                        }
                                                        else if (character.Destination.X < character.Location.X)
                                                        {
                                                            character.Region.X -= (int)character.Moved;
                                                        }
                                                    }

                                                    Task task = character.Job.CurrentTask;
                                                    if (task != null)
                                                    {
                                                        ProgressBar taskbar = task.TaskBar;
                                                        if (taskbar != null)
                                                        {
                                                            if (taskbar.Base_Texture != null)
                                                            {
                                                                taskbar.Base_Region = new Region(character.Region.X + (Main.Game.TileSize.X / 8), character.Region.Y + character.Region.Height - (Main.Game.TileSize.Y / 4), Main.Game.TileSize.X - (Main.Game.TileSize.X / 4), Main.Game.TileSize.Y / 8);
                                                                taskbar.Update();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void CenterToPlayer_OnFrame()
        {
            Army characters = CharacterManager.GetArmy("Characters");
            Character player = Handler.GetPlayer();

            int center_x = (Main.Game.ScreenWidth / 2) - (Main.Game.TileSize.X / 2);
            int center_y = (Main.Game.ScreenHeight / 2) - (Main.Game.TileSize.Y / 2) - (Main.Game.TileSize.Y * 2);

            float x_diff = center_x - player.Region.X;
            float y_diff = center_y - player.Region.Y;

            player.Region.X += x_diff;
            player.Region.Y += y_diff;

            World world = SceneManager.GetScene("Gameplay").World;
            Map map = world.Maps[0];

            Layer bottom_tiles = map.GetLayer("BottomTiles");
            if (bottom_tiles != null)
            {
                Layer middle_tiles = map.GetLayer("MiddleTiles");

                foreach (Tile tile in bottom_tiles.Tiles)
                {
                    if (x_diff != 0)
                    {
                        tile.Region.X += x_diff;
                    }
                    
                    if (y_diff != 0)
                    {
                        tile.Region.Y += y_diff;
                    }

                    Tile middle_tile = middle_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                    if (middle_tile != null)
                    {
                        if (x_diff != 0)
                        {
                            middle_tile.Region.X += x_diff;
                        }

                        if (y_diff != 0)
                        {
                            middle_tile.Region.Y += y_diff;
                        }
                    }
                }
            }

            foreach (Squad squad in characters.Squads)
            {
                foreach (Character character in squad.Characters)
                {
                    if (character.Type != "Player")
                    {
                        character.Region.X += x_diff;
                        character.Region.Y += y_diff;

                        Task task = character.Job.CurrentTask;
                        if (task != null)
                        {
                            ProgressBar taskbar = task.TaskBar;
                            if (taskbar != null)
                            {
                                if (taskbar.Base_Texture != null)
                                {
                                    taskbar.Base_Region.X += x_diff;
                                    taskbar.Base_Region.Y += y_diff;
                                    taskbar.Update();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateWorld(World world, Character player)
        {
            if (world.Visible)
            {
                Army characters = CharacterManager.GetArmy("Characters");

                List<Tile> visible = new List<Tile>();
                if (Handler.VisibleTiles.ContainsKey(player.ID))
                {
                    visible = Handler.VisibleTiles[player.ID];
                }

                Map map = world.Maps[0];

                Layer bottom_tiles = map.GetLayer("BottomTiles");
                Layer middle_tiles = map.GetLayer("MiddleTiles");
                Layer top_tiles = map.GetLayer("TopTiles");
                Layer roof_tiles = map.GetLayer("RoofTiles");

                int start_y = (int)player.Location.Y - Handler.SightDistance - 1;
                int end_y = (int)player.Location.Y + Handler.SightDistance + 1;
                int start_x = (int)player.Location.X - Handler.SightDistance - 1;
                int end_x = (int)player.Location.X + Handler.SightDistance + 1;

                for (int y = start_y; y <= end_y; y++)
                {
                    for (int x = start_x; x <= end_x; x++)
                    {
                        Vector2 location = new Vector2(x, y);

                        Tile tile = bottom_tiles.GetTile(location);
                        if (tile != null)
                        {
                            tile.InSight = WorldUtil.Location_IsVisible(player.ID, tile.Location);

                            Tile middle_tile = middle_tiles.GetTile(location);
                            middle_tile.InSight = tile.InSight;

                            Tile top_tile = top_tiles.GetTile(location);
                            top_tile.InSight = tile.InSight;

                            Tile roof_tile = roof_tiles.GetTile(location);
                            roof_tile.InSight = tile.InSight;
                        }
                    }
                }

                foreach (Tile tile in visible)
                {
                    tile.Visible = InputManager.MouseWithin(tile.Region.ToRectangle);
                }

                foreach (Squad squad in characters.Squads)
                {
                    foreach (Character character in squad.Characters)
                    {
                        if (WorldUtil.Location_IsVisible(player.ID, character.Location) ||
                            WorldUtil.Location_IsVisible(player.ID, character.Destination))
                        {
                            character.InSight = true;
                        }
                        else
                        {
                            character.InSight = false;
                        }
                    }
                }
            }
        }

        public static void Examine(Menu menu, string text)
        {
            Label examine = menu.GetLabel("Examine");
            examine.Text = text;

            int width = Main.Game.MenuSize_X * 4;
            int height = Main.Game.MenuSize_X;

            int X = InputManager.Mouse.X - (width / 2);
            if (X < 0)
            {
                X = 0;
            }
            else if (X > Main.Game.Resolution.X - width)
            {
                X = Main.Game.Resolution.X - width;
            }

            int Y = InputManager.Mouse.Y + 20;
            if (Y < 0)
            {
                Y = 0;
            }
            else if (Y > Main.Game.Resolution.Y - height)
            {
                Y = Main.Game.Resolution.Y - height;
            }

            examine.Region = new Region(X, Y, width, height);
            examine.Visible = true;
        }

        public static void AddMessage(string message)
        {
            int NewHours = (int)TimeManager.Now.Hours;
            string hours;
            string minutes;
            string seconds;
            string milliseconds;
            string am_pm = " AM";

            if (NewHours > 12)
            {
                NewHours -= 12;
                am_pm = " PM";
            }
            else if (NewHours == 0)
            {
                NewHours = 12;
            }
            else if (NewHours == 12)
            {
                am_pm = " PM";
            }

            if (NewHours < 10)
            {
                hours = "0" + NewHours.ToString();
            }
            else
            {
                hours = NewHours.ToString();
            }

            if (TimeManager.Now.Minutes < 10)
            {
                minutes = "0" + TimeManager.Now.Minutes.ToString();
            }
            else
            {
                minutes = TimeManager.Now.Minutes.ToString();
            }

            if (TimeManager.Now.Seconds < 10)
            {
                seconds = "0" + TimeManager.Now.Seconds.ToString();
            }
            else
            {
                seconds = TimeManager.Now.Seconds.ToString();
            }

            if (TimeManager.Now.Milliseconds < 10)
            {
                milliseconds = "00" + TimeManager.Now.Milliseconds.ToString();
            }
            else if (TimeManager.Now.Milliseconds < 100)
            {
                milliseconds = "0" + TimeManager.Now.Milliseconds.ToString();
            }
            else
            {
                milliseconds = TimeManager.Now.Milliseconds.ToString();
            }

            string datestamp = TimeManager.Now.Months.ToString() + "/" + TimeManager.Now.Days.ToString() + "/" + TimeManager.Now.Years.ToString();
            string timestamp = hours + ":" + minutes + ":" + seconds + "." + milliseconds + am_pm;

            List<string> messages = new List<string>();
            int max_length = 75;

            string full_message = datestamp + " " + timestamp + ": " + message;
            if (full_message.Length > max_length)
            {
                for (int m = 0; m < full_message.Length; m++)
                {
                    int index_break = 0;
                    for (int i = max_length; i > 0; i--)
                    {
                        if (full_message[i] == ' ')
                        {
                            index_break = i;
                            break;
                        }
                    }

                    string message_part = full_message.Substring(0, index_break);
                    messages.Add(message_part);

                    full_message = full_message.Remove(0, index_break);
                    m = 0;

                    if (full_message.Length < max_length)
                    {
                        messages.Add(full_message);
                        break;
                    }
                }
            }
            else
            {
                messages.Add(full_message);
            }

            foreach (string new_message in messages)
            {
                Handler.Messages.Add(new_message);
            }

            Menu UI = MenuManager.GetMenu("UI");

            int start = 0;
            if (Handler.Messages.Count >= Handler.MessageMax)
            {
                start = Handler.Messages.Count - Handler.MessageMax;
            }

            int end = Handler.Messages.Count;

            for (int i = start; i < end; i++)
            {
                int message_num = (i + Handler.MessageMax) - Handler.Messages.Count;

                Label label = UI.GetLabel("Message" + message_num.ToString());
                if (label != null)
                {
                    if (Handler.Messages.Count >= i + 1)
                    {
                        label.Text = Handler.Messages[i];
                    }
                    else
                    {
                        label.Text = "";
                    }
                }
            }
        }

        public static void Toggle_MainMenu()
        {
            Menu menu = MenuManager.GetMenu("Main");

            if (Main.Game.GameStarted)
            {
                menu.GetButton("Back").Visible = true;
                menu.GetButton("Play").Visible = false;

                Button save = menu.GetButton("Save");
                save.Visible = true;

                Button options = menu.GetButton("Options");
                options.Region = new Region(options.Region.X, save.Region.Y + Main.Game.MenuSize_Y, options.Region.Width, options.Region.Height);

                Button main = menu.GetButton("Main");
                main.Visible = true;
                main.Region = new Region(main.Region.X, options.Region.Y + Main.Game.MenuSize_Y, main.Region.Width, main.Region.Height);

                menu.GetButton("Exit").Visible = false;
            }
            else
            {
                menu.GetButton("Back").Visible = false;

                Button play = menu.GetButton("Play");
                play.Visible = true;

                menu.GetButton("Save").Visible = false;
                menu.GetButton("Main").Visible = false;

                Button options = menu.GetButton("Options");
                options.Region = new Region(options.Region.X, play.Region.Y + (Main.Game.MenuSize_Y * 2), options.Region.Width, options.Region.Height);

                Button exit = menu.GetButton("Exit");
                exit.Visible = true;
                exit.Region = new Region(exit.Region.X, options.Region.Y + Main.Game.MenuSize_Y, exit.Region.Width, exit.Region.Height);
            }
        }

        public static bool MouseOnPixel(Picture picture)
        {
            Texture2D texture = picture.Texture;
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);

            int x = (int)((InputManager.Mouse.X - picture.Region.X) / ((float)picture.Region.Width / picture.Texture.Width));
            int y = (int)((InputManager.Mouse.Y - picture.Region.Y) / ((float)picture.Region.Height / picture.Texture.Height));

            int index = (texture.Width * y) + x;
            if (index >= 0 && index < colors.Length)
            {
                Color check_color = colors[index];

                if (check_color != new Color(0, 0, 0, 0))
                {
                    if (picture.Value == 0)
                    {
                        for (int i = 0; i < colors.Length; i++)
                        {
                            Color color = colors[i];
                            if (color.R == 0 &&
                                color.G == 0 &&
                                color.B == 0 &&
                                color.A == 255)
                            {
                                colors[i] = new Color(254, 254, 254, 255);
                            }
                        }

                        texture.SetData(colors);
                        picture.Value = 1;

                        return true;
                    }
                    else if (picture.Value == 1)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public static void ResetHover(Picture picture)
        {
            Texture2D texture = picture.Texture;
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);

            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];
                if (color.R == 254 &&
                    color.G == 254 &&
                    color.B == 254 &&
                    color.A == 255)
                {
                    colors[i] = new Color(0, 0, 0, 255);
                }
            }

            texture.SetData(colors);
            picture.Value = 0;
        }

        public static string GetPixel(Picture picture)
        {
            Texture2D texture = picture.Texture;
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);

            int x = (int)((InputManager.Mouse.X - picture.Region.X) / ((float)picture.Region.Width / picture.Texture.Width));
            int y = (int)((InputManager.Mouse.Y - picture.Region.Y) / ((float)picture.Region.Height / picture.Texture.Height));

            int index = (texture.Width * y) + x;
            if (index >= 0 && index < colors.Length)
            {
                Color color = colors[index];

                if (color != new Color(0, 0, 0, 0))
                {
                    return "Color: (" + color.R + ", " + color.G + ", " + color.B + ", " + color.A + ")";
                }
                else
                {
                    return "Color: (0, 0, 0, 0)";
                }
            }

            return "";
        }

        public static void Texture_ChangeColor(Picture picture, Color new_color)
        {
            Texture2D texture = picture.Texture;
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);

            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];
                if (color.R == 0 &&
                    color.G == 0 &&
                    color.B == 0)
                {
                    //Reserved black
                    //Do nothing
                }
                else if (color.R == 254 &&
                         color.G == 254 &&
                         color.B == 254)
                {
                    //Reserved white
                    //Do nothing
                }
                else
                {
                    colors[i] = new_color;
                }
            }

            texture.SetData(colors);
        }

        public static string DetectKeyPresses(InputBox input, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name);

            if (InputManager.KeyPressed("Back"))
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
            }
            else
            {
                bool ok = true;
                if (sb.Length > 0)
                {
                    if (sb.Length == input.MaxLength)
                    {
                        ok = false;
                    }
                }

                if (ok)
                {
                    bool found = false;

                    if (found == false)
                    {
                        bool upper = false;
                        if (InputManager.KeyDown("LeftShift") ||
                            InputManager.KeyDown("RightShift"))
                        {
                            upper = true;
                        }

                        if (InputManager.KeyPressed("Space"))
                        {
                            found = true;
                            sb.Append(" ");
                        }

                        if (!found)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                if (InputManager.KeyPressed("D" + i))
                                {
                                    found = true;
                                    sb.Append(i.ToString());
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            for (char c = 'A'; c <= 'Z'; c++)
                            {
                                if (InputManager.KeyPressed(c.ToString()))
                                {
                                    found = true;

                                    if (upper)
                                    {
                                        sb.Append(c.ToString());
                                    }
                                    else
                                    {
                                        sb.Append(c.ToString().ToLower());
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static Color ColorFromName(string name)
        {
            if (name == "Black")
            {
                return new Color(16, 16, 16, 255);
            }
            else if (name == "Blue")
            {
                return Color.Blue;
            }
            else if (name == "Brown")
            {
                return new Color(66, 50, 50, 255);
            }
            else if (name == "Cyan")
            {
                return Color.Cyan;
            }
            else if (name == "Green")
            {
                return Color.Green;
            }
            else if (name == "Grey")
            {
                return new Color(80, 80, 80, 255);
            }
            else if (name == "Orange")
            {
                return new Color(230, 102, 0, 255);
            }
            else if (name == "Pink")
            {
                return new Color(255, 143, 255, 255);
            }
            else if (name == "Purple")
            {
                return new Color(97, 62, 110, 255);
            }
            else if (name == "Red")
            {
                return new Color(189, 0, 0, 255);
            }
            else if (name == "White" ||
                     name == "Default")
            {
                return Color.White;
            }
            else if (name == "Yellow")
            {
                return Color.Yellow;
            }
            else if (name == "Blonde")
            {
                return new Color(255, 255, 140, 255);
            }
            else
            {
                return Color.Transparent;
            }
        }

        public static Vector3 ColorToVector3(Color color)
        {
            Vector3 result = new Vector3();

            result.X = (((float)color.R * 100) / 255) / 100;
            result.Y = (((float)color.G * 100) / 255) / 100;
            result.Z = (((float)color.B * 100) / 255) / 100;

            return result;
        }

        public static List<long> OwnerIDs(Character character)
        {
            return new List<long> { character.ID };
        }

        public static bool NameStartsWithVowel(string name)
        {
            if (name[0] == 'A' ||
                name[0] == 'E' ||
                name[0] == 'I' ||
                name[0] == 'O' ||
                name[0] == 'U')
            {
                return true;
            }

            return false;
        }
    }
}
