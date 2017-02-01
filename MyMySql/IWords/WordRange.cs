using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords
{
    public class WordRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public IWord TypeOfWord { get; set; }
        public WordRange(int min, int max, IWord typeOfWord)
        {
            Min = min;
            Max = max;
            TypeOfWord = typeOfWord;
        }
    }
}
