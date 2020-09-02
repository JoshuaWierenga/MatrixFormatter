using System;
using System.Collections.Generic;
using System.Linq;
using MatrixFormatter.Format;
using Xamarin.Forms;

namespace MatrixFormatter
{
    public class MainViewModel : BindableObject
    {
        //todo remove the binds since changes to these doesn't update the matrix directly, should it?
        //todo Change default to be positive
        //todo Ensure input is a positive integer
        public static readonly BindableProperty MatrixRowsProperty =
            BindableProperty.Create(
                propertyName: "MatrixRows",
                returnType: typeof(string),
                declaringType: typeof(MainViewModel),
                "0");

        public static readonly BindableProperty MatrixColumnsProperty =
            BindableProperty.Create(
                propertyName: "MatrixColumns",
                returnType: typeof(string),
                declaringType: typeof(MainViewModel),
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

        public void CreateMatrix(Grid matrixGrid)
        {
            if (MatrixRows == 0 || MatrixColumns == 0 ||
                (MatrixRows == matrixGrid.RowDefinitions.Count && MatrixColumns == matrixGrid.ColumnDefinitions.Count))
            {
                return;
            }

            BorderEntry[,] cells = null;

            RowDefinition newRow = new RowDefinition
            {
                Height = GridLength.Auto
            };
            ColumnDefinition newColumn = new ColumnDefinition();

            if (matrixGrid.RowDefinitions.Count != 0)
            {
                cells = new BorderEntry[Math.Min(MatrixRows, matrixGrid.RowDefinitions.Count), Math.Min(MatrixColumns, matrixGrid.ColumnDefinitions.Count)];

                for (int i = 0; i < cells.GetLength(0); i++)
                {
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        cells[i, j] = (BorderEntry)matrixGrid.Children[i * matrixGrid.ColumnDefinitions.Count + j];
                    }
                }
            }

            //Todo figure out how to edit grid in place despite it being a single dimensional array
            matrixGrid.Children.Clear();

            for (int i = 0; i < Math.Max(MatrixRows, matrixGrid.RowDefinitions.Count); i++)
            {
                if (i < MatrixRows)
                {
                    if (matrixGrid.RowDefinitions.Count < i + 1)
                    {
                        matrixGrid.RowDefinitions.Add(newRow);
                    }

                    for (int j = 0; j < Math.Max(MatrixColumns, matrixGrid.ColumnDefinitions.Count); j++)
                    {
                        if (j < MatrixColumns)
                        {
                            if (matrixGrid.ColumnDefinitions.Count < j + 1)
                            {
                                matrixGrid.ColumnDefinitions.Add(newColumn);
                            }

                            if (i < cells?.GetLength(0) && j < cells.GetLength(1))
                            {
                                matrixGrid.Children.Add(cells[i, j], j, j + 1, i, i + 1);
                            }
                            else
                            {
                                matrixGrid.Children.Add(new BorderEntry(), j, j + 1, i, i + 1);
                            }
                        }
                        else
                        {
                            matrixGrid.ColumnDefinitions.RemoveAt(i);
                        }
                    }
                }
                //Todo fix this, it is currently not being used
                else if (i == MatrixRows && i >= matrixGrid.RowDefinitions.Count)
                {
                    matrixGrid.RowDefinitions.Add(new RowDefinition());
                }
                else
                {
                    matrixGrid.RowDefinitions.RemoveAt(i);
                }
            }
        }

        public string ExportMatrix(Grid matrixGrid)
        {
            bool latexFormat = SelectedFormat == MatrixStringFormat.LatexAmsmath;
            string matrixString = latexFormat ? @"\begin{matrix}" : "■(";

            foreach (BorderEntry view in matrixGrid.Children)
            {
                matrixString += view.Text;

                if (Grid.GetColumn(view) == matrixGrid.ColumnDefinitions.Count - 1)
                {
                    if (Grid.GetRow(view) != matrixGrid.RowDefinitions.Count - 1)
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
            return matrixString;
        }

        public void ImportMatrix(Grid matrixGrid, string onenoteMatrixString)
        {
            onenoteMatrixString = onenoteMatrixString.Substring(onenoteMatrixString.IndexOf('(') + 1);
            onenoteMatrixString = onenoteMatrixString.Remove(onenoteMatrixString.Length - 1);

            MatrixRows = onenoteMatrixString.Count(c => c == '@') + 1;
            MatrixColumns = onenoteMatrixString.Remove(onenoteMatrixString.IndexOf('@')).Count(c => c == '&') + 1;
            CreateMatrix(matrixGrid);

            string[] cellValues = onenoteMatrixString.Split('&', '@');

            for (int i = 0; i < cellValues.Length; i++)
            {
                ((BorderEntry)matrixGrid.Children[i]).Text = cellValues[i];
            }
        }
    }
}