using MyMySql.IWords;
using MyMySql.IWords.ILanguageWords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.TrieStuff
{
    public class TrieNode
    {
        public Keyword Value { get; set; }
        public TrieNode Parent { get; set; }
        public List<TrieNode> Children { get; set; }
        
        public bool EndOfCommand { get; set; }
        public Command FullCommand { get; set; }
        public List<List<WordRange>> ChildrenWordRanges { get; private set; }

        public TrieNode(Keyword value, TrieNode parent, List<TrieNode> children, bool endOfCommand, Command fullCommand)
        {
            Value = value;
            Parent = parent;
            ChildrenWordRanges = new List<List<WordRange>>();
            Children = children;
            EndOfCommand = endOfCommand;
            FullCommand = fullCommand;
        }

        public void FillChildrenWordRanges()
        {
            ChildrenWordRanges = new List<List<WordRange>>();
            foreach (TrieNode child in Children)
            {
                ChildrenWordRanges.AddRange(child.Value.ChildrenRanges);
            }
        }
    }
}
