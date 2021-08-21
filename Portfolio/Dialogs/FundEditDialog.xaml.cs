using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Dialogs
{
    public sealed partial class FundEditDialog : ContentDialog
    {
        private readonly Fund _fund;
        private int _companyID;

        public FundEditDialog()
        {
            InitializeComponent();
        }

        public FundEditDialog(Fund fund)
        {
            InitializeComponent();
            _fund = fund;
            CompaniesComboBox.ItemsSource = CompanyRepository.Get("");
            if (fund.ID == 0)
            {
                NameTextBox.Text = "";
                CompaniesComboBox.SelectedItem = null;
                ISINTextBox.Text = "";
                YahooCodeTextBox.Text = "";
                CommentTextBox.Text = "";
                PrimaryButtonText = "Créer";
                MornigstarIDTextBox.Text = "";
                Title = "Ajouter un fond";
                IsPrimaryButtonEnabled = false;
                _companyID = 0;
            }
            else
            {
                NameTextBox.Text = fund.Name;
                foreach (Company c in CompaniesComboBox.Items)
                {
                    if (c.ID == fund.CompanyID)
                    {
                        CompaniesComboBox.SelectedItem = c;
                        _companyID = c.ID;
                        break;
                    }
                }
                ISINTextBox.Text = fund.ISIN ?? "";
                YahooCodeTextBox.Text = fund.YahooCode ?? "";
                CommentTextBox.Text = fund.Comment ?? "";
                MornigstarIDTextBox.Text = fund.MorningstarID ?? "";
                PrimaryButtonText = "Modifer";
                Title = "Modifier un fond";
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog _1, ContentDialogButtonClickEventArgs _2)
        {
            string comment = CommentTextBox.Text == "" ? null : CommentTextBox.Text;
            string yahooCode = YahooCodeTextBox.Text == "" ? null : YahooCodeTextBox.Text;
            string isinCode = ISINTextBox.Text == "" ? null : ISINTextBox.Text;
            string morningStarID = MornigstarIDTextBox.Text == "" ? null : MornigstarIDTextBox.Text;
            Fund fund = new(id: _fund.ID,
                companyId: _companyID,
                name: NameTextBox.Text,
                isin: isinCode,
                yahooCode: yahooCode,
                morningstarID: morningStarID,
                comment: comment);
            if (_fund.ID == 0)
            {
                FundRepository.Insert(fund);
            }
            else
            {
                FundRepository.Update(fund);
            }
        }

        private void NameTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckFields();
        }

        private void CompaniesComboBox_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            _companyID = CompaniesComboBox.SelectedItem is not null ? (CompaniesComboBox.SelectedItem as Company).ID : 0;
            CheckFields();
        }

        private void CheckFields()
        {
            IsPrimaryButtonEnabled = NameTextBox.Text != ""
                && _companyID != 0
                && (ISINTextBox.Text != "" || YahooCodeTextBox.Text != "" || MornigstarIDTextBox.Text != "");
        }

        private void ISINTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckFields();
        }

        private void YahooCodeTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckFields();
        }

        private void MornigstarIDTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckFields();
        }
    }
}
