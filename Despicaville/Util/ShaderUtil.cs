using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Utility;
using OP_Engine.Controls;
using OP_Engine.Menus;
using OP_Engine.Rendering;
using OP_Engine.Tiles;

namespace Despicaville.Util
{
    public static class ShaderUtil
    {
        public static Effect? BloomExtract;
        public static Effect? BloomCombine;
        public static Effect? DynamicLighting;
        public static RenderTarget2D? DynamicLighting_Render;
        public static RenderTarget2D? DynamicLighting_LightMap;
        public static RenderTarget2D? RenderTarget_BrightAreas;
        public static RenderTarget2D? RenderTarget_Blurred;

        public static void Init()
        {
            if (Main.Game == null ||
                Main.Game.GraphicsManager == null ||
                Main.Game.RenderingManager?.BufferRenderer == null)
            {
                return;
            }

            DynamicLighting_Render = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            DynamicLighting_LightMap = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(DynamicLighting_LightMap);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.White);
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(Main.Game.RenderingManager.BufferRenderer.RenderTarget);

            RenderTarget_BrightAreas = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            RenderTarget_Blurred = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            BloomExtract = AssetManager.Shaders["BloomExtract"];

            BloomCombine = AssetManager.Shaders["BloomCombine"];
            BloomCombine.Parameters["BaseIntensity"].SetValue(1f);
            BloomCombine.Parameters["BloomSaturation"].SetValue(1.5f);
            BloomCombine.Parameters["BaseSaturation"].SetValue(1f);

