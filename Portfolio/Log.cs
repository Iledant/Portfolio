#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;

namespace Portfolio
{
    public class LogLine
    {
        public readonly DateTime Date;
        public readonly string Message;

        public LogLine(DateTime date, string message)
        {
            Date = date;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Date:dd/MM/yyy HH:mm:ss.fff}: {Message}";
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
                catch (Exception e)
                {
                    AddLine("Impossible de créer le fichier de log " + e.ToString());
                    Ok = false;
                }

            }
        }

        static Log()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            FileName = localSettings.Values["LogFileName"].ToString();
        }

        public static void AddLine(string message)
        {
            LogLine line = new(DateTime.Now, message);
            Lines.Add(line);
            if (_fs is not null)
            {
                _fs.WriteLineAsync(line.ToString());
            }
        }

    }

}
