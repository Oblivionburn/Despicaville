using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Rendering;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using Despicaville.Util;

namespace Despicaville
{
    public class LightingRenderer : Renderer
    {
        #region Variables



        #endregion

        #region Constructors

        public LightingRenderer() : base()
        {

        }

        #endregion

        #region Methods

        public override void Update()
        {
            if (Main.Game == null)
            {
                return;
            }

            Handler.light_maps.Clear();

            Map? map = WorldUtil.GetMap();

            Layer? bottom_tiles = map?.GetLayer("BottomTiles");
            Layer? middle_tiles = map?.GetLayer("MiddleTiles");

            int light_source_count = Handler.light_sources.Count;
            for (int l = 0; l < light_source_count; l++)
            {
                Point light_source = Handler.light_sources[l];
                if (Handler.light_maps.Count < Main.light_max_count)
                {
                    Tile? starting_tile = bottom_tiles?.GetTile(new Vector2(light_source.X, light_source.Y));
                    Tile? source_tile = middle_tiles?.GetTile(new Vector2(light_source.X, light_source.Y));

                    if (source_tile != null &&
                        starting_tile?.Region != null)
                    {
                        if (source_tile.IsLightSource)
                        {
                            Color drawColor = source_tile.LightColor;

                            int distance = Main.light_tile_distance;
                            int full_width = (int)(distance * Main.Game.TileSize.X);
                            int full_height = (int)(distance * Main.Game.TileSize.Y);
                            int resolution_width = Main.Game.Resolution.X;
                            int resolution_height = Main.Game.Resolution.Y;

                            if (starting_tile.Region.X >= 0 - full_width - 1 &&
                                starting_tile.Region.X <= resolution_width + full_width + 1)
                            {
                                if (starting_tile.Region.Y >= 0 - full_height - 1 &&
                                    starting_tile.Region.Y <= resolution_height + full_height + 1)
                                {
                                    int size_width = (int)Main.Game.TileSize.X;
                                    int size_height = (int)Main.Game.TileSize.Y;

                                    int half_width = size_width / 2;
                                    int half_height = size_height / 2;

                                    Vector2 region_center_point = new(starting_tile.Region.X + half_width, starting_tile.Region.Y + half_height);
                                    if (Handler.light_maps.ContainsKey(region_center_point))
                                    {
                                        Handler.light_maps[region_center_point].Clear();
                                    }
                                    else
                                    {
                                        Handler.light_maps.Add(region_center_point, new List<Tile>());
                                    }

                                    int min_x = light_source.X - distance;
                                    int max_x = light_source.X + distance;
                                    int min_y = light_source.Y - distance;
                                    int max_y = light_source.Y + distance;

                                    HashSet<Point> edge_coords = [];
                                    for (int x = min_x; x <= max_x; x++)
                                    {
                                        if (x == min_x ||
                                            x == max_x)
                                        {
                                            for (int y = min_y; y <= max_y; y++)
                                            {
                                                edge_coords.Add(new Point(x, y));
                                            }
                                        }
                                        else
                                        {
                                            edge_coords.Add(new Point(x, min_y));
                                            edge_coords.Add(new Point(x, max_y));
                                        }
                                    }

                                    foreach (Point p in edge_coords)
                                    {
                                        List<Point> points = Utility.GetLine(light_source, p);

                                        bool reverse = points[0] != light_source;

                                        int points_count = points.Count;
                                        for (int i = 0; i < points_count; i++)
                                        {
                                            //Reverse direction if first point is not where we started
                                            int index = reverse ? (points_count - 1) - i : i;

                                            Point point = points[index];
                                            Vector2 current_location = new(point.X, point.Y);

                                            Tile? bottom_tile = bottom_tiles?.GetTile(current_location);
                                            if (bottom_tile?.Region != null)
                                            {
                                                if (bottom_tile.BlocksMovement)
                                                {
                                                    break;
                                                }

                                                Tile? middle_tile = middle_tiles?.GetTile(current_location);
                                                if (middle_tile != null)
                                                {
                                                    if (middle_tile.BlocksMovement &&
                                                        middle_tile.BlocksSight)
                                                    {
                                                        break;
                                                    }
                                                }

                                                Tile current = new()
                                                {
                                                    Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height),
                                                    Location = new Location(current_location.X - min_x, current_location.Y - min_y, 0),
                                                    DrawColor = drawColor
                                                };

                                                if (current.Region.X >= 0 - size_width - 1)
                                                {
                                                    if (current.Region.X < resolution_width + size_width + 1)
                                                    {
                                                        if (current.Region.Y >= 0 - size_height - 1)
                                                        {
                                                            if (current.Region.Y < resolution_height + size_height + 1)
                                                            {
                                                                bool found = false;
                                                                foreach (Tile existing in Handler.light_maps[region_center_point])
                                                                {
                                                                    if (existing.Location != null &&
                                                                        existing.Location.X == current.Location.X &&
                                                                        existing.Location.Y == current.Location.Y)
                                                                    {
                                                                        found = true;
                                                                        break;
                                                                    }
                                                                }

                                                                if (!found)
                                                                {
                                                                    Handler.light_maps[region_center_point].Add(current);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Sort
                                    List<Tile> light_points = Handler.light_maps[region_center_point];
                                    int light_points_count = light_points.Count;
                                    for (int i = 0; i < light_points_count; i++)
                                    {
                                        for (int j = 0; j < light_points_count - 1; j++)
                                        {
                                            Tile first_point = light_points[j];
                                            if (first_point.Region == null)
                                            {
                                                continue;
                                            }

                                            Tile second_point = light_points[j + 1];
                                            if (second_point.Region == null)
                                            {
                                                continue;
                                            }

                                            if (first_point.Region.Y > second_point.Region.Y)
                                            {
                                                light_points[j + 1] = first_point;
                                                light_points[j] = second_point;
                                            }
                                            else if (first_point.Region.Y == second_point.Region.Y &&
                                                     first_point.Region.X > second_point.Region.X)
                                            {
                                                light_points[j + 1] = first_point;
                                                light_points[j] = second_point;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            if (Main.Game == null ||
                !Main.Game.GameStarted)
            {
                return;
            }

            Texture2D? light = Handler.GetTexture("point_light");
            if (light == null)
            {
                return;
            }

            int width = ((Main.light_tile_distance * 2) + 1);

            int light_sub_width = light.Width / width;
            int light_sub_height = light.Height / width;

            int count = Handler.light_maps.Count;
            for (int l = 0; l < count; l++)
            {
                var list = Handler.light_maps.Values.ToList();

                List<Tile> light_maps = list[l];

                for (int i = 0; i < light_maps.Count; i++)
                {
                    Tile map = light_maps[i];
                    if (map.Location == null ||
                        map.Region == null)
                    {
                        continue;
                    }

                    Vector2 coord = map.Location.ToVector2;

                    Rectangle region = new((int)map.Region.X, (int)map.Region.Y, (int)Main.Game.TileSize.X, (int)Main.Game.TileSize.Y);

                    int image_x = (int)coord.X * light_sub_width;
                    int image_y = (int)coord.Y * light_sub_height;
                    Rectangle image = new(image_x, image_y, light_sub_width, light_sub_height);

                    spriteBatch.Draw(light, region, image, map.DrawColor);
                }
            }
        }

        #endregion
    }
}
