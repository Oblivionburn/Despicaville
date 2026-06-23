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
using OP_Engine.Enums;
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

        public override void Update(Game? gameRef, ContentManager? content)
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
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
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
                Label? examine = GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }
            }

            foreach (ProgressBar bar in ProgressBars)
            {
                if (bar.Visible)
                {
                    if (bar.Base_Region != null &&
                        InputManager.MouseWithin(bar.Base_Region.ToRectangle))
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
            if (Main.Game == null)
            {
                return;
            }

            AssetManager.PlaySound_Random("Click");

            ProgressBar? sound_bar = GetProgressBar("Sound");
            Label? sound_label = GetLabel("Sound");

            ProgressBar? music_bar = GetProgressBar("Music");
            Label? music_label = GetLabel("Music");

            ProgressBar? ambience_bar = GetProgressBar("Ambience");
            Label? ambience_label = GetLabel("Ambience");

            switch (button.Name)
            {
                case "Back":
                    Back();
                    break;

                case "Controls":
                    Visible = false;
                    Active = false;

                    Picture? title = SceneManager.GetScene("Title")?.Menu?.GetPicture("Title");
                    if (title != null)
                    {
                        title.Visible = false;
                    }

                    Menu? menu = MenuManager.GetMenu("Controls");
                    if (menu != null)
                    {
                        menu.Visible = true;
                        menu.Active = true;
                    }
                    break;

                case "FullscreenOn":
                    Main.Game.ScreenType = ScreenType.Windowed;
                    Main.Game.ResetScreen();

                    button.Name = "FullscreenOff";
                    button.Texture = Handler.GetTexture("Button_FullscreenOff");
                    button.Texture_Highlight = Handler.GetTexture("Button_FullscreenOff_Hover");
                    break;

                case "FullscreenOff":
                    Main.Game.ScreenType = ScreenType.BorderlessFullscreen;
                    Main.Game.ResetScreen();

                    button.Name = "FullscreenOn";
                    button.Texture = Handler.GetTexture("Button_FullscreenOn");
                    button.Texture_Highlight = Handler.GetTexture("Button_FullscreenOn_Hover");
                    break;

                case "MusicOn":
                    if (music_bar != null)
                    {
                        music_bar.Visible = false;
                    }

                    SoundManager.StopMusic();
                    SoundManager.MusicEnabled = false;

                    if (music_label != null)
                    {
                        music_label.Text = @"0%";
                    }

                    button.Name = "MusicOff";
                    button.Texture = Handler.GetTexture("Button_MusicOff");
                    button.Texture_Highlight = Handler.GetTexture("Button_MusicOff_Hover");
                    break;

                case "MusicOff":
                    if (music_bar != null)
                    {
                        music_bar.Visible = true;

                        if (music_label != null)
                        {
                            music_label.Text = music_bar.Value + @"%";
                        }
                    }

                    SoundManager.MusicEnabled = true;

                    if (Main.Game.GameStarted)
                    {
                        SoundManager.NeedMusic = true;
                    }
                    else
                    {
                        AssetManager.PlayMusic_Random("Title", true);
                    }

                    button.Name = "MusicOn";
                    button.Texture = Handler.GetTexture("Button_MusicOn");
                    button.Texture_Highlight = Handler.GetTexture("Button_MusicOn_Hover");
                    break;

                case "SoundOn":
                    if (sound_bar != null)
                    {
                        sound_bar.Visible = false;
                    }

                    SoundManager.SoundEnabled = false;

                    try
                    {
                        SoundManager.StopSound();
                    }
                    catch (Exception e)
                    {
                        string ignore_me = e.Message;
                    }

                    if (sound_label != null)
                    {
                        sound_label.Text = @"0%";
                    }

                    button.Name = "SoundOff";
                    button.Texture = Handler.GetTexture("Button_SoundOff");
                    button.Texture_Highlight = Handler.GetTexture("Button_SoundOff_Hover");
                    break;

                case "SoundOff":
                    if (sound_bar != null)
                    {
                        sound_bar.Visible = true;

                        if (sound_label != null)
                        {
                            sound_label.Text = sound_bar.Value + @"%";
                        }
                    }

                    SoundManager.SoundEnabled = true;

                    button.Name = "SoundOn";
                    button.Texture = Handler.GetTexture("Button_SoundOn");
                    button.Texture_Highlight = Handler.GetTexture("Button_SoundOn_Hover");
                    break;

                case "AmbienceOn":
                    if (ambience_bar != null)
                    {
                        ambience_bar.Visible = false;
                    }

                    SoundManager.StopAmbient();
                    SoundManager.AmbientEnabled = false;

                    if (ambience_label != null)
                    {
                        ambience_label.Text = @"0%";
                    }

                    button.Name = "AmbienceOff";
                    button.Texture = Handler.GetTexture("Button_AmbienceOff");
                    button.Texture_Highlight = Handler.GetTexture("Button_AmbienceOff_Hover");
                    break;

                case "AmbienceOff":
                    if (ambience_bar != null)
                    {
                        ambience_bar.Visible = true;

                        if (ambience_label != null)
                        {
                            ambience_label.Text = ambience_bar.Value + @"%";
                        }
                    }

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

                    button.Name = "AmbienceOn";
                    button.Texture = Handler.GetTexture("Button_AmbienceOn");
                    button.Texture_Highlight = Handler.GetTexture("Button_AmbienceOn_Hover");
                    break;
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        private void Back()
        {
            SaveUtil.ExportINI();

            Visible = false;
            Active = false;

            Menu? menu = MenuManager.GetMenu("Main");
            if (menu != null)
            {
                menu.Visible = true;
                menu.Active = true;
            }
        }

        private void SetVolume(ProgressBar bar)
        {
            if (bar.Bar_Region == null ||
                bar.Bar_Texture == null ||
                bar.Base_Region == null ||
                InputManager.Mouse == null)
            {
                return;
            }

            bar.Bar_Region.Width = InputManager.Mouse.X - bar.Base_Region.X;
            float volume = ((bar.Bar_Region.Width * 100) / (float)bar.Base_Region.Width) + 1;

            float CurrentVal = ((volume - 1) * bar.Bar_Texture.Width) / 100;
            bar.Bar_Image.Width = (int)CurrentVal;

            bar.Value = (int)volume;

            if (bar.Name == "Music")
            {
                SoundManager.MusicVolume = volume / 100;
                SoundManager.MusicChannel.setVolume(SoundManager.MusicVolume);

                Label? music = GetLabel("Music");
                if (music != null)
                {
                    music.Text = ((int)volume).ToString() + @"%";
                }
            }
            else if (bar.Name == "Sound")
            {
                SoundManager.SoundVolume = volume / 100;

                Label? sound = GetLabel("Sound");
                if (sound != null)
                {
                    sound.Text = ((int)volume).ToString() + @"%";
                }
            }
            else if (bar.Name == "Ambience")
            {
                SoundManager.AmbientVolume = volume / 100;
                SoundManager.AmbientChannels[0].setVolume(SoundManager.AmbientVolume);

                Label? ambience = GetLabel("Ambience");
                if (ambience != null)
                {
                    ambience.Text = ((int)volume).ToString() + @"%";
                }
            }
        }

        private void GetVolume(ProgressBar bar)
        {
            if (bar.Name == "Music")
            {
                bar.Value = (int)(SoundManager.MusicVolume * 100);

                Label? music = GetLabel("Music");
                if (music != null)
                {
                    if (SoundManager.MusicEnabled)
                    {
                        music.Text = bar.Value.ToString() + @"%";
                    }
                    else
                    {
                        music.Text = @"0%";
                    }
                }
            }
            else if (bar.Name == "Sound")
            {
                bar.Value = (int)(SoundManager.SoundVolume * 100);

                Label? sound = GetLabel("Sound");
                if (sound != null)
                {
                    if (SoundManager.SoundEnabled)
                    {
                        sound.Text = bar.Value.ToString() + @"%";
                    }
                    else
                    {
                        sound.Text = @"0%";
                    }
                }
            }
            else if (bar.Name == "Ambience")
            {
                bar.Value = (int)(SoundManager.AmbientVolume * 100);

                Label? ambience = GetLabel("Ambience");
                if (ambience != null)
                {
                    if (SoundManager.AmbientEnabled)
                    {
                        ambience.Text = bar.Value.ToString() + @"%";
                    }
                    else
                    {
                        ambience.Text = @"0%";
                    }
                }
            }
        }

        public override void Load(ContentManager content)
        {
            if (Main.Game == null)
            {
                return;
            }

            Clear();

            Texture2D? frame = Handler.GetTexture("Frame");
            Texture2D? progressBase = Handler.GetTexture("ProgressBase");
            Texture2D? progressBar = Handler.GetTexture("ProgressBar");

            Texture2D? button_Back = Handler.GetTexture("Button_Back");
            Texture2D? button_Back_Hover = Handler.GetTexture("Button_Back_Hover");
            Texture2D? button_Back_Disabled = Handler.GetTexture("Button_Back_Disabled");

            Texture2D? button_Controls = Handler.GetTexture("Button_Controls");
            Texture2D? button_Controls_Hover = Handler.GetTexture("Button_Controls_Hover");

            Texture2D? button_FullscreenOn = Handler.GetTexture("Button_FullscreenOn");
            Texture2D? button_FullscreenOn_Hover = Handler.GetTexture("Button_FullscreenOn_Hover");

            Texture2D? button_FullscreenOff = Handler.GetTexture("Button_FullscreenOff");
            Texture2D? button_FullscreenOff_Hover = Handler.GetTexture("Button_FullscreenOff_Hover");

            Texture2D? button_MusicOn = Handler.GetTexture("Button_MusicOn");
            Texture2D? button_MusicOn_Hover = Handler.GetTexture("Button_MusicOn_Hover");

            Texture2D? button_MusicOff = Handler.GetTexture("Button_MusicOff");
            Texture2D? button_MusicOff_Hover = Handler.GetTexture("Button_MusicOff_Hover");

            Texture2D? button_AmbienceOn = Handler.GetTexture("Button_AmbienceOn");
            Texture2D? button_AmbienceOn_Hover = Handler.GetTexture("Button_AmbienceOn_Hover");

            Texture2D? button_AmbienceOff = Handler.GetTexture("Button_AmbienceOff");
            Texture2D? button_AmbienceOff_Hover = Handler.GetTexture("Button_AmbienceOff_Hover");

            Texture2D? button_SoundOn = Handler.GetTexture("Button_SoundOn");
            Texture2D? button_SoundOn_Hover = Handler.GetTexture("Button_SoundOn_Hover");

            Texture2D? button_SoundOff = Handler.GetTexture("Button_SoundOff");
            Texture2D? button_SoundOff_Hover = Handler.GetTexture("Button_SoundOff_Hover");

            int Y = (int)(Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 2));

            AddButton(Handler.GetID(), "Back", button_Back, button_Back_Hover, button_Back_Disabled,
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

            Button? back = GetButton("Back");
            if (back != null)
            {
                back.HoverText = "Back";
            }

            Y += 2;
            AddButton(Handler.GetID(), "Controls", button_Controls, button_Controls_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

            Button? controls = GetButton("Controls");
            if (controls != null)
            {
                controls.HoverText = "Keybindings";
            }

            Y += 1;
            if (Main.Game.GraphicsManager != null &&
                Main.Game.GraphicsManager.IsFullScreen)
            {
                AddButton(Handler.GetID(), "FullscreenOn", button_FullscreenOn, button_FullscreenOn_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? fullScreenOn = GetButton("FullscreenOn");
                if (fullScreenOn != null)
                {
                    fullScreenOn.Value = 1;
                    fullScreenOn.HoverText = "Toggle Fullscreen";
                }
            }
            else
            {
                AddButton(Handler.GetID(), "FullscreenOff", button_FullscreenOff, button_FullscreenOff_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? fullScreenOff = GetButton("FullscreenOff");
                if (fullScreenOff != null)
                {
                    fullScreenOff.HoverText = "Toggle Fullscreen";
                }
            }

            Y += 1;
            if (SoundManager.MusicEnabled)
            {
                AddButton(Handler.GetID(), "MusicOn", button_MusicOn, button_MusicOn_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? musicOn = GetButton("MusicOn");
                if (musicOn != null)
                {
                    musicOn.Value = 1;
                    musicOn.HoverText = "Toggle Music";
                }

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, progressBase, progressBar,
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            else
            {
                AddButton(Handler.GetID(), "MusicOff", button_MusicOff, button_MusicOff_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? musicOff = GetButton("MusicOff");
                if (musicOff != null)
                {
                    musicOff.HoverText = "Toggle Music";
                }

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, progressBase, progressBar,
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }

            ProgressBar? music_bar = GetProgressBar("Music");
            if (music_bar != null)
            {
                GetVolume(music_bar);
            }

            Y += 1;
            if (SoundManager.AmbientEnabled)
            {
                AddButton(Handler.GetID(), "AmbienceOn", button_AmbienceOn, button_AmbienceOn_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? ambienceOn = GetButton("AmbienceOn");
                if (ambienceOn != null)
                {
                    ambienceOn.Value = 1;
                    ambienceOn.HoverText = "Toggle Ambience";
                }

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, progressBase, progressBar,
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            else
            {
                AddButton(Handler.GetID(), "AmbienceOff", button_AmbienceOff, button_AmbienceOff_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? ambienceOff = GetButton("AmbienceOff");
                if (ambienceOff != null)
                {
                    ambienceOff.HoverText = "Toggle Ambience";
                }

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, progressBase, progressBar,
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }

            ProgressBar? ambience_bar = GetProgressBar("Ambience");
            if (ambience_bar != null)
            {
                GetVolume(ambience_bar);
            }

            Y += 1;
            if (SoundManager.SoundEnabled)
            {
                AddButton(Handler.GetID(), "SoundOn", button_SoundOn, button_SoundOn_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? soundOn = GetButton("SoundOn");
                if (soundOn != null)
                {
                    soundOn.Value = 1;
                    soundOn.HoverText = "Toggle Sound";
                }

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, progressBase, progressBar,
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }
            else
            {
                AddButton(Handler.GetID(), "SoundOff", button_SoundOff, button_SoundOff_Hover, null,
                    new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                Button? soundOff = GetButton("SoundOff");
                if (soundOff != null)
                {
                    soundOff.HoverText = "Toggle Sound";
                }

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, progressBase, progressBar,
                    new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), new Color(200, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y), true);
            }

            ProgressBar? sound_bar = GetProgressBar("Sound");
            if (sound_bar != null)
            {
                GetVolume(sound_bar);
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (Main.Game == null)
            {
                return;
            }

            int Y = (int)(Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 2));

            Button? back = GetButton("Back");
            if (back != null)
            {
                back.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Y += 2;
            Button? controls = GetButton("Controls");
            if (controls != null)
            {
                controls.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Y += 1;
            Button? fullscreen = GetButton("FullscreenOn") ?? GetButton("FullscreenOff");
            if (fullscreen != null)
            {
                fullscreen.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Y += 1;
            Button? music = GetButton("MusicOn") ?? GetButton("MusicOff");
            if (music != null)
            {
                music.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            ProgressBar? music_bar = GetProgressBar("Music");
            if (music_bar != null)
            {
                music_bar.Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            }

            Label? music_label = GetLabel("Music");
            if (music_label != null)
            {
                music_label.Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            }

            Y += 1;
            Button? ambience = GetButton("AmbienceOn") ?? GetButton("AmbienceOff");
            if (ambience != null)
            {
                ambience.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            ProgressBar? ambience_bar = GetProgressBar("Ambience");
            if (ambience_bar != null)
            {
                ambience_bar.Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            }

            Label? ambience_label = GetLabel("Ambience");
            if (ambience_label != null)
            {
                ambience_label.Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            }

            Y += 1;
            Button? sound = GetButton("SoundOn") ?? GetButton("SoundOff");
            if (sound != null)
            {
                sound.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X, Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            ProgressBar? sound_bar = GetProgressBar("Sound");
            if (sound_bar != null)
            {
                sound_bar.Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            }

            Label? sound_label = GetLabel("Sound");
            if (sound_label != null)
            {
                sound_label.Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 4, Main.Game.MenuSize_Y);
            }
        }

        #endregion
    }
}
