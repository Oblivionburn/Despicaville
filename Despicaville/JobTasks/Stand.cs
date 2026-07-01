using OP_Engine.Jobs;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class Stand : JobTask
    {
        public override void Action_End()
        {
            if (Owner_Character == null)
            {
                return;
            }

            if (Owner_Character.Unconscious)
            {
                Owner_Character.Unconscious = false;

                if (Owner_Character.Type == "Player")
                {
                    GameUtil.AddMessage("You regained consciousness.");
                }
            }
            else if (Owner_Character.Laying)
            {
                Owner_Character.Laying = false;

                if (Owner_Character.Type == "Player")
                {
                    GameUtil.AddMessage("You rose to your feet.");
                }
            }
        }
    }
}
