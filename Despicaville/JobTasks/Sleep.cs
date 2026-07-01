using Despicaville.Util;
using OP_Engine.Jobs;

namespace Despicaville.JobTasks
{
    public class Sleep : JobTask
    {
        public override void Action()
        {
            if (Owner_Character == null)
            {
                return;
            }

            CharacterUtil.Sleep(Owner_Character);
        }
    }
}
