using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class InventoryUtil
    {
        public static Item? NewItem(Item? asset)
        {
            if (asset == null)
            {
                return null;
            }

            Item new_item = new()
            {
                ID = Handler.GetID(),
                Amount = 1,
                Name = asset.Name,
                Description = asset.Description,
                Type = asset.Type,
                Rarity = asset.Rarity,
                Tier = asset.Tier,
                Assignment = asset.Assignment,
                Task = asset.Task,
                Sound = asset.Sound,
                SoundRange = asset.SoundRange,
                Icon = asset.Icon,
                Icon_Image = new Rectangle(asset.Icon_Image.X, asset.Icon_Image.Y, asset.Icon_Image.Width, asset.Icon_Image.Height),
                Icon_DrawColor = new Color(asset.Icon_DrawColor.R, asset.Icon_DrawColor.G, asset.Icon_DrawColor.B, asset.Icon_DrawColor.A),
                Texture = asset.Texture,
                Image = new Rectangle(asset.Image.X, asset.Image.Y, asset.Image.Width, asset.Image.Height),
                DrawColor = new Color(asset.DrawColor.R, asset.DrawColor.G, asset.DrawColor.B, asset.DrawColor.A),
                Visible = asset.Visible
            };
            new_item.Inventory.ID = Handler.GetID();
            new_item.Inventory.Name = asset.Inventory.Name;
            new_item.Inventory.Max_Value = asset.Inventory.Max_Value;

            foreach (string category in asset.Categories)
            {
                new_item.Categories.Add(category);
            }

            foreach (string material in asset.Materials)
            {
                new_item.Materials.Add(material);
            }

            foreach (Property property in asset.Properties)
            {
                new_item.Properties.Add(CopyProperty(property));
            }

            if (asset.Attachments != null)
            {
                new_item.Attachments = [];

                foreach (Item item in asset.Attachments)
                {
                    Item? newItem = NewItem(item);
                    if (newItem != null)
                    {
                        new_item.Attachments.Add(newItem);
                    }
                }
            }

            if (asset.Region != null)
            {
                new_item.Region = new Region(asset.Region.X, asset.Region.Y, asset.Region.Width, asset.Region.Height);
            }

            if (asset.Icon_Region != null)
            {
                new_item.Icon_Region = new Region(asset.Icon_Region.X, asset.Icon_Region.Y, asset.Icon_Region.Width, asset.Icon_Region.Height);
            }

            return new_item;
        }

        public static Property CopyProperty(Property original)
        {
            return new Property
            {
                Name = original.Name,
                Description = original.Description,
                Rate = original.Rate,
                Value = original.Value,
                Max_Value = original.Max_Value
            };
        }

        public static void TransferItem(Inventory? source, Inventory? target, Item? item)
        {
            if (source == null ||
                target == null ||
                item == null)
            {
                return;
            }

            source.Items.Remove(item);
            target.Items.Add(item);
        }

        public static void TransferItem_Stacking(Inventory source, Inventory target, Item item)
        {
            if (item.Amount > 1)
            {
                item.Amount--;
            }
            else
            {
                source.Items.Remove(item);
            }

            bool found = false;
            foreach (Item existing in target.Items)
            {
                if (existing.Name == item.Name)
                {
                    found = true;
                    existing.Amount++;
                    break;
                }
            }

            if (!found)
            {
                target.Items.Add(item);
            }
        }

        public static Item? Get_EquippedItem(Character? character, string? slot)
        {
            if (character != null)
            {
                Inventory inventory = character.Inventory;
                foreach (Item item in inventory.Items)
                {
                    if (item.Equipped &&
                        item.Assignment == slot)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public static Item? Get_Hair(Character character)
        {
            Item? result = null;

            foreach (Item item in character.Inventory.Items)
            {
                if (item.Name != null &&
                    item.Name.Contains("Hair"))
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        public static bool HasEmptyHand(Character character)
        {
            Item? leftHand = Get_EquippedItem(character, "Left Weapon Slot");
            Item? rightHand = Get_EquippedItem(character, "Right Weapon Slot");

            if (leftHand == null ||
                rightHand == null)
            {
                return true;
            }

            return false;
        }

        public static bool IsContainer(Item item)
        {
            if (!string.IsNullOrEmpty(item.Inventory.Name))
            {
                return true;
            }

            return false;
        }

        public static bool UseableItem(Item item)
        {
            if (IsFood(item) ||
                item.Task == "Inject")
            {
                return true;
            }

            return false;
        }

        public static bool IsFood(Item item)
        {
            if (item.Type == "Food" ||
                item.Task == "Eat" ||
                item.Task == "Drink")
            {
                return true;
            }

            return false;
        }

        public static bool IsMedical(Item item)
        {
            if (item.Task == "Set Break" ||
                item.Task == "Cover Wound" ||
                item.Task == "Clean Wound" ||
                item.Task == "Stitch Wound")
            {
                return true;
            }

            return false;
        }

        public static bool IsWeapon(Item item)
        {
            if (item.Task == "Swing" ||
                item.Task == "Stab" ||
                item.Task == "Shoot" ||
                item.Task == "Throw")
            {
                return true;
            }

            return false;
        }

        public static Inventory? GetInventory_FromGrid(Picture grid)
        {
            if (grid.Name == null)
            {
                return null;
            }

            string[] properties = grid.Name.Split(';');
            long id = long.Parse(properties[0]);

            Inventory? existing = InventoryManager.GetInventory(id);
            if (existing != null)
            {
                return existing;
            }

            return null;
        }

        public static string? GetCategory_FromTile(Tile tile)
        {
            if (tile.Name == null)
            {
                return null;
            }

            if (tile.Name.Contains("Bathroom"))
            {
                return "Bathroom";
            }
            else if (tile.Name.Contains("Bedroom"))
            {
                return "Bedroom";
            }
            else if (tile.Name.Contains("Driveway"))
            {
                return "Driveway";
            }
            else if (tile.Name.Contains("Grocery Store"))
            {
                return "Grocery Store";
            }
            else if (tile.Name.Contains("Hallway"))
            {
                return "Hallway";
            }
            else if (tile.Name.Contains("Kitchen"))
            {
                return "Kitchen";
            }
            else if (tile.Name.Contains("Lounge"))
            {
                return "Lounge";
            }
            else if (tile.Name.Contains("Office"))
            {
                return "Office";
            }
            else if (tile.Name.Contains("Outdoors"))
            {
                return "Outdoors";
            }
            else if (tile.Name.Contains("Store Counter"))
            {
                return "Store Counter";
            }

            return null;
        }

        public static List<Item> GenLoot(string category, string container, int max_items)
        {
            List<Item> items = [];

            if (string.IsNullOrEmpty(category) ||
                string.IsNullOrEmpty(container))
            {
                return items;
            }

            Inventory? assets = InventoryManager.GetInventory("Assets");
            if (assets == null ||
                assets.Items.Count == 0)
            {
                return items;
            }

            CryptoRandom random = new();

            List<Item> item_pool = [];

            foreach (Item item in assets.Items)
            {
                if (item.Categories.Contains(category) &&
                    item.Categories.Contains(container))
                {
                    random = new CryptoRandom();
                    int tier = random.Next(0, 101);

                    if (item.Tier <= tier)
                    {
                        item_pool.Add(item);
                    }
                }
            }

            if (item_pool.Count > 0)
            {
                int X = 0;
                int Y = 0;

                int amount = random.Next(0, max_items + 1);
                for (int i = 0; i < amount; i++)
                {
                    random = new CryptoRandom();
                    int choice = random.Next(0, item_pool.Count);

                    Item? new_item = NewItem(item_pool[choice]);
                    if (new_item != null)
                    {
                        new_item.Location = new Location(X, Y, 0);

                        items.Add(new_item);

                        X++;
                        if (X > 7)
                        {
                            X = 0;
                            Y++;
                        }
                    }
                }
            }

            return items;
        }
    }
}
