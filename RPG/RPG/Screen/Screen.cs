using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace RPG.Screen
{
    public abstract class Screen
    {
        protected ScreenManager screenManager;

        private bool draw, update;
        public bool DoDraw { get { return draw; } set { draw = value; } }
        public bool DoUpdate { 
            get { return update; } 
            set {
                update = value; 
                ScreenManager.oldKBState = ScreenManager.kbState; 
                ScreenManager.kbState = Keyboard.GetState();
            } 
        }

        public SpriteBatch SpriteBatch { get { return screenManager.getSpriteBatch(); } }
        public GraphicsDeviceManager Graphics { get { return screenManager.getGraphics(); } }
        public ContentManager Content { get { return screenManager.getContent(); } }

        public Screen(ScreenManager screenManager) {
            this.screenManager = screenManager;
            DoDraw = DoUpdate = true;
        }

        public ScreenManager getScreenManager() { return screenManager; }

        public abstract void Update(GameTime gTime);
        public abstract void Draw(GameTime gTime);
        public abstract void LoadContent();
        public abstract void UnloadContent();
    }
}
