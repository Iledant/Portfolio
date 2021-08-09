using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class DBViewModel : Bindable
    {
        private bool _dbOk = false;

        public bool DBOk
        {
            get => _dbOk;
            set
            {
                _dbOk = value;
                OnPropertyChanged(nameof(DBOk));
            }
        }
    }
}
