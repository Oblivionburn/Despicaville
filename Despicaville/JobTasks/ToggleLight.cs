using OP_Engine.Characters;
using OP_Engine.Jobs;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Scenes;

namespace Despicaville.JobTasks
{
    public class ToggleLight : JobTask
    {
        public override void Action_End()
        {
            Character character = GetOwner();
            if (character == null)
            {
                return;
            }

            AssetManager.PlaySound_Random_AtDistance("Click", Handler.Player.Location.ToVector2, Location.ToVector2, 2);

            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];

            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Tile tile = middle_tiles.GetTile(Location.ToVector2);
            tile.IsLightSource = !tile.IsLightSource;
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
