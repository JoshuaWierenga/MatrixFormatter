﻿using System;
using System.Collections.Generic;
using System.Linq;
using MatrixFormatter.Format;
using Xamarin.Forms;

namespace MatrixFormatter
{
    public class MainViewModel
    {
        //todo Readd bindings, removing them prevents the ui from updating to show the matrix size when importing is used
        //todo Change default to be positive
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

        public bool CreateMatrix(Grid matrixGrid)
        {
            if (MatrixRows <= 0 || MatrixColumns <= 0)
            {
                DependencyService.Get<IMessageToast>().DisplayToast("Invalid Matrix Size.");
                return false;
            }
            if (MatrixRows == matrixGrid.RowDefinitions.Count && MatrixColumns == matrixGrid.ColumnDefinitions.Count)
            {
                return false;
            }

            RowDefinition newRow = new RowDefinition
            {
                Height = GridLength.Auto
            };
            ColumnDefinition newColumn = new ColumnDefinition();

            //Remove unneeded cells
            for (int i = matrixGrid.Children.Count - 1; i >= MatrixRows; i--)
            {
                View cell = matrixGrid.Children[i];

                //TODO Consider jumping MatrixRow cells when i is between n*MatrixRows and N*MatrixRows + MatrixColumns
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

            return true;
        }

        public string ExportMatrix(Grid matrixGrid)
        {
            string matrixString = IsLatexSelected ? @"\begin{" + LatexDelimiters[SelectedLatexDelimiter] + "}" : "■(";

            OrderedParallelQuery<View> orderedCells = matrixGrid.Children.AsParallel().OrderBy(c =>
                Grid.GetRow(c) * matrixGrid.ColumnDefinitions.Count + Grid.GetColumn(c));

            foreach (BorderEntry view in orderedCells)
            {
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