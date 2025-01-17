using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Rendering;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Scenes;

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
            Handler.light_maps.Clear();

            Scene gameplay = SceneManager.GetScene("Gameplay");
            if (gameplay != null)
            {
                World world = gameplay.World;
                if (world != null)
                {
                    if (world.Maps.Count > 0)
                    {
                        Map map = world.Maps[0];

                        Layer bottom_tiles = map.GetLayer("BottomTiles");
                        Layer middle_tiles = map.GetLayer("MiddleTiles");

                        int light_source_count = Handler.light_sources.Count;
                        for (int l = 0; l < light_source_count; l++)
                        {
                            Point light_source = Handler.light_sources[l];
                            if (Handler.light_maps.Count < Main.light_max_count)
                            {
                                Tile starting_tile = bottom_tiles.GetTile(new Vector2(light_source.X, light_source.Y));
                                Tile source_tile = middle_tiles.GetTile(new Vector2(light_source.X, light_source.Y));

                                if (source_tile != null &&
                                    starting_tile != null)
                                {
                                    if (source_tile.IsLightSource)
                                    {
                                        Color drawColor = Color.White;
                                        if (source_tile.Name.Contains("TV"))
                                        {
                                            drawColor = new Color(0, 255, 255, 255);
                                        }
                                        else if (source_tile.Name.Contains("Lamp"))
                                        {
                                            drawColor = new Color(255, 240, 160, 255);
                                        }

                                        int distance = Main.light_tile_distance;
                                        int full_width = distance * Main.Game.TileSize.X;
                                        int full_height = distance * Main.Game.TileSize.Y;
                                        int resolution_width = Main.Game.Resolution.X;
                                        int resolution_height = Main.Game.Resolution.Y;

                                        if (starting_tile.Region.X >= 0 - full_width - 1 &&
                                            starting_tile.Region.X <= resolution_width + full_width + 1)
                                        {
                                            if (starting_tile.Region.Y >= 0 - full_height - 1 &&
                                                starting_tile.Region.Y <= resolution_height + full_height + 1)
                                            {
                                                int size_width = Main.Game.TileSize.X;
                                                int size_height = Main.Game.TileSize.Y;

                                                int half_width = size_width / 2;
                                                int half_height = size_height / 2;

                                                Vector2 region_center_point = new Vector2(starting_tile.Region.X + half_width, starting_tile.Region.Y + half_height);
                                                if (Handler.light_maps.ContainsKey(region_center_point))
                                                {
                                                    Handler.light_maps[region_center_point].Clear();
                                                }
                                                else
                                                {
                                                    Handler.light_maps.Add(region_center_point, new List<Something>());
                                                }

                                                int min_x = light_source.X - distance;
                                                int max_x = light_source.X + distance;
                                                int min_y = light_source.Y - distance;
                                                int max_y = light_source.Y + distance;

                                                HashSet<Point> edge_coords = new HashSet<Point>();
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
                                                        Vector2 current_location = new Vector2(point.X, point.Y);

                                                        Tile bottom_tile = bottom_tiles.GetTile(current_location);
                                                        if (bottom_tile != null)
                                                        {
                                                            if (bottom_tile.BlocksMovement)
                                                            {
                                                                break;
                                                            }

                                                            Tile middle_tile = middle_tiles.GetTile(current_location);
                                                            if (middle_tile != null)
                                                            {
                                                                if (middle_tile.BlocksMovement &&
                                                                    WorldUtil.BlocksSight(middle_tile.Name))
                                                                {
                                                                    break;
                                                                }
                                                            }

                                                            Something current = new Something();
                                                            current.Region = new Region(bottom_tile.Region.X, bottom_tile.Region.Y, bottom_tile.Region.Width, bottom_tile.Region.Height);
                                                            current.Location = new Location(current_location.X - min_x, current_location.Y - min_y, 0);
                                                            current.DrawColor = drawColor;

                                                            if (current.Region.X >= 0 - size_width - 1)
                                                            {
                                                                if (current.Region.X < resolution_width + size_width + 1)
                                                                {
                                                                    if (current.Region.Y >= 0 - size_height - 1)
                                                                    {
                                                                        if (current.Region.Y < resolution_height + size_height + 1)
                                                                        {
                                                                            bool found = false;
                                                                            foreach (Something existing in Handler.light_maps[region_center_point])
                                                                            {
                                                                                if (existing.Location.X == current.Location.X &&
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
                                                List<Something> light_points = Handler.light_maps[region_center_point];
                                                int light_points_count = light_points.Count;
                                                for (int i = 0; i < light_points_count; i++)
                                                {
                                                    for (int j = 0; j < light_points_count - 1; j++)
                                                    {
                                                        Something first_point = light_points[j];
                                                        Something second_point = light_points[j + 1];

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
                }
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D light = AssetManager.Textures["point_light"];
            int width = ((Main.light_tile_distance * 2) + 1);

            int light_sub_width = light.Width / width;
            int light_sub_height = light.Height / width;

            int count = Handler.light_maps.Count;
            for (int l = 0; l < Handler.light_maps.Count; l++)
            {
                var list = Handler.light_maps.Values.ToList();

                List<Something> light_maps = list[l];

                for (int i = 0; i < light_maps.Count; i++)
                {
                    Something map = light_maps[i];
                    Vector2 coord = new Vector2(map.Location.X, map.Location.Y);

                    Rectangle region = new Rectangle((int)map.Region.X, (int)map.Region.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);

                    int image_x = (int)coord.X * light_sub_width;
                    int image_y = (int)coord.Y * light_sub_height;
                    Rectangle image = new Rectangle(image_x, image_y, light_sub_width, light_sub_height);

                    spriteBatch.Draw(light, region, image, map.DrawColor);
                }
            }
        }

        #endregion
    }
}
