using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Despicaville.Util;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Inventories;
using OP_Engine.Characters;
using OP_Engine.Time;

namespace Despicaville.Menus
{
    public class Menu_Wounds : Menu
    {
        #region Variables

        private bool menu;
        private long selected_wound;

        Character player;
        List<Picture> GridList = new List<Picture>();

        private int inventory_x;
        private int inventory_y;

        #endregion

        #region Constructors

        public Menu_Wounds(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Wounds";
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
                    if (picture.Name != "Highlight")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (BodyPart bodyPart in player.BodyParts)
                {
                    foreach (Something wound in bodyPart.Wounds)
                    {
                        if (wound.Visible)
                        {
                            spriteBatch.Draw(wound.Texture, wound.Region.ToRectangle, Color.White);
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
            bool found_button = HoveringButton();
            bool found_wound = HoveringWound();
            bool found_grid = HoveringGrid();

            if ((!found_button &&
                !found_wound) ||
                menu)
            {
                Label label = GetLabel("Examine");
                if (label != null)
                {
                    label.Visible = false;
                }
            }

            if (!found_grid)
            {
                Picture picture = GetPicture("Highlight");
                if (picture != null)
                {
                    picture.Visible = false;
                }
            }
        }

        private bool HoveringButton()
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

            return found;
        }

        private bool HoveringWound()
        {
            bool found = false;

            foreach (BodyPart bodyPart in player.BodyParts)
            {
                foreach (Something wound in bodyPart.Wounds)
                {
                    if (InputManager.MouseWithin(wound.Region.ToRectangle))
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

            return found;
        }

        private bool HoveringGrid()
        {
            foreach (Picture grid in GridList)
            {
                if (InputManager.MouseWithin(grid.Region.ToRectangle))
                {
                    Picture highlight = GetPicture("Highlight");
                    highlight.Region = grid.Region;
                    highlight.Visible = true;
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

        private void OpenMenu(Something wound)
        {
            CloseMenu();

            selected_wound = wound.ID;
            menu = true;

            int x = (int)wound.Region.X + (int)wound.Region.Width;
            int y = (int)wound.Region.Y;
            int width = Main.Game.MenuSize_X * 3;
            int height = Main.Game.MenuSize_Y;

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel", "Cancel", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                new Region(x, y, width, height), false, true);

            y += Main.Game.MenuSize_Y;
            bool found = false;
            foreach (Item item in player.Inventory.Items)
            {
                if ((wound.Name == "Break" ||
                     wound.Name == "Fracture") &&
                    item.Name == "Splint")
                {
                    found = true;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Splint", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
                else if ((wound.Name == "Gunshot" ||
                          wound.Name == "Stab") &&
                         item.Name == "Needle & Thread")
                {
                    found = true;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Needle & Thread", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
                else if (wound.Name == "Cut" &&
                         item.Name == "Bandaid")
                {
                    found = true;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Bandaid", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
                else if ((wound.Name == "Stitched" ||
                          wound.Name == "Burn") &&
                         item.Name == "Bandage")
                {
                    found = true;
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Heal", "Use Bandage", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(x, y, width, height), false, true);
                    break;
                }
            }

            if (!found)
            {
                if (wound.Name == "Break" ||
                    wound.Name == "Fracture")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Splint", Color.White, AssetManager.Textures["Frame"],
                        new Region(x, y, width, height), true);
                }
                else if (wound.Name == "Gunshot" ||
                         wound.Name == "Stab")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Needle & Thread", Color.White, AssetManager.Textures["Frame"],
                        new Region(x, y, width, height), true);
                }
                else if (wound.Name == "Cut")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Bandaid", Color.White, AssetManager.Textures["Frame"],
                        new Region(x, y, width, height), true);
                }
                else if (wound.Name == "Stitched" ||
                         wound.Name == "Burn")
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Missing Bandage", Color.White, AssetManager.Textures["Frame"],
                        new Region(x, y, width, height), true);
                }
                else
                {
                    AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Missing", "Must Recover Naturally", Color.White, AssetManager.Textures["Frame"],
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
            foreach (BodyPart bodyPart in player.BodyParts)
            {
                foreach (Something wound in bodyPart.Wounds)
                {
                    if (wound.ID == selected_wound)
                    {
                        if (wound.Name == "Break" ||
                            wound.Name == "Fracture")
                        {
                            wound.Name = "Set";
                            wound.Texture = AssetManager.Textures["Wound_Set"];
                        }
                        else if (wound.Name == "Gunshot" ||
                                 wound.Name == "Stab")
                        {
                            wound.Name = "Stitched";
                            wound.Texture = AssetManager.Textures["Wound_Stitched"];
                        }
                        else if (wound.Name == "Cut" ||
                                 wound.Name == "Stitched" ||
                                 wound.Name == "Burn")
                        {
                            wound.Name = "Covered";
                            wound.Texture = AssetManager.Textures["Wound_Covered"];
                        }

                        wound.Image = new Rectangle(0, 0, wound.Texture.Width, wound.Texture.Height);
                        wound.Value /= 2;

                        break;
                    }
                }
            }
        }

        public override void Close()
        {
            ClearGrid();

            TimeManager.Paused = false;
            Visible = false;
            Active = false;
        }

        private void ClearGrid()
        {
            if (!string.IsNullOrEmpty(Handler.Selected_BodyPart))
            {
                Label label = GetLabel(Handler.Selected_BodyPart);
                if (label != null)
                {
                    Labels.Remove(label);
                }

                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        Picture existing = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                        if (existing != null)
                        {
                            Pictures.Remove(existing);

                            foreach (Picture grid in GridList)
                            {
                                if (grid.ID == existing.ID)
                                {
                                    GridList.Remove(existing);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadGrid()
        {
            if (!string.IsNullOrEmpty(Handler.Selected_BodyPart))
            {
                string body_part_name = CharacterUtil.BodyPartToName(Handler.Selected_BodyPart);

                AddLabel(AssetManager.Fonts["ControlFont"], 0, Handler.Selected_BodyPart, body_part_name + " Wounds", Color.White,
                    new Region(inventory_x + (Main.Game.MenuSize_X * 3), inventory_y, (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y), true);

                int starting_x = inventory_x;
                int starting_y = inventory_y + Main.Game.MenuSize_Y;

                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        AddPicture(Handler.GetID(), "x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                            new Region(starting_x + (Main.Game.MenuSize_X * x), starting_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                        Picture grid = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                        if (grid != null)
                        {
                            grid.Location = new Location(x, y, 0);
                            GridList.Add(grid);
                        }
                    }
                }

                int grid_x = 0;
                int grid_y = 0;

                foreach (BodyPart bodyPart in player.BodyParts)
                {
                    foreach (Something wound in bodyPart.Wounds)
                    {
                        Picture grid = GetPicture("x:" + grid_x.ToString() + ",y:" + grid_y.ToString());
                        if (grid != null)
                        {
                            wound.Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                            wound.Image = new Rectangle(0, 0, wound.Texture.Width, wound.Texture.Height);
                            wound.Visible = true;

                            grid_x++;
                            if (grid_x >= 10)
                            {
                                grid_x = 0;
                                grid_y++;
                            }
                        }
                    }
                }
            }
        }

        private void ResizeGrid()
        {
            if (player != null)
            {
                int starting_x = inventory_x;
                int starting_y = inventory_y + Main.Game.MenuSize_Y;

                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        Picture existing = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                        if (existing != null)
                        {
                            existing.Region = new Region(starting_x + (Main.Game.MenuSize_X * x), starting_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                        }
                    }
                }

                int grid_x = 0;
                int grid_y = 0;

                foreach (BodyPart bodyPart in player.BodyParts)
                {
                    foreach (Something wound in bodyPart.Wounds)
                    {
                        Picture grid = GetPicture("x:" + grid_x.ToString() + ",y:" + grid_y.ToString());
                        if (grid != null)
                        {
                            wound.Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                            wound.Image = new Rectangle(0, 0, wound.Texture.Width, wound.Texture.Height);

                            grid_x++;
                            if (grid_x >= 10)
                            {
                                grid_x = 0;
                                grid_y++;
                            }
                        }
                    }
                }
            }
        }

        private void ExamineItem(Something wound)
        {
            int width = (Main.Game.MenuSize_X * 4) + (Main.Game.MenuSize_X / 2);
            int height = Main.Game.MenuSize_Y + (Main.Game.MenuSize_Y / 2);

            Label examine = GetLabel("Examine");
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
            player = Handler.GetPlayer();
            LoadGrid();
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddButton(Handler.GetID(), "Close", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Close").HoverText = "Close";

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);
            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
        }

        public override void Resize(Point point)
        {
            inventory_y = (Main.Game.ScreenHeight / 2) - (Main.Game.MenuSize_Y * 8);
            inventory_x = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 5);

            int x = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X / 2);
            int y = inventory_y + (Main.Game.MenuSize_Y * 10) + Main.Game.MenuSize_Y;

            GetButton("Close").Region = new Region(x, y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            ResizeGrid();
        }

        #endregion
    }
}
