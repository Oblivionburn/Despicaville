using System;
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
    public class Menu_Inventory : Menu
    {
        #region Variables

        List<Picture> GridList = new List<Picture>();
        List<Picture> Other_GridList = new List<Picture>();

        private bool using_item;

        private bool moving;
        private Rectangle starting_pos;
        private long selected_Item;

        List<Inventory> Inventories = new List<Inventory>();
        private int inventory_y;
        private int inventory_x;

        Inventory other_inventory;
        private int other_inventory_y;
        private int other_inventory_x;

        #endregion

        #region Constructor

        public Menu_Inventory(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Inventory";
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (moving)
                {
                    MoveItem();
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
                foreach (Picture picture in Pictures)
                {
                    if (picture.Name != "Highlight")
                    {
                        picture.Draw(spriteBatch);
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

                foreach (Inventory inventory in Inventories)
                {
                    foreach (Item item in inventory.Items)
                    {
                        if (item.Icon_Visible)
                        {
                            spriteBatch.Draw(item.Icon, item.Icon_Region.ToRectangle, item.Icon_Image, item.Icon_DrawColor);
                        }
                    }
                }

                if (other_inventory != null)
                {
                    foreach (Item item in other_inventory.Items)
                    {
                        if (item.Icon_Visible)
                        {
                            spriteBatch.Draw(item.Icon, item.Icon_Region.ToRectangle, item.Icon_Image, item.Icon_DrawColor);
                        }
                    }
                }

                Character player = Handler.GetPlayer();
                if (player != null)
                {
                    foreach (Item item in player.Inventory.Items)
                    {
                        if (item.Icon_Visible)
                        {
                            spriteBatch.Draw(item.Icon, item.Icon_Region.ToRectangle, item.Icon_Image, item.Icon_DrawColor);
                        }
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
            bool found_item = HoveringItem();

            bool found_grid = false;
            bool found_slot = false;
            if (!found_item)
            {
                found_slot = FoundSlot();
                found_grid = HoveringGrid();
            }

            if ((!found_button &&
                !found_item &&
                !found_slot) ||
                moving)
            {
                Label label = GetLabel("Examine");
                if (label != null)
                {
                    label.Visible = false;
                }
            }

            if ((!found_button &&
                !found_item &&
                !found_slot &&
                !found_grid) ||
                moving)
            {
                Picture picture = GetPicture("Highlight");
                if (picture != null)
                {
                    picture.Visible = false;
                }
            }

            if (!found_item ||
                moving)
            {
                Picture highlight1 = GetPicture("SlotHighlight1");
                if (highlight1 != null)
                {
                    highlight1.Visible = false;
                }

                Picture highlight2 = GetPicture("SlotHighlight2");
                if (highlight2 != null)
                {
                    highlight2.Visible = false;
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

        private bool FoundSlot()
        {
            bool found_slot = HoveringSlot("Mask Slot");

            if (!found_slot)
            {
                found_slot = HoveringSlot("Hat Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Coat Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Shirt Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Backpack Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Right Glove Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Pants Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Left Glove Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Right Weapon Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Shoes Slot");
            }
            if (!found_slot)
            {
                found_slot = HoveringSlot("Left Weapon Slot");
            }

            return found_slot;
        }

        private bool HoveringSlot(string type)
        {
            bool found = false;

            foreach (Picture picture in Pictures)
            {
                if (picture.Name.Contains("Slot"))
                {
                    picture.Opacity = 0.8f;
                }
            }

            Picture slot = GetPicture(type);
            if (slot != null)
            {
                if (InputManager.MouseWithin(slot.Region.ToRectangle))
                {
                    found = true;

                    if (!moving)
                    {
                        GameUtil.Examine(this, slot.Name);
                    }

                    slot.Opacity = 1;

                    Picture highlight = GetPicture("Highlight");
                    highlight.Region = slot.Region;
                    highlight.Visible = true;
                }
            }

            return found;
        }

        private bool HoveringItem()
        {
            bool found = false;
            bool open_container = false;
            using_item = false;

            foreach (Picture picture in Pictures)
            {
                if (picture.Name.Contains("Slot"))
                {
                    picture.Opacity = 0.8f;
                }
            }

            Character player = Handler.GetPlayer();
            if (player != null)
            {
                foreach (Item item in player.Inventory.Items)
                {
                    if (item.Icon_Visible)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            Picture highlight = GetPicture("Highlight");
                            highlight.Region = item.Icon_Region;
                            highlight.Visible = true;

                            if (InputManager.Mouse_LB_Held &&
                                InputManager.Mouse_Moved)
                            {
                                moving = true;
                                starting_pos = item.Icon_Region.ToRectangle;
                                selected_Item = item.ID;

                                if (item.Equipped &&
                                    InventoryUtil.IsContainer(item))
                                {
                                    LoadInventories();
                                }
                            }

                            break;
                        }
                    }
                }
            }

            foreach (Inventory inventory in Inventories)
            {
                foreach (Item item in inventory.Items)
                {
                    if (item.Icon_Visible)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            Picture highlight = GetPicture("Highlight");
                            highlight.Region = item.Icon_Region;
                            highlight.Visible = true;

                            foreach (Picture grid in GridList)
                            {
                                if (InputManager.MouseWithin(grid.Region.ToRectangle))
                                {
                                    List<Picture> slots = GetSlots(item);
                                    if (slots.Count == 0)
                                    {
                                        GetPicture("SlotHighlight1").Visible = false;
                                        GetPicture("SlotHighlight2").Visible = false;
                                    }
                                    else if (slots.Count == 1)
                                    {
                                        Picture slot_highlight = GetPicture("SlotHighlight1");
                                        slot_highlight.Region = slots[0].Region;
                                        slot_highlight.Visible = true;

                                        GetPicture("SlotHighlight2").Visible = false;
                                    }
                                    else if (slots.Count == 2)
                                    {
                                        Picture slot_highlight1 = GetPicture("SlotHighlight1");
                                        slot_highlight1.Region = slots[0].Region;
                                        slot_highlight1.Visible = true;

                                        Picture slot_highlight2 = GetPicture("SlotHighlight2");
                                        slot_highlight2.Region = slots[1].Region;
                                        slot_highlight2.Visible = true;
                                    }

                                    break;
                                }
                            }

                            if (InputManager.Mouse_LB_Held &&
                                InputManager.Mouse_Moved)
                            {
                                moving = true;
                                starting_pos = item.Icon_Region.ToRectangle;
                                selected_Item = item.ID;

                                if (item.Equipped &&
                                    InventoryUtil.IsContainer(item))
                                {
                                    LoadInventories();
                                }
                            }
                            else if (InputManager.Mouse_RB_Pressed)
                            {
                                if (item.Type == "Container" &&
                                    !Handler.Trading_InventoryID.Contains(item.Inventory.ID))
                                {
                                    open_container = true;
                                    Handler.Trading = true;
                                    Handler.Trading_InventoryID.Add(item.Inventory.ID);
                                    GetButton("Back").Visible = true;
                                    LoadInventories();
                                }
                                else if (InventoryUtil.UseableItem(item))
                                {
                                    using_item = UseItem(item);
                                    if (using_item)
                                    {
                                        Close();
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                if (moving ||
                    open_container ||
                    using_item)
                {
                    break;
                }
            }

            if (!found &&
                other_inventory != null)
            {
                foreach (Item item in other_inventory.Items)
                {
                    if (item.Icon_Visible)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            Picture highlight = GetPicture("Highlight");
                            highlight.Region = item.Icon_Region;
                            highlight.Visible = true;

                            foreach (Picture grid in Other_GridList)
                            {
                                if (InputManager.MouseWithin(grid.Region.ToRectangle))
                                {
                                    List<Picture> slots = GetSlots(item);
                                    if (slots.Count == 0)
                                    {
                                        GetPicture("SlotHighlight1").Visible = false;
                                        GetPicture("SlotHighlight2").Visible = false;
                                    }
                                    else if (slots.Count == 1)
                                    {
                                        Picture slot_highlight = GetPicture("SlotHighlight1");
                                        slot_highlight.Region = slots[0].Region;
                                        slot_highlight.Visible = true;

                                        GetPicture("SlotHighlight2").Visible = false;
                                    }
                                    else if (slots.Count == 2)
                                    {
                                        Picture slot_highlight1 = GetPicture("SlotHighlight1");
                                        slot_highlight1.Region = slots[0].Region;
                                        slot_highlight1.Visible = true;

                                        Picture slot_highlight2 = GetPicture("SlotHighlight2");
                                        slot_highlight2.Region = slots[1].Region;
                                        slot_highlight2.Visible = true;
                                    }

                                    break;
                                }
                            }

                            if (InputManager.Mouse_LB_Held &&
                                InputManager.Mouse_Moved)
                            {
                                moving = true;
                                starting_pos = item.Icon_Region.ToRectangle;
                                selected_Item = item.ID;
                            }
                            else if (InputManager.Mouse_RB_Pressed)
                            {
                                if (item.Type == "Container" &&
                                    !Handler.Trading_InventoryID.Contains(item.Inventory.ID))
                                {
                                    Handler.Trading_InventoryID.Add(item.Inventory.ID);
                                    GetButton("Back").Visible = true;
                                    LoadInventories();
                                }
                                else if (InventoryUtil.UseableItem(item))
                                {
                                    using_item = UseItem(item);
                                    if (using_item)
                                    {
                                        Close();
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            return found;
        }

        private bool HoveringGrid()
        {
            bool found = false;

            foreach (Picture grid in GridList)
            {
                if (InputManager.MouseWithin(grid.Region.ToRectangle))
                {
                    found = true;

                    Picture highlight = GetPicture("Highlight");
                    highlight.Region = grid.Region;
                    highlight.Visible = true;

                    break;
                }
            }

            if (!found)
            {
                foreach (Picture grid in Other_GridList)
                {
                    if (InputManager.MouseWithin(grid.Region.ToRectangle))
                    {
                        found = true;

                        Picture highlight = GetPicture("Highlight");
                        highlight.Region = grid.Region;
                        highlight.Visible = true;

                        break;
                    }
                }
            }

            return found;
        }

        private void MoveItem()
        {
            Character player = Handler.GetPlayer();

            if (Inventories.Count > 0)
            {
                Item item = null;
                Inventory item_inventory = null;

                bool equipped = false;

                foreach (Item existing in player.Inventory.Items)
                {
                    if (existing.ID == selected_Item)
                    {
                        item = existing;
                        item_inventory = player.Inventory;
                        equipped = existing.Equipped;
                        break;
                    }
                }

                if (item_inventory == null &&
                    item == null)
                {
                    foreach (Inventory inventory in Inventories)
                    {
                        foreach (Item existing in inventory.Items)
                        {
                            if (existing.ID == selected_Item)
                            {
                                item = existing;
                                item_inventory = inventory;
                                equipped = existing.Equipped;
                                break;
                            }
                        }

                        if (item != null)
                        {
                            break;
                        }
                    }
                }

                if (item_inventory == null &&
                    item == null &&
                    other_inventory != null)
                {
                    foreach (Item existing in other_inventory.Items)
                    {
                        if (existing.ID == selected_Item)
                        {
                            item = existing;
                            item_inventory = other_inventory;
                            break;
                        }
                    }
                }

                if (item != null)
                {
                    bool found_grid = HoveringGrid();
                    bool found_slot = FoundSlot();
                    if (!found_slot &&
                        !found_grid)
                    {
                        GetPicture("Highlight").Visible = false;
                    }

                    if (InputManager.Mouse_LB_Held)
                    {
                        item.Icon_Region = new Region(InputManager.Mouse.X - (Main.Game.MenuSize_X / 2), InputManager.Mouse.Y - (Main.Game.MenuSize_Y / 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                    }
                    else
                    {
                        moving = false;

                        if (equipped)
                        {
                            if (!found_grid)
                            {
                                selected_Item = 0;
                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);

                                LoadInventories();
                            }
                            else
                            {
                                bool found = false;

                                foreach (Picture grid in GridList)
                                {
                                    if (InputManager.MouseWithin(grid.Region.ToRectangle))
                                    {
                                        Inventory existing = InventoryUtil.GetInventory_FromGrid(grid);
                                        if (existing != null)
                                        {
                                            found = true;

                                            bool slot_empty = true;

                                            foreach (Item existing_item in existing.Items)
                                            {
                                                if (existing_item.Location.X == grid.Location.X &&
                                                    existing_item.Location.Y == grid.Location.Y)
                                                {
                                                    slot_empty = false;
                                                    break;
                                                }
                                            }

                                            if (slot_empty)
                                            {
                                                item.Equipped = false;
                                                item.Location = new Location(grid.Location.X, grid.Location.Y, 0);
                                                item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);

                                                selected_Item = 0;

                                                InventoryUtil.TransferItem(Handler.GetPlayer().Inventory, existing, item);

                                                Picture slot = GetPicture(item.Assignment);
                                                if (slot != null)
                                                {
                                                    item.Assignment = "";
                                                    ResetSlot(slot);
                                                }
                                            }
                                            else
                                            {
                                                selected_Item = 0;
                                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                            }
                                        }
                                        else
                                        {
                                            selected_Item = 0;
                                            item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                        }

                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    foreach (Picture grid in Other_GridList)
                                    {
                                        if (InputManager.MouseWithin(grid.Region.ToRectangle))
                                        {
                                            Inventory existing = InventoryUtil.GetInventory_FromGrid(grid);
                                            if (existing != null)
                                            {
                                                found = true;

                                                bool slot_empty = true;

                                                foreach (Item existing_item in existing.Items)
                                                {
                                                    if (existing_item.Location.X == grid.Location.X &&
                                                        existing_item.Location.Y == grid.Location.Y)
                                                    {
                                                        slot_empty = false;
                                                        break;
                                                    }
                                                }

                                                if (slot_empty)
                                                {
                                                    item.Equipped = false;
                                                    item.Location = new Location(grid.Location.X, grid.Location.Y, 0);
                                                    item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);

                                                    selected_Item = 0;

                                                    InventoryUtil.TransferItem(Handler.GetPlayer().Inventory, existing, item);

                                                    Picture slot = GetPicture(item.Assignment);
                                                    if (slot != null)
                                                    {
                                                        item.Assignment = "";
                                                        ResetSlot(slot);
                                                    }
                                                }
                                                else
                                                {
                                                    selected_Item = 0;
                                                    item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                                }
                                            }
                                            else
                                            {
                                                selected_Item = 0;
                                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!found_slot &&
                                !found_grid)
                            {
                                selected_Item = 0;
                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                            }
                            else if (found_grid)
                            {
                                bool found = false;

                                foreach (Picture grid in GridList)
                                {
                                    if (InputManager.MouseWithin(grid.Region.ToRectangle))
                                    {
                                        found = true;

                                        Inventory existing = InventoryUtil.GetInventory_FromGrid(grid);
                                        if (existing != null)
                                        {
                                            string[] properties = grid.Name.Split(';');
                                            string[] coords = properties[1].Split(',');
                                            string[] x_coord = coords[0].Split(':');
                                            string[] y_coord = coords[1].Split(':');

                                            int x = int.Parse(x_coord[1]);
                                            int y = int.Parse(y_coord[1]);

                                            bool slot_empty = true;

                                            foreach (Item existing_item in existing.Items)
                                            {
                                                if (existing_item.Location.X == x &&
                                                    existing_item.Location.Y == y)
                                                {
                                                    slot_empty = false;
                                                    break;
                                                }
                                            }

                                            if (slot_empty)
                                            {
                                                item.Location = new Location(x, y, 0);
                                                item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);

                                                selected_Item = 0;

                                                if (existing.ID != item_inventory.ID)
                                                {
                                                    InventoryUtil.TransferItem(item_inventory, existing, item);
                                                }
                                            }
                                            else
                                            {
                                                selected_Item = 0;
                                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                            }
                                        }

                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    foreach (Picture grid in Other_GridList)
                                    {
                                        if (InputManager.MouseWithin(grid.Region.ToRectangle))
                                        {
                                            found = true;

                                            Inventory existing = InventoryUtil.GetInventory_FromGrid(grid);
                                            if (existing != null)
                                            {
                                                string[] properties = grid.Name.Split(';');
                                                string[] coords = properties[1].Split(',');
                                                string[] x_coord = coords[0].Split(':');
                                                string[] y_coord = coords[1].Split(':');

                                                int x = int.Parse(x_coord[1]);
                                                int y = int.Parse(y_coord[1]);

                                                bool slot_empty = true;

                                                foreach (Item existing_item in existing.Items)
                                                {
                                                    if (existing_item.Location.X == x &&
                                                        existing_item.Location.Y == y)
                                                    {
                                                        slot_empty = false;
                                                        break;
                                                    }
                                                }

                                                if (slot_empty)
                                                {
                                                    item.Location = new Location(x, y, 0);
                                                    item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);

                                                    selected_Item = 0;

                                                    if (existing.ID != item_inventory.ID)
                                                    {
                                                        InventoryUtil.TransferItem(item_inventory, existing, item);
                                                    }
                                                }
                                                else
                                                {
                                                    selected_Item = 0;
                                                    item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                                }
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            else if (found_slot)
                            {
                                Picture slot = null;

                                if (item.Type == "Gloves")
                                {
                                    if (HoveringSlot("Right Glove Slot") &&
                                        item.Name.Contains("Right"))
                                    {
                                        slot = GetPicture("Right Glove Slot");
                                    }
                                    else if (HoveringSlot("Left Glove Slot") &&
                                             item.Name.Contains("Left"))
                                    {
                                        slot = GetPicture("Left Glove Slot");
                                    }
                                }
                                else if (item.Type == "Weapon" ||
                                         item.Type == "Tool" ||
                                         item.Name.Contains("Bag") ||
                                         item.Name == "Basket" ||
                                         item.Name == "Briefcase" ||
                                         item.Name == "Purse" ||
                                         item.Name == "Cooler" ||
                                         item.Name == "First Aid Kit")
                                {
                                    if (HoveringSlot("Right Weapon Slot"))
                                    {
                                        slot = GetPicture("Right Weapon Slot");
                                    }
                                    else if (HoveringSlot("Left Weapon Slot"))
                                    {
                                        slot = GetPicture("Left Weapon Slot");
                                    }
                                }
                                else if (item.Name.Contains("Backpack") ||
                                         item.Name == "Quiver" ||
                                         item.Type == "Back")
                                {
                                    if (HoveringSlot("Backpack Slot"))
                                    {
                                        slot = GetPicture("Backpack Slot");
                                    }
                                }
                                else if (HoveringSlot(item.Type + " Slot"))
                                {
                                    slot = GetPicture(item.Type + " Slot");
                                }

                                if (slot != null)
                                {
                                    bool slot_empty = true;

                                    foreach (Item existing in player.Inventory.Items)
                                    {
                                        if (existing.Equipped &&
                                            existing.Assignment == slot.Name)
                                        {
                                            slot_empty = false;
                                            break;
                                        }
                                    }

                                    if (slot_empty)
                                    {
                                        item.Equipped = true;
                                        item.Icon_Region = new Region(slot.Region.X, slot.Region.Y, slot.Region.Width, slot.Region.Height);
                                        item.Assignment = slot.Name;

                                        selected_Item = 0;

                                        InventoryUtil.TransferItem(item_inventory, player.Inventory, item);

                                        slot.Texture = AssetManager.Textures["Slot_Empty"];

                                        LoadInventories();
                                    }
                                    else
                                    {
                                        selected_Item = 0;
                                        item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                    }
                                }
                                else
                                {
                                    selected_Item = 0;
                                    item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool UseItem(Item item)
        {
            bool okay = false;

            Character player = Handler.GetPlayer();

            if (InventoryUtil.IsFood(item))
            {
                Something hunger = item.GetProperty("Hunger");
                if (hunger != null)
                {
                    if (player.GetStat("Hunger").Value < 100)
                    {
                        okay = true;
                        Tasker.AddTask(player, "UseItem_" + item.ID, false, true, TimeSpan.FromSeconds(hunger.Value * -1), default, 0);
                    }
                }

                if (!okay)
                {
                    Something thirst = item.GetProperty("Thirst");
                    if (thirst != null)
                    {
                        if (player.GetStat("Thirst").Value < 100)
                        {
                            okay = true;
                            Tasker.AddTask(player, "UseItem_" + item.ID, false, true, TimeSpan.FromSeconds(thirst.Value * -1), default, 0);
                        }
                    }
                }
            }
            else if (item.Task == "Inject")
            {
                okay = true;
                Tasker.AddTask(player, "UseItem_" + item.ID, false, true, TimeSpan.FromSeconds(1), default, 0);
            }

            if (!okay)
            {
                Something hunger = item.GetProperty("Hunger");
                if (hunger != null)
                {
                    if (player.GetStat("Hunger").Value >= 100)
                    {
                        GameUtil.AddMessage("You are not hungry enough to eat the " + item.Name.ToLower() + ".");
                    }
                }
                else
                {
                    Something thirst = item.GetProperty("Thirst");
                    if (thirst != null)
                    {
                        if (player.GetStat("Thirst").Value >= 100)
                        {
                            GameUtil.AddMessage("You are not thirsty enough to drink the " + item.Name.ToLower() + ".");
                        }
                    }
                }
            }

            return okay;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Close")
            {
                Close();
            }
            else if (button.Name == "Back")
            {
                long last_inventory_id = Handler.Trading_InventoryID[Handler.Trading_InventoryID.Count - 1];
                Handler.Trading_InventoryID.Remove(last_inventory_id);

                if (Handler.Trading_InventoryID.Count <= 1)
                {
                    button.Visible = false;
                }

                if (Handler.Trading_InventoryID.Count == 0)
                {
                    Handler.Trading = false;
                }

                LoadInventories();
            }
        }

        public override void Open()
        {
            TimeManager.Paused = true;
            Load();
            Visible = true;
            Active = true;
        }

        public override void Close()
        {
            Character player = Handler.GetPlayer();
            foreach (Item item in player.Inventory.Items)
            {
                item.Icon_Visible = false;
            }

            foreach (Inventory inventory in Inventories)
            {
                foreach (Item item in inventory.Items)
                {
                    item.Icon_Visible = false;
                }
            }

            Clear_Inventory_Grids();
            Inventories.Clear();

            selected_Item = 0;
            other_inventory = null;
            
            if (!using_item)
            {
                Handler.Trading = false;
                Handler.Trading_InventoryID.Clear();
            }

            TimeManager.Paused = false;
            Visible = false;
            Active = false;
        }

        private void LoadEquipped()
        {
            Character player = Handler.GetPlayer();
            if (player != null)
            {
                SetSlot(player, "Mask Slot");
                SetSlot(player, "Hat Slot");
                SetSlot(player, "Coat Slot");
                SetSlot(player, "Shirt Slot");
                SetSlot(player, "Backpack Slot");
                SetSlot(player, "Right Glove Slot");
                SetSlot(player, "Pants Slot");
                SetSlot(player, "Left Glove Slot");
                SetSlot(player, "Right Weapon Slot");
                SetSlot(player, "Shoes Slot");
                SetSlot(player, "Left Weapon Slot");
            }
        }

        private void ResizeEquipped()
        {
            Character player = Handler.GetPlayer();
            if (player != null)
            {
                ResizeSlot(player, "Mask Slot");
                ResizeSlot(player, "Hat Slot");
                ResizeSlot(player, "Coat Slot");
                ResizeSlot(player, "Shirt Slot");
                ResizeSlot(player, "Backpack Slot");
                ResizeSlot(player, "Right Glove Slot");
                ResizeSlot(player, "Pants Slot");
                ResizeSlot(player, "Left Glove Slot");
                ResizeSlot(player, "Right Weapon Slot");
                ResizeSlot(player, "Shoes Slot");
                ResizeSlot(player, "Left Weapon Slot");
            }
        }

        private void LoadInventories()
        {
            Clear_Inventory_Grids();
            Inventories.Clear();

            if (Handler.Trading)
            {
                other_inventory = InventoryManager.GetInventory(Handler.Trading_InventoryID[Handler.Trading_InventoryID.Count - 1]);
            }
            else
            {
                other_inventory = null;
            }

            inventory_y = (Main.Game.ScreenHeight / 2) - (Main.Game.MenuSize_Y * 4) - Main.Game.MenuSize_Y;

            Character player = Handler.GetPlayer();
            if (player != null)
            {
                foreach (Item item in player.Inventory.Items)
                {
                    if (item.Equipped &&
                        InventoryUtil.IsContainer(item) &&
                        item.ID != selected_Item)
                    {
                        Inventories.Add(item.Inventory);
                    }
                }

                GetLabel("Name").Text = player.Name;
            }
            
            Load_Inventory_Grids();

            if (other_inventory != null)
            {
                Load_Other_Inventory_Grid();
            }
        }

        private void ResizeInventories()
        {
            inventory_y = (Main.Game.ScreenHeight / 2) - (Main.Game.MenuSize_Y * 4) - Main.Game.MenuSize_Y;

            Resize_Inventory_Grids();

            if (other_inventory != null)
            {
                Resize_Other_Inventory_Grid();
            }
        }

        private List<Picture> GetSlots(Item item)
        {
            List<Picture> slots = new List<Picture>();

            Picture slot = GetPicture(item.Type + " Slot");

            if (slot == null)
            {
                if (item.Type == "Gloves")
                {
                    if (item.Name.Contains("Right"))
                    {
                        Picture right_glove_slot = GetPicture("Right Glove Slot");
                        if (right_glove_slot != null)
                        {
                            slots.Add(right_glove_slot);
                        }
                    }
                    else if (item.Name.Contains("Left"))
                    {
                        Picture left_glove_slot = GetPicture("Left Glove Slot");
                        if (left_glove_slot != null)
                        {
                            slots.Add(left_glove_slot);
                        }
                    }
                }
                else if (item.Type == "Weapon" ||
                         item.Type == "Tool" ||
                         item.Name.Contains("Bag") ||
                         item.Name == "Basket" ||
                         item.Name == "Briefcase" ||
                         item.Name == "Purse" ||
                         item.Name == "Cooler" ||
                         item.Name == "First Aid Kit")
                {
                    Picture right_weapon_slot = GetPicture("Right Weapon Slot");
                    if (right_weapon_slot != null)
                    {
                        slots.Add(right_weapon_slot);
                    }

                    Picture left_weapon_slot = GetPicture("Left Weapon Slot");
                    if (left_weapon_slot != null)
                    {
                        slots.Add(left_weapon_slot);
                    }
                }
                else if (item.Name.Contains("Backpack") ||
                         item.Name == "Quiver" ||
                         item.Type == "Back")
                {
                    Picture backpack_slot = GetPicture("Backpack Slot");
                    if (backpack_slot != null)
                    {
                        slots.Add(backpack_slot);
                    }
                }
            }
            else if (slot != null)
            {
                slots.Add(slot);
            }

            return slots;
        }

        private void SetSlot(Character player, string slot)
        {
            Item equip = InventoryUtil.Get_EquippedItem(player, slot);
            if (equip != null)
            {
                Picture slot_pic = GetPicture(slot);
                if (slot_pic != null)
                {
                    equip.Icon_Region = new Region(slot_pic.Region.X, slot_pic.Region.Y, slot_pic.Region.Width, slot_pic.Region.Height);
                    equip.Icon_Image = new Rectangle(0, 0, equip.Icon.Width, equip.Icon.Height);
                    equip.Icon_Visible = true;
                    slot_pic.Texture = AssetManager.Textures["Slot_Empty"];
                }
            }
        }

        private void ResizeSlot(Character player, string slot)
        {
            Item equip = InventoryUtil.Get_EquippedItem(player, slot);
            if (equip != null)
            {
                Picture slot_pic = GetPicture(slot);
                if (slot_pic != null)
                {
                    equip.Icon_Region = new Region(slot_pic.Region.X, slot_pic.Region.Y, slot_pic.Region.Width, slot_pic.Region.Height);
                    equip.Icon_Image = new Rectangle(0, 0, equip.Icon.Width, equip.Icon.Height);
                }
            }
        }

        private void ResetSlot(Picture picture)
        {
            if (picture.Name.Contains("Mask"))
            {
                picture.Texture = AssetManager.Textures["Slot_Mask"];
            }
            else if (picture.Name.Contains("Hat"))
            {
                picture.Texture = AssetManager.Textures["Slot_Hat"];
            }
            else if (picture.Name.Contains("Coat"))
            {
                picture.Texture = AssetManager.Textures["Slot_Coat"];
            }
            else if (picture.Name.Contains("Shirt"))
            {
                picture.Texture = AssetManager.Textures["Slot_Shirt"];
            }
            else if (picture.Name.Contains("Backpack"))
            {
                picture.Texture = AssetManager.Textures["Slot_Backpack"];
            }
            else if (picture.Name.Contains("Right Glove"))
            {
                picture.Texture = AssetManager.Textures["Slot_Glove_Right"];
            }
            else if (picture.Name.Contains("Pants"))
            {
                picture.Texture = AssetManager.Textures["Slot_Pants"];
            }
            else if (picture.Name.Contains("Left Glove"))
            {
                picture.Texture = AssetManager.Textures["Slot_Glove_Left"];
            }
            else if (picture.Name.Contains("Right Weapon"))
            {
                picture.Texture = AssetManager.Textures["Slot_Weapon_Right"];
            }
            else if (picture.Name.Contains("Shoes"))
            {
                picture.Texture = AssetManager.Textures["Slot_Shoes"];
            }
            else if (picture.Name.Contains("Left Weapon"))
            {
                picture.Texture = AssetManager.Textures["Slot_Weapon_Left"];
            }
        }

        private void Clear_Inventory_Grids()
        {
            foreach (Inventory inventory in Inventories)
            {
                Label label = GetLabel(inventory.Name);
                if (label != null)
                {
                    Labels.Remove(label);
                }

                int Y = (int)inventory.Max_Value / 7;
                for (int y = 0; y <= Y; y++)
                {
                    for (int x = 0; x <= inventory.Max_Value; x++)
                    {
                        Picture existing = GetPicture(inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString());
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

            if (other_inventory != null)
            {
                Label label = GetLabel(other_inventory.Name);
                if (label != null)
                {
                    Labels.Remove(label);
                }

                int Y = (int)other_inventory.Max_Value / 7;
                for (int y = 0; y <= Y; y++)
                {
                    for (int x = 0; x <= other_inventory.Max_Value; x++)
                    {
                        Picture existing = GetPicture(other_inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString());
                        if (existing != null)
                        {
                            Pictures.Remove(existing);

                            foreach (Picture grid in Other_GridList)
                            {
                                if (grid.ID == existing.ID)
                                {
                                    Other_GridList.Remove(existing);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Load_Inventory_Grids()
        {
            if (Inventories.Count > 0)
            {
                foreach (Inventory inventory in Inventories)
                {
                    if (!string.IsNullOrEmpty(inventory.Name))
                    {
                        AddLabel(AssetManager.Fonts["ControlFont"], inventory.ID, inventory.Name, inventory.Name, Color.White,
                            new Region(inventory_x, inventory_y, (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y), true);

                        int starting_x = inventory_x;
                        int starting_y = inventory_y + Main.Game.MenuSize_Y;

                        int Y;
                        if (inventory.Max_Value / 7 >= 1)
                        {
                            Y = (int)Math.Ceiling(inventory.Max_Value / 7);
                        }
                        else
                        {
                            Y = 1;
                        }

                        int X;
                        if (inventory.Max_Value > 7)
                        {
                            X = 7;
                        }
                        else
                        {
                            X = (int)inventory.Max_Value;
                        }

                        for (int y = 0; y < Y; y++)
                        {
                            if ((y + 1) * X > inventory.Max_Value)
                            {
                                X = (int)inventory.Max_Value - (y * X);
                            }

                            for (int x = 0; x < X; x++)
                            {
                                AddPicture(Handler.GetID(), inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                                    new Region(starting_x + (Main.Game.MenuSize_X * x), starting_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                                Picture grid = GetPicture(inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString());
                                if (grid != null)
                                {
                                    grid.Location = new Location(x, y, 0);
                                    GridList.Add(grid);
                                }

                                foreach (Item item in inventory.Items)
                                {
                                    if (item.Location.X == x &&
                                        item.Location.Y == y)
                                    {
                                        item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                                        item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
                                        item.Icon_Visible = true;
                                        break;
                                    }
                                }
                            }
                        }

                        inventory_y += (Main.Game.MenuSize_Y * (Y + 1));
                    }
                }
            }
        }

        private void Resize_Inventory_Grids()
        {
            if (Inventories.Count > 0)
            {
                foreach (Inventory inventory in Inventories)
                {
                    if (!string.IsNullOrEmpty(inventory.Name))
                    {
                        Label label = GetLabel(inventory.Name);
                        if (label != null)
                        {
                            label.Region = new Region(inventory_x, inventory_y, (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y);
                        }

                        int starting_x = inventory_x;
                        int starting_y = inventory_y + Main.Game.MenuSize_Y;

                        int Y;
                        if (inventory.Max_Value / 7 >= 1)
                        {
                            Y = (int)inventory.Max_Value / 7;
                        }
                        else
                        {
                            Y = 1;
                        }

                        int X;
                        if (inventory.Max_Value > 7)
                        {
                            X = 7;
                        }
                        else
                        {
                            X = (int)inventory.Max_Value;
                        }

                        for (int y = 0; y < Y; y++)
                        {
                            if ((y + 1) * X > inventory.Max_Value)
                            {
                                X = (int)inventory.Max_Value - (y * X);
                            }

                            for (int x = 0; x < X; x++)
                            {
                                Picture grid = GetPicture(inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString());
                                if (grid != null)
                                {
                                    grid.Region = new Region(starting_x + (Main.Game.MenuSize_X * x), starting_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                                }

                                foreach (Item item in inventory.Items)
                                {
                                    if (item.Location.X == x &&
                                        item.Location.Y == y)
                                    {
                                        item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                                        item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
                                        break;
                                    }
                                }
                            }
                        }

                        inventory_y += (Main.Game.MenuSize_Y * 2);
                    }
                }
            }
        }

        private void Load_Other_Inventory_Grid()
        {
            AddLabel(AssetManager.Fonts["ControlFont"], other_inventory.ID, other_inventory.Name, other_inventory.Name, Color.White,
                new Region(other_inventory_x, other_inventory_y, (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y), true);

            int starting_x = other_inventory_x;
            int starting_y = other_inventory_y + Main.Game.MenuSize_Y;

            int Y;
            if (other_inventory.Max_Value / 7 >= 1)
            {
                Y = (int)Math.Ceiling(other_inventory.Max_Value / 7);
            }
            else
            {
                Y = 1;
            }

            int X;
            if (other_inventory.Max_Value > 7)
            {
                X = 7;
            }
            else
            {
                X = (int)other_inventory.Max_Value;
            }

            for (int y = 0; y < Y; y++)
            {
                if ((y + 1) * X > other_inventory.Max_Value)
                {
                    X = (int)other_inventory.Max_Value - (y * X);
                }

                for (int x = 0; x < X; x++)
                {
                    AddPicture(Handler.GetID(), other_inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(starting_x + (Main.Game.MenuSize_X * x), starting_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y), Color.White, true);

                    Picture grid = GetPicture(other_inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Location(x, y, 0);
                        Other_GridList.Add(grid);
                    }

                    foreach (Item item in other_inventory.Items)
                    {
                        if (item.Location.X == x &&
                            item.Location.Y == y)
                        {
                            item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                            item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
                            item.Icon_Visible = true;
                            break;
                        }
                    }
                }
            }
        }

        private void Resize_Other_Inventory_Grid()
        {
            Label label = GetLabel(other_inventory.Name);
            if (label != null)
            {
                label.Region = new Region(other_inventory_x, other_inventory_y, (Main.Game.MenuSize_X * 4), Main.Game.MenuSize_Y);
            }

            int starting_x = other_inventory_x;
            int starting_y = other_inventory_y + Main.Game.MenuSize_Y;

            int Y;
            if (other_inventory.Max_Value / 7 >= 1)
            {
                Y = (int)Math.Ceiling(other_inventory.Max_Value / 7);
            }
            else
            {
                Y = 1;
            }

            int X;
            if (other_inventory.Max_Value > 7)
            {
                X = 7;
            }
            else
            {
                X = (int)other_inventory.Max_Value;
            }

            for (int y = 0; y < Y; y++)
            {
                if ((y + 1) * X > other_inventory.Max_Value)
                {
                    X = (int)other_inventory.Max_Value - (y * X);
                }

                for (int x = 0; x < X; x++)
                {
                    Picture grid = GetPicture(other_inventory.ID + ";x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Region = new Region(starting_x + (Main.Game.MenuSize_X * x), starting_y + (Main.Game.MenuSize_Y * y), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                    }

                    foreach (Item item in other_inventory.Items)
                    {
                        if (item.Location.X == x &&
                            item.Location.Y == y)
                        {
                            item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                            item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
                            break;
                        }
                    }
                }
            }
        }

        private void ExamineItem(Item item)
        {
            int width = (Main.Game.MenuSize_X * 4) + (Main.Game.MenuSize_X / 2);
            int height = Main.Game.MenuSize_Y + (Main.Game.MenuSize_Y / 2);

            string text = item.Name;

            for (int i = 0; i < item.Properties.Count; i++)
            {
                Something property = item.Properties[i];
                text += "\n" + property.Name + ": " + property.Value;

                if (i < item.Properties.Count - 1)
                {
                    height += (Main.Game.MenuSize_Y / 2);
                }
            }

            Label examine = GetLabel("Examine");
            examine.Text = text;

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

            AddButton(Handler.GetID(), "Close", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Close").HoverText = "Close";

            AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            GetButton("Back").HoverText = "Back";

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name", "", Color.White, new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Mask Slot", AssetManager.Textures["Slot_Mask"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Hat Slot", AssetManager.Textures["Slot_Hat"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Coat Slot", AssetManager.Textures["Slot_Coat"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Shirt Slot", AssetManager.Textures["Slot_Shirt"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Backpack Slot", AssetManager.Textures["Slot_Backpack"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right Glove Slot", AssetManager.Textures["Slot_Glove_Right"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Pants Slot", AssetManager.Textures["Slot_Pants"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left Glove Slot", AssetManager.Textures["Slot_Glove_Left"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right Weapon Slot", AssetManager.Textures["Slot_Weapon_Right"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Shoes Slot", AssetManager.Textures["Slot_Shoes"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left Weapon Slot", AssetManager.Textures["Slot_Weapon_Left"], new Region(0, 0, 0, 0), Color.White, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);
            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "SlotHighlight1", AssetManager.Textures["Highlight"], new Region(0, 0, 0, 0), new Color(0, 255, 0, 255), false);
            AddPicture(Handler.GetID(), "SlotHighlight2", AssetManager.Textures["Highlight"], new Region(0, 0, 0, 0), new Color(0, 255, 0, 255), false);

            LoadEquipped();
            LoadInventories();

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int x = (Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 2);
            int y = (Main.Game.ScreenHeight / 2) - (Main.Game.MenuSize_Y * 4);

            inventory_y = y - Main.Game.MenuSize_Y;
            inventory_x = x + (Main.Game.MenuSize_X * 4);

            other_inventory_y = y - Main.Game.MenuSize_Y;
            other_inventory_x = x - (Main.Game.MenuSize_X * 8);

            if (Pictures.Count > 0)
            {
                GetLabel("Name").Region = new Region(x, y - Main.Game.MenuSize_Y, Main.Game.MenuSize_X * 3, Main.Game.MenuSize_Y);
                GetPicture("Mask Slot").Region = new Region(x, y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Hat Slot").Region = new Region(x + Main.Game.MenuSize_X, y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Coat Slot").Region = new Region(x, y + Main.Game.MenuSize_Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Shirt Slot").Region = new Region(x + Main.Game.MenuSize_X, y + Main.Game.MenuSize_Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Backpack Slot").Region = new Region(x + (Main.Game.MenuSize_X * 2), y + Main.Game.MenuSize_Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Right Glove Slot").Region = new Region(x, y + (Main.Game.MenuSize_Y * 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Pants Slot").Region = new Region(x + Main.Game.MenuSize_X, y + (Main.Game.MenuSize_Y * 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Left Glove Slot").Region = new Region(x + (Main.Game.MenuSize_X * 2), y + (Main.Game.MenuSize_Y * 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Right Weapon Slot").Region = new Region(x, y + (Main.Game.MenuSize_Y * 3), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Shoes Slot").Region = new Region(x + Main.Game.MenuSize_X, y + (Main.Game.MenuSize_Y * 3), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetPicture("Left Weapon Slot").Region = new Region(x + (Main.Game.MenuSize_X * 2), y + (Main.Game.MenuSize_Y * 3), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

                GetButton("Close").Region = new Region(x + Main.Game.MenuSize_X, y + (Main.Game.MenuSize_Y * 5), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
                GetButton("Back").Region = new Region(x - (Main.Game.MenuSize_X * 2), y + (Main.Game.MenuSize_Y * 5), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

                ResizeEquipped();
                ResizeInventories();
            }
        }

        #endregion
    }
}
