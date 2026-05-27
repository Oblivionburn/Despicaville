using Despicaville.Util;
using OP_Engine.Characters;
using OP_Engine.Jobs;

namespace Despicaville.JobTasks
{
    public class Wait : JobTask
    {
        public override void Action()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            CharacterUtil.Rest(character);
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
