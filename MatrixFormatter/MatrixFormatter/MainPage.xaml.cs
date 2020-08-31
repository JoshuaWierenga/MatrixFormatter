using System;
using Xamarin.Forms;

namespace MatrixFormatter
{
    public partial class MainPage : ContentPage
    {
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

        Grid matrix = new Grid();

        public MainPage()
        {
            InitializeComponent();
            MainLayout.Children.Add(matrix);
            BindingContext = this;
        }

        private void CreateMatrix_OnClicked(object sender, EventArgs e)
        {
            //Todo keep existing if possible
            matrix.RowDefinitions.Clear();
            matrix.ColumnDefinitions.Clear();
            matrix.Children.Clear();

            for (int i = 0; i < MatrixRows + 1; i++)
            {
                RowDefinition newRow = new RowDefinition();

                if (i < MatrixRows)
                {
                    newRow.Height = GridLength.Auto;
                    
                    matrix.ColumnDefinitions.Add(new ColumnDefinition());

                    for (int j = 0; j < MatrixColumns; j++)
                    {
                        matrix.Children.Add(new Entry(), i, i + 1, j, j + 1);
                    }
                }

                matrix.RowDefinitions.Add(newRow);
            }
        }
    }
}
