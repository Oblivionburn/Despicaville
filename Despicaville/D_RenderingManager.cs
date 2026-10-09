using Despicaville.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Menus;
using OP_Engine.Rendering;
using OP_Engine.Scenes;
using OP_Engine.Time;
using OP_Engine.Utility;

namespace Despicaville
{
    public class D_RenderingManager : RenderingManager
    {
        public D_RenderingManager(Game game) : base(game)
        {

        }

        public override void Draw(GameWindow? window, GraphicsDeviceManager? graphicsManager, SpriteBatch? spriteBatch, Point resolution)
        {
            if (window == null ||
                spriteBatch == null ||
                graphicsManager == null ||
                LightingRenderer == null ||
                Lighting == null ||
                AddLightingRenderer == null ||
                BufferRenderer?.RenderTarget == null ||
                FinalRenderer?.RenderTarget == null)
            {
                return;
            }

            //Don't bother drawing if the window is minimized
            if (window.ClientBounds.Width == 0 ||
                window.ClientBounds.Height == 0)
            {
                return;
            }

            if (UsingDefaults)
            {
                /*
                    This is a very basic setup for rendering whatever's currently visible in SceneManager,
                        applying deferred lighting on the scene, and then rendering menus ontop of that.

                    Can just not run InitDefaults() if you want to use your own custom rendering.
                */

                //Set ambient light in case the color changed
                LightingRenderer.GraphicsClearColor = Lighting.DrawColor;

                //Render lighting
                LightingRenderer.Draw(spriteBatch, resolution);

                //=================================
                // Draw world to Buffer
                //---------------------------------
                graphicsManager.GraphicsDevice.SetRenderTarget(BufferRenderer.RenderTarget);
                graphicsManager.GraphicsDevice.Clear(Color.Black);

                //Render world
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                SceneManager.Draw_WorldsOnly(spriteBatch, resolution, Color.White);
                spriteBatch.End();

                //Add lighting to world
                //AddLightingRenderer.Draw(spriteBatch, resolution);

                //Apply shaders
                ApplyShaders(BufferRenderer.RenderTarget);

                //Render world with no lighting applied
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                SceneManager.Draw_WorldsOnly(spriteBatch, resolution);
                spriteBatch.End();

                //---------------------------------
                // End of drawing to Buffer
                //=================================

                //Draw Buffer to Final RenderTarget
                graphicsManager.GraphicsDevice.SetRenderTarget(FinalRenderer.RenderTarget);
                graphicsManager.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                spriteBatch.Draw(BufferRenderer.RenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, resolution.X, resolution.Y), Color.White);
                spriteBatch.End();

                //Draw Final RenderTarget to screen
                graphicsManager.GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                spriteBatch.Draw(FinalRenderer.RenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, resolution.X, resolution.Y), Color.White);
                spriteBatch.End();

                //=================================
                // Draw menus
                //---------------------------------
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                //Render scene specific menus
                SceneManager.Draw_MenusOnly(spriteBatch);

                //Render standalone menus
                MenuManager.Draw(spriteBatch);

                spriteBatch.End();
            }
        }

        public override void ApplyShaders(RenderTarget2D renderTarget)
        {
            if (Main.Game == null ||
                Main.Game.GraphicsManager == null ||
                Main.Game.SpriteBatch == null ||
                ShaderUtil.RenderTarget_Blurred == null)
            {
                return;
            }

            ShaderUtil.Apply_DynamicLighting(renderTarget);

            //Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            //Main.Game.SpriteBatch.Draw(ShaderUtil.DynamicLighting_LightMap, new Rectangle(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), Color.White * 0.5f);
            //Main.Game.SpriteBatch.End();

            if (TimeManager.Paused)
            {
                if (ShaderUtil.RenderTarget_Blurred.Width != Main.Game.Resolution.X ||
                    ShaderUtil.RenderTarget_Blurred.Height != Main.Game.Resolution.Y)
                {
                    ShaderUtil.RenderTarget_Blurred = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
                }

                Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(ShaderUtil.RenderTarget_Blurred);
                Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

                ShaderUtil.Apply_GaussianBlur(Main.Game.SpriteBatch, 5, renderTarget, new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), false);
                Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(renderTarget);

                ShaderUtil.Apply_GaussianBlur(Main.Game.SpriteBatch, 5, ShaderUtil.RenderTarget_Blurred, new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), true);
            }
        }
    }
}
