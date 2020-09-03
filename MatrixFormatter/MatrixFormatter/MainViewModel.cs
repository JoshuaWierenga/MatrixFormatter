using System;
using System.Collections.Generic;
using System.Linq;
using MatrixFormatter.Format;
using Xamarin.Forms;

namespace MatrixFormatter
{
    public class MainViewModel
    {
        //todo Change default to be positive
        //todo Ensure input is a positive integer
        public int MatrixRows { get; set; }
        public int MatrixColumns { get; set; }

        public MatrixStringFormat SelectedFormat { get; set; }

        public List<string> MatrixStringFormats { get; } = Enum.GetNames(typeof(MatrixStringFormat)).Select(b => b.SplitCamelCase()).ToList();

        public bool IsLatexSelected => SelectedFormat == MatrixStringFormat.LatexAmsmath;

        private static readonly Dictionary<string, string> LatexDelimiters = new Dictionary<string, string>
        {
            {"No Delimiters", "matrix"},
            {"Parentheses", "pmatrix"},
            {"Brackets", "bmatrix"},
            {"Braces", "Bmatrix"},
            {"Pipes", "vmatrix"},
            {"Double Pipes", "Vmatrix"}
        };

        public List<string> LatexDelimiterNames { get; } = LatexDelimiters.Keys.ToList();

        public string SelectedLatexDelimiter { get; set; } = LatexDelimiters.Keys.First();

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
            string matrixString = IsLatexSelected ? @"\begin{" + LatexDelimiters[SelectedLatexDelimiter] + "}" : "■(";

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
                            matrixString += IsLatexSelected ? @"\\" : "@";
                        }
                    }
                    else
                    {
                        matrixString += IsLatexSelected ? " & " : "&";
                    }
                }
            }

            matrixString += IsLatexSelected ? @"\end{" + LatexDelimiters[SelectedLatexDelimiter] + "}" : ")";
            return matrixString;
        }

        public void ImportMatrix(Grid matrixGrid, string matrixString)
        {
            matrixString = matrixString.Substring(matrixString.IndexOf(IsLatexSelected ? '}' : '(') + 1);
            matrixString = matrixString.Remove(matrixString.Length - (IsLatexSelected ? 12 : 1));

            if (IsLatexSelected)
            {
                MatrixRows = matrixString.Count(c => c == '\\') / 2 + 1;
            }
            else
            {
                MatrixRows = matrixString.Count(c => c == '@') + 1;
            }

            MatrixColumns = matrixString
                .Remove(matrixString.IndexOf(IsLatexSelected ? @"\\" : "@", StringComparison.Ordinal))
                .Count(c => c == '&') + 1;

            CreateMatrix(matrixGrid);

            string[] rowColumnDelimiters = IsLatexSelected ? new[]{ " & ", @"\\" } : new[]{ "&", "@" } ;
            string[] cellValues = matrixString.Split(rowColumnDelimiters, StringSplitOptions.None);

            for (int i = 0; i < cellValues.Length; i++)
            {
                ((BorderEntry)matrixGrid.Children[i]).Text = cellValues[i];
            }
        }
    }
}