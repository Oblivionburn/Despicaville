using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Turn : JobTask
    {
        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (Direction != character.Direction)
            {
                if (Direction == Direction.North)
                {
                    character.FaceNorth();
                }
                else if (Direction == Direction.East)
                {
                    character.FaceEast();
                }
                else if (Direction == Direction.South)
                {
                    character.FaceSouth();
                }
                else if (Direction == Direction.West)
                {
                    character.FaceWest();
                }
            }

            CharacterUtil.UpdateGear(character);
            CharacterUtil.UpdateSight(character);

            if (character.Target_ID > 0)
            {
                character.InCombat = true;
            }
        }

        public Character GetOwner()
        {
            if (Handler.Player.ID == OwnerID)
            {
                return Handler.Player;
            }

            Army army = CharacterManager.Armies[0];
            Squad citizens = army.Squads[1];

            int count = citizens.Characters.Count;
            for (int c = 0; c < count; c++)
            {
                Character citizen = citizens.Characters[c];
                if (citizen.ID == OwnerID)
                {
                    return citizen;
                }
            }

            return null;
        }
    }
}
