using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Portfolio.ViewModel
{
    public abstract class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string NullDateTimeDisplay(string prefix, DateTime? date)
        {
            if (date is not null)
            {
                DateTime d = (DateTime)date;
                return prefix + d.ToString("dd/mm/yy");
            }
            return prefix + "-";
        }

        public static string NullDoubleDisplay(string prefix, double? val)
        {
            if (val is not null)
            {
                double v = (double)val;
                return $"{prefix}{v:f}";
            }
            return "{prefix}-";
        }

        public static string NullIntDisplay(string prefix, int? val)
        {
            if (val is not null)
            {
                int v = (int)val;
                return prefix + v.ToString();
            }
            return prefix + "-";
        }
    }
}
