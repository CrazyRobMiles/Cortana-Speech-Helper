using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using ScaryFairground;
using AdventureEngine;
using AdventureDemo.Common;
using Windows.Phone.UI.Input;


// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace AdventureDemo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        public new static App Current
        {
            get { return (App)Application.Current; }
        }


        private Frame rootFrame;

        private TransitionCollection transitions;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        #region Voice Command Stuff

        private GameDesign adventureValue = null;

        public GameDesign Adventure
        {
            get
            {
                if (adventureValue == null)
                    adventureValue = new ScaryFairgroundGame();
                return adventureValue;
            }
        }

Type[] destinationFrameTypesValue = null;

Type[] destinationFrameTypes
{
    get
    {
        if (destinationFrameTypesValue == null)
            destinationFrameTypesValue = new Type[] {
            typeof(MainPage) };
        return destinationFrameTypesValue;
    }
}

        public Type GetVoiceCommandPageType(string targetPageName)
        {
            foreach (Type t in destinationFrameTypes)
            {
                string pathClass = targetPageName.Remove(targetPageName.Length - 5, 5);
                if (t.Name == pathClass)
                {
                    return t;
                }
            }
            return typeof(MainPage);
        }

        /// <summary>
        /// Sets up the game - called at the end of OnLaunched so it gets run frequently enough
        /// </summary>
        private async void setupVoiceCommands()
        {

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFile file = await folder.CreateFileAsync("AdventureCommands.xml", CreationCollisionOption.ReplaceExisting);

            using (Stream outputStream = await file.OpenStreamForWriteAsync())
            {
                TextWriter outputWriter = new StreamWriter(outputStream);

                string language = CultureInfo.CurrentCulture.Name;

                Adventure.SaveVoiceCommands(new string[] { language }, outputWriter);
            }

            StorageFile input = await folder.GetFileAsync("AdventureCommands.xml");

            await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(input);
        }


        protected override void OnActivated(IActivatedEventArgs e)
        {
            // Was the app activated by a voice command?
            if (e.Kind != Windows.ApplicationModel.Activation.ActivationKind.VoiceCommand)
            {
                return;
            }

            var commandArgs = e as Windows.ApplicationModel.Activation.VoiceCommandActivatedEventArgs;
            Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

            // The commandMode is either "voice" or "text", and it indicates how the voice command was entered by the user.
            // We should respect "text" mode by providing feedback in a silent form.
            string commandMode = this.SemanticInterpretation("commandMode", speechRecognitionResult);

            // If so, get the name of the voice command, the actual text spoken, and the value of Command/Navigate@Target.
            string voiceCommandName = speechRecognitionResult.RulePath[0];
            string textSpoken = speechRecognitionResult.Text;
            string navigationTarget = this.SemanticInterpretation("NavigationTarget", speechRecognitionResult);

            string navigationParameterString = string.Format("{0}|{1}", commandMode, textSpoken);

            Type navigateToPageType = GetVoiceCommandPageType(navigationTarget);

            this.EnsureRootFrame(e.PreviousExecutionState);

            if (!this.rootFrame.Navigate(navigateToPageType, navigationParameterString))
            {
                throw new Exception("Failed to create voice command page");
            }
        }


        private string SemanticInterpretation(string key, Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult)
        {
            if (speechRecognitionResult.SemanticInterpretation.Properties.ContainsKey(key))
            {
                return speechRecognitionResult.SemanticInterpretation.Properties[key][0];
            }
            else
            {
                return "unknown";
            }
        }

        #endregion

        /// <summary>
        /// Both the OnLaunched and OnActivated event handlers need to make sure the root frame has been created, so the common 
        /// code to do that is factored into this method and called from both.
        /// </summary>
        private async void EnsureRootFrame(ApplicationExecutionState previousExecutionState)
        {
            this.rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (this.rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                this.rootFrame = new Frame();

                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(this.rootFrame, "AppFrame");

                this.rootFrame.CacheSize = 1;

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Load state from previously suspended application
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = this.rootFrame;
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            setupVoiceCommands();  // set up the voice commands

            this.EnsureRootFrame(e.PreviousExecutionState);

            if (this.rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (this.rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in this.rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                this.rootFrame.ContentTransitions = null;
                this.rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!this.rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Save application state (and stop any background activity if applicable)
            await SuspensionManager.SaveAsync();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}