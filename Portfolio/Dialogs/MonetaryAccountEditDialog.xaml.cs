using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Dialogs
{
    public sealed partial class MonetaryAccountEditDialog : ContentDialog
    {
        private readonly MonetaryAccount _account;

        public MonetaryAccountEditDialog()
        {
            InitializeComponent();
        }

        public MonetaryAccountEditDialog(MonetaryAccount account)
        {
            InitializeComponent();
            _account = account;
            if (_account.ID == 0)
            {
                Title = "Nouveau fond monétaire";
                PrimaryButtonText = "Créer";
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                Title = "Modifier le fond monétaire";
                PrimaryButtonText = "Modifier";
                IsPrimaryButtonEnabled = true;
            }
            NameTextBox.Text = _account.Name;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog _1, ContentDialogButtonClickEventArgs _2)
        {
            MonetaryAccount account = new(id: _account.ID, name: NameTextBox.Text, portfolioID: _account.PortfolioID);
            if (_account.ID == 0)
            {
                MonetaryAccountRepository.Insert(account);
            }
            else
            {
                MonetaryAccountRepository.Update(account);
            }
        }

        private void NameTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            IsPrimaryButtonEnabled = NameTextBox.Text != "";
        }
    }
}
