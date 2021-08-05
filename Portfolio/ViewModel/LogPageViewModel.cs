using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class LogPageViewModel : Bindable
    {
        private List<LogLine> _logLines;

        public List<LogLine> LogLines
        {
            get => _logLines;

            set
            {
                _logLines = value;
                OnPropertyChanged(nameof(LogLines));
            }
        }

        public void GetLogLines()
        {
            LogLines = Log.Lines;
        }
    }
}
