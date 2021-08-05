using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Portfolio.Pages
{
    public class DeleteDialog
    {
        private readonly ContentDialog _dialog;

        public DeleteDialog(string titlePattern)
        {
            _dialog = new ContentDialog
            {
                Title = $"Supprimer {titlePattern} ?",
                Content = $"La suppression ne pourra pas être annulée",
                PrimaryButtonText = "Supprimer",
                CloseButtonText = "Annuler"
            };
        }

        public IAsyncOperation<ContentDialogResult> ShowAsync()
        {
            return _dialog.ShowAsync();
        }
    }
}
