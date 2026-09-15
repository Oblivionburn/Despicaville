using Despicaville.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Rendering;
using OP_Engine.Time;
using OP_Engine.Utility;

namespace Despicaville
{
    public class D_RenderingManager : RenderingManager
    {
        public D_RenderingManager(Game game) : base(game)
        {

        }

        public override void ApplyShaders(RenderTarget2D renderTarget)
        {
            if (Main.Game == null ||
                Main.Game.GraphicsManager == null ||
                Main.Game.SpriteBatch == null ||
                ShaderUtil.RenderTarget_Blurred == null ||
                BufferRenderer?.RenderTarget == null)
            {
                return;
            }

            if (TimeManager.Paused)
            {
                if (ShaderUtil.RenderTarget_Blurred.Width != Main.Game.Resolution.X ||
                    ShaderUtil.RenderTarget_Blurred.Height != Main.Game.Resolution.Y)
                {
                    ShaderUtil.RenderTarget_Blurred = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
                }

                Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(ShaderUtil.RenderTarget_Blurred);
                Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

                ShaderUtil.Apply_GaussianBlur(Main.Game.SpriteBatch, 5, BufferRenderer.RenderTarget, new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), false);
                Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(BufferRenderer.RenderTarget);

                ShaderUtil.Apply_GaussianBlur(Main.Game.SpriteBatch, 5, ShaderUtil.RenderTarget_Blurred, new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), true);
            }
        }
    }
}
