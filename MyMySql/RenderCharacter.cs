using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public class RenderCharacter : IComparable<RenderCharacter>, IEquatable<RenderCharacter>
    {
        public char Character { get; set; }
        public Color CharacterColor { get; set; }
        public Color? SquigglyLineColor { get; set; }
        public int CompareTo(RenderCharacter other)
        {
            return other.CompareTo(this);
        }
        
        public bool Equals(RenderCharacter other)
        {
            return other.Equals(this);
        }

        public RenderCharacter(char character, Color characterColor, Color? squigglyLineColor)
        {
            Character = character;
            CharacterColor = characterColor;
            SquigglyLineColor = squigglyLineColor;
        }

        public static explicit operator char (RenderCharacter character)
        {
            return character.Character;
        }
    }
}
