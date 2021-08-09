using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class LogViewModel : Bindable
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
    }
}
