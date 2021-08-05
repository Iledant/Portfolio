using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page d'édition ou de création d'un portefeuille.
    /// </summary>
    public sealed partial class PortFolioEditPage : Page
    {
        private PortFolio _portFolio;

        public PortFolioEditPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PortFolio)
            {
                _portFolio = e.Parameter as PortFolio;

                if (_portFolio.ID == 0)
                {
                    Title.Text = "Créer un portefeuille";
                    OkButton.Content = "Créer";
                    OkButton.IsEnabled = false;
                }
                else
                {
                    Title.Text = "Modifier le portefeuille";
                    OkButton.Content = "Modifier";
                }
                NameBox.Text = _portFolio.Name;
                CommentBox.Text = _portFolio.Comment ?? "";
            }
            base.OnNavigatedTo(e);
        }

        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            _ = NameBox.Focus(FocusState.Programmatic);
        }

        private void NameBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            OkButton.IsEnabled = NameBox.Text != "";
        }

        private void PortFolioEditPageCancelButton_Clicked(object _1, RoutedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(CompaniesPage));
        }

        private void PortFolioEditPageOkButton_Clicked(object _1, RoutedEventArgs _2)
        {
            string comment = CommentBox.Text == "" ? null : CommentBox.Text;
            PortFolio portFolio = new(id: _portFolio.ID, name: NameBox.Text, comment);
            if (_portFolio.ID == 0)
                _ = PortfolioRepository.Insert(portFolio);
            else
                _ = PortfolioRepository.Update(portFolio);
            if (DB.State == DBState.AlreadyExists)
            {
                AlreadyExistTextError.Visibility = Visibility.Visible;
                return;
            }
            Frame.Navigate(typeof(PortfoliosPage));
        }

    }
}
