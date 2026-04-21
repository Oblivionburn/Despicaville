using Despicaville.Util;
using OP_Engine.Characters;
using OP_Engine.Jobs;

namespace Despicaville.Tasks
{
    public class Sleep : Task
    {
        public override void Action()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            CharacterUtil.Sleep(character);
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
