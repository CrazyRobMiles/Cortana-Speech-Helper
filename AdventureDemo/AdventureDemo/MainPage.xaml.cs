using AdventureDemo.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AdventureDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

protected async override void OnNavigatedTo(NavigationEventArgs e)
{
    await App.Current.Adventure.LoadStatus();

    string[] parameters = e.Parameter.ToString().Split('|');

    if (parameters.Length > 1)
    {
        string reply = App.Current.Adventure.PerformCommand(parameters[1]);

        if (parameters[0] != "text" && e.NavigationMode == NavigationMode.New)
        {
            var speechSynthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            var stream = await speechSynthesizer.SynthesizeTextToStreamAsync(reply);
            speechOutputMediaElement.SetSource(stream, stream.ContentType);
            speechOutputMediaElement.Play();
        }
        await App.Current.Adventure.SaveStatus();
    }

    AdventureTextBlock.Text = App.Current.Adventure.History.ToString();
}

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private async void TextEntryButton_Click(object sender, RoutedEventArgs e)
        {
            string reply = App.Current.Adventure.PerformCommand(CommandTextBox.Text);

            AdventureTextBlock.Text = App.Current.Adventure.History.ToString();

            await App.Current.Adventure.SaveStatus();
        }

    }
}
