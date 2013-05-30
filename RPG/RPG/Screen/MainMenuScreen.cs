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
    class MainMenuScreen : MenuScreen
    {
        Texture2D background;

        public MainMenuScreen(ScreenManager screenManager) :
            base(screenManager, new String[] { "Play", "Load", "Help", "Exit" },
                    new Action[] { MenuItemFunctions.Play, MenuItemFunctions.Load, MenuItemFunctions.MainMenuHelp, MenuItemFunctions.Exit })
        { }

        public override void LoadContent() {
            background = Content.Load<Texture2D>("MainMenuBackground");
            
            base.LoadContent();
        }

        public override void Update(GameTime gTime) {
            base.Update(gTime);
        }

        public override void Draw(GameTime gTime) {
            SpriteBatch.Draw(background, Graphics.GraphicsDevice.Viewport.Bounds, Color.White);

            base.Draw(gTime);
        }
    }
}
