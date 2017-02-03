using MyMySql.ICustomWords;
using MyMySql.IWords.ICustomWords;
using MyMySql.IWords.ILanguageWords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords
{
    public class Command
    {
        public List<List<CommandKeywordInfo>> KeywordsInCommand { get; set; }
        public List<TableWord> TablesInCommand { get; set; }
        public List<ColumnWord> ColumnsInCommand { get; set; }
        public List<CustomCustomWord> CustomCustomsInCommand { get; set; }
        
        public InputInfo Input { get; set; }
        public Type Output { get; set; }
        public Command ChildCommand { get; set; }
        
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, InputInfo input, Type output)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = input;
            Output = output;
            ChildCommand = null;
        }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, Command dictionaryCommand)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = dictionaryCommand.Input;
            Output = dictionaryCommand.Output;
            ChildCommand = null;
        }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, Command childCommand, Command dictionaryCommand)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = dictionaryCommand.Input;
            Output = dictionaryCommand.Output;
            ChildCommand = childCommand;
        }
        public void GetCustomWordsInCommand(IWord currentWord, bool parentIsLogicOpperation)
        {
            if (currentWord.AllWordType == AllWordTypes.Table && !currentWord.Initializing)
            {
                TablesInCommand.Add((TableWord)currentWord);
            }
            else if (currentWord.AllWordType == AllWordTypes.Column && !currentWord.Initializing)
            {
                ColumnsInCommand.Add((ColumnWord)currentWord);
            }
            else if (currentWord.AllWordType == AllWordTypes.CustomCustom && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                CustomCustomsInCommand.Add((CustomCustomWord)currentWord);
            }

            foreach (IWord child in currentWord.Children)
            {
                GetCustomWordsInCommand(child, currentWord.AllWordType == AllWordTypes.LogicOperation);
            }
        }
    }
    public struct CommandKeywordInfo
    {
        public Keyword CommandKeyword;
        public List<List<WordRange>> KeywordRangesThatDontWork;
    }
    public struct InputInfo
    {
        public Type InputType;
        public List<string> KeywordsNotAllowedAsInput;
    }
}
