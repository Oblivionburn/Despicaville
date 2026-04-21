using Despicaville.Util;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Jobs;
using OP_Engine.Utility;

namespace Despicaville.Tasks
{
    public class UseItem : Task
    {
        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Inventory inventory = null;
            Item item = null;

            foreach (Item existing in character.Inventory.Items)
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
                    inventory = character.Inventory;
                    break;
                }
            }

            if (item == null)
            {
                if (Handler.Trading)
                {
                    Inventory other_inventory = InventoryManager.GetInventory(Handler.Trading_InventoryID[Handler.Trading_InventoryID.Count - 1]);
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

            if (item != null)
            {
                int eat = 0;
                int drink = 0;

                if (InventoryUtil.IsFood(item) ||
                    item.Task == "Inject")
                {
                    Property hunger = item.GetProperty("Hunger");
                    if (hunger != null)
                    {
                        eat++;

                        Property stat = character.GetStat("Hunger");
                        stat.Value += hunger.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property thirst = item.GetProperty("Thirst");
                    if (thirst != null)
                    {
                        drink++;

                        Property stat = character.GetStat("Thirst");
                        stat.Value += thirst.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property stamina = item.GetProperty("Stamina");
                    if (stamina != null)
                    {
                        eat++;
                        drink++;

                        Property stat = character.GetStat("Stamina");
                        stat.Value += stamina.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property consciousness = item.GetProperty("Consciousness");
                    if (consciousness != null)
                    {
                        eat++;

                        Property stat = character.GetStat("Consciousness");
                        stat.Value += consciousness.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property paranoia = item.GetProperty("Paranoia");
                    if (paranoia != null)
                    {
                        eat++;

                        Property stat = character.GetStat("Paranoia");
                        stat.Value += paranoia.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property bladder = item.GetProperty("Bladder");
                    if (bladder != null)
                    {
                        drink++;

                        Property stat = character.GetStat("Bladder");
                        stat.Value += bladder.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property blood = item.GetProperty("Blood");
                    if (blood != null)
                    {
                        drink++;

                        Property stat = character.GetStat("Blood");
                        stat.Value += blood.Value;

                        if (stat.Value > stat.Max_Value)
                        {
                            stat.Value = stat.Max_Value;
                        }
                    }

                    Property pain = item.GetProperty("Pain");
                    if (pain != null)
                    {
                        eat++;

                        Property stat = character.GetStatusEffect("Painkiller");
                        if (stat != null)
                        {
                            stat.Value += pain.Value;

                            if (stat.Value > stat.Max_Value)
                            {
                                stat.Value = stat.Max_Value;
                            }
                        }
                        else
                        {
                            character.StatusEffects.Add(new Property
                            {
                                Name = "Painkiller",
                                Value = pain.Value
                            });
                        }
                    }

                    Property poison = item.GetProperty("Poison");
                    if (poison != null)
                    {
                        drink++;

                        Property stat = character.GetStatusEffect("Poisoned");
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
                            character.StatusEffects.Add(new Property
                            {
                                Name = "Poisoned",
                                Value = poison.Value
                            });
                        }
                    }

                    inventory.Items.Remove(item);

                    Inventory assets = InventoryManager.GetInventory("Assets");
                    Item new_item = null;

                    if (item.Name.Contains("Bottle"))
                    {
                        new_item = assets.GetItem("Bottle");
                    }
                    else if (item.Name.Contains("Bucket"))
                    {
                        new_item = assets.GetItem("Bucket");
                    }
                    else if (item.Name.Contains("Cannister"))
                    {
                        new_item = assets.GetItem("Cannister");
                    }
                    else if (item.Name.Contains("Syringe"))
                    {
                        new_item = assets.GetItem("Syringe");
                    }

                    if (new_item != null)
                    {
                        Item copy = InventoryUtil.CopyItem(new_item, true);
                        character.Inventory.Items.Add(copy);
                    }
                }

                if (character.Type == "Player")
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

        public Character GetOwner()
        {
            Army army = CharacterManager.GetArmy("Characters");
            if (army != null)
            {
                int squadCount = army.Squads.Count;
                for (int s = 0; s < squadCount; s++)
                {
                    Squad squad = army.Squads[s];

                    int charCount = squad.Characters.Count;
                    for (int c = 0; c < charCount; c++)
                    {
                        Character existing = squad.Characters[c];
                        if (existing.ID == OwnerID)
                        {
                            return existing;
                        }
                    }
                }
            }

            return null;
        }
    }
}
