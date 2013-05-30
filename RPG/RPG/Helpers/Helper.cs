using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPG.Helpers
{
    class Helper
    {
        public static Texture2D ParseSpriteSheet(Texture2D ss, int idx, int size) {
            return Helper.ParseSpriteSheet(ss, idx, 0, size);
        }

        public static Texture2D ParseSpriteSheet(Texture2D ss, int idxX, int idxY, int size) {
            GraphicsDevice graphics = ss.GraphicsDevice;
            RenderTarget2D targ = new RenderTarget2D(graphics, size, size);
            SpriteBatch spriteBatch = new SpriteBatch(graphics);

            graphics.SetRenderTarget(targ);
            graphics.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(ss, Vector2.Zero, new Rectangle(idxX * size, idxY * size, size, size), Color.White);
            spriteBatch.End();

            // Reset
            graphics.SetRenderTarget(null);

            return (Texture2D) targ;
        }
    }
}
