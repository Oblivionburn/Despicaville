using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Controls;
using OP_Engine.Utility;
using OP_Engine.Time;

using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Main : Menu
    {
        #region Variables

        private int TextWidth = (Main.Game.ScreenWidth / 16);
        private int TextHeight = (Main.Game.ScreenHeight / 32);

        #endregion

        #region Constructor

        public Menu_Main(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Main";
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

                        if (InputManager.Mouse_LB_Pressed_Flush)
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
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Back")
            {
                Close();
            }
            else if (button.Name == "Play")
            {
                Visible = false;
                Active = false;

                SceneManager.GetScene("CharGen").Load();
                SceneManager.ChangeScene("CharGen");
            }
            else if (button.Name == "Save")
            {
                TimeManager.Paused = false;
                //Handler.Save();
                //GameUtil.ShowAlert("Your progress has been saved!");
            }
            else if (button.Name == "Main")
            {
                GameUtil.ReturnToTitle();
            }
            else if (button.Name == "Options")
            {
                Visible = false;
                Active = false;

                MenuManager.GetMenu("Options").Visible = true;
                MenuManager.GetMenu("Options").Active = true;
            }
            else if (button.Name == "Exit")
            {
                Main.Game.Quit = true;
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        public override void Open()
        {
            TimeManager.Paused = true;
            Visible = true;
            Active = true;
        }

        public override void Close()
        {
            TimeManager.Paused = false;
            Visible = false;
            Active = false;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 2);

            AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, false);
            GetButton("Back").HoverText = "Resume";

            Y += 2;
            AddButton(Handler.GetID(), "Play", AssetManager.Textures["Button_Play"], AssetManager.Textures["Button_Play_Hover"], AssetManager.Textures["Button_Play_Disabled"],
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
            GetButton("Play").HoverText = "Play";

            AddButton(Handler.GetID(), "Save", AssetManager.Textures["Button_Save"], AssetManager.Textures["Button_Save_Hover"], AssetManager.Textures["Button_Save_Disabled"],
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, false);
            GetButton("Save").HoverText = "Save Game";

            Y += 1;
            AddButton(Handler.GetID(), "Main", AssetManager.Textures["Button_Main"], AssetManager.Textures["Button_Main_Hover"], null,
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, false);
            GetButton("Main").HoverText = "Back to Title";

            AddButton(Handler.GetID(), "Options", AssetManager.Textures["Button_Options"], AssetManager.Textures["Button_Options_Hover"], null,
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), GetButton("Play").Region.Y + (Main.Game.MenuSize_Y * 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
            GetButton("Options").HoverText = "Options";

            AddButton(Handler.GetID(), "Exit", AssetManager.Textures["Button_Exit"], AssetManager.Textures["Button_Exit_Hover"], null,
                new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), GetButton("Play").Region.Y + (Main.Game.MenuSize_Y * 3), Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);
            GetButton("Exit").HoverText = "Exit";

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Version", "v" + Main.Version, Color.White,
                new Region(Main.Game.ScreenWidth - (Main.Game.MenuSize_X * 2) - 16, Main.Game.ScreenHeight - Main.Game.MenuSize_X, Main.Game.MenuSize_X * 2, Main.Game.MenuSize_X), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 2);

            Button back = GetButton("Back");
            back.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Y += 2;
            Button play = GetButton("Play");
            play.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Button save = GetButton("Save");
            save.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Y += 1;
            Button main = GetButton("Main");
            main.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Button options = GetButton("Options");
            options.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), GetButton("Play").Region.Y + (Main.Game.MenuSize_Y * 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Button exit = GetButton("Exit");
            exit.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), GetButton("Play").Region.Y + (Main.Game.MenuSize_Y * 3), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            Label version = GetLabel("Version");
            version.Region = new Region(Main.Game.ScreenWidth - (Main.Game.MenuSize_X * 2) - 16, Main.Game.ScreenHeight - Main.Game.MenuSize_X, Main.Game.MenuSize_X * 2, Main.Game.MenuSize_X);

            Label examine = GetLabel("Examine");
            examine.Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
