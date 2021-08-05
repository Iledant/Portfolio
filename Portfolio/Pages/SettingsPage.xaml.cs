using Portfolio.Repositories;
using Portfolio.ViewModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de configuration de l'application.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private enum ConnexionMode { None, Pending, Success, Fail }
        private DBViewModel ViewModel;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            HostTextBox.Text = localSettings.Values["host"] as string ?? "";
            PortTextBox.Text = localSettings.Values["port"] as string ?? "";
            UsernameTextBox.Text = localSettings.Values["username"] as string ?? "";
            PasswordTextBox.Text = localSettings.Values["password"] as string ?? "";

            string logDirName = localSettings.Values["LogFileName"] as string ?? "";
            if (logDirName != "")
            {
                int fileNamePosition = logDirName.LastIndexOf("portfolio.log");
                if (fileNamePosition == -1)
                {
                    logDirName = "";
                } else
                {
                    logDirName = logDirName.Substring(0, fileNamePosition);
                }
            }
            LogDirNameTextBlock.Text = "Répertoire : " + logDirName;
            CheckValidateButtonIsEnabled();
            DisplayConnexionState(pending: false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is not null and DBViewModel)
            {
                ViewModel = e.Parameter as DBViewModel;
            }
            base.OnNavigatedTo(e);
        }

        private void CheckValidateButtonIsEnabled()
        {
            ValidateButton.IsEnabled = HostTextBox.Text != ""
                && PortTextBox.Text != ""
                && UsernameTextBox.Text != ""
                && PasswordTextBox.Text != "";
        }

        private void DisplayConnexionState(bool pending)
        {
            if (pending)
            {
                ConnexionPending.Visibility = Visibility.Visible;
                ConnexionFailed.Visibility = Visibility.Collapsed;
                ConnexionSucceed.Visibility = Visibility.Collapsed;
                return;
            }
            ConnexionPending.Visibility = Visibility.Collapsed;
            ConnexionFailed.Visibility = DB.Ok ? Visibility.Collapsed : Visibility.Visible;
            ConnexionSucceed.Visibility = DB.Ok ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ValidateButton_Click(object _1, RoutedEventArgs _2)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["host"] = HostTextBox.Text;
            localSettings.Values["port"] = PortTextBox.Text;
            localSettings.Values["username"] = UsernameTextBox.Text;
            localSettings.Values["password"] = PasswordTextBox.Text;
            DisplayConnexionState(pending: true);
            await Task.Delay(1);
            _ = DB.ChangeParamsAndCheckConnection();
            if (ViewModel != null)
            {
                ViewModel.DBOk = DB.Ok;
            }
            DisplayConnexionState(pending: false);
        }

        private void HostTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckValidateButtonIsEnabled();
        }

        private void PortTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckValidateButtonIsEnabled();
        }

        private void UsernameTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckValidateButtonIsEnabled();
        }

        private void PasswordTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckValidateButtonIsEnabled();
        }

        private async void LogDirNameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string logFileName = folder.Path + "\\portfolio.log";
                localSettings.Values["LogFileName"] = logFileName;
                LogDirNameTextBlock.Text = "Répertoire : " + logFileName;
            }
        }
    }
}
