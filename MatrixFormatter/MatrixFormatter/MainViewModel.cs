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

            RowDefinition newRow = new RowDefinition
            {
                Height = GridLength.Auto
            };
            ColumnDefinition newColumn = new ColumnDefinition();

            //TODO consider changing bounds to improve speed, i.e. no cell in [0, MatrixRows - 1] will be removed when MatrixRows < matrixGrid.ColumnDefinitions.Count
            //Remove unneeded cells
            for (int i = matrixGrid.Children.Count - 1; i >= 0; i--)
            {
                View cell = matrixGrid.Children[i];

                //TODO same as above comment but consider jumping entire rows if possible
                if (Grid.GetRow(cell) >= MatrixRows || Grid.GetColumn(cell) >= MatrixColumns)
                {
                    matrixGrid.Children.Remove(cell);
                }
            }

            //Remove unneeded rows
            for (int i = matrixGrid.RowDefinitions.Count - 1; i >= MatrixRows; i--)
            {
                matrixGrid.RowDefinitions.RemoveAt(i);
            }

            //Remove unneeded columns
            for (int i = matrixGrid.ColumnDefinitions.Count - 1; i >= MatrixColumns; i--)
            {
                matrixGrid.ColumnDefinitions.RemoveAt(i);
            }

            //Add new columns with cells
            for (int i = matrixGrid.ColumnDefinitions.Count; i < MatrixColumns; i++)
            {
                matrixGrid.ColumnDefinitions.Add(newColumn);

                for (int j = 0; j < matrixGrid.RowDefinitions.Count; j++)
                {
                    matrixGrid.Children.Add(new BorderEntry(), i, j);
                }
            }

            //Add new rows with cells
            for (int i = matrixGrid.RowDefinitions.Count; i < MatrixRows; i++)
            {
                matrixGrid.RowDefinitions.Add(newRow);

                for (int j = 0; j < matrixGrid.ColumnDefinitions.Count; j++)
                {
                    matrixGrid.Children.Add(new BorderEntry(), j, i);
                }
            }

        }

        public string ExportMatrix(Grid matrixGrid)
        {
            bool latexFormat = SelectedFormat == MatrixStringFormat.LatexAmsmath;
            string matrixString = latexFormat ? @"\begin{matrix}" : "■(";

            for (int i = 0; i < matrixGrid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < matrixGrid.ColumnDefinitions.Count; j++)
                {
                    //TODO Determine if any performance can be gained by using OrderBy and so getting a list of all children according to this condition
                    BorderEntry view = (BorderEntry)matrixGrid.Children.Single(c => Grid.GetRow(c) == i && Grid.GetColumn(c) == j);
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