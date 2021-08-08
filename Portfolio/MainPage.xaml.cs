using Portfolio.Models;
using Portfolio.Pages;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Portfolio
{
    /// <summary>
    /// La page principale.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly List<(string Tag, string Title, Type Page)> _pages = new()
        {
            ("import", "Import", typeof(YahooFinanceHistorical)),
            ("companies", "Compagnies", typeof(CompaniesPage)),
            ("funds", "Fonds", typeof(FundsPage)),
            ("fundSearch", "Recherche de fonds", typeof(FindFundPage)),
            ("portfolios", "Portefeuilles", typeof(PortfoliosPage)),
            ("logs", "Log", typeof(LogPage)),
        };

        private readonly DBViewModel ViewModel;

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new DBViewModel();
            Window.Current.CoreWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(CoreWindow _, WindowSizeChangedEventArgs args)
        {
            Config.LaunchSize = args.Size;
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                _ = ContentFrame.Navigate(typeof(SettingsPage), ViewModel, args.RecommendedNavigationTransitionInfo);
                sender.Header = "Paramètres";
                return;
            }

            if (args.InvokedItemContainer != null)
            {
                string navItemTag = args.InvokedItemContainer.Tag.ToString();
                (string Tag, string Title, Type Page) item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                Type preNavPageType = ContentFrame.CurrentSourcePageType;

                if (!Equals(item.Page, preNavPageType))
                {
                    _ = ContentFrame.Navigate(item.Page, null, args.RecommendedNavigationTransitionInfo);
                    sender.Header = item.Title;
                }
            }
        }

        private void NavView_BackRequested(NavigationView _1, NavigationViewBackRequestedEventArgs _2)
        {
            if (ContentFrame.CanGoBack && (!NavView.IsPaneOpen ||
                (NavView.DisplayMode != NavigationViewDisplayMode.Compact
                && NavView.DisplayMode != NavigationViewDisplayMode.Minimal)))
            {
                ContentFrame.GoBack();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = DB.GetConnection();
            Log.AddLine("Lancement de Portfolio");
            ViewModel.DBOk = DB.Ok;
            if (DB.Ok)
            {
                NavView.Header = "Portfolio";
            }
            else
            {
                _ = ContentFrame.Navigate(typeof(SettingsPage), ViewModel);
                NavView.Header = "Connexion impossible à la base de données";
            }

        }
    }
}
