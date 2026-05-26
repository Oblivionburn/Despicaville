using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Enums;
using OP_Engine.Inventories;
using OP_Engine.Jobs;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class CombatUtil
    {
        public static Dictionary<string, string> AttackChoice(Character character)
        {
            Dictionary<string, string> attack = new Dictionary<string, string>();

            Item rightHandItem = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
            if (rightHandItem != null)
            {
                if (IsAttack(rightHandItem.Task))
                {
                    attack.Add(rightHandItem.Name, rightHandItem.Task);
                }
            }

            if (attack.Count == 0)
            {
                Item leftHandItem = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");
                if (leftHandItem != null)
                {
                    if (IsAttack(leftHandItem.Task))
                    {
                        attack.Add(leftHandItem.Name, rightHandItem.Task);
                    }
                }
            }

            if (attack.Count == 0)
            {
                string attacking_with;

                CryptoRandom random = new CryptoRandom();
                int hand_choice = random.Next(0, 2);
                if (hand_choice == 0)
                {
                    attacking_with = "Right Hand";
                }
                else
                {
                    attacking_with = "Left Hand";
                }

                attack.Add(attacking_with, "Punch");
            }

            return attack;
        }

        public static bool CanAttack_Ranged(Character attacker)
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

        public static bool IsAttack(string taskName)
        {
            if (taskName == "Grab" ||
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

        public static bool IsRanged(string weapon)
        {
            if (weapon.Contains("Pistol") ||
                weapon.Contains("Rifle") ||
                weapon.Contains("Machine") ||
                weapon.Contains("Shotgun") ||
                weapon.Contains("Bow") ||
                weapon.Contains("Crossbow"))
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

        public static bool IsSevered(BodyPart bodyPart)
        {
            if (bodyPart != null)
            {
                foreach (Wound wound in bodyPart.Wounds)
                {
                    if (wound.Name == "Sever")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool BodyPart_HasHP(BodyPart bodyPart)
        {
            Property hp = bodyPart.GetStat("HP");
            if (hp != null)
            {
                if (hp.Value > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsWoundType(Property property)
        {
            if (property.Name.Contains("Sever") || 
                property.Name.Contains("Break") ||
                property.Name.Contains("Fracture") ||
                property.Name.Contains("Bruise") ||
                property.Name.Contains("Burn") ||
                property.Name.Contains("Cut") ||
                property.Name.Contains("Gunshot") ||
                property.Name.Contains("Stab"))
            {
                return true;
            }

            return false;
        }

        public static string RandomBodyPart(Character attacker, Character defender)
        {
            List<string> parts = new List<string>();

            int agi_boost = (int)(attacker.Stats.Agility / 5);

            if (Utility.RandomPercent(20 + agi_boost))
            {
                parts.Add("Groin");
            }
            if (Utility.RandomPercent(20 + agi_boost))
            {
                parts.Add("Neck");
            }
            if (Utility.RandomPercent(40 + agi_boost))
            {
                parts.Add("Head");
            }
            if (Utility.RandomPercent(60 + agi_boost))
            {
                parts.Add("Right_Hand");
            }
            if (Utility.RandomPercent(60 + agi_boost))
            {
                parts.Add("Left_Hand");
            }
            if (Utility.RandomPercent(60 + agi_boost))
            {
                parts.Add("Right_Foot");
            }
            if (Utility.RandomPercent(60 + agi_boost))
            {
                parts.Add("Left_Foot");
            }
            if (Utility.RandomPercent(80 + agi_boost))
            {
                parts.Add("Right_Arm");
            }
            if (Utility.RandomPercent(80 + agi_boost))
            {
                parts.Add("Left_Arm");
            }
            if (Utility.RandomPercent(80 + agi_boost))
            {
                parts.Add("Right_Leg");
            }
            if (Utility.RandomPercent(80 + agi_boost))
            {
                parts.Add("Left_Leg");
            }

            foreach (string part in parts)
            {
                BodyPart bodyPart = defender.GetBodyPart(part);
                if (bodyPart != null)
                {
                    bool severed = IsSevered(bodyPart);
                    bool hasHP = BodyPart_HasHP(bodyPart);

                    if (!severed &&
                        hasHP)
                    {
                        return part;
                    }
                }
            }

            return "Torso";
        }

        public static float ChanceToHitBodyPart(Character attacker, Character defender, string body_part)
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

            float agi_chance = attacker.Stats.Agility - defender.Stats.Agility;
            float per_chance = attacker.Stats.Perception / 10;
            float luk_chance = attacker.Stats.Luck / 10;

            int distance = WorldUtil.GetDistance(attacker.Location, defender.Location);
            int distance_penalty = distance * 2;

            float chance = base_chance + agi_chance + per_chance + luk_chance - distance_penalty;
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
            if (attack_type == "Stab" ||
                attack_type == "Punch")
            {
                base_speed = 2000;
            }
            else if (attack_type == "Shoot" ||
                     attack_type == "Throw")
            {
                base_speed = 3000;
            }
            else if (attack_type == "Swing")
            {
                base_speed = 4000;
            }

            int strength = (int)character.Stats.Strength;
            if (attack_type == "Shoot")
            {
                strength = 0;
            }

            int perception = (int)character.Stats.Perception;
            int agility = (int)character.Stats.Agility;

            int speed = base_speed - strength - agility;
            if (attack_type == "Shoot" ||
                attack_type == "Throw")
            {
                speed -= perception;
            }

            return speed;
        }

        public static int AttackSound_Hit(Character defender, Tile tile, string weapon, string action)
        {
            switch (action)
            {
                case "Stab":
                case "Cut":
                    if (defender != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance("Stab", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 2);
                    }
                    else if (tile != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance("Stab", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 2);
                    }
                    return 1;

                case "Shoot":
                    if (weapon.Contains("Pistol"))
                    {
                        if (defender != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Pistol", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 20);
                        }
                        else if (tile != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Pistol", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 20);
                        }
                        return 20;
                    }
                    else if (weapon.Contains("Rifle") ||
                             weapon.Contains("Machine"))
                    {
                        if (defender != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Rifle", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 30);
                        }
                        else if (tile != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Rifle", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 30);
                        }
                        return 30;
                    }
                    else if (weapon.Contains("Shotgun"))
                    {
                        if (defender != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Shotgun", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 40);
                        }
                        else if (tile != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Shotgun", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 40);
                        }
                        return 40;
                    }
                    else if (weapon.Contains("Bow") ||
                             weapon.Contains("Crossbow"))
                    {
                        if (defender != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Bow", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 2);
                        }
                        else if (tile != null)
                        {
                            AssetManager.PlaySound_Random_AtDistance("Bow", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 2);
                        }
                        return 1;
                    }
                    break;

                default:
                    if (defender != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 2);
                    }
                    else if (tile != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 2);
                    }
                    return 2;
            }

            return 0;
        }

        public static void DoDamage(Character attacker, Character defender, string weapon, string action, string body_part)
        {
            BodyPart bodyPart = defender.GetBodyPart(body_part);

            string wound = "";

            Item item = attacker.Inventory.GetItem(weapon);
            if (item != null)
            {
                Property wound_sever = item.GetProperty("Sever");
                Property wound_break = item.GetProperty("Break");
                Property wound_fracture = item.GetProperty("Fracture");
                Property wound_bruise = item.GetProperty("Bruise");
                Property wound_stab = item.GetProperty("Stab");
                Property wound_cut = item.GetProperty("Cut");
                Property wound_gunshot = item.GetProperty("Gunshot");

                if (wound_sever != null &&
                    body_part != "Torso" &&
                    body_part != "Groin" &&
                    ((action == "Swing" && Utility.RandomPercent(attacker.Stats.Strength)) ||
                    action != "Swing"))
                {
                    wound = "Sever";
                }
                else if (wound_break != null &&
                         Utility.RandomPercent(attacker.Stats.Strength))
                {
                    wound = "Break";
                }
                else if (wound_fracture != null &&
                         Utility.RandomPercent(attacker.Stats.Strength))
                {
                    wound = "Fracture";
                }
                else if (wound_bruise != null)
                {
                    wound = "Bruise";
                }
                else if (wound_cut != null)
                {
                    wound = "Cut";
                }
                else if (wound_gunshot != null)
                {
                    wound = "Gunshot";
                }
                else if (wound_stab != null)
                {
                    wound = "Stab";
                }

                if (wound == "Sever")
                {
                    SeverLimb(attacker, defender, bodyPart);
                }
                else if (!string.IsNullOrEmpty(wound))
                {
                    AddWound(attacker, defender, bodyPart, wound, true);
                }
                
                CharacterUtil.UpdatePain(defender);
            }
            else if (action == "Punch")
            {
                if (attacker.Stats.Strength >= 80)
                {
                    if (Utility.RandomPercent(attacker.Stats.Strength / 2))
                    {
                        wound = "Break";
                    }
                    else if (Utility.RandomPercent(attacker.Stats.Strength))
                    {
                        wound = "Fracture";
                    }
                    else
                    {
                        wound = "Bruise";
                    }
                }
                else if (attacker.Stats.Strength >= 60)
                {
                    if (Utility.RandomPercent(attacker.Stats.Strength / 2))
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
                    AddWound(attacker, defender, bodyPart, wound, true);
                }

                CharacterUtil.UpdatePain(defender);
            }

            if (wound != "Sever")
            {
                defender.StatusEffects.Add(new Property
                {
                    Name = "Damage",
                    Description = "Stage=0;Direction=" + WorldUtil.GetDirection(attacker.Location, defender.Location).ToString() + ";Offset=0,0;Fade=0"
                });
            }
        }

        public static void DrawDamage(SpriteBatch spriteBatch, Character character, Point resolution, Color color)
        {
            Property damage = character.GetStatusEffect("Damage");

            if (character.Visible &&
                character.Texture != null &&
                character.Region != null &&
                character.Region.X >= character.Texture.Width * -2 &&
                character.Region.X < resolution.X + character.Texture.Width * 2 &&
                character.Region.Y >= character.Texture.Height * -2 &&
                character.Region.Y < resolution.Y + character.Texture.Height * 2)
            {
                string[] parts = damage.Description.Split(';');

                string[] stageParts = parts[0].Split('=');
                int stage = int.Parse(stageParts[1]);

                string[] directionParts = parts[1].Split('=');
                Enum.TryParse(directionParts[1], out Direction direction);

                string[] offsetParts = parts[2].Split('=');
                string[] offset_coords = offsetParts[1].Split(',');
                int offset_x = int.Parse(offset_coords[0]);
                int offset_y = int.Parse(offset_coords[1]);

                float distance_x = Main.Game.TileSize_X / 8;
                float distance_y = Main.Game.TileSize_Y / 8;

                string[] fadeParts = parts[3].Split('=');
                float fade = float.Parse(fadeParts[1]);

                switch (stage)
                {
                    case 0:
                        switch (direction)
                        {
                            case Direction.North:
                                if (offset_y > (distance_y * -1))
                                {
                                    offset_y--;
                                }
                                else
                                {
                                    stage = 1;
                                }
                                break;

                            case Direction.East:
                                if (offset_x < distance_x)
                                {
                                    offset_x++;
                                }
                                else
                                {
                                    stage = 1;
                                }
                                break;

                            case Direction.South:
                                if (offset_y < distance_y)
                                {
                                    offset_y++;
                                }
                                else
                                {
                                    stage = 1;
                                }
                                break;

                            case Direction.West:
                                if (offset_x > (distance_x * -1))
                                {
                                    offset_x--;
                                }
                                else
                                {
                                    stage = 1;
                                }
                                break;
                        }
                        break;

                    case 1:
                        switch (direction)
                        {
                            case Direction.North:
                                if (offset_y < 0)
                                {
                                    offset_y++;
                                }
                                else
                                {
                                    stage = 2;
                                }
                                break;

                            case Direction.East:
                                if (offset_x > distance_x)
                                {
                                    offset_x--;
                                }
                                else
                                {
                                    stage = 2;
                                }
                                break;

                            case Direction.South:
                                if (offset_y < distance_y)
                                {
                                    offset_y--;
                                }
                                else
                                {
                                    stage = 2;
                                }
                                break;

                            case Direction.West:
                                if (offset_x < (distance_x * -1))
                                {
                                    offset_x++;
                                }
                                else
                                {
                                    stage = 2;
                                }
                                break;
                        }
                        break;
                }

                Rectangle region = new Rectangle((int)character.Region.X + offset_x, (int)character.Region.Y + offset_y, (int)character.Region.Width, (int)character.Region.Height);

                Color damageColor = Color.Red;
                if (stage == 1)
                {
                    damageColor = Color.Lerp(Color.Red, color, fade);
                    fade += 0.1f;
                }

                spriteBatch.Draw(character.Texture, region, character.Image, damageColor);

                int count = character.Inventory.Items.Count;
                for (int j = 0; j < count; j++)
                {
                    Item item = character.Inventory.Items[j];

                    if (item.Visible &&
                        item.Texture != null &&
                        item.Region != null &&
                        item.Region.X >= item.Texture.Width * -2 &&
                        item.Region.X < resolution.X + item.Texture.Width * 2 &&
                        item.Region.Y >= item.Texture.Height * -2 &&
                        item.Region.Y < resolution.Y + item.Texture.Height * 2)
                    {
                        spriteBatch.Draw(item.Texture, region, item.Image, damageColor);
                    }
                }

                Task task = character.Job.CurrentTask;
                task?.TaskBar?.Draw(spriteBatch);

                if (stage == 2)
                {
                    character.StatusEffects.Remove(damage);
                }
                else
                {
                    damage.Description = "Stage=" + stage + ";Direction=" + direction.ToString() + ";Offset=" + offset_x + "," + offset_y + ";Fade=" + fade;
                }
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
                        AddWound(attacker, defender, part, "Break", true);
                    }
                    else if (Utility.RandomPercent(50))
                    {
                        AddWound(attacker, defender, part, "Burn", true);
                    }
                }
                else
                {
                    AddWound(attacker, defender, part, "Burn", true);
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
                    AddWound(attacker, defender, right_hand, "Sever", false);
                }
            }
            else if (part.Name == "Left_Arm")
            {
                BodyPart left_hand = defender.GetBodyPart("Left_Hand");
                if (left_hand.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, left_hand, "Sever", false);
                }
            }
            else if (part.Name == "Right_Leg")
            {
                BodyPart right_foot = defender.GetBodyPart("Right_Foot");
                if (right_foot.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, right_foot, "Sever", false);
                }
            }
            else if (part.Name == "Left_Leg")
            {
                BodyPart left_foot = defender.GetBodyPart("Left_Foot");
                if (left_foot.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, left_foot, "Sever", false);
                }
            }
            else if (part.Name == "Neck")
            {
                BodyPart head = defender.GetBodyPart("Head");
                if (head.GetWounds("Sever").Count == 0)
                {
                    AddWound(attacker, defender, head, "Sever", false);
                }
            }
            else if (part.Name == "Head")
            {
                defender.Dead = true;
            }

            AddWound(attacker, defender, part, "Sever", true);
        }

        public static void AddWound(Character attacker, Character defender, BodyPart part, string wound_type, bool log)
        {
            Wound wound = new Wound
            {
                ID = Handler.GetID(),
                Name = wound_type
            };

            if (wound_type == "Break")
            {
                wound.Value = 604800; //7 days

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been broken.");
                    }
                    else if (attacker.Type == "Player" &&
                             !defender.Dead &&
                             !defender.Unconscious)
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You broke the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Fracture")
            {
                wound.Value = 432000; //5 days

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been fractured.");
                    }
                    else if (attacker.Type == "Player" &&
                             !defender.Dead &&
                             !defender.Unconscious)
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You fractured the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Gunshot")
            {
                wound.Value = 259200; //3 days

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been shot.");
                    }
                    else if (attacker.Type == "Player")
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You shot the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Stab")
            {
                wound.Value = 86400; //1 day

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been stabbed.");
                    }
                    else if (attacker.Type == "Player")
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You stabbed the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Burn")
            {
                wound.Value = 21600; //6 hours

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been burned.");
                    }
                    else if (attacker.Type == "Player")
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You burned the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Cut")
            {
                wound.Value = 10800; //3 hours

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been cut.");
                    }
                    else if (attacker.Type == "Player")
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You cut the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Bruise")
            {
                wound.Value = 3600; //1 hour

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been bruised.");
                    }
                    else if (attacker.Type == "Player")
                    {
                        if (!defender.Dead &&
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
                        else
                        {
                            GameUtil.AddMessage("You hit the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }
            else if (wound_type == "Sever")
            {
                wound.Value = -1; //Never

                if (log)
                {
                    if (defender.Type == "Player")
                    {
                        GameUtil.AddMessage("Your " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " has been severed.");
                    }
                    else if (attacker.Type == "Player")
                    {
                        if (!defender.Dead &&
                            !defender.Unconscious)
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
                        else
                        {
                            GameUtil.AddMessage("You severed the " + CharacterUtil.BodyPartToName(part.Name).ToLower() + " of " + defender.Name + ".");
                        }
                    }
                }
            }

            if (wound_type != "Sever")
            {
                wound.Texture = AssetManager.Textures["Wound_" + wound_type];
            }
            
            part.Wounds.Add(wound);
        }
    }
}
