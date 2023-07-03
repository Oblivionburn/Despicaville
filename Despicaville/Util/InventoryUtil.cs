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
        public static Item CopyItem(Item original, bool new_item)
        {
            Item copy = new Item();

            if (new_item)
            {
                copy.ID = Handler.GetID();
                copy.Inventory.ID = Handler.GetID();
                copy.Visible = false;
                copy.Amount = 1;
            }
            else
            {
                copy.ID = original.ID;
                copy.Inventory.ID = original.Inventory.ID;
                copy.Amount = original.Amount;
                copy.Visible = original.Visible;
            }

            copy.Name = original.Name;
            copy.Description = original.Description;
            copy.Type = original.Type;
            copy.Rarity = original.Rarity;
            copy.Tier = original.Tier;
            copy.Assignment = original.Assignment;
            copy.Task = original.Task;

            copy.Inventory.Name = original.Inventory.Name;
            copy.Inventory.Max_Value = original.Inventory.Max_Value;

            foreach (string category in original.Categories)
            {
                copy.Categories.Add(category);
            }

            foreach (string material in original.Materials)
            {
                copy.Materials.Add(material);
            }

            foreach (Something property in original.Properties)
            {
                copy.Properties.Add(CopyProperty(property, new_item));
            }

            if (original.Attachments != null)
            {
                copy.Attachments = new List<Item>();

                foreach (Item item in original.Attachments)
                {
                    copy.Attachments.Add(CopyItem(item, new_item));
                }
            }

            copy.Texture = original.Texture;
            copy.Image = new Rectangle(original.Image.X, original.Image.Y, original.Image.Width, original.Image.Height);
            copy.DrawColor = new Color(original.DrawColor.R, original.DrawColor.G, original.DrawColor.B, original.DrawColor.A);
            copy.Icon = original.Icon;
            copy.Icon_Image = new Rectangle(original.Icon_Image.X, original.Icon_Image.Y, original.Icon_Image.Width, original.Icon_Image.Height);
            copy.Icon_DrawColor = new Color(original.Icon_DrawColor.R, original.Icon_DrawColor.G, original.Icon_DrawColor.B, original.Icon_DrawColor.A);

            if (original.Region != null)
            {
                copy.Region = new Region(original.Region.X, original.Region.Y, original.Region.Width, original.Region.Height);
            }
            
            if (original.Icon_Region != null)
            {
                copy.Icon_Region = new Region(original.Icon_Region.X, original.Icon_Region.Y, original.Icon_Region.Width, original.Icon_Region.Height);
            }
            
            return copy;
        }

        public static Something CopyProperty(Something original, bool new_property)
        {
            Something property = new Something();

            if (new_property)
            {
                property.ID = Handler.GetID();
            }
            else
            {
                property.ID = original.ID;
            }

            property.Name = original.Name;
            property.Type = original.Type;
            property.Description = original.Description;
            property.Rate = original.Rate;
            property.Value = original.Value;
            property.Max_Value = original.Max_Value;

            return property;
        }

        public static void TransferItem(Inventory source, Inventory target, Item item)
        {
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

        public static Item Get_EquippedItem(Character character, string slot)
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

        public static Item Get_Hair(Character character)
        {
            Item result = null;

            foreach (Item item in character.Inventory.Items)
            {
                if (item.Name.Contains("Hair"))
                {
                    result = item;
                    break;
                }
            }

            return result;
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

        public static Inventory GetInventory_FromGrid(Picture grid)
        {
            string[] properties = grid.Name.Split(';');
            long id = long.Parse(properties[0]);

            Inventory existing = InventoryManager.GetInventory(id);
            if (existing != null)
            {
                return existing;
            }

            return null;
        }

        public static string GetCategory_FromTile(Tile tile)
        {
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

        public static Item GenAsset(string name, string description, List<string> categories, int tier, string type, string task, int pain, float blood_loss, string effect)
        {
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Name = name;
            item.Description = description;
            item.Tier = tier;
            item.Task = task;
            item.Icon = AssetManager.Textures[item.Description];
            item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
            item.Icon_DrawColor = Color.White;

            if (categories != null)
            {
                foreach (string category in categories)
                {
                    item.Categories.Add(category);
                }
            }
            
            item.Type = type;
            if (type == "Container")
            {
                item.Inventory.ID = Handler.GetID();
                item.Inventory.Name = item.Name;

                if (item.Name == "Large Backpack" ||
                    item.Name == "Duffel Bag" ||
                    item.Name == "Gym Bag" ||
                    item.Name.Contains("Wooden Chest") ||
                    item.Name == "Barrel" ||
                    item.Name == "Wooden Crate")
                {
                    item.Inventory.Max_Value = 28;
                }
                else if (item.Name == "Medium Backpack" ||
                         item.Name == "Suitcase")
                {
                    item.Inventory.Max_Value = 21;
                }
                else if (item.Name == "Small Backpack" ||
                         item.Name == "Safe" ||
                         item.Name.Contains("Cardboard Box"))
                {
                    item.Inventory.Max_Value = 14;
                }
                else if (item.Name == "Basket" ||
                         item.Name == "Briefcase" ||
                         item.Name == "Paper Bag" ||
                         item.Name == "Plastic Bag" ||
                         item.Name == "Purse" ||
                         item.Name == "Cooler" ||
                         item.Name == "First Aid Kit" ||
                         item.Name == "Present")
                {
                    item.Inventory.Max_Value = 7;
                }
                else if (item.Name == "Quiver" ||
                         item.Name == "Coffin")
                {
                    item.Inventory.Max_Value = 1;
                }
            }

            if (pain > 0)
            {
                Something pain_property = new Something();
                pain_property.Name = "Pain";
                pain_property.Value = pain;
                item.Properties.Add(pain_property);
            }
            
            if (blood_loss > 0)
            {
                Something blood_loss_property = new Something();
                blood_loss_property.Name = "Blood Loss";
                blood_loss_property.Value = blood_loss;
                item.Properties.Add(blood_loss_property);
            }
            
            if (!string.IsNullOrEmpty(effect))
            {
                Something effect_property = new Something();
                effect_property.Name = effect;
                item.Properties.Add(effect_property);
            }
            
            return item;
        }

        public static List<Item> GetLoot(string category, string container, int max_items)
        {
            List<Item> items = new List<Item>();
            
            Inventory assets = InventoryManager.GetInventory("Assets");
            if (assets != null)
            {
                List<Item> item_pool = new List<Item>();

                foreach (Item item in assets.Items)
                {
                    if (item.Categories.Contains(category))
                    {
                        if (container != null)
                        {
                            if (container == "Large Backpack" ||
                                container == "Duffel Bag" ||
                                container == "Gym Bag" ||
                                container == "Medium Backpack" ||
                                container == "Suitcase" ||
                                container == "Small Backpack")
                            {
                                if (item.Type == "Boots" ||
                                    item.Type == "Pants" ||
                                    item.Type == "Shirt" ||
                                    item.Type == "Mask" ||
                                    item.Type == "Hat" ||
                                    item.Type == "Back" ||
                                    item.Type == "Gloves")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container.Contains("Wooden Chest") ||
                                     container == "Barrel" ||
                                     container == "Wooden Crate" ||
                                     container == "Safe" ||
                                     container.Contains("Cardboard Box"))
                            {
                                if (item.Type == "Misc" ||
                                    item.Type == "Weapon" ||
                                    item.Type == "Ammo" ||
                                    item.Type == "Tool")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container == "Briefcase" ||
                                     container == "Purse")
                            {
                                if (item.Type == "Misc")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container == "Present")
                            {
                                item_pool.Add(item);
                            }
                            else if (container == "First Aid Kit")
                            {
                                if (item.Type == "Medical")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container.Contains("Cardboard Box"))
                            {
                                if (item.Type == "Misc" ||
                                    item.Type == "Ammo")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container == "Counter" ||
                                     container == "Desk")
                            {
                                if (item.Type == "Misc" ||
                                    item.Type == "Weapon" ||
                                    item.Type == "Container" ||
                                    item.Type == "Tool")
                                {
                                    item_pool.Add(item);
                                }
                                else if (category == "Bathroom" &&
                                         item.Type == "Medical")
                                {
                                    item_pool.Add(item);
                                }
                                else if (item.Type == "Ammo")
                                {
                                    if (category == "Office" ||
                                        category == "Store Counter" ||
                                        category == "Bedroom")
                                    {
                                        item_pool.Add(item);
                                    }
                                }
                            }
                            else if (container == "Bookshelf")
                            {
                                if (item.Type == "Misc")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container == "Fridge" ||
                                     container == "Cooler")
                            {
                                if (item.Type == "Food")
                                {
                                    item_pool.Add(item);
                                }
                            }
                            else if (container == "Dresser")
                            {
                                if (item.Type == "Boots" ||
                                    item.Type == "Pants" ||
                                    item.Type == "Shirt" ||
                                    item.Type == "Mask" ||
                                    item.Type == "Hat" ||
                                    item.Type == "Back" ||
                                    item.Type == "Gloves" ||
                                    item.Type == "Coat" ||
                                    item.Type == "Shield" ||
                                    item.Type == "Container" ||
                                    item.Type == "Ammo")
                                {
                                    item_pool.Add(item);
                                }
                            }
                        }
                        else if (category == "Outdoors")
                        {
                            if (item.Type == "Misc" ||
                                item.Type == "Weapon" ||
                                item.Type == "Tool")
                            {
                                item_pool.Add(item);
                            }
                        }
                    }
                }

                if (item_pool.Count > 0)
                {
                    int X = 0;
                    int Y = 0;

                    CryptoRandom random = new CryptoRandom();

                    int amount;
                    if (category == "Outdoors")
                    {
                        amount = random.Next(0, 3);
                    }
                    else
                    {
                        amount = random.Next(0, max_items + 1);
                    }

                    for (int i = 0; i < amount; i++)
                    {
                        random = new CryptoRandom();
                        int tier = random.Next(0, 101);

                        List<Item> possible_items = new List<Item>();
                        foreach (Item existing in item_pool)
                        {
                            if (existing.Tier <= tier)
                            {
                                possible_items.Add(existing);
                            }
                        }

                        if (possible_items.Count > 0)
                        {
                            random = new CryptoRandom();
                            int choice = random.Next(0, possible_items.Count);

                            Item new_item = CopyItem(possible_items[choice], true);
                            new_item.Location = new Vector3(X, Y, 0);

                            if (new_item.Name == "Small Backpack" ||
                                new_item.Name == "Cape" ||
                                new_item.Name == "Boots" ||
                                new_item.Name == "Vest" ||
                                new_item.Name == "Glove - Left" ||
                                new_item.Name == "Glove - Right" ||
                                new_item.Name == "Mitten - Left" ||
                                new_item.Name == "Mitten - Right" ||
                                new_item.Name == "Hat" ||
                                new_item.Name == "Winter Hat" ||
                                new_item.Name == "Pants" ||
                                new_item.Name == "Shorts" ||
                                new_item.Name == "Skirt" ||
                                new_item.Name == "Present" ||
                                new_item.Name == "Ribbon" ||
                                new_item.Name == "Corset" ||
                                new_item.Name == "Dress" ||
                                new_item.Name == "Gown" ||
                                new_item.Name == "Hoodie" ||
                                new_item.Name == "Jersey" ||
                                new_item.Name == "Shirt" ||
                                new_item.Name == "Tank Top" ||
                                new_item.Name == "Shoes" ||
                                new_item.Name == "Sandals" ||
                                new_item.Name == "Bunny Slippers")
                            {
                                if (new_item.Name == "Hat")
                                {
                                    new_item.Texture = AssetManager.Textures["Hat"];
                                    new_item.Visible = true;
                                }
                                else if (new_item.Name == "Shirt")
                                {
                                    new_item.Texture = AssetManager.Textures["Shirt"];
                                    new_item.Visible = true;
                                }

                                random = new CryptoRandom();
                                int color_choice = random.Next(0, Handler.Colors.Length);
                                Color new_color = GameUtil.ColorFromName(Handler.Colors[color_choice]);

                                new_item.Name = Handler.Colors[color_choice] + " " + new_item.Name;

                                new_item.DrawColor = new_color;
                                new_item.Icon_DrawColor = new_color;
                            }

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
            }

            return items;
        }
    }
}
