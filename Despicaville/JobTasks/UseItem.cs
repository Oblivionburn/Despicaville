using Despicaville.Util;
using OP_Engine.Inventories;
using OP_Engine.Jobs;
using OP_Engine.Utility;

namespace Despicaville.JobTasks
{
    public class UseItem : JobTask
    {
        public override void Action_End()
        {
            if (Owner_Character == null)
            {
                return;
            }

            Inventory? inventory = null;
            Item? item = null;

            foreach (Item existing in Owner_Character.Inventory.Items)
            {
                if (InventoryUtil.IsContainer(existing))
                {
                    foreach (Item container_item in existing.Inventory.Items)
                    {
                        if (Name == "UseItem_" + container_item.ID)
                        {
                            item = container_item;
                            inventory = existing.Inventory;
                            break;
                        }
                    }

                    if (item != null)
                    {
                        break;
                    }
                }
                else if (Name == "UseItem_" + existing.ID)
                {
                    item = existing;
                    inventory = Owner_Character.Inventory;
                    break;
                }
            }

            if (inventory == null)
            {
                return;
            }

            if (item == null)
            {
                if (Handler.Trading)
                {
                    Inventory? other_inventory = InventoryManager.GetInventory(Handler.Trading_InventoryID[Handler.Trading_InventoryID.Count - 1]);
                    if (other_inventory != null)
                    {
                        foreach (Item existing in other_inventory.Items)
                        {
                            if (InventoryUtil.IsContainer(existing))
                            {
                                foreach (Item container_item in existing.Inventory.Items)
                                {
                                    if (Name == "UseItem_" + container_item.ID)
                                    {
                                        item = container_item;
                                        inventory = existing.Inventory;
                                        break;
                                    }
                                }

                                if (item != null)
                                {
                                    break;
                                }
                            }
                            else if (Name == "UseItem_" + existing.ID)
                            {
                                item = existing;
                                inventory = other_inventory;
                                break;
                            }
                        }
                    }
                }
            }

            if (item == null)
            {
                return;
            }

            int eat = 0;
            int drink = 0;

            if (InventoryUtil.IsFood(item) ||
                item.Task == "Inject")
            {
                Property? hunger = item.GetProperty("Hunger");
                if (hunger != null)
                {
                    eat++;

                    Owner_Character.Stats.Hunger += hunger.Value;

                    if (Owner_Character.Stats.Hunger > 100)
                    {
                        Owner_Character.Stats.Hunger = 100;
                    }
                }

                Property? thirst = item.GetProperty("Thirst");
                if (thirst != null)
                {
                    drink++;

                    Owner_Character.Stats.Thirst += thirst.Value;

                    if (Owner_Character.Stats.Thirst > 100)
                    {
                        Owner_Character.Stats.Thirst = 100;
                    }
                }

                Property? stamina = item.GetProperty("Stamina");
                if (stamina != null)
                {
                    eat++;
                    drink++;

                    Owner_Character.Stats.Stamina += stamina.Value;

                    if (Owner_Character.Stats.Stamina > 100)
                    {
                        Owner_Character.Stats.Stamina = 100;
                    }
                }

                Property? consciousness = item.GetProperty("Consciousness");
                if (consciousness != null)
                {
                    eat++;

                    Owner_Character.Stats.Consciousness += consciousness.Value;

                    if (Owner_Character.Stats.Consciousness > 100)
                    {
                        Owner_Character.Stats.Consciousness = 100;
                    }
                }

                Property? paranoia = item.GetProperty("Paranoia");
                if (paranoia != null)
                {
                    eat++;

                    Owner_Character.Stats.Paranoia += paranoia.Value;

                    if (Owner_Character.Stats.Paranoia > 100)
                    {
                        Owner_Character.Stats.Paranoia = 100;
                    }
                }

                Property? bladder = item.GetProperty("Bladder");
                if (bladder != null)
                {
                    drink++;

                    Owner_Character.Stats.Bladder += bladder.Value;

                    if (Owner_Character.Stats.Bladder > 100)
                    {
                        Owner_Character.Stats.Bladder = 100;
                    }
                }

                Property? blood = item.GetProperty("Blood");
                if (blood != null)
                {
                    drink++;

                    Owner_Character.Stats.Blood += blood.Value;

                    if (Owner_Character.Stats.Blood > 100)
                    {
                        Owner_Character.Stats.Blood = 100;
                    }
                }

                Property? pain = item.GetProperty("Pain");
                if (pain != null)
                {
                    eat++;

                    Property? stat = Owner_Character.GetStatusEffect("Painkiller");
                    if (stat != null)
                    {
                        stat.Value += pain.Value;

                        if (stat.Value > 100)
                        {
                            stat.Value = 100;
                        }
                    }
                    else
                    {
                        Owner_Character.StatusEffects.Add(new Property
                        {
                            Name = "Painkiller",
                            Value = pain.Value
                        });
                    }
                }

                Property? poison = item.GetProperty("Poison");
                if (poison != null)
                {
                    drink++;

                    Property? stat = Owner_Character.GetStatusEffect("Poisoned");
                    if (stat != null)
                    {
                        stat.Value += poison.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }
                    else
                    {
                        Owner_Character.StatusEffects.Add(new Property
                        {
                            Name = "Poisoned",
                            Value = poison.Value
                        });
                    }
                }

                inventory.Items.Remove(item);

                Inventory? assets = InventoryManager.GetInventory("Assets");
                Item? asset = null;

                if (item.Name != null &&
                    assets != null)
                {
                    if (item.Name.Contains("Bottle"))
                    {
                        asset = assets.GetItem("Bottle");
                    }
                    else if (item.Name.Contains("Bucket"))
                    {
                        asset = assets.GetItem("Bucket");
                    }
                    else if (item.Name.Contains("Cannister"))
                    {
                        asset = assets.GetItem("Cannister");
                    }
                    else if (item.Name.Contains("Syringe"))
                    {
                        asset = assets.GetItem("Syringe");
                    }
                }

                if (asset != null)
                {
                    Item? copy = InventoryUtil.NewItem(asset);
                    if (copy != null)
                    {
                        Owner_Character.Inventory.Items.Add(copy);
                    }
                }
            }

            if (Owner_Character.Type == "Player")
            {
                if (InventoryUtil.IsFood(item))
                {
                    if (item.Task == "Inject")
                    {
                        GameUtil.AddMessage("You injected a " + item.Name + ".");
                    }
                    else if (eat >= drink)
                    {
                        if (GameUtil.NameStartsWithVowel(item.Name))
                        {
                            GameUtil.AddMessage("You ate an " + item.Name + ".");
                        }
                        else
                        {
                            GameUtil.AddMessage("You ate a " + item.Name + ".");
                        }
                    }
                    else if (drink > eat)
                    {
                        if (GameUtil.NameStartsWithVowel(item.Name))
                        {
                            GameUtil.AddMessage("You drank an " + item.Name + ".");
                        }
                        else
                        {
                            GameUtil.AddMessage("You drank a " + item.Name + ".");
                        }
                    }
                }
            }
        }
    }
}
