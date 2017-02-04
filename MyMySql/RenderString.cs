using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public class RenderString : IComparable<RenderString>, IEnumerable<RenderCharacter>, IEnumerable, IEquatable<RenderString>, ICommandReturn
    {
        public List<RenderCharacter> Characters { get; set; }

        public Table ReturnTable { get; set; }

        public RenderString ReturnString
        {
            get
            {
                return this;
            }

            set
            {
                Characters = value.Characters;
                ReturnTable = value.ReturnTable;
            }
        }

        public int CompareTo(RenderString other)
        {
            return other.CompareTo(this);
        }
        
        public bool Equals(RenderString other)
        {
            return other.Equals(this);
        }

        public IEnumerator<RenderCharacter> GetEnumerator()
        {
            return Characters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Characters.GetEnumerator();
        }

        public RenderString(List<RenderCharacter> characters)
        {
            Characters = characters;
            ReturnTable = null;
        }
        public RenderString(string renderString, Color stringColor)
        {
            Characters = new List<RenderCharacter>();
            for(int i = 0; i < renderString.Length; i++)
            {
                Characters.Add(new RenderCharacter(renderString[i], stringColor, null));
            }
            ReturnTable = new Table("");
        }
        public RenderString(string renderString, Color stringColor, Color SquigglyLineColor)
        {
            Characters = new List<RenderCharacter>();
            for (int i = 0; i < renderString.Length; i++)
            {
                Characters.Add(new RenderCharacter(renderString[i], stringColor, SquigglyLineColor));
            }
            ReturnTable = new Table("");
        }

        public override string ToString()
        {
            char[] output = new char[Characters.Count];
            for(int i = 0; i < Characters.Count; i++)
            {
                output[i] = Characters[i].Character;
            }
            return new string(output);
        }

    }
}
