using System;
using System.Linq;
using System.Collections.Generic;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Inventories;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Attack : Task
    {
        Dictionary<string, string> AttackingWith = null;

        Item rightHandItem = null;
        Item leftHandItem = null;
        bool dual_wield = false;

        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            rightHandItem = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
            leftHandItem = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");

            dual_wield = false;

            if (rightHandItem != null &&
                InventoryUtil.IsWeapon(rightHandItem) &&
                leftHandItem != null &&
                InventoryUtil.IsWeapon(leftHandItem))
            {
                dual_wield = true;
            }

            if (dual_wield)
            {
                bool effect = false;

                string rightWeapon = rightHandItem.Name;
                string rightAction = rightHandItem.Task;

                if (rightAction == "Swing" ||
                    rightAction == "Stab" ||
                    rightAction == "Cut")
                {
                    AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);

                    effect = true;
                    TimeSpan startTime = TimeSpan.FromMilliseconds(StartTime.TotalMilliseconds);
                    int duration = (int)(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

                    WorldUtil.AddEffect_Animated(Location.ToVector3, Direction, "Swing", startTime, duration);
                }
                else if (rightAction == "Throw")
                {
                    AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 3);
                }
                else if (rightAction == "Shoot")
                {
                    if (rightWeapon == "Sling")
                    {
                        AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 5);
                    }
                    else if (rightWeapon.Contains("Bow"))
                    {
                        AssetManager.PlaySound_Random_AtDistance("Bow", Handler.Player.Location.ToVector2, character.Location.ToVector2, 5);
                    }
                }

                string leftWeapon = leftHandItem.Name;
                string leftAction = leftHandItem.Task;

                if (leftAction == "Swing" ||
                    leftAction == "Stab" ||
                    leftAction == "Cut")
                {
                    AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);

                    if (!effect)
                    {
                        TimeSpan startTime = TimeSpan.FromMilliseconds(StartTime.TotalMilliseconds);
                        int duration = (int)(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

                        WorldUtil.AddEffect_Animated(Location.ToVector3, Direction, "Swing", startTime, duration);
                    }
                }
                else if (leftAction == "Throw")
                {
                    AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 3);
                }
                else if (leftAction == "Shoot")
                {
                    if (leftWeapon == "Sling")
                    {
                        AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 5);
                    }
                    else if (leftWeapon.Contains("Bow"))
                    {
                        AssetManager.PlaySound_Random_AtDistance("Bow", Handler.Player.Location.ToVector2, character.Location.ToVector2, 5);
                    }
                }
            }
            else
            {
                AttackingWith = CombatUtil.AttackChoice(character);
                string weapon = AttackingWith.ElementAt(0).Key;
                string action = AttackingWith.ElementAt(0).Value;

                if (action == "Punch" ||
                    action == "Swing" ||
                    action == "Stab" ||
                    action == "Cut")
                {
                    AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);

                    TimeSpan startTime = TimeSpan.FromMilliseconds(StartTime.TotalMilliseconds);
                    int duration = (int)(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

                    WorldUtil.AddEffect_Animated(Location.ToVector3, Direction, "Swing", startTime, duration);
                }
                else if (action == "Throw")
                {
                    AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 3);
                }
                else if (action == "Shoot")
                {
                    if (weapon == "Sling")
                    {
                        AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 5);
                    }
                    else if (weapon.Contains("Bow"))
                    {
                        AssetManager.PlaySound_Random_AtDistance("Bow", Handler.Player.Location.ToVector2, character.Location.ToVector2, 5);
                    }
                }
            }
        }

        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Character target = WorldUtil.GetCharacter(Location);
            if (target != null)
            {
                if (dual_wield)
                {
                    WeaponAttack_Character(character, target, rightHandItem);
                    WeaponAttack_Character(character, target, leftHandItem);
                }
                else
                {
                    string weapon = AttackingWith.ElementAt(0).Key;

                    Item weaponItem = null;
                    for (int i = 0; i < character.Inventory.Items.Count; i++)
                    {
                        Item item = character.Inventory.Items[i];
                        if (item.Name == weapon)
                        {
                            weaponItem = item;
                            break;
                        }
                    }

                    if (weaponItem != null)
                    {
                        WeaponAttack_Character(character, target, weaponItem);
                    }
                    else
                    {
                        MeleeAttack_Character(character, target);
                    }
                }

                Handler.Selected_BodyPart = null;
            }
            else
            {
                Tile tile = null;

                Tile top_tile = WorldUtil.GetFurniture(Handler.TopFurniture, Location);
                if (top_tile?.Texture != null)
                {
                    tile = top_tile;
                }

                if (tile == null)
                {
                    Tile middle_tile = WorldUtil.GetFurniture(Handler.MiddleFurniture, Location);
                    if (middle_tile?.Texture != null)
                    {
                        tile = middle_tile;
                    }
                }

                if (tile == null)
                {
                    Map map = WorldUtil.GetMap();
                    Layer bottom_tiles = map.GetLayer("BottomTiles");
                    Tile bottom_tile = bottom_tiles.GetTile(Location.ToVector2);
                    if (bottom_tile != null)
                    {
                        tile = bottom_tile;
                    }
                }

                if (tile != null)
                {
                    if (dual_wield)
                    {
                        WeaponAttack_Tile(character, tile, rightHandItem);
                        WeaponAttack_Tile(character, tile, leftHandItem);
                    }
                    else
                    {
                        string weapon = AttackingWith.ElementAt(0).Key;

                        Item weaponItem = null;
                        for (int i = 0; i < character.Inventory.Items.Count; i++)
                        {
                            Item item = character.Inventory.Items[i];
                            if (item.Name == weapon)
                            {
                                weaponItem = item;
                                break;
                            }
                        }

                        if (weaponItem != null)
                        {
                            WeaponAttack_Tile(character, tile, weaponItem);
                        }
                        else
                        {
                            MeleeAttack_Tile(character, tile);
                        }
                    }
                }
            }

            if (character.Type != "Player")
            {
                CryptoRandom random = new CryptoRandom();
                int waitTime = random.Next(0, 3);
                if (waitTime > 0)
                {
                    character.Job.Tasks.Add(new Wait
                    {
                        Name = "Wait",
                        OwnerID = character.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(waitTime))
                    });
                }
            }
        }

        public void WeaponAttack_Character(Character attacker, Character defender, Item weapon)
        {
            string bodyPart = Handler.Selected_BodyPart;
            if (string.IsNullOrEmpty(bodyPart))
            {
                bodyPart = CombatUtil.RandomBodyPart(attacker, defender);
            }

            int maxStunTime = 0;

            switch (bodyPart)
            {
                case "Neck":
                case "Groin":
                    maxStunTime = 5;
                    break;

                case "Head":
                    maxStunTime = 4;
                    break;

                case "Right_Hand":
                case "Left_Hand":
                case "Right_Foot":
                case "Left_Foot":
                    maxStunTime = 3;
                    break;

                case "Right_Arm":
                case "Left_Arm":
                case "Right_Leg":
                case "Left_Leg":
                    maxStunTime = 2;
                    break;

                case "Torso":
                    maxStunTime = 1;
                    break;
            }

            float hitChance = CombatUtil.ChanceToHitBodyPart(attacker, defender, bodyPart);
            if (Utility.RandomPercent(hitChance))
            {
                AssetManager.PlaySound_Random_AtDistance(weapon.Sound, Handler.Player.Location.ToVector2, defender.Location.ToVector2, weapon.SoundRange);

                CombatUtil.DoDamage(attacker, defender, weapon.Name, weapon.Task, bodyPart);

                if (defender.Unconscious)
                {
                    if (attacker.Type != "Player")
                    {
                        attacker.InCombat = false;
                        attacker.Target_ID = -1;
                    }

                    defender.InCombat = false;
                    defender.Target_ID = -1;
                }
                else if (defender.Type != "Player")
                {
                    defender.Target_ID = attacker.ID;
                    defender.InCombat = true;
                }

                CryptoRandom random = new CryptoRandom();
                int stunTime = random.Next(0, maxStunTime);
                if (stunTime > 0)
                {
                    defender.Job.Tasks.Add(new Wait
                    {
                        Name = "Wait",
                        OwnerID = defender.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(stunTime))
                    });

                    if (defender.Type == "Player")
                    {
                        TimeTracker.Tick(stunTime);
                    }
                }
            }
            else if (attacker.Type == "Player")
            {
                switch (weapon.Task)
                {
                    case "Punch":
                    case "Stab":
                    case "Cut":
                        GameUtil.AddMessage("You tried to " + weapon.Task.ToLower() + " their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                        break;

                    case "Shoot":
                        if (weapon != null &&
                            weapon.Name != "Sling" &&
                            !weapon.Name.Contains("Bow"))
                        {
                            AssetManager.PlaySound_Random_AtDistance(weapon.Sound, Handler.Player.Location.ToVector2, defender.Location.ToVector2, weapon.SoundRange);
                        }

                        GameUtil.AddMessage("You tried to " + weapon.Task.ToLower() + " their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                        break;

                    default:
                        GameUtil.AddMessage("You tried to hit their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                        break;
                }
            }
            else if (defender.Type == "Player")
            {
                switch (weapon.Task)
                {
                    case "Punch":
                    case "Stab":
                    case "Cut":
                        GameUtil.AddMessage(attacker.Name + " tried to " + weapon.Task.ToLower() + " your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                        break;

                    case "Shoot":
                        if (weapon.Name != "Sling" &&
                            !weapon.Name.Contains("Bow"))
                        {
                            AssetManager.PlaySound_Random_AtDistance(weapon.Sound, Handler.Player.Location.ToVector2, attacker.Location.ToVector2, weapon.SoundRange);
                        }

                        GameUtil.AddMessage(attacker.Name + " tried to " + weapon.Task.ToLower() + " your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                        break;

                    default:
                        GameUtil.AddMessage(attacker.Name + " tried to hit your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                        break;
                }
            }
        }

        public void WeaponAttack_Tile(Character attacker, Tile tile, Item weapon)
        {
            AssetManager.PlaySound_Random_AtDistance(weapon.Sound, Handler.Player.Location.ToVector2, tile.Location.ToVector2, weapon.SoundRange);

            if (attacker.Type == "Player")
            {
                switch (weapon.Task)
                {
                    case "Stab":
                        GameUtil.AddMessage("You stabbed a " + WorldUtil.GetTile_Name(tile) + ".");
                        break;

                    case "Cut":
                        GameUtil.AddMessage("You cut a " + WorldUtil.GetTile_Name(tile) + ".");
                        break;

                    case "Shoot":
                        GameUtil.AddMessage("You shot a " + WorldUtil.GetTile_Name(tile) + ".");
                        break;

                    default:
                        GameUtil.AddMessage("You hit a " + WorldUtil.GetTile_Name(tile) + ".");
                        break;
                }
            }
        }

        public void MeleeAttack_Character(Character attacker, Character defender)
        {
            string bodyPart = Handler.Selected_BodyPart;
            if (string.IsNullOrEmpty(bodyPart))
            {
                bodyPart = CombatUtil.RandomBodyPart(attacker, defender);
            }

            int maxStunTime = 0;

            switch (bodyPart)
            {
                case "Neck":
                case "Groin":
                    maxStunTime = 5;
                    break;

                case "Head":
                    maxStunTime = 4;
                    break;

                case "Right_Hand":
                case "Left_Hand":
                case "Right_Foot":
                case "Left_Foot":
                    maxStunTime = 3;
                    break;

                case "Right_Arm":
                case "Left_Arm":
                case "Right_Leg":
                case "Left_Leg":
                    maxStunTime = 2;
                    break;

                case "Torso":
                    maxStunTime = 1;
                    break;
            }

            float hitChance = CombatUtil.ChanceToHitBodyPart(attacker, defender, bodyPart);
            if (Utility.RandomPercent(hitChance))
            {
                AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, defender.Location.ToVector2, 2);
                CombatUtil.DoDamage(attacker, defender, "Right Hand", "Punch", bodyPart);

                if (defender.Unconscious)
                {
                    if (attacker.Type != "Player")
                    {
                        attacker.InCombat = false;
                        attacker.Target_ID = -1;
                    }

                    defender.InCombat = false;
                    defender.Target_ID = -1;
                }
                else if (defender.Type != "Player")
                {
                    defender.Target_ID = attacker.ID;
                    defender.InCombat = true;
                }

                CryptoRandom random = new CryptoRandom();
                int stunTime = random.Next(0, maxStunTime);
                if (stunTime > 0)
                {
                    defender.Job.Tasks.Add(new Wait
                    {
                        Name = "Wait",
                        OwnerID = defender.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(stunTime))
                    });

                    if (defender.Type == "Player")
                    {
                        TimeTracker.Tick(stunTime);
                    }
                }
            }
            else if (attacker.Type == "Player")
            {
                GameUtil.AddMessage("You tried to punch their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
            }
            else if (defender.Type == "Player")
            {
                GameUtil.AddMessage(attacker.Name + " tried to punch your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
            }
        }

        public void MeleeAttack_Tile(Character attacker, Tile tile)
        {
            AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, tile.Location.ToVector2, 2);
            
            if (attacker.Type == "Player")
            {
                GameUtil.AddMessage("You punched a " + WorldUtil.GetTile_Name(tile) + ".");
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
