using LD43.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LD43
{
    public static class Effects
    {
        private static Stack<Effect> effectPool;
        private static LinkedList<Effect> currentEffects;
        private static Dictionary<string, EffectData> effectData;


        public static void Initialize(XmlDocument def, Texture2D atlas)
        {
            LoadEffects(def, atlas);
            currentEffects = new LinkedList<Effect>();
            effectPool = new Stack<Effect>(20);
            for (int i = 0; i < 10; i++)
                effectPool.Push(new Effect());
        }

        private static void LoadEffects(XmlDocument xmlDef, Texture2D atlas)
        {
            XmlNodeList nodes = xmlDef.SelectNodes("effects/effect");
            effectData = new Dictionary<string, EffectData>(nodes.Count);

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string id = node.Attributes["id"].Value;
                double frameTime = double.Parse(node.Attributes["frameTime"].Value) / 1000.0;
                int w = 16;
                int h = 16;

                XmlNodeList frameNodes = node.SelectNodes("frame");
                Rectangle[] rects = new Rectangle[frameNodes.Count];
                for (int k = 0; k < frameNodes.Count; k++)
                {
                    int x = int.Parse(frameNodes[k].Attributes["x"].Value) * w;
                    int y = int.Parse(frameNodes[k].Attributes["y"].Value) * h;
                    rects[k] = new Rectangle(x, y, w, h);
                }

                Point offset = new Point(int.Parse(node.Attributes["offsetX"].Value), int.Parse(node.Attributes["offsetY"].Value));

                EffectData d = new EffectData(id, atlas, frameTime, rects, offset);
                effectData.Add(id, d);
            }
        }

        public static void Reset()
        {
            foreach (Effect e in currentEffects)
            {
                effectPool.Push(e);
            }

            currentEffects.Clear();
        }

        public static void Update(double dt)
        {
            LinkedListNode<Effect> node = currentEffects.First;
            while (node != null)
            {
                LinkedListNode<Effect> next = node.Next;
                Effect e = node.Value;
                e.frameTimer += dt;
                if (e.frameTimer >= e.frameTime)
                {
                    e.frameTimer -= e.frameTime;
                    e.currentFrameIndex++;
                    if (e.currentFrameIndex >= e.rects.Length)
                    {
                        if (e.loop)
                        {
                            e.currentFrameIndex = 0;
                        }
                        else
                        {
                            currentEffects.Remove(node);
                            effectPool.Push(e);
                        }
                    }

                }
                if (e.loop)
                {
                    e.lifeTimer += dt;
                    if (e.lifeTimer >= e.lifeTime)
                    {
                        currentEffects.Remove(node);
                    }
                }
                node = next;
            }


        }

        public static void Render(SpriteBatch spriteBatch)
        {
            foreach (Effect e in currentEffects)
            {
                spriteBatch.Draw(e.texture, new Rectangle(e.position + e.offset, e.size), e.rects[e.currentFrameIndex], Color.White, 0.0f, Vector2.Zero, e.spriteEffects, 0);
            }
        }

        public static void ShowEffect(string id, Point position, bool flipEffect = false, double loopTime = 0.0)
        {
            if (!effectData.ContainsKey(id))
            {
                Debug.WriteLine($"ERROR: Effect {id} does not exist.");
                return;
            }

            EffectData data = effectData[id];
            Effect e;
            if (effectPool.Count > 0)
                e = effectPool.Pop();
            else
                e = new Effect();

            e.SetData(data.texture, data.frameRects, data.frameTime, position, data.offset, flipEffect, loopTime);

            currentEffects.AddFirst(e);

            Debug.WriteLine($"Current Effects count: {currentEffects.Count}, Effects in Pool: {effectPool.Count}");
        }

        class Effect
        {
            public Rectangle[] rects;
            public Texture2D texture;

            public int currentFrameIndex;
            public Point position;
            public Point size;
            public Point offset;

            public SpriteEffects spriteEffects;
            public bool loop;
            public double lifeTimer;
            public double lifeTime;

            public double frameTimer;
            public double frameTime;

            public void SetData(Texture2D texture, Rectangle[] rects, double frameTime, Point position, Point offset, bool drawFliped, double time = 0.0)
            {
                currentFrameIndex = 0;
                this.position = position;
                this.size = rects[0].Size;
                if (drawFliped)
                {
                    int wantedX = Map.TileSize.X + offset.X * -1;
                    int xNow = Map.TileSize.X + offset.X;
                    int offsetX = wantedX - xNow;
                    this.offset = new Point(offsetX - 24, offset.Y);
                }
                else
                    this.offset = offset;
                this.texture = texture;
                this.rects = rects;
                this.lifeTimer = 0.0;
                this.lifeTime = time;
                this.spriteEffects = drawFliped ? SpriteEffects.FlipVertically : SpriteEffects.None;
                this.loop = time > 0.0;
                this.frameTimer = 0.0;
                this.frameTime = frameTime;
            }
        }

    }

    public struct EffectData
    {
        public string id;
        public Texture2D texture;
        public Rectangle[] frameRects;
        public double frameTime;
        public Point offset;

        public EffectData(string id, Texture2D texture, double frameTime, Rectangle[] frameRects, Point offset)
        {
            this.id = id;
            this.texture = texture;
            this.frameTime = frameTime;
            this.frameRects = frameRects;
            this.offset = offset;
        }
    }
}
