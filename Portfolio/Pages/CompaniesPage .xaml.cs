using Portfolio.Dialogs;
using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using System.Threading.Tasks;
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
            ShowEditDialogAndHandle(company);
        }

        private void SearchBox_DataContextChanged(object _, DebounceSearchEventArgs e)
        {
            _search = e.Text;
            ViewModel.Fetch(_search);
        }

        private void AddCommand_ExecuteRequested(XamlUICommand _1, ExecuteRequestedEventArgs _2)
        {
            ShowEditDialogAndHandle(new Company());
        }

        private void AddButton_Click(object _1, RoutedEventArgs _2)
        {
            ShowEditDialogAndHandle(new Company());
        }

        private async void ShowEditDialogAndHandle(Company company)
        {
            CompanyEditDialog dialog = new(company);
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (DB.State == DBState.AlreadyExists)
                {
                    AlreadyExistsStackPannel.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    AlreadyExistsStackPannel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ViewModel.Fetch(_search);
                }
            }
        }

        private void CompaniesGridView_DoubleTapped(object _1, DoubleTappedRoutedEventArgs _2)
        {
            if (CompaniesGridView.SelectedItem is not null)
            {
                ShowEditDialogAndHandle(CompaniesGridView.SelectedItem as Company);
            }
        }
    }
}
