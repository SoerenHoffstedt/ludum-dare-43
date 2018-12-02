using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43
{
    public class Difficulty
    {
        public static Difficulty Normal = new Difficulty(12, 28, 28);
        public static Difficulty Harder = new Difficulty(9, 20, 20);

        public readonly int startingFood;
        public readonly int startingWood;
        public readonly int startingStone;
        private Difficulty(int food, int wood, int stone)
        {
            startingFood = food;
            startingWood = wood;
            startingStone = stone;
        }
    }
}
