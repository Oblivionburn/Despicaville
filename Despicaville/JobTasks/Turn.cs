using OP_Engine.Jobs;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Turn : JobTask
    {
        public override void Action_End()
        {
            if (Owner_Character == null)
            {
                return;
            }

            if (Direction != Owner_Character.Direction)
            {
                if (Direction == Direction.North)
                {
                    Owner_Character.FaceNorth();
                }
                else if (Direction == Direction.East)
                {
                    Owner_Character.FaceEast();
                }
                else if (Direction == Direction.South)
                {
                    Owner_Character.FaceSouth();
                }
                else if (Direction == Direction.West)
                {
                    Owner_Character.FaceWest();
                }
            }

            CharacterUtil.UpdateGear(Owner_Character);
            CharacterUtil.UpdateSight(Owner_Character);

            if (Owner_Character.Target_ID > 0)
            {
                Owner_Character.InCombat = true;
            }
        }
    }
}
