using System;
using System.Collections.Generic;
using System.Linq;
using MatrixFormatter.Format;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MatrixFormatter
{
    //Add ViewModel to reduce the size of this class
    public partial class MainPage : ContentPage
    {
        //todo remove the binds since changes to these doesn't update the matrix directly, should it?
        //todo Change default to be positive
        //todo Ensure input is a positive integer
        public static readonly BindableProperty MatrixRowsProperty =
            BindableProperty.Create(
                propertyName: "MatrixRows",
                returnType: typeof(string),
                declaringType: typeof(MainPage),
                "0");

        public static readonly BindableProperty MatrixColumnsProperty =
            BindableProperty.Create(
                propertyName: "MatrixColumns",
                returnType: typeof(string),
                declaringType: typeof(MainPage),
                "0");

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

        public MatrixStringFormat SelectedFormat { get; set; }

        public List<string> MatrixStringFormats
        {
            get
            {
                return Enum.GetNames(typeof(MatrixStringFormat)).Select(b => b.SplitCamelCase()).ToList();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void CreateMatrix()
        {
            if (MatrixRows == 0 || MatrixColumns == 0 ||
                (MatrixRows == MatrixGrid.RowDefinitions.Count && MatrixColumns == MatrixGrid.ColumnDefinitions.Count))
            {
                return;
            }

            BorderEntry[,] cells = null;

            RowDefinition newRow = new RowDefinition
            {
                Height = GridLength.Auto
            };
            ColumnDefinition newColumn = new ColumnDefinition();

            if (MatrixGrid.RowDefinitions.Count != 0)
            {
                cells = new BorderEntry[Math.Min(MatrixRows, MatrixGrid.RowDefinitions.Count), Math.Min(MatrixColumns, MatrixGrid.ColumnDefinitions.Count)];

                for (int i = 0; i < cells.GetLength(0); i++)
                {
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        cells[i, j] = (BorderEntry)MatrixGrid.Children[i * MatrixGrid.ColumnDefinitions.Count + j];
                    }
                }
            }
            
            //Todo figure out how to edit grid in place despite it being a single dimensional array
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
                                MatrixGrid.Children.Add(new BorderEntry(), j, j + 1, i, i + 1);
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

            ToggleCellsButton.IsVisible = true;
            ExportButton.IsVisible = true;
        }

        private void CreateMatrix_OnClicked(object sender, EventArgs e)
        {
            CreateMatrix();
        }

        private void CreateIdentityMatrix_OnClicked(object sender, EventArgs e)
        {
            if (MatrixRows != MatrixColumns)
            {
                DependencyService.Get<IMessageToast>().DisplayToast("Matrix must be square.");
                return;
            }

            CreateMatrix();

            foreach (BorderEntry view in MatrixGrid.Children)
            {
                view.Text = Grid.GetRow(view) == Grid.GetColumn(view) ? "1" : "0";
            }
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

        //todo Support latex bracket/paren delimiters, i.e. pmatrix, Bmatrix,...
        private void Export_OnClicked(object sender, EventArgs e)
        {
            bool latexFormat = SelectedFormat == MatrixStringFormat.LatexAmsmath;
            string matrixString = latexFormat ? @"\begin{matrix}" : "■(";

            foreach (BorderEntry view in MatrixGrid.Children)
            {
                matrixString += view.Text;

                if (Grid.GetColumn(view) == MatrixGrid.ColumnDefinitions.Count - 1)
                {
                    if (Grid.GetRow(view) != MatrixGrid.RowDefinitions.Count - 1)
                    {
                        matrixString += latexFormat ? @"\\" : "@";
                    }
                }
                else
                {
                    matrixString += latexFormat ? " & " : "&";
                }
            }

            matrixString += latexFormat ? @"\end{matrix}" : ")";

            //todo Determine how to use mathml clipboard type to allow pasting into onenote within a equation
            Clipboard.SetTextAsync(matrixString);
            DependencyService.Get<IMessageToast>().DisplayToast("Copied to Clipboard.");
        }

        private async void Import_OnClicked(object sender, EventArgs e)
        {
            if (SelectedFormat == MatrixStringFormat.LatexAmsmath)
            {
                DependencyService.Get<IMessageToast>().DisplayToast("Not yet supported.");
                return;
            }

            string onenoteMatrixString = await Clipboard.GetTextAsync();

            if (onenoteMatrixString.Length < 2 || !onenoteMatrixString.Contains('('))
            {
                DependencyService.Get<IMessageToast>().DisplayToast("Malformed matrix in clipboard.");
                return;
            }

            onenoteMatrixString = onenoteMatrixString.Substring(onenoteMatrixString.IndexOf('(') + 1);
            onenoteMatrixString = onenoteMatrixString.Remove(onenoteMatrixString.Length - 1);

            MatrixRows = onenoteMatrixString.Count(c => c == '@') + 1;
            MatrixColumns = onenoteMatrixString.Remove(onenoteMatrixString.IndexOf('@')).Count(c => c == '&') + 1;
            CreateMatrix();

            string[] cellValues = onenoteMatrixString.Split('&', '@');

            for (int i = 0; i < cellValues.Length; i++)
            {
                ((BorderEntry) MatrixGrid.Children[i]).Text = cellValues[i];
            }
        }
    }
}