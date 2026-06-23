using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using Despicaville.Util;

namespace Despicaville.JobTasks
{
    public class ToggleTV : JobTask
    {
        public override void Action_End()
        {
            if (Location == null)
            {
                return;
            }

            Character? character = GetOwner();
            if (character == null)
            {
                return;
            }

            if (Handler.Player?.Location != null)
            {
                AssetManager.PlaySound_Random_AtDistance("Click", Handler.Player.Location.ToVector2, Location.ToVector2, 2);
            }

            Map? map = WorldUtil.GetMap();

            Layer? middle_tiles = map?.GetLayer("MiddleTiles");
            Tile? tile = middle_tiles?.GetTile(Location.ToVector2);
            if (tile != null)
            {
                tile.IsLightSource = !tile.IsLightSource;
            }
        }

        public Character? GetOwner()
        {
            if (Handler.Player?.ID == OwnerID)
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
