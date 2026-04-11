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
        public override void Action_Start()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            AssetManager.PlaySound_Random_AtDistance("Swing", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);

            TimeSpan startTime = TimeSpan.FromMilliseconds(StartTime.TotalMilliseconds);
            int duration = (int)(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

            WorldUtil.AddEffect_Animated(Location.ToVector2, Direction, "Swing", startTime, duration);
        }

        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(character);
            string weapon = AttackingWith.ElementAt(0).Key;
            string action = AttackingWith.ElementAt(0).Value;

            string bodyPart = "";

            int waitTime = 0;

            CryptoRandom random = new CryptoRandom();
            int choice = random.Next(0, 12);
            switch (choice)
            {
                case 0:
                    bodyPart = "Torso";
                    waitTime = 1;
                    break;

                case 1:
                    bodyPart = "Right_Arm";
                    break;

                case 2:
                    bodyPart = "Left_Arm";
                    break;

                case 3:
                    bodyPart = "Right_Leg";
                    break;

                case 4:
                    bodyPart = "Left_Leg";
                    break;

                case 5:
                    bodyPart = "Head";
                    waitTime = 2;
                    break;

                case 6:
                    bodyPart = "Groin";
                    waitTime = 3;
                    break;

                case 7:
                    bodyPart = "Right_Hand";
                    break;

                case 8:
                    bodyPart = "Left_Hand";
                    break;

                case 9:
                    bodyPart = "Right_Foot";
                    break;

                case 10:
                    bodyPart = "Left_Foot";
                    break;

                case 11:
                    bodyPart = "Neck";
                    waitTime = 3;
                    break;
            }

            Character target = WorldUtil.GetCharacter(Location);
            if (target != null)
            {
                AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);
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

                if (waitTime > 0)
                {
                    target.Job.Tasks.Add(new Wait
                    {
                        Name = "Wait",
                        OwnerIDs = new List<long> { target.ID },
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(waitTime))
                    });
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

                if (tile != null)
                {
                    AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, character.Location.ToVector2, 2);
                    GameUtil.AddMessage("You hit a " + WorldUtil.GetTile_Name(tile) + ".");
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
