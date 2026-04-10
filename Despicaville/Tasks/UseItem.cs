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
                    Something hunger = item.GetProperty("Hunger");
                    if (hunger != null)
                    {
                        eat++;
                        character.GetStat("Hunger").IncreaseValue(hunger.Value);
                    }

                    Something thirst = item.GetProperty("Thirst");
                    if (thirst != null)
                    {
                        drink++;
                        character.GetStat("Thirst").IncreaseValue(thirst.Value);
                    }

                    Something stamina = item.GetProperty("Stamina");
                    if (stamina != null)
                    {
                        eat++;
                        drink++;
                        character.GetStat("Stamina").IncreaseValue(stamina.Value);
                    }

                    Something consciousness = item.GetProperty("Consciousness");
                    if (consciousness != null)
                    {
                        eat++;
                        character.GetStat("Consciousness").IncreaseValue(consciousness.Value);
                    }

                    Something paranoia = item.GetProperty("Paranoia");
                    if (paranoia != null)
                    {
                        eat++;
                        character.GetStat("Paranoia").IncreaseValue(paranoia.Value);
                    }

                    Something bladder = item.GetProperty("Bladder");
                    if (bladder != null)
                    {
                        drink++;
                        character.GetStat("Bladder").IncreaseValue(bladder.Value);
                    }

                    Something blood = item.GetProperty("Blood");
                    if (blood != null)
                    {
                        drink++;
                        character.GetStat("Blood").IncreaseValue(blood.Value);
                    }

                    Something pain = item.GetProperty("Pain");
                    if (pain != null)
                    {
                        eat++;

                        Something painKiller = character.GetStatusEffect("Painkiller");
                        if (painKiller != null)
                        {
                            painKiller.IncreaseValue(pain.Value);
                        }
                        else
                        {
                            character.StatusEffects.Add(new Something
                            {
                                Name = "Painkiller",
                                Value = pain.Value
                            });
                        }
                    }

                    Something poison = item.GetProperty("Poison");
                    if (poison != null)
                    {
                        drink++;

                        Something poisoned = character.GetStatusEffect("Poisoned");
                        if (poisoned != null)
                        {
                            poisoned.IncreaseValue(poison.Value);
                        }
                        else
                        {
                            character.StatusEffects.Add(new Something
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
            if (OwnerIDs.Count > 0)
            {
                long id = OwnerIDs[0];

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
                            if (existing.ID == id)
                            {
                                return existing;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
