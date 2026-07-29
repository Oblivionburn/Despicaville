using Despicaville.Util;
using OP_Engine.Jobs;

namespace Despicaville.SubTasks
{
    public class Wait : JobTask
    {
        public override void Action()
        {
            if (Owner_Character == null)
            {
                return;
            }

            CharacterUtil.Rest(Owner_Character);
        }
    }
}
