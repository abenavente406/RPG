using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Helpers
{
    class RName
    {
        static string[] format = new string[] { "cvcc", "cvcv", "cvccvcvc", "ccvcc", "cvvcv", "ccvcv", "vvccv", "cvc" };
        static string[] adjs = new string[] {
            "Destoryer", "Bold", "Clever", "Clumsy", "Fierce", "Ancient", "Angry", "Awful", "Barbarous", "Berserk", "Brave",
            "Bright", "Crazy", "Delirious", "Demonic", "Drunk", "Feeble", "Fifth", "Fourth", "Third", "Second", "First", 
            "Great", "Gullible", "Hideous", "Hysterical", "Bear", "Beast", "Infamous", "Intelligent", "Invincible",
            "Lazy", "Meek", "Mute", "Naive", "Nasty", "Nutty", "Obscene", "Odd", "Psychotic", "Quaint", "Quick", "Rabid",
            "Ruthless", "Selfish", "Sharp", "Slow", "Strong", "Swift", "Terrible", "Dog", "Ugly", "Vengeful", "Violent",
            "Wicked", "Wise", "Witty", "Wrathful", "Wretched", "Vicious", "Fierce"
        };

        public static string newCoolName() {
            return newName() + " The " + adjs[Rand.Next(adjs.Length)];
        }

        public static string newName() {
            string name = "";
            string nFormat = format[Rand.Next(format.Length - 1)];
            for (int i=0; i<nFormat.Length; i++) {
                if (i == 0) {
                    if (nFormat[i] == 'c')
                        name += getFistConst();
                    else
                        name += getFistVowel();
                } else {
                    if (nFormat[i] == 'c')
                        name += getBodyConst();
                    else
                        name += getBodyVowel();
                }
            }
            return name;
        }

        private static string getFistVowel()
        {
            int r = Rand.Next(276);
            if ((r -= 116) <= 0) return "A";
            else if ((r -= 20) <= 0) return "E";
            else if ((r -= 63) <= 0) return "I";
            else if ((r -= 62) <= 0) return "O";
            else if ((r -= 15) <= 0) return "U";
            else return "E";
        }

        private static string getFistConst() {
            int r = Rand.Next(724);
            if ((r -= 47) <= 0) return "B";
            else if ((r -= 35) <= 0) return "C";
            else if ((r -= 27) <= 0) return "D";
            else if ((r -= 38) <= 0) return "F";
            else if ((r -= 20) <= 0) return "G";
            else if ((r -= 72) <= 0) return "H";
            else if ((r -= 6) <= 0) return "J";
            else if ((r -= 6) <= 0) return "K";
            else if ((r -= 27) <= 0) return "L";
            else if ((r -= 44) <= 0) return "M";
            else if ((r -= 23) <= 0) return "N";
            else if ((r -= 25) <= 0) return "P";
            else if ((r -= 47) <= 0) return "Qu";
            else if ((r -= 2) <= 0) return "R";
            else if ((r -= 16) <= 0) return "S";
            else if ((r -= 166) <= 0) return "T";
            else if ((r -= 6) <= 0) return "V";
            else if ((r -= 67) <= 0) return "W";
            else if ((r -= 1) <= 0) return "X";
            else if ((r -= 16) <= 0) return "Y";
            else if ((r -= 1) <= 0) return "Z";
            else return "T";
        }

        private static char getBodyVowel() {
            int r = Rand.Next(381);
            if ((r -= 82) <= 0) return 'a';
            else if ((r -= 127) <= 0) return 'e';
            else if ((r -= 70) <= 0) return 'i';
            else if ((r -= 75) <= 0) return 'o';
            else if ((r -= 27) <= 0) return 'u';
            else return 'e';
        }

        private static char getBodyConst() {
            int r = Rand.Next(599);
            if ((r -= 15) <= 0) return 'b';
            else if ((r -= 28) <= 0) return 'c';
            else if ((r -= 43) <= 0) return 'd';
            else if ((r -= 22) <= 0) return 'f';
            else if ((r -= 20) <= 0) return 'g';
            else if ((r -= 61) <= 0) return 'h';
            else if ((r -= 2) <= 0) return 'j';
            else if ((r -= 8) <= 0) return 'k';
            else if ((r -= 40) <= 0) return 'l';
            else if ((r -= 24) <= 0) return 'm';
            else if ((r -= 67) <= 0) return 'n';
            else if ((r -= 19) <= 0) return 'p';
            else if ((r -= 1) <= 0) return 'q';
            else if ((r -= 60) <= 0) return 'r';
            else if ((r -= 63) <= 0) return 's';
            else if ((r -= 91) <= 0) return 't';
            else if ((r -= 27) <= 0) return 'u';
            else if ((r -= 10) <= 0) return 'v';
            else if ((r -= 23) <= 0) return 'w';
            else if ((r -= 2) <= 0) return 'x';
            else if ((r -= 1) <= 0) return 'z';
            else return 't';
        }

        public static Random Rand { get { return Screen.ScreenManager.Rand; } }
    }
}
