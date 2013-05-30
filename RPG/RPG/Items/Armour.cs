using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Screen;

namespace RPG.Items
{
    public enum ArmourParts { Head, Body, Legs };

    [Serializable()]
    public class Armour : ISerializable
    {
        public static readonly Armour NONE = new Armour(null, null, GameScreen.Items[ItemId.None], ArmourParts.Body, 0);

        public readonly Item Item;
        public readonly Texture2D Attack, Stand;
        public readonly ArmourParts Part;
        public readonly float Mult;

        public Armour(SerializationInfo info, StreamingContext cntxt)  : this((Item) info.GetValue("Armour_Item", typeof(Item))) { }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("Armour_Item", Item);
        }

        public Armour(Item i) {
            if (i != null) {
                Armour a = GameScreen.Armours[i.Id];
                this.Item = i;
                this.Part = a.Part;
                this.Mult = a.Mult;

                this.Stand = a.Stand;
                this.Attack = a.Attack;
            } else {
                this.Item = GameScreen.Items[ItemId.None];
                this.Part = ArmourParts.Body;
                this.Mult = 0;

                this.Stand = null;
                this.Attack = null;
            }
        }

        public Armour(Armour a, Item i)
        {
            this.Item = i;
            this.Part = a.Part;
            this.Mult = a.Mult;

            this.Stand = a.Stand;
            this.Attack = a.Attack;
        }

        public Armour(Texture2D stndTex, Texture2D attTex, string name, ItemId id, ArmourParts part, float mult, bool add) {
            // Add its item to the Item list
            Item i = new Item(id, name, stndTex, Item.Equip);
            if (add) GameScreen.Items.Add(id, i);

            this.Item = i;
            this.Part = part;
            this.Mult = mult;

            this.Stand = stndTex;
            this.Attack = attTex;
        }

        public Armour(Texture2D stndTex, Texture2D attTex, Item i, ArmourParts part, float mult) {
            this.Item = i;
            this.Part = part;
            this.Mult = mult;

            this.Stand = stndTex;
            this.Attack = attTex;
        }

        public override string ToString() {
            return ToString(100);
        }
        public string ToString(int nameSize) {
            if (Item != null) {
                string useName = Item.Name;
                if (useName.Length > nameSize - 3)
                    useName = useName.Substring(0, nameSize-3) + "...";
                // Mult > 0 then is reducing multiplier (-)
                return useName + " (" + ((Mult > 0) ? "-" : "+") + Math.Abs(Mult).ToString("0.00") + ")";
            } else {
                return "";
            }
        }
    }
}
