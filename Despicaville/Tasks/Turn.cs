using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.Tasks
{
    public class Turn : Task
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
                if (Direction == Direction.Up)
                {
                    character.FaceNorth();
                }
                else if (Direction == Direction.Right)
                {
                    character.FaceEast();
                }
                else if (Direction == Direction.Down)
                {
                    character.FaceSouth();
                }
                else if (Direction == Direction.Left)
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
