using OP_Engine.Characters;
using OP_Engine.Jobs;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Stand : JobTask
    {
        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (character.Unconscious)
            {
                character.Unconscious = false;

                if (character.Type == "Player")
                {
                    GameUtil.AddMessage("You regained consciousness.");
                }
            }
            else if (character.Laying)
            {
                character.Laying = false;

                if (character.Type == "Player")
                {
                    GameUtil.AddMessage("You rose to your feet.");
                }
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
