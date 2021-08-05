using Portfolio.ViewModel;
using Windows.UI.Xaml.Controls;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace Portfolio.Pages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class LogPage : Page
    {
        private readonly LogPageViewModel ViewModel;

        public LogPage()
        {
            InitializeComponent();
            ViewModel = new();
            ViewModel.GetLogLines();
        }
    }
}
