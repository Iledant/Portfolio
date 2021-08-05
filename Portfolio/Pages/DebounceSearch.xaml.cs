using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Pages
{
    public sealed partial class DebounceSearch : UserControl
    {
        public event EventHandler<DebounceSearchEventArgs> TextChanged;

        public DebounceSearch()
        {
            InitializeComponent();
        }

        private async void SearchText_Changed(object _1, TextChangedEventArgs _2)
        {
            async Task<bool> UserKeepsTyping()
            {
                string txt = SearchBox.Text;
                await Task.Delay(500);
                return txt != SearchBox.Text;
            }
            if (await UserKeepsTyping())
            {
                return;
            }

            TextChanged?.Invoke(this, new DebounceSearchEventArgs(SearchBox.Text));
        }
    }

    public class DebounceSearchEventArgs : EventArgs
    {
        public string Text { get; set; }

        public DebounceSearchEventArgs(string text) => Text = text;
    }
}
