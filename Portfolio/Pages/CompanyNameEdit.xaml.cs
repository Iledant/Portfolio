using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// Page d'édition d'une compagnie.
    /// </summary>
    public sealed partial class CompanyNameEdit : Page
    {
        private Company _company;

        public CompanyNameEdit()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Company)
            {
                _company = e.Parameter as Company;

                if (_company.ID == 0)
                {
                    Title.Text = "Créer une compagnie";
                    OkButton.Content = "Créer";
                    OkButton.IsEnabled = false;
                }
                else
                {
                    Title.Text = "Modifier la compagnie";
                    OkButton.Content = "Modifier";
                }
                NameBox.Text = _company.Name;
                CommentBox.Text = _company.Comment ?? "";
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

        private void CompanyEditPageCancelButton_Clicked(object _1, RoutedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(CompaniesPage));
        }

        private void CompanyEditPageOkButton_Clicked(object _1, RoutedEventArgs _2)
        {
            string comment = CommentBox.Text == "" ? null : CommentBox.Text;
            Company company = new(id: _company.ID, name: NameBox.Text, comment);
            if (_company.ID == 0)
                CompanyRepository.Insert(company);
            else
                CompanyRepository.Update(company);
            if (DB.State == DBState.AlreadyExists)
            {
                AlreadyExistTextError.Visibility = Visibility.Visible;
                return;
            }
            Frame.Navigate(typeof(CompaniesPage));
        }

    }
}
