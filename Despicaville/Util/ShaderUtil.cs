using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Utility;
using OP_Engine.Controls;
using OP_Engine.Menus;

namespace Despicaville.Util
{
    public static class ShaderUtil
    {
        public static Effect? BloomExtract;
        public static Effect? BloomCombine;
        public static RenderTarget2D? RenderTarget_BrightAreas;
        public static RenderTarget2D? RenderTarget_Blurred;

        public static void Init()
        {
            if (Main.Game == null ||
                Main.Game.GraphicsManager == null)
            {
                return;
            }

            RenderTarget_BrightAreas = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            RenderTarget_Blurred = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            BloomExtract = AssetManager.Shaders["BloomExtract"];

            BloomCombine = AssetManager.Shaders["BloomCombine"];
            BloomCombine.Parameters["BaseIntensity"].SetValue(1f);
            BloomCombine.Parameters["BloomSaturation"].SetValue(1.5f);
            BloomCombine.Parameters["BaseSaturation"].SetValue(1f);
        }

        public static Vector2[] HorizontalOffsets(int radius, float textureWidth)
        {
            Vector2[] offsets = new Vector2[radius * 2 + 1];

            int index;
            float xOffset = 1.0f / textureWidth;

            for (int i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsets[index] = new Vector2(i * xOffset, 0.0f);
            }

            return offsets;
        }

        public static Vector2[] VerticalOffsets(int radius, float textureHeight)
        {
            Vector2[] offsets = new Vector2[radius * 2 + 1];

            int index;
            float yOffset = 1.0f / textureHeight;

            for (int i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsets[index] = new Vector2(0.0f, i * yOffset);
            }

            return offsets;
        }

        public static float[] Kernel(int radius, float amount)
        {
            float[] kernel = new float[radius * 2 + 1];
            float sigma = radius / amount;

            float twoSigmaSquare = 2.0f * sigma * sigma;
            float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            float total = 0.0f;
            float distance;
            int index;

            for (int i = -radius; i <= radius; ++i)
            {
                distance = i * i;
                index = i + radius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (int i = 0; i < kernel.Length; ++i)
            {
                kernel[i] /= total;
            }

            return kernel;
        }

        public static void Apply_GaussianBlur(SpriteBatch spriteBatch, int radius, Texture2D texture, Region region, bool vertical)
        {
            if (Main.Game == null ||
                Main.Game.SpriteBatch == null)
            {
                return;
            }

            Effect GaussianBlur = AssetManager.Shaders["GaussianBlur"];
            GaussianBlur.Parameters["SampleWeights"].SetValue(Kernel(radius, 2.0f));
            GaussianBlur.Parameters["Texture1"].SetValue(texture);

            if (vertical)
            {
                GaussianBlur.Parameters["SampleOffsets"].SetValue(VerticalOffsets(radius, texture.Height));
            }
            else
            {
                GaussianBlur.Parameters["SampleOffsets"].SetValue(HorizontalOffsets(radius, texture.Width));
            }

            Main.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, GaussianBlur);
            spriteBatch.Draw(texture, region.ToRectangle, Color.White);

            Main.Game.SpriteBatch.End();
        }

        public static void Apply_Bloom(SpriteBatch spriteBatch, Menu menu)
        {
            if (Main.Game == null ||
                Main.Game.GraphicsManager == null ||
                Main.BufferRenderer == null ||
                Main.FinalRenderer == null ||
                BloomExtract == null ||
                BloomCombine == null ||
                RenderTarget_BrightAreas == null ||
                RenderTarget_Blurred == null)
            {
                return;
            }

            int blurRadius = 5;

            if (RenderTarget_BrightAreas.Width != Main.Game.Resolution.X ||
                RenderTarget_BrightAreas.Height != Main.Game.Resolution.Y)
            {
                RenderTarget_BrightAreas = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            }

            if (RenderTarget_Blurred.Width != Main.Game.Resolution.X ||
                RenderTarget_Blurred.Height != Main.Game.Resolution.Y)
            {
                RenderTarget_Blurred = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            }

            //Pass 1: draw BufferRenderer into RenderTarget_BrightAreas, using BloomExtract shader to extract only the brightest parts of the image
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(RenderTarget_BrightAreas);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, BloomExtract);

            for (int i = 0; i < menu.Pictures.Count; i++)
            {
                Picture picture = menu.Pictures[i];
                if (picture.Region == null)
                {
                    continue;
                }

                BloomExtract.Parameters["render_target"].SetValue(picture.Texture);

                BloomExtract.Parameters["BloomThreshold"].SetValue(0.45f);
                BloomCombine.Parameters["BloomIntensity"].SetValue(picture.Opacity * 1.5f);

                spriteBatch.Draw(picture.Texture, picture.Region.ToRectangle, picture.Image, picture.DrawColor * picture.Opacity);
            }

            spriteBatch.End();

            //Pass 2: draw from RenderTarget_BrightAreas into RenderTarget_Blurred, using a shader to apply a horizontal gaussian blur filter
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(RenderTarget_Blurred);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

            Apply_GaussianBlur(spriteBatch, blurRadius, RenderTarget_BrightAreas, new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), false);

            //Pass 3: draw from RenderTarget_Blurred back into RenderTarget_BrightAreas, using a shader to apply a vertical gaussian blur filter
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(RenderTarget_BrightAreas);

            Apply_GaussianBlur(spriteBatch, blurRadius, RenderTarget_Blurred, new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), true);

            //Pass 4: draw RenderTarget_BrightAreas and BufferRenderer into FinalRenderer, using a shader that combines them to
            //produce the final bloomed result
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(Main.FinalRenderer.RenderTarget);

            BloomCombine.Parameters["base_target"].SetValue(Main.BufferRenderer.RenderTarget);
            BloomCombine.Parameters["bloom_target"].SetValue(RenderTarget_BrightAreas);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, BloomCombine);
            spriteBatch.Draw(RenderTarget_BrightAreas, new Rectangle(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), Color.White);
            spriteBatch.End();
        }
    }
}
