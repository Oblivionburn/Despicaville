using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Inventories;
using OP_Engine.Characters;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Wounds : Menu
    {
        #region Variables

        private bool menu;
        private long selected_wound;

        Item? selected_item;
        List<Picture> GridList = [];

        private int starting_x;
        private int starting_y;

        #endregion

        #region Constructors

        public Menu_Wounds()
        {
            ID = Handler.GetID();
            Name = "Wounds";
            Load();
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
                    if (picture.Name != "Highlight")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                if (Handler.Selected_BodyPart != null)
                {
                    BodyPart? bodyPart = Handler.Player?.GetBodyPart(Handler.Selected_BodyPart);
                    if (bodyPart != null)
                    {
                        foreach (Wound wound in bodyPart.Wounds)
                        {
                            if (wound.Visible &&
                                wound.Region != null)
                            {
                                spriteBatch.Draw(wound.Texture, wound.Region.ToRectangle, Color.White);
                            }
                        }
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
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

                GetLabel("Examine")?.Draw(spriteBatch);
            }
        }

        private void UpdateControls()
        {
            bool found_button = HoveringButton();
            bool found_wound = HoveringWound();
            bool found_grid = HoveringGrid();

            if ((!found_button &&
                !found_wound) ||
                menu)
            {
                Label? label = GetLabel("Examine");
                if (label != null)
                {
                    label.Visible = false;
                }
            }

            if (!found_grid)
            {
                Picture? picture = GetPicture("Highlight");
                if (picture != null)
                {
                    picture.Visible = false;
                }
            }

            if (!found_button &&
                !found_grid)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    Close();
                }
            }

            if (InputManager.KeyPressed("Cancel"))
            {
                Close();
            }
        }

        private bool HoveringButton()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

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
                }
            }

            return found;
        }

        private bool HoveringWound()
        {
            bool found = false;

            if (Handler.Selected_BodyPart == null)
            {
                return false;
            }

            BodyPart? bodyPart = Handler.Player?.GetBodyPart(Handler.Selected_BodyPart);
            if (bodyPart != null)
            {
                foreach (Wound wound in bodyPart.Wounds)
                {
                    if (wound.Visible)
                    {
                        if (wound.Region != null &&
                            InputManager.MouseWithin(wound.Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(wound);

                            if (InputManager.Mouse_RB_Pressed)
                            {
                                found = false;
                                OpenMenu(wound);
                                break;
                            }
                        }
                    }
                }
            }

            return found;
        }

        private bool HoveringGrid()
        {
            foreach (Picture grid in GridList)
            {
                if (grid.Region != null &&
                    InputManager.MouseWithin(grid.Region.ToRectangle))
                {
                    Picture? highlight = GetPicture("Highlight");
                    if (highlight != null)
                    {
                        highlight.Region = grid.Region;
                        highlight.Visible = true;
                    }
                    
                    return true;
                }
            }

            return false;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Close")
            {
                Close();
            }
            else if (button.Name == "Cancel")
            {
                CloseMenu();
            }
            else if (button.Name == "Heal")
            {
                Heal();
                CloseMenu();
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        private void OpenMenu(Wound wound)
        {
            if (Main.Game == null)
            {
                return;
            }

            CloseMenu();

            if (Handler.Player == null ||
                wound.Region == null)
            {
                return;
            }

            selected_wound = wound.ID;
            menu = true;

            int x = (int)wound.Region.X + (int)wound.Region.Width;
            int y = (int)wound.Region.Y;
            int width = (int)(Main.Game.MenuSize_X * 3);
            int height = (int)Main.Game.MenuSize_Y;

            Texture2D? frame = Handler.GetTexture("Frame");

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel", "Cancel", Color.White, Color.Red, frame, frame, null,
                new Region(x, y, width, height), false, true);

            y += (int)Main.Game.MenuSize_Y;
            bool found = false;
            foreach (Item item in Handler.Player.Inventory.Items)
            {
                if ((wound.Name == "Break" ||
                     wound.Name == "Fracture") &&
                    item.Name == "Splint")
                {
                    found = true;
                    selected_item = item;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Splint", Color.White, Color.Red, frame, frame, null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
                else if ((wound.Name == "Gunshot" ||
                          wound.Name == "Stab") &&
                         item.Name == "Needle & Thread")
                {
                    found = true;
                    selected_item = item;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Needle & Thread", Color.White, Color.Red, frame, frame, null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
                else if (wound.Name == "Cut" &&
                         item.Name == "Bandaid")
                {
                    found = true;
                    selected_item = item;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Bandaid", Color.White, Color.Red, frame, frame, null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
                else if ((wound.Name == "Stitched" ||
                          wound.Name == "Burn") &&
                         item.Name == "Bandage")
                {
                    found = true;
                    selected_item = item;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Bandage", Color.White, Color.Red, frame, frame, null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
            }

            if (!found)
            {
                selected_item = null;

                if (wound.Name == "Break" ||
                    wound.Name == "Fracture")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Splint", Color.White, frame,
                        new Region(x, y, width, height), true);
                }
                else if (wound.Name == "Gunshot" ||
                         wound.Name == "Stab")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Needle & Thread", Color.White, frame,
                        new Region(x, y, width, height), true);
                }
                else if (wound.Name == "Cut")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Bandaid", Color.White, frame,
                        new Region(x, y, width, height), true);
                }
                else if (wound.Name == "Stitched" ||
                         wound.Name == "Burn")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Bandage", Color.White, frame,
                        new Region(x, y, width, height), true);
                }
                else
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Must Recover Naturally", Color.White, frame,
                        new Region(x, y, width, height), true);
                }
            }
        }

        private void CloseMenu()
        {
            menu = false;

            for (int i = 0; i < Buttons.Count; i++)
            {
                Button button = Buttons[i];

                if (button.Name == "Cancel" ||
                    button.Name == "Heal")
                {
                    Buttons.Remove(button);
                    i--;
                }
            }

            for (int i = 0; i < Labels.Count; i++)
            {
                Label label = Labels[i];

                if (label.Name == "Missing")
                {
                    Labels.Remove(label);
                    i--;
                }
            }
        }

        private void Heal()
        {
            if (Handler.Selected_BodyPart == null ||
                selected_item == null)
            {
                return;
            }

            BodyPart? bodyPart = Handler.Player?.GetBodyPart(Handler.Selected_BodyPart);
            if (bodyPart != null)
            {
                foreach (Wound wound in bodyPart.Wounds)
                {
                    if (wound.ID == selected_wound)
                    {
                        if (wound.Name == "Break" ||
                            wound.Name == "Fracture")
                        {
                            wound.Name = "Set";
                            wound.Texture = Handler.GetTexture("Wound_Set");
                        }
                        else if (wound.Name == "Gunshot" ||
                                 wound.Name == "Stab")
                        {
                            wound.Name = "Stitched";
                            wound.Texture = Handler.GetTexture("Wound_Stitched");
                        }
                        else if (wound.Name == "Cut" ||
                                 wound.Name == "Stitched" ||
                                 wound.Name == "Burn")
                        {
                            wound.Name = "Covered";
                            wound.Texture = Handler.GetTexture("Wound_Covered");
                        }

                        if (wound.Texture != null)
                        {
                            wound.Image = new Rectangle(0, 0, wound.Texture.Width, wound.Texture.Height);
                        }

                        wound.Value /= 2;

                        Handler.Player?.Inventory.Items.Remove(selected_item);
                        selected_item = null;

                        break;
                    }
                }
            }
        }

        public override void Close()
        {
            InputManager.Keyboard?.Flush();
            InputManager.Mouse?.Flush();

            CloseMenu();

            Handler.Selected_BodyPart = "";
            Visible = false;
            Active = false;

            TimeManager.Paused = false;
        }

        private void ClearGrid()
        {
            if (!string.IsNullOrEmpty(Handler.Selected_BodyPart))
            {
                Label? label = GetLabel(Handler.Selected_BodyPart);
                if (label != null)
                {
                    Labels.Remove(label);
                }

                for (int i = 0; i < GridList.Count; i++)
                {
                    Picture grid = GridList[i];

                    Pictures.Remove(grid);
                    GridList.Remove(grid);

                    i--;
                }

                BodyPart? bodyPart = Handler.Player?.GetBodyPart(Handler.Selected_BodyPart);
                if (bodyPart != null)
                {
                    foreach (Wound wound in bodyPart.Wounds)
                    {
                        wound.Visible = false;
                    }
                }
            }
        }

        private void LoadGrid()
        {
            if (Main.Game == null)
            {
                return;
            }

            ClearGrid();

            if (!string.IsNullOrEmpty(Handler.Selected_BodyPart))
            {
                string? body_part_name = CharacterUtil.BodyPartToName(Handler.Selected_BodyPart);

                AddLabel(AssetManager.Fonts["ControlFont"], 0, Handler.Selected_BodyPart, body_part_name + " Wounds", Color.White,
                    new Region(starting_x + (Main.Game.MenuSize_X * 3), starting_y, (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y), true);

                float base_x = starting_x;
                float base_y = starting_y + Main.Game.MenuSize_Y;

                int x = 0;
                int y = 0;

                BodyPart? bodyPart = Handler.Player?.GetBodyPart(Handler.Selected_BodyPart);
                if (bodyPart != null)
                {
                    Texture2D? grid_texture = Handler.GetTexture("Grid");

                    foreach (Wound wound in bodyPart.Wounds)
                    {
                        AddPicture(Handler.GetID(), "x:" + x.ToString() + ",y:" + y.ToString(), grid_texture,
                            new Region(base_x + (Main.Game.MenuSize_X * x), base_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                        Picture? grid = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                        if (grid != null)
                        {
                            grid.Location = new Location(x, y, 0);
                            GridList.Add(grid);

                            wound.Region = grid.Region;
                            wound.Visible = true;

                            x++;
                            if (x >= 10)
                            {
                                x = 0;
                                y++;
                            }
                        }
                    }
                }
            }
        }

        private void ResizeGrid()
        {
            if (Main.Game == null)
            {
                return;
            }

            float base_x = starting_x;
            float base_y = starting_y + Main.Game.MenuSize_Y;

            int x = 0;
            int y = 0;

            int count = GridList.Count;
            for (int i = 0; i < count; i++)
            {
                Picture grid = GridList[i];

                grid.Region = new Region(base_x + (Main.Game.MenuSize_X * x), base_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

                x++;
                if (x >= 10)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private void ExamineItem(Wound wound)
        {
            if (Main.Game == null)
            {
                return;
            }

            Label? examine = GetLabel("Examine");
            if (examine == null ||
                InputManager.Mouse == null)
            {
                return;
            }

            int width = (int)((Main.Game.MenuSize_X * 4) + (Main.Game.MenuSize_X / 2));
            int height = (int)(Main.Game.MenuSize_Y + (Main.Game.MenuSize_Y / 2));

            examine.Text = wound.Name + "\n" + "Time To Heal: " + GameUtil.SecondsToTime(wound.Value);

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

        public override void Load()
        {
            Clear();

            Texture2D? frame = Handler.GetTexture("Frame");
            Texture2D? grid_Hover = Handler.GetTexture("Grid_Hover");

            Texture2D? button_Back = Handler.GetTexture("Button_Back");
            Texture2D? button_Back_Hover = Handler.GetTexture("Button_Back_Hover");
            Texture2D? button_Back_Disabled = Handler.GetTexture("Button_Back_Disabled");

            AddButton(Handler.GetID(), "Close", button_Back, button_Back_Hover, button_Back_Disabled,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? close = GetButton("Close");
            if (close != null)
            {
                close.HoverText = "Close";
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);
            AddPicture(Handler.GetID(), "Highlight", grid_Hover, new Region(0, 0, 0, 0), Color.White, false);

            LoadGrid();

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

            starting_y = (int)((Main.Game.ScreenHeight / 2) - (Main.Game.MenuSize_Y * 8));
            starting_x = (int)((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5));

            Button? close = GetButton("Close");
            if (close != null)
            {
                close.Region = new Region(starting_x, starting_y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            ResizeGrid();
        }

        #endregion
    }
}
