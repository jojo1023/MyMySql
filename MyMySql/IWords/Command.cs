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
        public Func<ICommandReturn, List<IWord>, Command, ICommandReturn> CommandFucntion { get; set; }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, InputInfo input, Type output, Func<ICommandReturn, List<IWord>, Command, ICommandReturn> commandFucntion)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = input;
            Output = output;
            ChildCommand = null;
            CommandFucntion = commandFucntion;
        }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, Command dictionaryCommand)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = dictionaryCommand.Input;
            Output = dictionaryCommand.Output;
            ChildCommand = null;
            CommandFucntion = dictionaryCommand.CommandFucntion;
        }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, Command childCommand, Command dictionaryCommand)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = dictionaryCommand.Input;
            Output = dictionaryCommand.Output;
            ChildCommand = childCommand;
            CommandFucntion = dictionaryCommand.CommandFucntion;
        }
        public void GetCustomWordsInCommand()
        {
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            CustomCustomsInCommand = new List<CustomCustomWord>();
            foreach (List<CommandKeywordInfo> keywords in KeywordsInCommand)
            {
                if (keywords.Count == 1)
                {
                    GetCustomWordsInCommandRecursive(keywords[0].CommandKeyword, false);
                }
                else
                {
                    throw new Exception("Command Group Compiler Failed Exception");
                }
            }
            if (ChildCommand != null)
            {
                ChildCommand.GetCustomWordsInCommand();
                ColumnsInCommand.AddRange(ChildCommand.ColumnsInCommand);
                CustomCustomsInCommand.AddRange(ChildCommand.CustomCustomsInCommand);
                TablesInCommand.AddRange(ChildCommand.TablesInCommand);
            }
        }
        void GetCustomWordsInCommandRecursive(IWord currentWord, bool parentIsLogicOpperation)
        {
            if (currentWord.AllWordType == AllWordTypes.Table && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                TablesInCommand.Add((TableWord)currentWord);
            }
            else if (currentWord.AllWordType == AllWordTypes.Column && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                ColumnsInCommand.Add((ColumnWord)currentWord);
            }
            else if (currentWord.AllWordType == AllWordTypes.CustomCustom && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                CustomCustomsInCommand.Add((CustomCustomWord)currentWord);
            }

            foreach (IWord child in currentWord.Children)
            {
                GetCustomWordsInCommandRecursive(child, currentWord.AllWordType == AllWordTypes.LogicOperation);
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
