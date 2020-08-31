using System;
using Xamarin.Forms;

namespace MatrixFormatter
{
    public partial class MainPage : ContentPage
    {
        //todo Change default to be positive
        //todo Ensure input is a positive integer
        public static readonly BindableProperty MatrixRowsProperty =
            BindableProperty.Create(
                propertyName: "MatrixRows",
                returnType: typeof(string),
                declaringType: typeof(MainPage),
                "0",
                BindingMode.OneWayToSource);

        public static readonly BindableProperty MatrixColumnsProperty =
            BindableProperty.Create(
                propertyName: "MatrixColumns",
                returnType: typeof(string),
                declaringType: typeof(MainPage),
                "0",
                BindingMode.OneWayToSource);

        public int MatrixRows
        {
            get => int.Parse((string)GetValue(MatrixRowsProperty));
            set => SetValue(MatrixRowsProperty, value);
        }
        public int MatrixColumns
        {
            get => int.Parse((string)GetValue(MatrixColumnsProperty));
            set => SetValue(MatrixColumnsProperty, value);
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void CreateMatrix_OnClicked(object sender, EventArgs e)
        {
            if (MatrixRows == 0 || MatrixColumns == 0 || 
                (MatrixRows == MatrixGrid.RowDefinitions.Count && MatrixColumns == MatrixGrid.ColumnDefinitions.Count))
            {
                return;
            }

            View[,] cells = null;

            RowDefinition newRow = new RowDefinition
            {
                Height = GridLength.Auto
            };
            ColumnDefinition newColumn = new ColumnDefinition();

            if (MatrixGrid.RowDefinitions.Count != 0)
            {
                 cells = new View[Math.Min(MatrixRows, MatrixGrid.RowDefinitions.Count), Math.Min(MatrixColumns, MatrixGrid.ColumnDefinitions.Count)];

                for (int i = 0; i < cells.GetLength(0); i++)
                {
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        cells[i, j] = MatrixGrid.Children[i * MatrixGrid.ColumnDefinitions.Count + j];
                    }
                }
            }
            
            //Todo figure out how to edit grid in place despite its single dimensional array
            MatrixGrid.Children.Clear();

            for (int i = 0; i < Math.Max(MatrixRows, MatrixGrid.RowDefinitions.Count); i++)
            {
                if (i < MatrixRows)
                {
                    if (MatrixGrid.RowDefinitions.Count < i + 1)
                    {
                        MatrixGrid.RowDefinitions.Add(newRow);
                    }

                    for (int j = 0; j < Math.Max(MatrixColumns, MatrixGrid.ColumnDefinitions.Count); j++)
                    {
                        if (j < MatrixColumns)
                        {
                            if (MatrixGrid.ColumnDefinitions.Count < j + 1)
                            {
                                MatrixGrid.ColumnDefinitions.Add(newColumn);
                            }

                            if (i < cells?.GetLength(0) && j < cells.GetLength(1))
                            {
                                MatrixGrid.Children.Add(cells[i, j], j, j + 1, i, i + 1);
                            }
                            else
                            {
                                MatrixGrid.Children.Add(new Entry(), j, j + 1, i, i + 1);
                            }
                        }
                        else
                        {
                            MatrixGrid.ColumnDefinitions.RemoveAt(i);
                        }
                    }
                }
                //Todo fix this, it is currently not being used
                else if (i == MatrixRows && i >= MatrixGrid.RowDefinitions.Count)
                {
                    MatrixGrid.RowDefinitions.Add(new RowDefinition());
                }
                else
                {
                    MatrixGrid.RowDefinitions.RemoveAt(i);
                }
            }
        }
    }
}
