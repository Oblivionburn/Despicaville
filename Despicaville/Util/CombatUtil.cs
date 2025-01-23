using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class CombatUtil
    {
        public static void Update_Player_BodyStat(Character player, string body_part)
        {
            if (player != null)
            {
                Menu UI = MenuManager.GetMenu("UI");

                Picture picture = UI.GetPicture("Paperdoll_" + body_part);
                if (picture != null)
                {
                    BodyPart part = player.GetBodyPart(body_part);
                    if (part != null)
                    {
                        Something hp = part.GetStat("HP");
                        if (hp != null)
                        {
                            if (hp.Value >= 80)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(0, 200, 0));
                            }
                            else if (hp.Value >= 60)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(200, 200, 0));
                            }
                            else if (hp.Value >= 40)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(200, 100, 0));
                            }
                            else if (hp.Value >= 20)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(200, 0, 0));
                            }
                            else if (hp.Value > 0)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(100, 0, 100));
                            }
                            else if (hp.Value <= 0)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(12, 12, 12));
                            }
                        }
                    }
                }
            }
        }

        public static void Update_Citizen_BodyStat(Character character, string body_part)
        {
            if (character != null)
            {
                Menu combat = MenuManager.GetMenu("Combat");

                Picture picture = combat.GetPicture("Paperdoll_" + body_part);
                if (picture != null)
                {
                    BodyPart part = character.GetBodyPart(body_part);
                    if (part != null)
                    {
                        Something hp = part.GetStat("HP");
                        if (hp != null)
                        {
                            if (hp.Value >= 80)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(0, 200, 0));
                            }
                            else if (hp.Value >= 60)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(200, 200, 0));
                            }
                            else if (hp.Value >= 40)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(200, 100, 0));
                            }
                            else if (hp.Value >= 20)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(200, 0, 0));
                            }
                            else if (hp.Value > 0)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(100, 0, 100));
                            }
                            else if (hp.Value <= 0)
                            {
                                GameUtil.Texture_ChangeColor(picture, new Color(12, 12, 12));
                            }
                        }
                    }
                }
            }
        }

        public static bool CanAttack_Ranged(Character attacker, Character defender)
        {
            Item rightHandItem = InventoryUtil.Get_EquippedItem(attacker, "Right Weapon Slot");
            if (rightHandItem != null)
            {
                if (rightHandItem.Task == "Shoot")
                {
                    return true;
                }
            }

            Item leftHandItem = InventoryUtil.Get_EquippedItem(attacker, "Left Weapon Slot");
            if (leftHandItem != null)
            {
                if (leftHandItem.Task == "Shoot")
                {
                    return true;
                }
            }

            return false;
        }

        public static float ChanceToHitBodyPart(Character attacker, Character defender, string body_part, string attack_type)
        {
            float base_chance = 0;

            if (body_part == "Head")
            {
                base_chance = 70;
            }
            else if (body_part == "Neck")
            {
                base_chance = 20;
            }
            else if (body_part == "Torso")
            {
                base_chance = 100;
            }
            else if (body_part == "Groin")
            {
                base_chance = 50;
            }
            else if (body_part == "Right_Arm")
            {
                base_chance = 75;
            }
            else if (body_part == "Right_Hand")
            {
                base_chance = 40;
            }
            else if (body_part == "Left_Arm")
            {
                base_chance = 75;
            }
            else if (body_part == "Left_Hand")
            {
                base_chance = 40;
            }
            else if (body_part == "Right_Leg")
            {
                base_chance = 75;
            }
            else if (body_part == "Right_Foot")
            {
                base_chance = 25;
            }
            else if (body_part == "Left_Leg")
            {
                base_chance = 75;
            }
            else if (body_part == "Left_Foot")
            {
                base_chance = 25;
            }

            Something attacker_agility = attacker.GetStat("Agility");
            Something defender_agility = defender.GetStat("Agility");
            float agi_chance = attacker_agility.Value - defender_agility.Value;

            Something attacker_perception = attacker.GetStat("Perception");
            float per_chance = attacker_perception.Value / 10;
            if (attack_type == "Shoot")
            {
                per_chance = attacker_perception.Value / 4;
            }

            Something attacker_luck = attacker.GetStat("Luck");
            float luk_chance = attacker_luck.Value / 10;

            float chance = base_chance + agi_chance + per_chance + luk_chance;
            if (chance > 100)
            {
                chance = 100;
            }
            else if (chance < 0)
            {
                chance = 0;
            }

            return chance;
        }

        public static int AttackTime(Character character, string attack_type)
        {
            int base_speed = 0;
            if (attack_type == "Grab" ||
                attack_type == "Stab")
            {
                base_speed = 3000;
            }
            else if (attack_type == "Throw")
            {
                base_speed = 5000;
            }
            else if (attack_type == "Punch")
            {
                base_speed = 4000;
            }
            else if (attack_type == "Swing")
            {
                base_speed = 6000;
            }
            else if (attack_type == "Shoot")
            {
                base_speed = 2000;
            }

            Something strength = character.GetStat("Strength");
            int strength_bonus = (int)strength.Value;
            if (attack_type == "Shoot")
            {
                strength_bonus = 0;
            }

            Something perception = character.GetStat("Perception");
            int perception_bonus = (int)(perception.Value / 2);

            Something agility = character.GetStat("Agility");
            int agility_bonus = (int)agility.Value;

            int speed = base_speed - strength_bonus - agility_bonus;
            if (attack_type == "Shoot" ||
                attack_type == "Throw")
            {
                speed -= perception_bonus;
            }

            return speed;
        }

        public static bool IsAttack(string taskName)
        {
            if (taskName == "Grab"||
                taskName == "Stab" ||
                taskName == "Punch" ||
                taskName == "Swing" ||
                taskName == "Throw" ||
                taskName == "Shoot")
            {
                return true;
            }

            return false;
        }

        public static bool InRange(Character attacker, Character defender, string action)
        {
            if (action == "Grab" ||
                action == "Punch" ||
                action == "Stab" ||
                action == "Swing")
            {
                return WorldUtil.NextTo(attacker.Location, defender.Location);
            }
            else
            {
                return WorldUtil.Location_IsVisible(attacker.ID, defender.Location);
            }
        }

        public static int AttackSound_Hit(Character character, string action)
        {
            Character player = Handler.GetPlayer();

            Vector2 player_loc = new Vector2(player.Location.X, player.Location.Y);
            Vector2 character_loc = new Vector2(character.Location.X, character.Location.Y);

            if (action == "Punch")
            {
                AssetManager.PlaySound_Random_AtDistance("Punch", player_loc, character_loc, 3);
                return 3;
            }
            else if (action == "Throw" ||
                     action == "Swing")
            {
                AssetManager.PlaySound_Random_AtDistance("Swing", player_loc, character_loc, 2);
                return 2;
            }
            else if (action == "Stab")
            {
                AssetManager.PlaySound_Random_AtDistance("Stab", player_loc, character_loc, 2);
                return 1;
            }
            else if (action == "Shoot")
            {
                Item leftHandItem = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");
                if (leftHandItem != null)
                {
                    if (leftHandItem.Task == "Shoot")
                    {
                        if (leftHandItem.Name.Contains("Pistol"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Pistol", player_loc, character_loc, 20);
                            return 20;
                        }
                        else if (leftHandItem.Name.Contains("Shotgun"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Shotgun", player_loc, character_loc, 40);
                            return 40;
                        }
                        else if (leftHandItem.Name.Contains("Machine") ||
                                 leftHandItem.Name.Contains("Rifle"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Rifle", player_loc, character_loc, 40);
                            return 40;
                        }
                    }
                }
                
                Item rightHandItem = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
                if (rightHandItem != null)
                {
                    if (rightHandItem.Task == "Shoot")
                    {
                        if (rightHandItem.Name.Contains("Pistol"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Pistol", player_loc, character_loc, 20);
                            return 20;
                        }
                        else if (rightHandItem.Name.Contains("Shotgun"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Shotgun", player_loc, character_loc, 40);
                            return 40;
                        }
                        else if (rightHandItem.Name.Contains("Machine") ||
                                 rightHandItem.Name.Contains("Rifle"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Rifle", player_loc, character_loc, 40);
                            return 40;
                        }
                    }
                }
            }

            return 0;
        }

        public static int AttackSound_Miss(Character character, string action)
        {
            Character player = Handler.GetPlayer();

            Vector2 player_loc = new Vector2(player.Location.X, player.Location.Y);
            Vector2 character_loc = new Vector2(character.Location.X, character.Location.Y);

            if (action == "Punch")
            {
                AssetManager.PlaySound_Random_AtDistance("Swing", player_loc, character_loc, 2);
                return 0;
            }
            else if (action == "Throw" ||
                     action == "Swing")
            {
                AssetManager.PlaySound_Random_AtDistance("Swing", player_loc, character_loc, 2);
                return 1;
            }
            else if (action == "Shoot")
            {
                Item leftHandItem = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");
                if (leftHandItem != null)
                {
                    if (leftHandItem.Task == "Shoot")
                    {
                        if (leftHandItem.Name.Contains("Pistol"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Pistol", player_loc, character_loc, 20);
                            return 20;
                        }
                        else if (leftHandItem.Name.Contains("Shotgun"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Shotgun", player_loc, character_loc, 40);
                            return 40;
                        }
                        else if (leftHandItem.Name.Contains("Machine"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Rifle", player_loc, character_loc, 40);
                            return 40;
                        }
                    }
                }

                Item rightHandItem = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
                if (rightHandItem != null)
                {
                    if (rightHandItem.Task == "Shoot")
                    {
                        if (rightHandItem.Name.Contains("Pistol"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Pistol", player_loc, character_loc, 20);
                            return 20;
                        }
                        else if (rightHandItem.Name.Contains("Shotgun"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Shotgun", player_loc, character_loc, 40);
                            return 40;
                        }
                        else if (rightHandItem.Name.Contains("Machine"))
                        {
                            AssetManager.PlaySound_Random_AtDistance("Rifle", player_loc, character_loc, 40);
                            return 40;
                        }
                    }
                }
            }

            return 0;
        }

        public static void DoDamage(Character attacker, Character defender, string weapon, string action, string body_part)
        {
            BodyPart bodyPart = defender.GetBodyPart(CharacterUtil.BodyPartFromName(body_part));
            Something strength = attacker.GetStat("Strength");

            string wound = "";

            Item item = attacker.Inventory.GetItem(weapon);
            if (item != null)
            {
                float pain_value = 2;
                Something pain = item.GetProperty("Pain");
                if (pain != null)
                {
                    pain_value = pain.Value;
                }

                Something lose_limb = item.GetProperty("Lose Limb");
                Something explode = item.GetProperty("Explode");
                Something burn = item.GetProperty("Burn");
                Something blood_loss = item.GetProperty("Blood Loss");

                if (action == "Throw" &&
                    explode != null)
                {
                    Explode(attacker, defender);
                }
                else if (action == "Swing" &&
                         lose_limb != null &&
                         Utility.RandomPercent(strength.Value))
                {
                    wound = "Sever";
                }
                else if (action == "Swing" &&
                         pain_value >= 20 &&
                         Utility.RandomPercent(strength.Value))
                {
                    wound = "Break";
                }
                else if (action == "Swing" &&
                         blood_loss != null)
                {
                    wound = "Cut";
                }
                else if (action == "Swing" &&
                         pain_value >= 10 &&
                         Utility.RandomPercent(strength.Value))
                {
                    wound = "Fracture";
                }
                else if (action == "Swing" &&
                         pain_value > 0)
                {
                    wound = "Bruise";
                }
                else if (action == "Shoot" &&
                         weapon != "Bow" &&
                         burn == null)
                {
                    wound = "Gunshot";
                }
                else if (action == "Stab")
                {
                    wound = "Stab";
                }
                else if (burn != null)
                {
                    wound = "Burn";
                }
                else if (action == "Grab")
                {
                    defender.StatusEffects.Add(new Something
                    {
                        Name = body_part + "_GrabbedBy_" + attacker.ID
                    });
                }

                if (!string.IsNullOrEmpty(wound))
                {
                    AddWound(attacker, defender, bodyPart, wound);
                }
                
                CharacterUtil.UpdatePain(defender);

                Something hooked = item.GetProperty("Hooked");
                if (hooked != null)
                {
                    defender.StatusEffects.Add(new Something
                    {
                        Name = "HookedBy_" + attacker.ID
                    });
                }

                Something blind = item.GetProperty("Blind");
                if (blind != null)
                {
                    defender.StatusEffects.Add(new Something
                    {
                        Name = "Blind",
                        Value = 3
                    });
                }

                Something gas = item.GetProperty("Gas");
                if (gas != null)
                {
                    defender.StatusEffects.Add(new Something
                    {
                        Name = "Gas"
                    });
                }

                Something net = item.GetProperty("Net");
                if (net != null)
                {
                    defender.StatusEffects.Add(new Something
                    {
                        Name = "Net"
                    });
                }
            }
            else if (action == "Punch")
            {
                if (strength.Value >= 80)
                {
                    if (Utility.RandomPercent(strength.Value / 2))
                    {
                        wound = "Break";
                    }
                    else if (Utility.RandomPercent(strength.Value))
                    {
                        wound = "Fracture";
                    }
                    else
                    {
                        wound = "Bruise";
                    }
                }
                else if (strength.Value >= 60)
                {
                    if (Utility.RandomPercent(strength.Value / 2))
                    {
                        wound = "Fracture";
                    }
                    else
                    {
                        wound = "Bruise";
                    }
                }
                else
                {
                    wound = "Bruise";
                }

                if (!string.IsNullOrEmpty(wound))
                {
                    AddWound(attacker, defender, bodyPart, wound);
                }

                CharacterUtil.UpdatePain(defender);
            }
        }

        public static void Explode(Character attacker, Character defender)
        {
            for (int i = 0; i < defender.BodyParts.Count; i++)
            {
                BodyPart part = defender.BodyParts[i];
                if (part.Name != "Torso")
                {
                    if (Utility.RandomPercent(25))
                    {
                        SeverLimb(attacker, defender, part);
                    }
                    else if (Utility.RandomPercent(50))
                    {
                        AddWound(attacker, defender, part, "Break");
                    }
                    else if (Utility.RandomPercent(50))
                    {
                        AddWound(attacker, defender, part, "Burn");
                    }
                }
                else
                {
                    AddWound(attacker, defender, part, "Burn");
                }
            }
        }

        public static void SeverLimb(Character attacker, Character defender, BodyPart part)
        {
            if (part.Name == "Right_Arm")
            {
                BodyPart right_hand = defender.GetBodyPart("Right_Hand");
                if (right_hand.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, right_hand, "Sever");
                }
            }
            else if (part.Name == "Left_Arm")
            {
                BodyPart left_hand = defender.GetBodyPart("Left_Hand");
                if (left_hand.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, left_hand, "Sever");
                }
            }
            else if (part.Name == "Right_Leg")
            {
                BodyPart right_foot = defender.GetBodyPart("Right_Foot");
                if (right_foot.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, right_foot, "Sever");
                }
            }
            else if (part.Name == "Left_Leg")
            {
                BodyPart left_foot = defender.GetBodyPart("Left_Foot");
                if (left_foot.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, left_foot, "Sever");
                }
            }

            AddWound(attacker, defender, part, "Sever");

            if (defender.Type == "Player")
            {
                GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been severed.");
            }
            else if (attacker.Type == "Player")
            {
                CryptoRandom random = new CryptoRandom();
                int reaction = random.Next(0, 4);
                if (reaction == 0)
                {
                    GameUtil.AddMessage(defender.Name + " screams as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is severed.");
                }
                else if (reaction == 1)
                {
                    GameUtil.AddMessage(defender.Name + " wails as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is severed.");
                }
                else if (reaction == 2)
                {
                    GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is severed.");
                }
                else if (reaction == 3)
                {
                    GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is severed.");
                }
            }
        }

        public static void AddWound(Character attacker, Character defender, BodyPart part, string wound_type)
        {
            Something wound = new Something();
            wound.ID = Handler.GetID();
            wound.Name = wound_type;

            if (wound_type == "Break")
            {
                wound.Value = 7257600; //3 months

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been broken.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " screams as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " breaks.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " wails as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " breaks.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " breaks.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " breaks.");
                    }
                }
            }
            else if (wound_type == "Fracture")
            {
                wound.Value = 2419200; //1 month

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been fractured.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " screams as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " fractures.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " wails as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " fractures.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " fractures.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " fractures.");
                    }
                }
            }
            else if (wound_type == "Gunshot")
            {
                wound.Value = 2419200; //1 month

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been shot.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " screams as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is shot.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " wails as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is shot.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is shot.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is shot.");
                    }
                }
            }
            else if (wound_type == "Stab")
            {
                wound.Value = 604800; //7 days

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been stabbed.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " screams as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is stabbed.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " wails as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is stabbed.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is stabbed.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is stabbed.");
                    }
                }
            }
            else if (wound_type == "Cut")
            {
                wound.Value = 259200; //3 days

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been cut.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is cut.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is cut.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " gasps as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is cut.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " groans as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is cut.");
                    }
                }
            }
            else if (wound_type == "Burn")
            {
                wound.Value = 86400; //1 day

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been burned.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " screams as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is burned.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " wails as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is burned.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is burned.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is burned.");
                    }
                }
            }
            else if (wound_type == "Bruise")
            {
                wound.Value = 86400; //1 day

                if (defender.Type == "Player")
                {
                    GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been bruised.");
                }
                else if (attacker.Type == "Player" &&
                         !defender.Dead &&
                         !defender.Unconscious)
                {
                    CryptoRandom random = new CryptoRandom();
                    int reaction = random.Next(0, 4);
                    if (reaction == 0)
                    {
                        GameUtil.AddMessage(defender.Name + " yells as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is bruised.");
                    }
                    else if (reaction == 1)
                    {
                        GameUtil.AddMessage(defender.Name + " cries out as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is bruised.");
                    }
                    else if (reaction == 2)
                    {
                        GameUtil.AddMessage(defender.Name + " gasps as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is bruised.");
                    }
                    else if (reaction == 3)
                    {
                        GameUtil.AddMessage(defender.Name + " groans as their " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " is bruised.");
                    }
                }
            }
            else if (wound_type == "Sever")
            {
                wound.Value = -1; //Never
            }

            wound.Texture = AssetManager.Textures["Wound_" + wound_type];
            part.Wounds.Add(wound);
        }
    }
}
