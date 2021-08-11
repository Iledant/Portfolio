using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml.Controls;

namespace Portfolio.Dialogs
{
    public sealed partial class CompanyEditDialog : ContentDialog
    {
        private readonly Company _company;

        public CompanyEditDialog()
        {
            InitializeComponent();
        }

        public CompanyEditDialog(Company company)
        {
            InitializeComponent();
            _company = company;
            if (company.ID == 0)
            {
                NameTextBox.Text = "";
                CommentTextBox.Text = "";
                Title = "Nouvelle compagnie";
                PrimaryButtonText = "Créer";
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                NameTextBox.Text = company.Name;
                CommentTextBox.Text = company.Comment ?? "";
                Title = "Modifier la compagnie";
                PrimaryButtonText = "Modifier";
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog _1, ContentDialogButtonClickEventArgs _2)
        {
            string comment = CommentTextBox.Text == "" ? null : CommentTextBox.Text;
            Company company = new(id: _company.ID, name: NameTextBox.Text, comment);
            if (_company.ID == 0)
            {
                CompanyRepository.Insert(company);
            }
            else
            {
                CompanyRepository.Update(company);
            }
        }

        private void NameTextBox_TextChanged(object _1, Windows.UI.Xaml.RoutedEventArgs _2)
        {
            IsPrimaryButtonEnabled = NameTextBox.Text != "";
        }
    }
}