            DynamicLighting = AssetManager.Shaders["DynamicLighting"];
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

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, GaussianBlur);
            spriteBatch.Draw(texture, region.ToRectangle, Color.White);
            spriteBatch.End();
        }

        public static void Apply_Bloom(SpriteBatch spriteBatch, Menu menu)
        {
            if (Main.Game == null ||
                Main.Game.GraphicsManager == null ||
                Main.Game.RenderingManager?.BufferRenderer == null ||
                Main.Game.RenderingManager?.FinalRenderer == null ||
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
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(Main.Game.RenderingManager.FinalRenderer.RenderTarget);

            BloomCombine.Parameters["base_target"].SetValue(Main.Game.RenderingManager.BufferRenderer.RenderTarget);
            BloomCombine.Parameters["bloom_target"].SetValue(RenderTarget_BrightAreas);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, BloomCombine);
            spriteBatch.Draw(RenderTarget_BrightAreas, new Rectangle(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), Color.White);
            spriteBatch.End();
        }

        public static void Apply_DynamicLighting(RenderTarget2D renderTarget)
        {
            if (Main.Game == null ||
                Main.Game.SpriteBatch == null ||
                Main.Game.GraphicsManager == null ||
                DynamicLighting == null ||
                DynamicLighting_LightMap == null ||
                DynamicLighting_Render == null ||
                Main.Game.RenderingManager == null ||
                Main.Game.RenderingManager.BufferRenderer == null ||
                RenderingManager.Lighting == null)
            {
                return;
            }

            Texture2D? black = Handler.GetTexture("Black");
            if (black == null)
            {
                return;
            }

            Handler.light_radius = (Main.light_tile_distance * Main.Game.TileSize_X) + (Main.Game.TileSize_X / 2);
            DynamicLighting.Parameters["light_radius"].SetValue(Handler.light_radius);

            DynamicLighting.Parameters["resolution"].SetValue(new Vector2(Main.Game.ScreenWidth, Main.Game.ScreenHeight));
            DynamicLighting.Parameters["render_target"].SetValue(renderTarget);

            if (DynamicLighting_LightMap.Width != Main.Game.Resolution.X ||
                DynamicLighting_LightMap.Height != Main.Game.Resolution.Y)
            {
                DynamicLighting_LightMap = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            }

            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(DynamicLighting_LightMap);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.White);

            Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            Map? map = WorldUtil.GetMap();
            if (map != null)
            {
                Layer? bottom_tiles = map.GetLayer("BottomTiles");
                Layer? middle_tiles = map.GetLayer("MiddleTiles");

                if (bottom_tiles != null &&
                    middle_tiles != null)
                {
                    float min_x = 0 - Main.Game.TileSize_X;
                    float max_x = Main.Game.ScreenWidth + Main.Game.TileSize_X;
                    float min_y = 0 - Main.Game.TileSize_Y;
                    float max_y = Main.Game.ScreenHeight + Main.Game.TileSize_Y;

                    int bottom_count = bottom_tiles.Tiles.Count;
                    for (int i = 0; i < bottom_count; i++)
                    {
                        Tile bottom_tile = bottom_tiles.Tiles[i];
                        if (bottom_tile.Region == null ||
                            bottom_tile.Location == null)
                        {
                            continue;
                        }

                        if (bottom_tile.Region.X < min_x || bottom_tile.Region.X > max_x ||
                            bottom_tile.Region.Y < min_y || bottom_tile.Region.Y > max_y)
                        {
                            continue;
                        }

                        if (bottom_tile.BlocksMovement)
                        {
                            Main.Game.SpriteBatch.Draw(black, bottom_tile.Region.ToRectangle, Color.White);
                        }

                        Tile? middle_tile = middle_tiles.GetTile(bottom_tile.Location.ToVector2);
                        if (middle_tile?.Region != null)
                        {
                            if (middle_tile.BlocksMovement &&
                                middle_tile.BlocksSight)
                            {
                                Main.Game.SpriteBatch.Draw(black, bottom_tile.Region.ToRectangle, Color.White);
                            }
                        }
                    }
                }
            }

            Main.Game.SpriteBatch.End();

            DynamicLighting.Parameters["light_map"].SetValue(DynamicLighting_LightMap);

            float Lighting_R = Utility.ConvertValueToRange(RenderingManager.Lighting.DrawColor.R, 0, 255, 0, 1);
            float Lighting_G = Utility.ConvertValueToRange(RenderingManager.Lighting.DrawColor.G, 0, 255, 0, 1);
            float Lighting_B = Utility.ConvertValueToRange(RenderingManager.Lighting.DrawColor.B, 0, 255, 0, 1);
            float Lighting_A = Utility.ConvertValueToRange(RenderingManager.Lighting.DrawColor.A, 0, 255, 0, 1);
            Vector4 lighting = new(Lighting_R, Lighting_G, Lighting_B, Lighting_A);
            DynamicLighting.Parameters["ambient_light"].SetValue(lighting);

            Handler.light_count = 0;
            Handler.lights.Clear();
            Handler.light_colors.Clear();

            int light_source_count = Handler.dynamic_lights.Count;
            for (int l = 0; l < light_source_count; l++)
            {
                Tile dynamic_light = Handler.dynamic_lights[l];

                if (dynamic_light.Region == null ||
                    !dynamic_light.IsLightSource)
                {
                    continue;
                }

                float light_width = Main.light_tile_distance * Main.Game.TileSize.X;
                float light_height = Main.light_tile_distance * Main.Game.TileSize.Y;

                float min_x = 0 - light_width;
                float max_x = Main.Game.ScreenWidth + light_width;
                float min_y = 0 - light_height;
                float max_y = Main.Game.ScreenHeight + light_height;

                if (dynamic_light.Region.X < min_x || dynamic_light.Region.X > max_x ||
                    dynamic_light.Region.Y < min_y || dynamic_light.Region.Y > max_y)
                {
                    continue;
                }

                Handler.light_count++;

                float half_width = Main.Game.TileSize.X / 2;
                float half_height = Main.Game.TileSize.Y / 2;

                Vector2 region_center_point = new(dynamic_light.Region.X + half_width, dynamic_light.Region.Y + half_height);
                Handler.lights.Add(region_center_point);

                float r = Utility.ConvertValueToRange(dynamic_light.LightColor.R, 0, 255, 0, 1);
                float g = Utility.ConvertValueToRange(dynamic_light.LightColor.G, 0, 255, 0, 1);
                float b = Utility.ConvertValueToRange(dynamic_light.LightColor.B, 0, 255, 0, 1);
                float a = Utility.ConvertValueToRange(dynamic_light.LightColor.A, 0, 255, 0, 1);
                Handler.light_colors.Add(new(r, g, b, a));
            }

            if (DynamicLighting_Render.Width != Main.Game.Resolution.X ||
                DynamicLighting_Render.Height != Main.Game.Resolution.Y)
            {
                DynamicLighting_Render = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            }

            if (Handler.light_count > 0)
            {
                DynamicLighting.Parameters["light_count"].SetValue(Handler.light_count);
                DynamicLighting.Parameters["lights"].SetValue(Handler.lights.ToArray());
                DynamicLighting.Parameters["light_colors"].SetValue(Handler.light_colors.ToArray());
            }

            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(DynamicLighting_Render);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

            Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, DynamicLighting);
            Main.Game.SpriteBatch.Draw(renderTarget, new Rectangle(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), Color.White);
            Main.Game.SpriteBatch.End();

            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(renderTarget);
            Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            Main.Game.SpriteBatch.Draw(DynamicLighting_Render, new Rectangle(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y), Color.White);
            Main.Game.SpriteBatch.End();
        }
    }
}
