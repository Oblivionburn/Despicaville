using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Sounds;
using OP_Engine.Controls;
using OP_Engine.Weathers;
using OP_Engine.Utility;
using OP_Engine.Scenes;

using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Options : Menu
    {
        #region Variables



        #endregion

        #region Constructor

        public Menu_Options(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Options";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                UpdateControls();
                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Pictures)
                {
                    picture.Draw(spriteBatch);
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (ProgressBar bar in ProgressBars)
                {
                    bar.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name != "Examine")
                    {
                        label.Draw(spriteBatch);
                    }
                }

                foreach (Label label in Labels)
                {
                    if (label.Name == "Examine")
                    {
                        label.Draw(spriteBatch);
                        break;
                    }
                }
            }
        }

        private void UpdateControls()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);
                            break;
                        }
                    }
                    else if (InputManager.Mouse_Moved)
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            if (!found)
            {
                GetLabel("Examine").Visible = false;
            }

            foreach (ProgressBar bar in ProgressBars)
            {
                if (bar.Visible)
                {
                    if (InputManager.MouseWithin(bar.Base_Region.ToRectangle))
                    {
                        bar.Opacity = 1;

                        if (InputManager.Mouse_LB_Held)
                        {
                            SetVolume(bar);
                        }
                    }
                    else if (InputManager.Mouse_Moved)
                    {
                        bar.Opacity = 0.8f;
                    }
                }
            }

            if (InputManager.KeyPressed("Cancel"))
            {
                Back();
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Controls")
            {
                Visible = false;
                Active = false;

                SceneManager.GetScene("Title").Menu.GetPicture("Title").Visible = false;

                Menu menu = MenuManager.GetMenu("Controls");
                menu.Visible = true;
                menu.Active = true;
            }
            else if (button.Name == "FullscreenOn")
            {
                Main.Game.ScreenType = ScreenType.Windowed;
                Main.Game.ResetScreen();

                button.Name = "FullscreenOff";
                button.Texture = AssetManager.Textures["Button_FullscreenOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_FullscreenOff_Hover"];
            }
            else if (button.Name == "FullscreenOff")
            {
                Main.Game.ScreenType = ScreenType.BorderlessFullscreen;
                Main.Game.ResetScreen();

                button.Name = "FullscreenOn";
                button.Texture = AssetManager.Textures["Button_FullscreenOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_FullscreenOn_Hover"];
            }
            else if (button.Name == "MusicOn")
            {
                GetProgressBar("Music").Visible = false;

                SoundManager.StopMusic();
                SoundManager.MusicEnabled = false;

                GetLabel("Music").Text = @"0%";

                button.Name = "MusicOff";
                button.Texture = AssetManager.Textures["Button_MusicOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_MusicOff_Hover"];
            }
            else if (button.Name == "MusicOff")
            {
                GetProgressBar("Music").Visible = true;
                SoundManager.MusicEnabled = true;

                if (Main.Game.GameStarted)
                {
                    SoundManager.NeedMusic = true;
                }
                else
                {
                    AssetManager.PlayMusic_Random("Title", true);
                }

                GetLabel("Music").Text = GetProgressBar("Music").Value + @"%";

                button.Name = "MusicOn";
                button.Texture = AssetManager.Textures["Button_MusicOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_MusicOn_Hover"];
            }
            else if (button.Name == "SoundOn")
            {
                GetProgressBar("Sound").Visible = false;
                SoundManager.SoundEnabled = false;

                try
                {
                    SoundManager.StopSound();
                }
                catch (Exception e)
                {
                    string ignore_me = e.Message;
                }

                GetLabel("Sound").Text = @"0%";

                button.Name = "SoundOff";
                button.Texture = AssetManager.Textures["Button_SoundOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_SoundOff_Hover"];
            }
            else if (button.Name == "SoundOff")
            {
                GetProgressBar("Sound").Visible = true;
                SoundManager.SoundEnabled = true;
                GetLabel("Sound").Text = GetProgressBar("Sound").Value + @"%";

                button.Name = "SoundOn";
                button.Texture = AssetManager.Textures["Button_SoundOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_SoundOn_Hover"];
            }
            else if (button.Name == "AmbienceOn")
            {
                GetProgressBar("Ambience").Visible = false;

                SoundManager.StopAmbient();
                SoundManager.AmbientEnabled = false;

                GetLabel("Ambience").Text = @"0%";

                button.Name = "AmbienceOff";
                button.Texture = AssetManager.Textures["Button_AmbienceOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_AmbienceOff_Hover"];
            }
            else if (button.Name == "AmbienceOff")
            {
                GetProgressBar("Ambience").Visible = true;
                SoundManager.AmbientEnabled = true;

                if (WeatherManager.CurrentWeather == WeatherType.Rain)
                {
                    AssetManager.PlayAmbient_Random("Rain", true);
                }
                else if (WeatherManager.CurrentWeather == WeatherType.Fog)
                {
                    AssetManager.PlayAmbient_Random("Wind", true);
                }
                else if (WeatherManager.CurrentWeather == WeatherType.Storm)
                {
                    AssetManager.PlayAmbient_Random("Storm", true);
                }

                GetLabel("Ambience").Text = GetProgressBar("Ambience").Value + @"%";

                button.Name = "AmbienceOn";
                button.Texture = AssetManager.Textures["Button_AmbienceOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_AmbienceOn_Hover"];
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        private void Back()
        {
            Save.ExportINI();

            Visible = false;
            Active = false;

            Menu menu = MenuManager.GetMenu("Main");
            menu.Visible = true;
            menu.Active = true;
        }

        private void SetVolume(ProgressBar bar)
        {
            bar.Bar_Region.Width = InputManager.Mouse.X - bar.Base_Region.X;
            float volume = ((bar.Bar_Region.Width * 100) / (float)bar.Base_Region.Width) + 1;

            float CurrentVal = ((volume - 1) * bar.Bar_Texture.Width) / 100;
            bar.Bar_Image.Width = (int)CurrentVal;

            bar.Value = (int)volume;

            if (bar.Name == "Music")
            {
                SoundManager.MusicVolume = volume / 100;
                SoundManager.MusicChannel.setVolume(SoundManager.MusicVolume);

                GetLabel("Music").Text = ((int)volume).ToString() + @"%";
            }
            else if (bar.Name == "Sound")
            {
                SoundManager.SoundVolume = volume / 100;
                GetLabel("Sound").Text = ((int)volume).ToString() + @"%";
            }
            else if (bar.Name == "Ambience")
            {
                SoundManager.AmbientVolume = volume / 100;
                SoundManager.AmbientChannel.setVolume(SoundManager.AmbientVolume);

                GetLabel("Ambience").Text = ((int)volume).ToString() + @"%";
            }
        }

        private void GetVolume(ProgressBar bar)
        {
            if (bar.Name == "Music")
            {
                bar.Value = (int)(SoundManager.MusicVolume * 100);

                if (SoundManager.MusicEnabled)
                {
                    GetLabel("Music").Text = bar.Value.ToString() + @"%";
                }
                else
                {
                    GetLabel("Music").Text = @"0%";
                }
            }
            else if (bar.Name == "Sound")
            {
                bar.Value = (int)(SoundManager.SoundVolume * 100);

                if (SoundManager.SoundEnabled)
                {
                    GetLabel("Sound").Text = bar.Value.ToString() + @"%";
                }
                else
                {
                    GetLabel("Sound").Text = @"0%";
                }
            }
            else if (bar.Name == "Ambience")
            {
                bar.Value = (int)(SoundManager.AmbientVolume * 100);

                if (SoundManager.AmbientEnabled)
                {
                    GetLabel("Ambience").Text = bar.Value.ToString() + @"%";
                }
                else
                {
                    GetLabel("Ambience").Text = @"0%";
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 2);

            AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
            GetButton("Back").HoverText = "Back";

            Y += 2;
            AddButton(Handler.GetID(), "Controls", AssetManager.Textures["Button_Controls"], AssetManager.Textures["Button_Controls_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
            GetButton("Controls").HoverText = "Keybindings";

            Y += 1;
            if (Main.Game.GraphicsManager.IsFullScreen)
            {
                AddButton(Handler.GetID(), "FullscreenOn", AssetManager.Textures["Button_FullscreenOn"], AssetManager.Textures["Button_FullscreenOn_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("FullscreenOn").Value = 1;
                GetButton("FullscreenOn").HoverText = "Toggle Fullscreen";
            }
            else
            {
                AddButton(Handler.GetID(), "FullscreenOff", AssetManager.Textures["Button_FullscreenOff"], AssetManager.Textures["Button_FullscreenOff_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("FullscreenOff").HoverText = "Toggle Fullscreen";
            }

            Y += 1;
            if (SoundManager.MusicEnabled)
            {
                AddButton(Handler.GetID(), "MusicOn", AssetManager.Textures["Button_MusicOn"], AssetManager.Textures["Button_MusicOn_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("MusicOn").Value = 1;
                GetButton("MusicOn").HoverText = "Toggle Music";

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            else
            {
                AddButton(Handler.GetID(), "MusicOff", AssetManager.Textures["Button_MusicOff"], AssetManager.Textures["Button_MusicOff_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("MusicOff").HoverText = "Toggle Music";

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            GetVolume(GetProgressBar("Music"));

            Y += 1;
            if (SoundManager.AmbientEnabled)
            {
                AddButton(Handler.GetID(), "AmbienceOn", AssetManager.Textures["Button_AmbienceOn"], AssetManager.Textures["Button_AmbienceOn_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("AmbienceOn").Value = 1;
                GetButton("AmbienceOn").HoverText = "Toggle Ambience";

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            else
            {
                AddButton(Handler.GetID(), "AmbienceOff", AssetManager.Textures["Button_AmbienceOff"], AssetManager.Textures["Button_AmbienceOff_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("AmbienceOff").HoverText = "Toggle Ambience";

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            GetVolume(GetProgressBar("Ambience"));

            Y += 1;
            if (SoundManager.SoundEnabled)
            {
                AddButton(Handler.GetID(), "SoundOn", AssetManager.Textures["Button_SoundOn"], AssetManager.Textures["Button_SoundOn_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("SoundOn").Value = 1;
                GetButton("SoundOn").HoverText = "Toggle Sound";

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            else
            {
                AddButton(Handler.GetID(), "SoundOff", AssetManager.Textures["Button_SoundOff"], AssetManager.Textures["Button_SoundOff_Hover"], null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
                GetButton("SoundOff").HoverText = "Toggle Sound";

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            GetVolume(GetProgressBar("Sound"));

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 2);

            GetButton("Back").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Y += 2;
            GetButton("Controls").Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Y += 1;
            Button fullscreen = GetButton("FullscreenOn");
            if (fullscreen == null)
            {
                fullscreen = GetButton("FullscreenOff");
            }
            fullscreen.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Y += 1;
            Button music = GetButton("MusicOn");
            if (music == null)
            {
                music = GetButton("MusicOff");
            }
            music.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            GetProgressBar("Music").Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            GetLabel("Music").Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);

            Y += 1;
            Button ambience = GetButton("AmbienceOn");
            if (ambience == null)
            {
                ambience = GetButton("AmbienceOff");
            }
            ambience.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            GetProgressBar("Ambience").Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            GetLabel("Ambience").Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);

            Y += 1;
            Button sound = GetButton("SoundOn");
            if (sound == null)
            {
                sound = GetButton("SoundOff");
            }
            sound.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            GetProgressBar("Sound").Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            GetLabel("Sound").Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
        }

        #endregion
    }
}
