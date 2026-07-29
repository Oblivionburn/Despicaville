using System;
using System.Linq;
using System.Collections.Generic;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Characters;
using Despicaville.Util;
using Despicaville.SubTasks;

namespace Despicaville.MajorTasks
{
    public class Attacking : JobTask
    {
        Character? target = null;

        public override void Action_Start()
        {
            if (Owner_Character == null)
            {
                return;
            }

            target = CharacterUtil.GetCharacter_Target(Owner_Character);
        }

        public override void Action()
        {
            if (Owner_Character?.Location == null ||
                TimeManager.Now == null ||
                target == null)
            {
                return;
            }

            if (SubTasks.Count > 0)
            {
                //We're still busy doing stuff
                return;
            }

            if (!Owner_Character.InCombat)
            {
                EndTime = new TimeHandler(TimeManager.Now);
                return;
            }

            if (target?.Location == null)
            {
                Tasker.AbortTask(Owner_Character);
                return;
            }

            Direction direction = WorldUtil.GetDirection(Owner_Character.Location, target.Location);

            if (Owner_Character.Direction != direction)
            {
                SubTasks.Add(new Turn
                {
                    Name = "Turn",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                    Direction = direction
                });
            }
            else if (!WorldUtil.InRange(target.Location, Owner_Character.Location, 1))
            {
                SubTasks.Add(new Move
                {
                    Name = "Walk",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Direction = direction
                });
            }
            else
            {
                Location location = new();
                if (Owner_Character.Direction == Direction.North)
                {
                    location = new Location(Owner_Character.Location.X, Owner_Character.Location.Y - 1, 1);
                }
                else if (Owner_Character.Direction == Direction.East)
                {
                    location = new Location(Owner_Character.Location.X + 1, Owner_Character.Location.Y, 1);
                }
                else if (Owner_Character.Direction == Direction.South)
                {
                    location = new Location(Owner_Character.Location.X, Owner_Character.Location.Y + 1, 1);
                }
                else if (Owner_Character.Direction == Direction.West)
                {
                    location = new Location(Owner_Character.Location.X - 1, Owner_Character.Location.Y, 1);
                }

                Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(Owner_Character);
                string action = AttackingWith.ElementAt(0).Value;

                int attackTime = CombatUtil.AttackTime(Owner_Character, action);
                SubTasks.Add(new Attack
                {
                    Name = "Attack",
                    Owner_Character = Owner_Character,
                    Location = location,
                    Direction = direction,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(attackTime)),
                    TaskBar = CharacterUtil.GenTaskbar(Owner_Character, attackTime)
                });
            }
        }
    }
}
