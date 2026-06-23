using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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

        public override void Update(Game? gameRef, ContentManager? content)
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
            if (Main.Game == null)
            {
                return;
            }

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
                Label? examine = GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }
            }

            if (Main.Game.GameStarted &&
                InputManager.KeyPressed("Cancel"))
            {
                Close();
            }
        }

        private void CheckClick(Button button)
        {
            if (Main.Game == null)
            {
                return;
            }

            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

            if (button.Name == "Back")
            {
                Close();
            }
            else if (button.Name == "Play")
            {
                Visible = false;
                Active = false;

                SceneManager.GetScene("CharGen")?.Load();
                SceneManager.ChangeScene("CharGen");
            }
            else if (button.Name == "MapEditor")
            {
                Visible = false;
                Active = false;

                Picture? title = SceneManager.GetScene("Title")?.Menu?.GetPicture("Title");
                if (title != null)
                {
                    title.Visible = false;
                }

                Menu? mapEditor = MenuManager.GetMenu("MapEditor");
                if (mapEditor != null)
                {
                    mapEditor.Load();
                    mapEditor.Visible = true;
                    mapEditor.Active = true;
                }
            }
            else if (button.Name == "Main")
            {
                GameUtil.ReturnToTitle();
            }
            else if (button.Name == "Options")
            {
                Visible = false;
                Active = false;

                Menu? options = MenuManager.GetMenu("Options");
                if (options != null)
                {
                    options.Visible = true;
                    options.Active = true;
                }
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

            Menu? menu_health = MenuManager.GetMenu("Health");
            if (menu_health != null)
            {
                menu_health.Active = false;
                menu_health.Visible = false;
            }

            Menu? menu_ui = MenuManager.GetMenu("UI");
            if (menu_ui != null)
            {
                menu_ui.Active = false;
                menu_ui.Visible = false;
            }
        }

        public override void Close()
        {
            Menu? ui = MenuManager.GetMenu("UI");
            if (ui != null)
            {
                ui.Visible = true;
                ui.Active = true;
            }

            if (Handler.Menu_Health)
            {
                Menu? menu_health = MenuManager.GetMenu("Health");
                if (menu_health != null)
                {
                    menu_health.Active = true;
                    menu_health.Visible = true;
                }
            }

            InputManager.Keyboard?.Flush();
            TimeManager.Paused = false;
            Visible = false;
            Active = false;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            Texture2D? frame = Handler.GetTexture("Frame");

            Texture2D? button_Back = Handler.GetTexture("Button_Back");
            Texture2D? button_Back_Hover = Handler.GetTexture("Button_Back_Hover");
            Texture2D? button_Back_Disabled = Handler.GetTexture("Button_Back_Disabled");

            Texture2D? button_Play = Handler.GetTexture("Button_Play");
            Texture2D? button_Play_Hover = Handler.GetTexture("Button_Play_Hover");
            Texture2D? button_Play_Disabled = Handler.GetTexture("Button_Play_Disabled");

            Texture2D? button_Map = Handler.GetTexture("Button_Map");
            Texture2D? button_Map_Hover = Handler.GetTexture("Button_Map_Hover");

            Texture2D? button_Options = Handler.GetTexture("Button_Options");
            Texture2D? button_Options_Hover = Handler.GetTexture("Button_Options_Hover");

            Texture2D? button_Main = Handler.GetTexture("Button_Main");
            Texture2D? button_Main_Hover = Handler.GetTexture("Button_Main_Hover");

            Texture2D? button_Exit = Handler.GetTexture("Button_Exit");
            Texture2D? button_Exit_Hover = Handler.GetTexture("Button_Exit_Hover");

            AddButton(Handler.GetID(), "Back", button_Back, button_Back_Hover, button_Back_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? back = GetButton("Back");
            if (back != null)
            {
                back.HoverText = "Resume";
            }

            AddButton(Handler.GetID(), "Play", button_Play, button_Play_Hover, button_Play_Disabled,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? play = GetButton("Play");
            if (play != null)
            {
                play.HoverText = "Play";
            }

            AddButton(Handler.GetID(), "MapEditor", button_Map, button_Map_Hover, null,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? mapEditor = GetButton("MapEditor");
            if (mapEditor != null)
            {
                mapEditor.HoverText = "Map Editor";
            }

            AddButton(Handler.GetID(), "Options", button_Options, button_Options_Hover, null,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? options = GetButton("Options");
            if (options != null)
            {
                options.HoverText = "Options";
            }

            AddButton(Handler.GetID(), "Main", button_Main, button_Main_Hover, null,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? main = GetButton("Main");
            if (main != null)
            {
                main.HoverText = "Back to Title";
            }

            AddButton(Handler.GetID(), "Exit", button_Exit, button_Exit_Hover, null,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? exit = GetButton("Exit");
            if (exit != null)
            {
                exit.HoverText = "Exit";
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Version", "v" + Main.Version, Color.White,
                new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);

            if (Main.Game == null)
            {
                return;
            }
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (Main.Game == null)
            {
                return;
            }

            float Y = (Main.Game.ScreenHeight / 2) - (Main.Game.MenuSize_Y / 2);
            float X = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2);

            Button? back = GetButton("Back");
            if (back != null)
            {
                back.Region = new Region(X, Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }
            

            Button? play = GetButton("Play");
            if (play != null)
            {
                play.Region = new Region(X, Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Y += Main.Game.MenuSize_Y;

            Button? mapEditor = GetButton("MapEditor");
            if (mapEditor != null)
            {
                mapEditor.Region = new Region(X, Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Y += Main.Game.MenuSize_Y;

            Button? options = GetButton("Options");
            if (options != null)
            {
                options.Region = new Region(X, Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Y += Main.Game.MenuSize_Y;
            Button? main = GetButton("Main");
            if (main != null)
            {
                main.Region = new Region(X, Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Button? exit = GetButton("Exit");
            if (exit != null)
            {
                exit.Region = new Region(X, Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Label? version = GetLabel("Version");
            if (version != null)
            {
                version.Region = new Region(Main.Game.ScreenWidth - (Main.Game.MenuSize_X * 2) - 16, Main.Game.ScreenHeight - Main.Game.MenuSize_X,
                    Main.Game.MenuSize_X * 2, Main.Game.MenuSize_X);
            }

            Label? examine = GetLabel("Examine");
            if (examine != null)
            {
                examine.Region = new Region(0, 0, 0, 0);
            }
        }

        #endregion
    }
}
