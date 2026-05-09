using System;
using System.Linq;
using System.Collections.Generic;
using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Attack : Task
    {
        Dictionary<string, string> AttackingWith = null;

        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            AttackingWith = CombatUtil.AttackChoice(character);
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
                AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);
            }
        }

        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            string weapon = AttackingWith.ElementAt(0).Key;
            string action = AttackingWith.ElementAt(0).Value;

            Character target = WorldUtil.GetCharacter(Location);
            if (target != null)
            {
                string bodyPart = Handler.Selected_BodyPart;
                if (string.IsNullOrEmpty(bodyPart))
                {
                    bodyPart = CombatUtil.RandomBodyPart(character, target);
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

                float hitChance = CombatUtil.ChanceToHitBodyPart(character, target, bodyPart);
                if (Utility.RandomPercent(hitChance))
                {
                    CombatUtil.AttackSound_Hit(target, null, weapon, action);
                    CombatUtil.DoDamage(character, target, weapon, action, bodyPart);

                    if (target.Unconscious)
                    {
                        if (character.Type != "Player")
                        {
                            character.InCombat = false;
                            character.Target_ID = -1;
                        }

                        target.InCombat = false;
                        target.Target_ID = -1;
                    }
                    else if (target.Type != "Player")
                    {
                        target.Target_ID = character.ID;
                        target.InCombat = true;
                    }

                    CryptoRandom random = new CryptoRandom();
                    int stunTime = random.Next(0, maxStunTime);
                    if (stunTime > 0)
                    {
                        target.Job.Tasks.Add(new Wait
                        {
                            Name = "Wait",
                            OwnerID = target.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(stunTime))
                        });

                        if (target.Type == "Player")
                        {
                            TimeTracker.Tick(stunTime);
                        }
                    }
                }
                else
                {
                    if (character.Type == "Player")
                    {
                        switch (action)
                        {
                            case "Punch":
                            case "Stab":
                            case "Cut":
                                GameUtil.AddMessage("You tried to " + action.ToLower() + " their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                                break;

                            case "Shoot":
                                CombatUtil.AttackSound_Hit(target, null, weapon, action);
                                GameUtil.AddMessage("You tried to " + action.ToLower() + " their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                                break;

                            default:
                                GameUtil.AddMessage("You tried to hit their " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                                break;
                        }
                    }
                    else if (target.Type == "Player")
                    {
                        switch (action)
                        {
                            case "Punch":
                            case "Stab":
                            case "Cut":
                                GameUtil.AddMessage(character.Name + " tried to " + action.ToLower() + " your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                                break;

                            case "Shoot":
                                CombatUtil.AttackSound_Hit(target, null, weapon, action);
                                GameUtil.AddMessage(character.Name + " tried to " + action.ToLower() + " your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                                break;

                            default:
                                GameUtil.AddMessage(character.Name + " tried to hit your " + CharacterUtil.BodyPartToName(bodyPart).ToLower() + ", but missed.");
                                break;
                        }
                    }
                }
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
                    CombatUtil.AttackSound_Hit(null, tile, weapon, action);

                    if (character.Type == "Player")
                    {
                        switch (action)
                        {
                            case "Punch":
                                GameUtil.AddMessage("You punched a " + WorldUtil.GetTile_Name(tile) + ".");
                                break;

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
