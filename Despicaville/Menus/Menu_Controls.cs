using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Controls;
using OP_Engine.Utility;
using OP_Engine.Scenes;

using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Controls : Menu
    {
        #region Variables

        bool toggle_control;
        string input_name;

        #endregion

        #region Constructor

        public Menu_Controls(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Controls";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (toggle_control)
                {
                    SetInput(content);
                }
                else
                {
                    UpdateControls();
                }

                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                if (toggle_control)
                {
                    foreach (Label label in Labels)
                    {
                        if (label.Name == "ChangeControl")
                        {
                            label.Draw(spriteBatch);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Picture picture in Pictures)
                    {
                        picture.Draw(spriteBatch);
                    }

                    foreach (Button button in Buttons)
                    {
                        button.Draw(spriteBatch);
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

            if (InputManager.KeyPressed("Cancel") &&
                !toggle_control)
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
            else
            {
                foreach (var map in InputManager.Keyboard.KeysMapped)
                {
                    if (button.Name == map.Key)
                    {
                        toggle_control = true;
                        GetLabel("ChangeControl").Visible = true;
                        input_name = map.Key;
                        break;
                    }
                }
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        private void Back()
        {
            Save.ExportINI();

            Visible = false;
            Active = false;

            SceneManager.GetScene("Title").Menu.GetPicture("Title").Visible = true;

            Menu options = MenuManager.GetMenu("Options");
            options.Visible = true;
            options.Active = true;
        }

        private void SetInput(ContentManager content)
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (InputManager.KeyPressed(key))
                {
                    InputManager.Keyboard.KeysMapped[input_name] = key;
                    toggle_control = false;
                    break;
                }
            }

            if (!toggle_control)
            {
                Load(content);
                Reset();
            }
        }

        private void Reset()
        {
            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (!string.IsNullOrEmpty(button.Text))
                        {
                            button.Opacity = 1;
                        }

                        button.Selected = true;
                    }
                    else if (InputManager.Mouse_Moved)
                    {
                        if (!string.IsNullOrEmpty(button.Text))
                        {
                            button.Opacity = 0.8f;
                        }

                        button.Selected = false;
                    }
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            Color label_color = new Color(255, 255, 255, 255);

            AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Back").HoverText = "Back";

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Label_Control", "Control", label_color, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Label_Key", "Key", label_color, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ChangeControl", "Press any Key...", Color.White, new Region(0, 0, 0, 0), false);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Up", "Up", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Up_Key", InputManager.GetMappedKey("Up").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right", "Right", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Key", InputManager.GetMappedKey("Right").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Down", "Down", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Down_Key", InputManager.GetMappedKey("Down").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left", "Left", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Key", InputManager.GetMappedKey("Left").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Crouch", "Crouch", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Crouch_Key", InputManager.GetMappedKey("Crouch").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Run", "Run", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Run_Key", InputManager.GetMappedKey("Run").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Interact", "Interact", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Interact_Key", InputManager.GetMappedKey("Interact").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel", "Cancel", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel_Key", InputManager.GetMappedKey("Cancel").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Inventory", "Inventory", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Inventory_Key", InputManager.GetMappedKey("Inventory").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Stats", "Stats", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Stats_Key", InputManager.GetMappedKey("Stats").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Skills", "Skills", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Skills_Key", InputManager.GetMappedKey("Skills").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Map", "Map", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Map_Key", InputManager.GetMappedKey("Map").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Wait", "Wait", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Wait_Key", InputManager.GetMappedKey("Wait").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int height = Main.Game.MenuSize_Y / 2;
            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 8);
            int label_width = 4;

            GetButton("Back").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Back").HoverText = "Back";

            Y += 4;
            GetLabel("Label_Control").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Label_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Up").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Up_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            GetLabel("ChangeControl").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 8, Main.Game.MenuSize_Y);

            Y += 1;
            GetButton("Right").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Right_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Down").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Down_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Left").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Left_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Crouch").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Crouch_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Run").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Run_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Interact").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Interact_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Cancel").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Cancel_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Inventory").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Inventory_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Stats").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Stats_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Skills").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Skills_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Map").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Map_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);

            Y += 1;
            GetButton("Wait").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            GetLabel("Wait_Key").Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
        }

        #endregion
    }
}
