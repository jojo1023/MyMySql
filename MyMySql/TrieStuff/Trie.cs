using MyMySql.IWords;
using MyMySql.IWords.ILanguageWords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.TrieStuff
{
    public class Trie
    {
        public TrieNode BaseNode { get; set; }
        public Trie()
        {
            BaseNode = new TrieNode(null, null, new List<TrieNode>(), false, null);
        }
        public TrieNode AddNode(Keyword value, TrieNode parent, bool endOfCommand, Command fullCommand)
        {
            TrieNode returnNode;
            parent.Children.Add(returnNode = new TrieNode(value, parent, new List<TrieNode>(), endOfCommand, fullCommand));
            return returnNode;
        }
        public Command GetCommand(List<Keyword> keywords, TrieNode currentNode, int keywordIndex)
        {
            if (keywordIndex < keywords.Count)
            {
                foreach (TrieNode child in currentNode.Children)
                {
                    Keyword currentKeyword = new Keyword(child.Value.Input, child.Value.Ranges);
                    currentKeyword.Children = new List<IWord>(keywords[keywordIndex].Children);
                    if (child.Value.Input == keywords[keywordIndex].Input && ListContainsRanges(keywords[keywordIndex].RangesThatWorked, child.Value.ChildrenRanges) >= 0)
                    {
                        if (keywordIndex + 1 >= keywords.Count && child.EndOfCommand)
                        {
                            return new Command(new List<List<CommandKeywordInfo>>() { new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = currentKeyword, KeywordRangesThatDontWork = new List<List<WordRange>>() } } }, child.FullCommand);
                        }
                        else
                        {
                            Command possibleReturnCommand = GetCommand(keywords, child, keywordIndex + 1);
                            if (possibleReturnCommand != null)
                            {
                                possibleReturnCommand.KeywordsInCommand.Add(new List<CommandKeywordInfo>() { new CommandKeywordInfo() { CommandKeyword = currentKeyword, KeywordRangesThatDontWork = new List<List<WordRange>>() } });
                                return possibleReturnCommand;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public void AddCommand(Command command)
        {
            foreach (List<CommandKeywordInfo> keywordsInfo in command.KeywordsInCommand)
            {
                BaseNode = AddKeywordChain(keywordsInfo, 0, BaseNode, command);
            }
        }
        TrieNode AddKeywordChain(List<CommandKeywordInfo> keywordsInfo, int keywordIndex, TrieNode currentNode, Command dictionaryCommand)
        {
            TrieNode returnNode = currentNode;

            foreach (List<WordRange> ranges in keywordsInfo[keywordIndex].CommandKeyword.ChildrenRanges)
            {
                if (ListContainsRanges(ranges, keywordsInfo[keywordIndex].KeywordRangesThatDontWork) < 0)
                {
                    int containsRanges = ListContainsRanges(ranges, currentNode.ChildrenWordRanges);

                    if (containsRanges < 0 || ListContainsKeyword(keywordsInfo[keywordIndex].CommandKeyword.Input, currentNode.Children) < 0)
                    {
                        TrieNode newNode = new TrieNode(new Keyword(keywordsInfo[keywordIndex].CommandKeyword.Input, new List<List<WordRange>>() { ranges }), currentNode, new List<TrieNode>(), false, null);
                        if (keywordIndex + 1 >= keywordsInfo.Count)
                        {
                            newNode.EndOfCommand = true;
                            List<CommandKeywordInfo> commandKeywords = GetKeywordsFromEnd(newNode);
                            commandKeywords.Reverse();
                            newNode.FullCommand = new Command(new List<List<CommandKeywordInfo>>() { commandKeywords }, dictionaryCommand);
                        }
                        currentNode.Children.Add(newNode);
                        currentNode.FillChildrenWordRanges();
                        if (keywordIndex + 1 < keywordsInfo.Count)
                        {
                            AddKeywordChain(keywordsInfo, keywordIndex + 1, newNode, null);
                        }

                    }
                    else if (keywordIndex + 1 < keywordsInfo.Count)
                    {
                        AddKeywordChain(keywordsInfo, keywordIndex + 1, currentNode.Children[containsRanges], null);
                    }
                }
            }
            return returnNode;
        }
        int ListContainsRanges(List<WordRange> ranges, List<List<WordRange>> listOfRanges)
        {
            int returnInt = -1;
            if (ranges != null && listOfRanges != null)
            {
                foreach (List<WordRange> otherRanges in listOfRanges)
                {
                    if (otherRanges != null)
                    {
                        bool followsRanges = false;
                        if (ranges.Count == otherRanges.Count)
                        {
                            for (int i = 0; i < otherRanges.Count; i++)
                            {
                                if (otherRanges[i] == ranges[i])
                                {
                                    followsRanges = true;
                                    returnInt = listOfRanges.IndexOf(otherRanges);
                                    break;
                                }
                            }
                            if (followsRanges)
                            {
                                break;
                            }
                        }
                        else
                        {
                            followsRanges = false;
                        }
                    }
                }
            }
            return returnInt;
        }

        int ListContainsKeyword(string keywordInput, List<TrieNode> nodes)
        {
            int returnInt = -1;
            for(int i = 0; i < nodes.Count; i++)
            {
                if(nodes[i].Value.Input == keywordInput)
                {
                    return i;
                }
            }
            return returnInt;
        }

        List<CommandKeywordInfo> GetKeywordsFromEnd(TrieNode endNode)
        {
            List<CommandKeywordInfo> retunList = new List<CommandKeywordInfo>();
            retunList.Add(new CommandKeywordInfo() { CommandKeyword = endNode.Value, KeywordRangesThatDontWork = new List<List<WordRange>>() });
            if(endNode.Parent != null && endNode.Parent != BaseNode)
            {
                retunList.AddRange(GetKeywordsFromEnd(endNode.Parent));
            }
            return retunList;
        }
    }
}
