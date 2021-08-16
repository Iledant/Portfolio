using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Portfolio.Controls
{
    public enum HeaderSortState { Ascending, Descending, Neutral }

    public sealed partial class SmallSortHeaderIcon : UserControl
    {
        public event EventHandler Clicked;

        public Brush ForegroundPointeredBrush
        {
            get => (Brush)GetValue(ForegroundPointeredBrushProperty);
            set => SetValue(ForegroundPointeredBrushProperty, value);
        }

        public static readonly DependencyProperty ForegroundPointeredBrushProperty =
            DependencyProperty.Register(nameof(ForegroundPointeredBrush),
                typeof(Brush),
                typeof(SmallSortHeaderIcon),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush ForegroundBrush
        {
            get => (Brush)GetValue(ForegroundBrushProperty);
            set => SetValue(ForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty ForegroundBrushProperty =
            DependencyProperty.Register(nameof(ForegroundBrush),
                typeof(Brush),
                typeof(SmallSortHeaderIcon),
                new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public HeaderSortState State
        {
            get { return (HeaderSortState)GetValue(StateProperty); }
            set
            {
                SetValue(StateProperty, value);
                UpdateIcon();
            }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State),
                typeof(HeaderSortState),
                typeof(SmallSortHeaderIcon),
                new PropertyMetadata(HeaderSortState.Neutral));

        public SmallSortHeaderIcon()
        {
            InitializeComponent();
        }

        private void Icon_PointerEntered(object _1, Windows.UI.Xaml.Input.PointerRoutedEventArgs _2)
        {
            Icon.Foreground = ForegroundPointeredBrush;
        }

        private void Icon_PointerExited(object _1, Windows.UI.Xaml.Input.PointerRoutedEventArgs _2)
        {
            Icon.Foreground = ForegroundBrush;
        }

        private void Icon_Tapped(object _1, Windows.UI.Xaml.Input.TappedRoutedEventArgs _2)
        {
            Clicked?.Invoke(this, new EventArgs());
        }

        private void UpdateIcon()
        {
            Icon.Glyph = State switch
            {
                HeaderSortState.Neutral => "\uE738",
                HeaderSortState.Ascending => "\uE96D",
                HeaderSortState.Descending => "\uE96E",
                _ => throw new NotImplementedException()
            };
        }
    }
}
