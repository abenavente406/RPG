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
    class MainMenuHelpScreen : MenuScreen
    {
        public MainMenuHelpScreen(ScreenManager sm)
            : base(sm, new string[] { "Back" }, new Action[] { MenuItemFunctions.MainMenu })
        {}

        public override void Update(GameTime gTime) {
            base.Update(gTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gTime) {
            SpriteBatch.DrawString(ScreenManager.Font, "Go left => A", new Vector2(160, 23), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Go right => D", new Vector2(150, 53), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Block => W", new Vector2(164, 83), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Inventory => I", new Vector2(149, 113), Color.White);

            SpriteBatch.DrawString(ScreenManager.Font, "S <= Duck", new Vector2(377, 23), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Space <= Jump", new Vector2(377, 53), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "1-3 <= Attack", new Vector2(377, 83), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Enter <= Interact", new Vector2(377, 113), Color.White);

            base.Draw(gTime);
        }
    }
}
