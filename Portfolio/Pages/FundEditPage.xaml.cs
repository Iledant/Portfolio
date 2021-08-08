using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de création ou de modification d'un fond.
    /// </summary>
    public sealed partial class FundEditPage : Page
    {
        private Fund _fund;
        private Company _company;
        private readonly FundViewModel ViewModel;

        public FundEditPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Fund)
            {
                _fund = e.Parameter as Fund;

                if (_fund.ID == 0)
                {
                    Title.Text = "Créer un fond";
                    OkButton.Content = "Créer";
                    _company = new();
                }
                else
                {
                    Title.Text = "Modifier le fond";
                    OkButton.Content = "Modifier";
                    _company = new(id: _fund.CompanyID, name: _fund.CompanyName);
                    FetchFundDatas();
                }
                NameBox.Text = _fund.Name;
                CommentBox.Text = _fund.Comment ?? "";
                CompanyName.Text = _company.Name;
                IsinBox.Text = _fund.ISIN ?? "";
                YahooCodeBox.Text = _fund.YahooCode ?? "";
            }
            base.OnNavigatedTo(e);
            CheckOkButtonIsEnabled();
        }

        private void FetchFundDatas()
        {
            if (_fund.ID == 0)
            {
                return;
            }

            ViewModel.FetchFundDatas(_fund.ID);
        }

        private void NameBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckOkButtonIsEnabled();
        }

        private void CancelButton_Click(object _1, RoutedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(FundsPage));
        }

        private void CheckOkButtonIsEnabled()
        {
            OkButton.IsEnabled = _company.ID != 0
                && NameBox.Text != ""
                && (IsinBox.Text != "" || YahooCodeBox.Text != "");
        }

        private async void CompanyName_TextChanged(AutoSuggestBox _, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                async Task<bool> UserKeepsTyping()
                {
                    string txt = CompanyName.Text;
                    await Task.Delay(500);
                    return txt != CompanyName.Text;
                }
                if (await UserKeepsTyping())
                {
                    return;
                }

                List<Company> suggestions = CompanyRepository.GetSuggestions(CompanyName.Text);
                CompanyName.ItemsSource = suggestions;
            }
        }

        private void CompanyName_SuggestionChosen(AutoSuggestBox _, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Company)
            {
                CompanyName.Text = (args.SelectedItem as Company).Name;
            }
        }

        private void CompanyName_QuerySubmitted(AutoSuggestBox _1, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is not null and Company)
            {
                _company = args.ChosenSuggestion as Company;
                CompanyName.Text = _company.Name;
                CheckOkButtonIsEnabled();
            }
        }

        private void CompanyName_LostFocus(object _1, RoutedEventArgs _2)
        {
            CompanyName.Text = (_company is not null && _company.ID != 0) ? _company.Name : "";
        }

        private void OkButton_Click(object _1, RoutedEventArgs _2)
        {
            Fund fund = new(id: _fund.ID,
                name: NameBox.Text,
                isin: IsinBox.Text != "" ? IsinBox.Text : "",
                yahooCode: YahooCodeBox.Text != "" ? YahooCodeBox.Text : "",
                comment: CommentBox.Text != "" ? CommentBox.Text : "",
                companyId: _company.ID,
                companyName: _company.Name);
            if (_fund.ID == 0)
            {
                _ = FundRepository.Insert(fund);
            }
            else
            {
                _ = FundRepository.Update(fund);
            }
            _ = Frame.Navigate(typeof(FundsPage));
        }

        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            _ = NameBox.Focus(FocusState.Programmatic);
        }

        private void IsinBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckOkButtonIsEnabled();
        }

        private void YahooCodeBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            CheckOkButtonIsEnabled();
        }

    }
}
