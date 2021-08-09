﻿using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class DBViewModel : Bindable
    {
        private bool _dbOk = false;
        private List<LogLine> _logLines;

        public bool DBOk
        {
            get => _dbOk;
            set
            {
                _dbOk = value;
                OnPropertyChanged(nameof(DBOk));
            }
        }

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
