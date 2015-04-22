using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace AdventureEngine
{

    /// <summary>
    /// A helper class for creating adventures that use single and two word commands.
    /// 
    /// Version 1.0
    /// Rob Miles
    /// April 2015
    /// </summary>

    public static class Config
    {
        public static XNamespace Namespace = "http://schemas.microsoft.com/voicecommands/1.1";

    }

    public delegate string VoiceCommandDespatcherDelegate(string option);

    public abstract class GameCommand
    {
        public VoiceCommandDespatcherDelegate ExecuteDelegate = null;

        public string[] CommandNames { get; set; }

        public string CommandFeedback { get; set; }

        public string NavigatePath { get; set; }

        public abstract void AddVoiceCommandXML(XElement destination);
        public abstract void AddVoicePhraseXML(XElement destination);

        public GameCommand(string[] names, string path, VoiceCommandDespatcherDelegate execute, string feedback)
        {
            CommandNames = names;
            NavigatePath = path;
            ExecuteDelegate = execute;
            CommandFeedback = feedback;
        }
    }

    public class VoiceCommandNoOptions : GameCommand
    {

        public override void AddVoiceCommandXML(XElement destination)
        {
            XElement Command = new XElement(Config.Namespace + "Command",
                new XAttribute("Name", CommandNames[0]),
                new XElement(Config.Namespace + "Example", CommandNames[0]));
            foreach (string commandName in CommandNames)
            {
                Command.Add(new XElement(Config.Namespace + "ListenFor", commandName));
            }
            Command.Add(new XElement(Config.Namespace + "Feedback", CommandFeedback));
            Command.Add(new XElement(Config.Namespace + "Navigate",
                new XAttribute("Target", NavigatePath)));
            destination.Add(Command);
        }

        public override void AddVoicePhraseXML(XElement destination)
        {
        }

        public VoiceCommandNoOptions(string[] names, string path, VoiceCommandDespatcherDelegate execute, string feedback)
            : base(names, path, execute, feedback)
        {
        }

    }

    public class VoiceCommandWithOptions : GameCommand
    {

        public string CommandOptionName { get; set; }

        public string[] CommandOptions { get; set; }

        public override void AddVoiceCommandXML(XElement destination)
        {
            XElement Command = new XElement(Config.Namespace + "Command",
                new XAttribute("Name", CommandNames[0]),
                new XElement(Config.Namespace + "Example", CommandNames[0] + " " + CommandOptions[0]));
            foreach (string commandName in CommandNames)
            {
                string commandString = commandName + " {" + CommandOptionName + "}";
                Command.Add(new XElement(Config.Namespace + "ListenFor", commandString));
            }
            Command.Add(new XElement(Config.Namespace + "Feedback", CommandFeedback + " {" + CommandOptionName + "}"));
            Command.Add(new XElement(Config.Namespace + "Navigate",
                new XAttribute("Target", NavigatePath)));
            destination.Add(Command);
        }

        public override void AddVoicePhraseXML(XElement destination)
        {
            XElement PhraseList = new XElement(Config.Namespace + "PhraseList",
                new XAttribute("Label", CommandOptionName));
            foreach (string commandOption in CommandOptions)
            {
                PhraseList.Add(new XElement(Config.Namespace + "Item", commandOption));
            }
            destination.Add(PhraseList);
        }

        public VoiceCommandWithOptions(string[] names, string path, VoiceCommandDespatcherDelegate execute, string feedback, string optionName, string[] options)
            : base(names, path, execute, feedback)
        {
            CommandOptionName = optionName;
            CommandOptions = options;
        }
    }

    public abstract class GameDesign
    {
        public string Name { get; set; }

        public string VoiceCommandPrefix { get; set; }

        public string VoiceCommandExample { get; set; }

        public List<GameCommand> VoiceCommands = new List<GameCommand>();

        public void SaveVoiceCommands(string[] languages, TextWriter writer)
        {
            XElement VoiceCommandElement = new XElement(Config.Namespace + "VoiceCommands");

            foreach (string language in languages)
            {
                XElement CommandSet = new XElement(Config.Namespace + "CommandSet",
                        new XAttribute(XNamespace.Xml + "lang", language));

                CommandSet.Add(new XElement(Config.Namespace + "CommandPrefix", VoiceCommandPrefix));
                CommandSet.Add(new XElement(Config.Namespace + "Example", VoiceCommandExample));

                foreach (GameCommand command in VoiceCommands)
                {
                    command.AddVoiceCommandXML(CommandSet);
                }

                foreach (GameCommand command in VoiceCommands)
                {
                    command.AddVoicePhraseXML(CommandSet);
                }

                VoiceCommandElement.Add(CommandSet);
            }

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                VoiceCommandElement);

            doc.Save(writer);
        }


        public GameCommand FindCommand(string commandName)
        {
            commandName = commandName.ToUpper();

            foreach ( GameCommand command in VoiceCommands )
            {
                foreach (string name in command.CommandNames)
                    if (name.ToUpper() == commandName)
                        return command;
            }
            return null;
        }

        public VoiceCommandDespatcherDelegate FindCommandDelegate(string commandText)
        {
            GameCommand command = FindCommand(commandText);

            if (command != null)
            {
                return command.ExecuteDelegate;
            }

            return null;
        }

        public string PerformCommand(string command)
        {
            string result = "";

            string[] commands = command.Split(new char[] { ' ' });
 
            VoiceCommandDespatcherDelegate processor = FindCommandDelegate(commands[0]);

            if (processor != null)
            {
                if (commands.Length == 1)
                    result = processor("");
                else
                    result = processor(commands[1]);
            }
            else
                result = InvalidCommand(command);

            History.AppendLine(command);
            History.AppendLine(result);

            return result;
        }

        abstract public string InvalidCommand(string commandText);


        static string HistoryFile = "CommandHistory.txt";

        public StringBuilder History = new StringBuilder();

        virtual async public Task<bool> SaveStatus()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(HistoryFile, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(file, History.ToString());
            return true;
        }

        virtual async public Task<bool> LoadStatus()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile file = await folder.GetFileAsync(HistoryFile);
                string loadedInteractions = await Windows.Storage.FileIO.ReadTextAsync(file);
                History = new StringBuilder(loadedInteractions);
            }
            catch (FileNotFoundException)
            {
                History = new StringBuilder();
            }
            return true;
        }

        public GameDesign()
        {
        }
    }
}
