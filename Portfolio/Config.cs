using Portfolio.Repositories;
using System;
using Windows.Foundation;
using Windows.Storage;

namespace Portfolio
{
    public static class Config
    {
        private static DBConnexionParams _dBConnexionParams;
        private static Size _launchSize;

        public static DBConnexionParams DBConnexionParams
        {
            get => _dBConnexionParams;
            set
            {
                if (value is null)
                {
                    return;
                }

                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["host"] = value.Host;
                localSettings.Values["port"] = value.Port;
                localSettings.Values["username"] = value.Username;
                localSettings.Values["password"] = value.Password;
                _dBConnexionParams = value;
            }
        }

        public static Size LaunchSize
        {
            get => _launchSize;
            set
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["lauchWidth"] = value.Width.ToString();
                localSettings.Values["lauchHeight"] = value.Height.ToString();
            }
        }

        public static void FetchLaunchSize()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string width = localSettings.Values["lauchWidth"] as string ?? "800";
            string height = localSettings.Values["lauchHeight"] as string ?? "600";

            try
            {
                _launchSize.Width = int.Parse(width);
                _launchSize.Height = int.Parse(height);
            }
            catch (Exception)
            {
                _launchSize = new Size(width: 800, height: 600);
            }
        }

        public static void FetchDBConnexionParams()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string host = localSettings.Values["host"] as string;
            string port = localSettings.Values["port"] as string;
            string username = localSettings.Values["username"] as string;
            string password = localSettings.Values["password"] as string;
            _dBConnexionParams = new DBConnexionParams(host: host, port: port, username: username, password: password);
        }
    }
}
