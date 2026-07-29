using System;
using OP_Engine.Enums;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Time;
using Despicaville.Util;
using Despicaville.SubTasks;

namespace Despicaville.MajorTasks
{
    public class Wander : JobTask
    {
        public override void Action_Start()
        {
            if (Owner_Character == null ||
                TimeManager.Now == null)
            {
                return;
            }

            Direction direction = Direction.Nowhere;

            CryptoRandom random = new();
            int choice = random.Next(1, 101);
            if (choice <= 28)
            {
                direction = Direction.North;
            }
            else if (choice <= 50)
            {
                direction = Direction.East;
            }
            else if (choice <= 72)
            {
                direction = Direction.South;
            }
            else if (choice <= 100)
            {
                direction = Direction.West;
            }

            random = new CryptoRandom();
            choice = random.Next(1, 11);
            if (choice <= 5)
            {
                SubTasks.Add(new Wait
                {
                    Name = "Wait",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(30))
                });
            }
            else if (choice > 5 &&
                     choice <= 8)
            {
                SubTasks.Add(new Move
                {
                    Name = "Walk",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    Direction = direction
                });
            }
            else if (choice > 8)
            {
                SubTasks.Add(new Turn
                {
                    Name = "Turn",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Owner_Character))),
                    Direction = direction
                });

                SubTasks.Add(new Wait
                {
                    Name = "Wait",
                    Owner_Character = Owner_Character,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromSeconds(10))
                });
            }
        }

        public override void Action()
        {
            if (TimeManager.Now == null)
            {
                return;
            }

            //Have the SubTasks finished yet?
            if (SubTasks.Count == 0)
            {
                EndTime = new TimeHandler(TimeManager.Now);
            }
        }
    }
}
