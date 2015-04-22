using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AdventureEngine;
using Windows.Storage;
using System.Threading.Tasks;

namespace ScaryFairground
{

    /// <summary>
    /// Not much of a game, but it should show you how the framework fits together.
    /// 
    /// Version 1.0
    /// Rob Miles April 2015
    /// 
    /// </summary>
    public class ScaryFairgroundGame : GameDesign
    {
        #region Game Commmands

        string Look(string option)
        {
            return "You are in a Scary Fairground. Ooh look, a clown with a chainsaw.";
        }

        string Inventory(string option)
        {
            return "Inventory";
        }

        string Move(string option)
        {
            return "There is no way to travel " + option + ". Yet.";
        }

        string Take(string option)
        {
            return "Taking " + option;
        }

        string Use(string option)
        {
            return "Using " + option;
        }

        public override string InvalidCommand(string option)
        {
            return "I don't know how to do that.";
        }

        #endregion

        async public override Task<bool> SaveStatus()
        {
            return await base.SaveStatus();
        }

        async override public Task<bool> LoadStatus()
        {
            return await base.LoadStatus();
        }

        public ScaryFairgroundGame()
        {
            Name = "Scary Fairground";
            VoiceCommandPrefix = "Fairground";
            VoiceCommandExample = "Fairground";

            VoiceCommands.Add(new VoiceCommandNoOptions(
                new string[] { "Inventory" },                   // command names
                "MainPage.xaml",                                // page to navigate to
                new VoiceCommandDespatcherDelegate(Inventory),  // method to call 
                "Checking inventory"));                         // feedback

            VoiceCommands.Add(new VoiceCommandNoOptions(
                new string[] { "Look", "View" },           // command names
                "MainPage.xaml",                           // page to navigate to
                new VoiceCommandDespatcherDelegate(Look),  // method to call
                "Looking"));                               // spoken feedback

            VoiceCommands.Add(new VoiceCommandWithOptions(
                new string[] { "Move", "Go" },             // command names
                "MainPage.xaml",                           // page to navigate to
                new VoiceCommandDespatcherDelegate(Move),    // method to call
                "Going",                                   // spoken feedback
                "direction",                               // phraselist name
                new string[] { "North", "South", "East",   // phraselist options
            "West", "Up", "Down" }));

            VoiceCommands.Add(new VoiceCommandWithOptions(
                new string[] { "Take", "Pickup", "Grab" },
                "MainPage.xaml",
                new VoiceCommandDespatcherDelegate(Take),
                "Taking",
                "takeobject",
                new string[] { "wand", "shotgun", "chicken", "cheese" }));

            VoiceCommands.Add(new VoiceCommandWithOptions(
                new string[] { "Use" },
                "MainPage.xaml",
                new VoiceCommandDespatcherDelegate(Use),
                "Using",
                "useobject",
                new string[] { "wand", "shotgun", "chicken", "cheese" }));
        }
    }
}
