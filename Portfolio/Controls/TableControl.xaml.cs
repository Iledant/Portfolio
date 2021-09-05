#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Portfolio.Controls
{
    public interface ICellContent
    {
        public string GetText();
        public TextAlignment GetAlignment();
    }

    public class TableContent
    {
        public List<ICellContent> Headers;
        public List<List<ICellContent>> Cells;
        public List<ICellContent> BottomCells;

        public TableContent(List<ICellContent> headers, List<List<ICellContent>> cells, List<ICellContent> bottomCells)
        {
            if (headers.Count != bottomCells.Count)
            {
                throw new ArgumentException();
            }
            foreach (List<ICellContent> row in cells)
            {
                if (row.Count != headers.Count)
                {
                    throw new ArgumentException();
                }
            }

            Headers = headers;
            Cells = cells;
            BottomCells = bottomCells;
        }
    }

    public class HeaderSort
    {
        public int Index;
        public HeaderSortState State;
    }

    public sealed partial class TableControl : UserControl
    {
        #region private members
        private readonly CultureInfo _ci = new("fr-FR");
        private double _headerHeight;
        private double _headerAndCellHeight;
        private int _rowsCount;
        private int _pointeredRow = 0;
        private int _cellBeginIndex = 0;
        private SmallSortHeaderIcon? _selectedHeader = null;
        private HeaderSortState _sortState = HeaderSortState.Neutral;
        private List<SmallSortHeaderIcon> _sortIcons = new();
        #endregion

        #region constructor
        public TableControl()
        {
            InitializeComponent();
        }
        #endregion

        #region events
        public event EventHandler<HeaderSort>? SortClicked;
        #endregion

        #region property dependency
        public TableContent TableContent
        {
            get { return (TableContent)GetValue(TableContentProperty); }
            set
            {
                SetValue(TableContentProperty, value);
                GenerateTable();
            }
        }

        public static readonly DependencyProperty TableContentProperty =
            DependencyProperty.Register(nameof(TableContent),
                typeof(TableContent),
                typeof(TableControl),
                new PropertyMetadata(null));

        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set { SetValue(HeaderForegroundProperty, value); }
        }

        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register(nameof(HeaderForeground),
                typeof(Brush),
                typeof(TableControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush CellBackground
        {
            get => (Brush)GetValue(CellBackgroundProperty);
            set => SetValue(CellBackgroundProperty, value);
        }

        public static readonly DependencyProperty CellBackgroundProperty =
            DependencyProperty.Register(nameof(CellBackground),
                typeof(Brush),
                typeof(TableControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 10, 16, 16))));

        public Brush CellForeground
        {
            get => (Brush)GetValue(CellForegroundProperty);
            set => SetValue(CellForegroundProperty, value);
        }

        public static readonly DependencyProperty CellForegroundProperty =
            DependencyProperty.Register(nameof(CellForeground),
                typeof(Brush),
                typeof(TableControl),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush HeaderCellBackground
        {
            get => (Brush)GetValue(HeaderCellBackgroundProperty);
            set => SetValue(HeaderCellBackgroundProperty, value);
        }

        public static readonly DependencyProperty HeaderCellBackgroundProperty =
            DependencyProperty.Register(nameof(HeaderCellBackground),
                typeof(Brush),
                typeof(TableControl),
                new PropertyMetadata(new SolidColorBrush(Colors.CadetBlue)));

        public Brush HeaderCellForeground
        {
            get => (Brush)GetValue(HeaderCellForegroundProperty);
            set => SetValue(HeaderCellForegroundProperty, value);
        }

        public static readonly DependencyProperty HeaderCellForegroundProperty =
            DependencyProperty.Register(nameof(HeaderCellForeground),
                typeof(Brush),
                typeof(TableControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush PointeredCellBackground
        {
            get => (Brush)GetValue(PointeredCellBackgroundProperty);
            set => SetValue(PointeredCellBackgroundProperty, value);
        }

        public static readonly DependencyProperty PointeredCellBackgroundProperty =
            DependencyProperty.Register(nameof(PointeredCellBackground),
                typeof(Brush),
                typeof(TableControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 19, 31, 32))));

        public Thickness CellPadding
        {
            get => (Thickness)GetValue(CellPaddingProperty);
            set => SetValue(CellPaddingProperty, value);
        }

        public static readonly DependencyProperty CellPaddingProperty =
            DependencyProperty.Register(nameof(CellPadding),
                typeof(int),
                typeof(TableControl),
                new PropertyMetadata(new Thickness(6)));
        #endregion

        #region methods
        private void Table_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Table.Children is null)
            {
                return;
            }

            Pointer ptr = e.Pointer;
            if (ptr.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PointerPoint point = e.GetCurrentPoint(Table);
                if ((point.Position.Y < _headerHeight || point.Position.Y > _headerAndCellHeight))
                {
                    if (_pointeredRow != 0)
                    {
                        ClearPointeredRow();
                        _pointeredRow = 0;
                    }
                }
                else
                {
                    int row = (int)(point.Position.Y * _rowsCount / _headerAndCellHeight);
                    if (row != _pointeredRow)
                    {
                        ClearPointeredRow();
                        if (row > 0)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                Border border = (Border)(Table.Children[6 * row + i]);
                                border.Background = PointeredCellBackground;
                            }
                            _pointeredRow = row;
                        }
                    }
                }

            }
            e.Handled = true;
        }

        private void Table_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ClearPointeredRow();
            _pointeredRow = 0;
            e.Handled = true;
        }
        #endregion

        #region private methods
        private void AddHeaders()
        {
            for (int i = 0; i < TableContent.Headers.Count; i++)
            {
                ColumnDefinition columnDefinition = new();
                columnDefinition.Width = GridLength.Auto;
                Table.ColumnDefinitions.Add(columnDefinition);
            }

            for (int i = 0; i < TableContent.Headers.Count; i++)
            {
                Border border = new();
                border.Background = HeaderCellBackground;
                border.Padding = CellPadding;
                Grid.SetColumn(border, i);
                StackPanel stackPanel = new();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.Spacing = 6;
                TextBlock textBlock = new();
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.Foreground = HeaderForeground;
                textBlock.Text = TableContent.Headers[i].GetText();
                SmallSortHeaderIcon sortIcon = new();
                sortIcon.Clicked += SortIcon_Clicked;
                _sortIcons.Add(sortIcon);
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(sortIcon);
                border.Child = stackPanel;
                Table.Children.Add(border);
            }
            Table.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _headerHeight = Table.DesiredSize.Height;
        }

        private void SortIcon_Clicked(object sender, EventArgs e)
        {
            if (sender is not SmallSortHeaderIcon)
            {
                return;
            }
            SmallSortHeaderIcon selected = (SmallSortHeaderIcon)sender;

            if (selected != _selectedHeader)
            {
                if (_selectedHeader is not null)
                {
                    _selectedHeader.State = HeaderSortState.Neutral;
                }
                _selectedHeader = selected;
                _sortState = HeaderSortState.Ascending;
            }
            else
            {
                _sortState = _sortState switch
                {
                    HeaderSortState.Ascending => HeaderSortState.Descending,
                    HeaderSortState.Descending => HeaderSortState.Neutral,
                    HeaderSortState.Neutral => HeaderSortState.Ascending,
                    _ => throw new NotImplementedException()
                };
            }
            selected.State = _sortState;
            if (SortClicked is not null)
            {
                SortClicked.Invoke(this, new HeaderSort { Index = _sortIcons.IndexOf(selected), State = _sortState });
                UpdateCellTexts();
            }
        }


        private void GenerateTable()
        {
            for (int i = 0; i < TableContent.Cells.Count + 2; i++)
            {
                Table.RowDefinitions.Add(new());
            }
            AddHeaders();

            _cellBeginIndex = Table.Children.Count;
            for (int i = 0; i < TableContent.Cells.Count; i++)
            {
                for (int j = 0; j < TableContent.Cells[i].Count; j++)
                {
                    AddCell(TableContent.Cells[i][j].GetText(), TableContent.Cells[i][j].GetAlignment(), j, i + 1);

                }
            }

            Table.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _headerAndCellHeight = Table.DesiredSize.Height;
            _rowsCount = TableContent.Cells.Count + 1;

            for (int i = 0; i < TableContent.BottomCells.Count; i++)
            {
                AddCell(TableContent.BottomCells[i].GetText(),
                    TableContent.BottomCells[i].GetAlignment(),
                    i,
                    _rowsCount + 1,
                    HeaderCellBackground,
                    HeaderCellForeground);
            }
        }

        private void AddCell(string text, TextAlignment alignment, int column, int row, Brush? background = null, Brush? foreground = null)
        {
            TextBlock textblock = new();
            textblock.Text = text;
            textblock.Foreground = foreground ?? CellForeground;
            textblock.TextAlignment = alignment;
            Border border = new();
            border.Child = textblock;
            border.Padding = CellPadding;
            border.Background = background ?? CellBackground;
            Table.Children.Add(border);
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);
        }

        private void UpdateCellTexts()
        {
            if (Table.Children is null)
            {
                return;
            }

            Border border;
            TextBlock textblock;
            for (int i = 0; i < TableContent.Cells.Count; i++)
            {
                for (int j = 0; j < TableContent.Headers.Count; j++)
                {
                    border = (Border)Table.Children[_cellBeginIndex + (i * 6) + j];
                    textblock = (TextBlock)border.Child;
                    textblock.Text = TableContent.Cells[i][j].GetText();
                }
            }
        }

        private void ClearPointeredRow()
        {
            if (_pointeredRow == 0 || Table.Children is null)
            {
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                ((Border)Table.Children[6 * _pointeredRow + i]).Background = CellBackground;
            }
        }
        #endregion
    }
}
