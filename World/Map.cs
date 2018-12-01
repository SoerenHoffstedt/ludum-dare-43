using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.World
{
    public class Map
    {
        public static Point TileSize = new Point(16, 16);
        public static Point Size;
        public Tile[,] tiles;

        public Map()
        {
            LoadMap();
        }

        private void LoadMap()
        {
            var lines = File.ReadAllLines("Content/level.txt");

            int w = int.Parse(lines[0]);
            int h = int.Parse(lines[1]);
            Size = new Point(w, h);
            tiles = new Tile[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    tiles[x, y] = new Tile(new Point(x, y));
                }
            }

            tiles[10, Size.Y - 1].Exists = false;
            tiles[11, Size.Y - 1].Exists = false;
            tiles[11, Size.Y - 2].Exists = false;
            tiles[12, Size.Y - 1].Exists = false;
            tiles[0, Size.Y - 1].Exists = false;

            tiles[10, Size.Y - 5].Exists = false;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!tiles[x, y].Exists)
                        continue;

                    int neighbourCount = 0;
                    if (IsInRange(x, y - 1) && tiles[x, y - 1].Exists)
                        neighbourCount += 1;
                    if (IsInRange(x + 1, y) && tiles[x + 1, y].Exists)
                        neighbourCount += 2;
                    if (IsInRange(x, y + 1) && tiles[x, y + 1].Exists)
                        neighbourCount += 4;
                    if (IsInRange(x - 1, y) && tiles[x - 1, y].Exists)
                        neighbourCount += 8;

                    tiles[x, y].sprite = Assets.TileSprites[neighbourCount];
                }
            }

            char[] splitters = { ',', ':' };

            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i];
                //Format of a line: x,y: Something
                //Something is Food,Wood,Stone,Hut, or something similar.
                string[] splits = line.Split(splitters);
                int x = int.Parse(splits[0]);
                int y = int.Parse(splits[1]);
                splits[2] = splits[2].Trim();

                switch (splits[2])
                {
                    case "Wood":
                        break;
                    case "Food":
                        break;
                    case "Stone":
                        break;
                    case "Hut":
                        break;
                }

            }
        }

        public void Update(float dt)
        {

        }

        public void Render(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    if(tiles[x, y].Exists && tiles[x,y].sprite != null)
                        tiles[x, y].sprite.Render(spriteBatch, new Point(x, y) * TileSize);
                }
            }
        }

        #region Helper

        public IEnumerable<Point> IterateNeighboursFourDir(int x, int y)
        {
            if (IsInRange(x - 1, y))
                yield return new Point(x - 1, y);
            if (IsInRange(x + 1, y))
                yield return new Point(x + 1, y);
            if (IsInRange(x, y - 1))
                yield return new Point(x, y - 1);
            if (IsInRange(x, y + 1))
                yield return new Point(x, y + 1);
        }

        public bool IsInRange(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size.X && y < Size.Y;
        }

        public bool IsInRange(Point p)
        {
            return IsInRange(p.X, p.Y);
        }

        public Tile this[int x, int y]
        {
            get { return tiles[x, y]; }
            set { tiles[x, y] = value; }
        }

        public Tile this[Point p]
        {
            get { return tiles[p.X, p.Y]; }
            set { tiles[p.X, p.Y] = value; }
        }

        #endregion

    }
}
