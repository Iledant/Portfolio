using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page d'affichage des compagnies.
    /// </summary>
    public sealed partial class CompaniesPage : Page
    {
        private readonly CompaniesViewModel ViewModel;
        private string _search;

        public CompaniesPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            ViewModel.Fetch("");
        }

        private async void DeleteCommand_ExecuteRequested(XamlUICommand _, ExecuteRequestedEventArgs args)
        {
            if (args == null)
            {
                return;
            }
            DeleteDialog deleteDialog = new("la compagnie");

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Company company= args.Parameter as Company;
                ViewModel.Delete(company, _search);
            }
        }

        private void EditCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args == null)
            {
                return;
            }

            Company company = args.Parameter as Company;
            _ = Frame.Navigate(typeof(CompanyNameEdit), company);
        }


        private void SearchBox_DataContextChanged(object _, DebounceSearchEventArgs e)
        {
            _search = e.Text;
            ViewModel.Fetch(_search);
        }

        private void AddCommand_ExecuteRequested(XamlUICommand _1, ExecuteRequestedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(CompanyNameEdit), new Company());
        }

        private void AddButton_Click(object _1, RoutedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(CompanyNameEdit), new Company());
        }
    }
}
