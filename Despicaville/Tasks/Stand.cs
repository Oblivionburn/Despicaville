using OP_Engine.Characters;
using OP_Engine.Jobs;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Stand : Task
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
