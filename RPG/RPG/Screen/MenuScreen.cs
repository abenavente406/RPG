using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG.Screen
{
    public class MenuScreen : Screen {
        protected static Vector2 SELECTED_EXTRA_SIZE = ScreenManager.Font.MeasureString("[  ]");

        protected List<MenuItem> menuItems;
        int curSelection;

        public MenuScreen(ScreenManager sm ,String[] items, Action[] funcs) : base(sm) {
            menuItems = new List<MenuItem>();
            curSelection = 0;

            // Default
            if (items == null) items = new string[] { "Back" };
            if (funcs == null) funcs = new Action[] { MenuItemFunctions.BackToGame };

            for (int i = 0; i < items.GetLength(0); i++)
                menuItems.Add(new MenuItem(this, items[i], funcs[i], MenuItem.MenuItemAlignment.Center));

            if (menuItems.Count == 0) {
                Console.WriteLine("Tried to make a menu screen with no items!");
                Environment.Exit(1);
            }
        }

        protected void setMenuItem(int idx, Action func) {
            this.menuItems[idx].Func = func;
        }

        public override void Update(GameTime gTime) {
            if (ScreenManager.kbState.IsKeyDown(Keys.Down) && ScreenManager.oldKBState.IsKeyUp(Keys.Down)) {
                curSelection++;
                updateSelected();
            } else if (ScreenManager.kbState.IsKeyDown(Keys.Up) && ScreenManager.oldKBState.IsKeyUp(Keys.Up)) {
                curSelection--;
                updateSelected();
            } else if (ScreenManager.kbState.IsKeyDown(Keys.Enter) && ScreenManager.oldKBState.IsKeyUp(Keys.Enter)) {
                menuItems[curSelection].run();
            }
        }
        
        protected void updateSelected() {
            curSelection %= menuItems.Count;
            if (curSelection < 0) curSelection = menuItems.Count - 1;
        }

        public override void Draw(GameTime gTime) { Draw(gTime, Color.White); }
        public virtual void Draw(GameTime gTime, Color textColor) {
            for (int i=0; i < menuItems.Count; i++) {
                MenuItem item = menuItems[i];

                bool isSelected = (i == curSelection);
                float yPosOffset = -(SELECTED_EXTRA_SIZE.Y * 1.5f * (menuItems.Count - 1) / 2) + (i * SELECTED_EXTRA_SIZE.Y * 1.5f);
                item.draw(isSelected, (int) yPosOffset, textColor);
            }
        }

        public override void LoadContent() { }
        public override void UnloadContent() { }
    }
}
