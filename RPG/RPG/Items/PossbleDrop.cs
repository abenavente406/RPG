using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Items
{
    public class PossibleDrop
    {
        public readonly Item Item;
        public readonly double Chance;

        public PossibleDrop(Item i, double c) {
            Item = i;
            Chance = c;
        }
    }
}
