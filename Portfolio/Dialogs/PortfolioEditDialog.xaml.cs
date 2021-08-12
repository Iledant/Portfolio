using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml.Controls;

namespace Portfolio.Dialogs
{
    public sealed partial class PortfolioEditDialog : ContentDialog
    {
        private readonly PortFolio _portfolio;

        public PortfolioEditDialog()
        {
            InitializeComponent();
        }

        public PortfolioEditDialog(PortFolio portfolio)
        {
            InitializeComponent();
            _portfolio = portfolio;
            if (portfolio.ID == 0)
            {
                NameTextBox.Text = "";
                CommentTextBox.Text = "";
                Title = "Créer un portefeuille";
                PrimaryButtonText = "Créer";
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                NameTextBox.Text = portfolio.Name;
                CommentTextBox.Text = portfolio.Comment ?? "";
                Title = "Modifer un portefeuille";
                PrimaryButtonText = "Modifier";
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog _1, ContentDialogButtonClickEventArgs _2)
        {
            string comment = CommentTextBox.Text == "" ? null : CommentTextBox.Text;
            PortFolio portfolio = new(id: _portfolio.ID, name: NameTextBox.Text, comment: comment);
            if (_portfolio.ID == 0)
            {
                PortfolioRepository.Insert(portfolio);
            }
            else
            {
                PortfolioRepository.Update(portfolio);
            }
        }

        private void NameTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            IsPrimaryButtonEnabled = NameTextBox.Text != "";
        }
    }
}
