using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Sprites
{
    public class SpriteParts {
        public enum Part { Attack, Block, Crouch, CrouchAttack, Dead, Move };

        public static readonly Part[] Entity = { Part.Attack, Part.Block, Part.Crouch, Part.CrouchAttack, Part.Dead, Part.Move };
    }
}
