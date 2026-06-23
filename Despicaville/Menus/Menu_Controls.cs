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
        string? input_name;

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

        public override void Update(Game? gameRef, ContentManager? content)
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
            else if (InputManager.Keyboard != null)
            {
                foreach (var map in InputManager.Keyboard.KeysMapped)
                {
                    if (button.Name == map.Key)
                    {
                        toggle_control = true;

                        Label? label = GetLabel("ChangeControl");
                        if (label != null)
                        {
                            label.Visible = true;
                        }
                        
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
            SaveUtil.ExportINI();

            Visible = false;
            Active = false;

            Picture? title = SceneManager.GetScene("Title")?.Menu?.GetPicture("Title");
            if (title != null)
            {
                title.Visible = true;
            }

            Menu? options = MenuManager.GetMenu("Options");
            if (options != null)
            {
                options.Visible = true;
                options.Active = true;
            }
        }

        private void SetInput(ContentManager? content)
        {
            if (InputManager.Keyboard != null &&
                input_name != null)
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
        }

        private void Reset()
        {
            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
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

        public override void Load(ContentManager? content)
        {
            Clear();

            Color label_color = new(255, 255, 255, 255);

            Texture2D? buttonFrame = Handler.GetTexture("ButtonFrame");
            Texture2D? buttonFrame_Highlight = Handler.GetTexture("ButtonFrame_Highlight");

            AddButton(Handler.GetID(), "Back", Handler.GetTexture("Button_Back"), Handler.GetTexture("Button_Back_Hover"), Handler.GetTexture("Button_Back_Disabled"),
                new Region(0, 0, 0, 0), Color.White, true);

            Button? back = GetButton("Back");
            if (back != null)
            {
                back.HoverText = "Back";
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Label_Control", "Control", label_color, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Label_Key", "Key", label_color, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ChangeControl", "Press any Key...", Color.White, new Region(0, 0, 0, 0), false);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Up", "Up", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Up_Key", InputManager.GetMappedKey("Up").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right", "Right", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Key", InputManager.GetMappedKey("Right").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Down", "Down", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Down_Key", InputManager.GetMappedKey("Down").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left", "Left", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Key", InputManager.GetMappedKey("Left").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Crouch", "Crouch", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Crouch_Key", InputManager.GetMappedKey("Crouch").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Run", "Run", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Run_Key", InputManager.GetMappedKey("Run").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat", "Combat", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_Key", InputManager.GetMappedKey("Combat").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel", "Cancel", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel_Key", InputManager.GetMappedKey("Cancel").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Inventory", "Inventory", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Inventory_Key", InputManager.GetMappedKey("Inventory").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Map", "Map", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Map_Key", InputManager.GetMappedKey("Map").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Wait", "Wait", Color.White, Color.Red, buttonFrame, buttonFrame_Highlight, buttonFrame,
                new Region(0, 0, 0, 0), false, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Wait_Key", InputManager.GetMappedKey("Wait").ToString(), label_color,
                new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, Handler.GetTexture("Frame"), new Region(0, 0, 0, 0), false);

            if (Main.Game != null)
            {
                Resize(Main.Game.Resolution);
            }
        }

        public override void Resize(Point point)
        {
            if (Main.Game == null)
            {
                return;
            }

            int height = (int)(Main.Game.MenuSize_Y / 2);
            int Y = (int)(Main.Game.ScreenHeight / (Main.Game.MenuSize_Y * 8));
            int label_width = 4;

            Button? back = GetButton("Back");
            if (back != null)
            {
                back.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                back.HoverText = "Back";
            }

            Y += 4;

            Label? label_Control = GetLabel("Label_Control");
            if (label_Control != null)
            {
                label_Control.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }
            
            Label? label_Key = GetLabel("Label_Key");
            if (label_Key != null)
            {
                label_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? up = GetButton("Up");
            if (up != null)
            {
                up.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }
            
            Label? up_Key = GetLabel("Up_Key");
            if (up_Key != null)
            {
                up_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }
            
            Label? changeControl = GetLabel("ChangeControl");
            if (changeControl != null)
            {
                changeControl.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y * Y, Main.Game.MenuSize_X * 8, Main.Game.MenuSize_Y);
            }

            Y += 1;
            Button? right = GetButton("Right");
            if (right != null)
            {
                right.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? right_Key = GetLabel("Right_Key");
            if (right_Key != null)
            {
                right_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? down = GetButton("Down");
            if (down != null)
            {
                down.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? down_Key = GetLabel("Down_Key");
            if (down_Key != null)
            {
                down_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }            

            Y += 1;
            Button? left = GetButton("Left");
            if (left != null)
            {
                left.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? left_Key = GetLabel("Left_Key");
            if (left_Key != null)
            {
                left_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? crouch = GetButton("Crouch");
            if (crouch != null)
            {
                crouch.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? crouch_Key = GetLabel("Crouch_Key");
            if (crouch_Key != null)
            {
                crouch_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? run = GetButton("Run");
            if (run != null)
            {
                run.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? run_Key = GetLabel("Run_Key");
            if (run_Key != null)
            {
                run_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? combat = GetButton("Combat");
            if (combat != null)
            {
                combat.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? combat_Key = GetLabel("Combat_Key");
            if (combat_Key != null)
            {
                combat_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? cancel = GetButton("Cancel");
            if (cancel != null)
            {
                cancel.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? cancel_Key = GetLabel("Cancel_Key");
            if (cancel_Key != null)
            {
                cancel_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? inventory = GetButton("Inventory");
            if (inventory != null)
            {
                inventory.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }
            
            Label? inventory_Key = GetLabel("Inventory_Key");
            if (inventory_Key != null)
            {
                inventory_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? map = GetButton("Map");
            if (map != null)
            {
                map.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? map_Key = GetLabel("Map_Key");
            if (map_Key != null)
            {
                map_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }

            Y += 1;
            Button? wait = GetButton("Wait");
            if (wait != null)
            {
                wait.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5), height * Y, Main.Game.MenuSize_X * 4, height);
            }

            Label? wait_Key = GetLabel("Wait_Key");
            if (wait_Key != null)
            {
                wait_Key.Region = new Region((Main.Game.ScreenWidth / 2), height * Y, Main.Game.MenuSize_X * label_width, height);
            }
        }

        #endregion
    }
}
