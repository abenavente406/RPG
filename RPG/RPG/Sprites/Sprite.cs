using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RPG.Sprites
{
    public enum Animation { RedSpiral };
    public enum BackgroundId { Cave1 };
    public enum EntitySpriteId { Warrior, Warlock, Wraith, Skeleton_King };
    public enum AttackSpriteId { Fireball, Iceball, Scurge_Shot, Raise_Death };
    public enum TerrainSpriteId { None, Stone_Wall, Stone2_Wall, Door, EmptyMagicWall, ClosedChest, OpenChest, IronDoor };
    public enum GUISpriteId { Blocking, Ducking, Standing };

    public class Sprite
    {
        private int ticks = 0;
        private int frameTime, frameCount;
        private ContentManager content;
        private string baseName;

        public int FrameIdx { get; private set; }
        public Dictionary<SpriteParts.Part, Texture2D> Parts { get; private set; }
        public readonly Texture2D Base;

        public Sprite(Texture2D tex) {
            FrameIdx = frameTime = frameCount = 0;
            Base = tex;
        }

        public Sprite(Texture2D[] texs, int frameCount=0, int frameTime=0) {
            this.FrameIdx = 0;
            this.frameTime = frameTime;
            this.frameCount = frameCount;
            Base = texs[0];
            Parts = new Dictionary<SpriteParts.Part, Texture2D>(texs.Length - 1);
            for (int i=1; i<texs.Length; i++) {
                Parts.Add((SpriteParts.Part) i, texs[i]);
            }
        }

        public Sprite(ContentManager content, String baseName) {
            FrameIdx = frameTime = frameCount = 0;
            this.content = content;
            this.baseName = baseName;
            Base = content.Load<Texture2D>(baseName + "/" + baseName);
        }

        private Sprite(Dictionary<SpriteParts.Part, Texture2D> parts, Texture2D baseTex, int frameCount, int frameTime) {
            FrameIdx = 0;
            this.frameCount = frameCount;
            this.frameTime = frameTime;
            this.Parts = parts;
            this.Base = baseTex;
        }

        public Sprite Clone() {
            return new Sprite(Parts, Base, frameCount, frameTime);
        }

        public void setFrame(int time, int count) {
            this.frameTime = time;
            this.frameCount = count;
        }

        public bool hasSpriteParts(SpriteParts.Part[] parts) {
            if (Parts == null) return false;

            foreach (SpriteParts.Part p in parts) {
                if (!Parts.ContainsKey(p))
                    return false;
            }
            return true;
        }

        public Texture2D getFrame(int i) {
            return Parts.ElementAt(i).Value;
        }

        public Texture2D getSpritePart(SpriteParts.Part part) {
            return Parts[part];
        }

        public Sprite loadSpriteParts(SpriteParts.Part[] parts, int frameCount=0, int frameTime=0) {
            if (content == null)
                throw new ArgumentNullException("This sprite can not load parts (Content is null)");

            this.frameTime = frameTime;
            this.frameCount = frameCount;
            Parts = new Dictionary<SpriteParts.Part, Texture2D>(parts.Length);

            foreach (SpriteParts.Part p in parts) {
                Parts.Add(p, content.Load<Texture2D>(baseName + "/" + baseName + "_" + p.ToString()));
            }

            return this;
        }

        public Texture2D getFrame() {
            if (Parts != null && frameCount != 0)
                return Parts.ElementAt(FrameIdx).Value;
            else
                return Base;
        }

        public void tick(int elapsed) {
            if (frameCount == 0) return;

            ticks += elapsed;
            if (ticks >= frameTime) {
                ticks -= frameTime;
                FrameIdx = (FrameIdx + 1) % (frameCount - 1);
            }
        }
    }
}
