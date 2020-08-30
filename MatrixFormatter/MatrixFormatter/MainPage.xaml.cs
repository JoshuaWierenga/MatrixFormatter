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

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void CreateMatrix_OnClicked(object sender, EventArgs e)
        {
            int test = MatrixRows;
            int test2 = MatrixColumns;
        }
    }
}
