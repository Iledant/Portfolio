#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Portfolio
{
    public enum LogState { Error, Warning, Info }

    public class LogLine
    {
        public readonly DateTime Date;
        public readonly string Message;
        public readonly LogState State;

        private static readonly Brush ErrorBrush = new SolidColorBrush(Colors.Red);
        private static readonly Brush WarningBrush = new SolidColorBrush(Colors.Orange);
        private static readonly Brush InfoBrush = new SolidColorBrush(Colors.Green);

        public LogLine(DateTime date, string message, LogState state = LogState.Info)
        {
            Date = date;
            Message = message;
            State = state;
        }

        public Brush StateColor => State switch
        {
            LogState.Error => ErrorBrush,
            LogState.Warning => WarningBrush,
            LogState.Info => InfoBrush,
            _ => throw new NotFiniteNumberException()
        };

        public override string ToString()
        {
            string logState = State switch
            {
                LogState.Error => "E",
                LogState.Warning => "W",
                LogState.Info => "I",
                _ => throw new NotImplementedException()
            };
            return $"{Date:dd/MM/yyy HH:mm:ss.fff} [{logState}]: {Message}";
        }
    }

    public static class Log
    {

        private static string? _fileName;
        private static StreamWriter? _fs;

        public static bool Ok { get; private set; }
        public static List<LogLine> Lines { get; } = new();

        public static string? FileName
        {
            set
            {
                if (value is null)
                {
                    return;
                }
                _fileName = value;
                if (_fs is not null)
                {
                    _fs.Close();
                    _fs.Dispose();
                }
                try
                {
                    _fs = new(_fileName, append: true);
                    ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                    localSettings.Values["LogFileName"] = _fileName;
                }
                catch (UnauthorizedAccessException e)
                {
                    AddLine("Droits d'accès manquant pour le fichier log : " + e.Message, LogState.Error);
                    Ok = false;
                }
                catch (DirectoryNotFoundException e)
                {
                    AddLine("Chemin incorrect pour le fichier log : " + e.Message, LogState.Error);
                }
            }
        }

        static Log()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            FileName = localSettings.Values["LogFileName"].ToString();
        }

        public static void AddLine(string message, LogState state = LogState.Info)
        {
            LogLine line = new(DateTime.Now, message, state);
            Lines.Add(line);
            if (_fs is not null)
            {
                _fs.WriteLineAsync(line.ToString());
            }
        }

    }

}
