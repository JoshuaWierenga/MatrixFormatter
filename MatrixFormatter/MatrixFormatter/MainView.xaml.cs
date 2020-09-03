using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MatrixFormatter
{
    public partial class MainView : ContentPage
    {
        private readonly MainViewModel _viewModel = new MainViewModel();

        public MainView()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        private void ShowMatrixButtons()
        {
            ToggleCellsButton.IsVisible = true;
            ExportButton.IsVisible = true;
        }

        private void CreateMatrix_OnClicked(object sender, EventArgs e)
        {
            _viewModel.CreateMatrix(MatrixGrid);
            ShowMatrixButtons();
        }

        private void CreateIdentityMatrix_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.MatrixRows != _viewModel.MatrixColumns)
            {
                DependencyService.Get<IMessageToast>().DisplayToast("Matrix must be square.");
                return;
            }

            _viewModel.CreateMatrix(MatrixGrid);

            //todo Make part of CreateMatrix?
            foreach (BorderEntry view in MatrixGrid.Children)
            {
                view.Text = Grid.GetRow(view) == Grid.GetColumn(view) ? "1" : "0";
            }

            ShowMatrixButtons();
        }

        private void ToggleCells_OnClicked(object sender, EventArgs e)
        {
            foreach (BorderEntry view in MatrixGrid.Children)
            {
                view.IsReadOnly = !view.IsReadOnly;
                view.IsBorderVisible = !view.IsBorderVisible;
            }

            Button toggleButton = (Button)sender;
            toggleButton.Text = toggleButton.Text == "Hide Matrix Cells" ? "Show Matrix Cells" : "Hide Matrix Cells";
        }

        private async void Export_OnClicked(object sender, EventArgs e)
        {
            string matrixString = _viewModel.ExportMatrix(MatrixGrid);

            //todo Determine how to use mathml clipboard type to allow pasting into onenote within a equation
            await Clipboard.SetTextAsync(matrixString);
            DependencyService.Get<IMessageToast>().DisplayToast("Copied to Clipboard.");
        }

        private async void Import_OnClicked(object sender, EventArgs e)
        {
            string matrixString = await Clipboard.GetTextAsync();

            //TODO fix input check
            if ((!_viewModel.IsLatexSelected && !matrixString.Contains('(')) 
                || (_viewModel.IsLatexSelected && !matrixString.Contains('{')))
            {
                DependencyService.Get<IMessageToast>().DisplayToast("Malformed matrix in clipboard.");
                return;
            }

            _viewModel.ImportMatrix(MatrixGrid, matrixString);

            ShowMatrixButtons();
        }

        private void FormatPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            LatexDelimiterPicker.IsVisible = _viewModel.IsLatexSelected;
        }
    }
}