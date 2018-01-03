using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using Board;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Media;

namespace BoardTests
{
    [TestClass]
    public class BoardTest
    {
        [TestMethod]
        public void CreateBoard_ValidParameters_GeneratesBoard()
        {
            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();
            RowDefinition row3 = new RowDefinition();

            Border border1 = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1)
            };
            Border border2 = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1)
            };
            Border border3 = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1)
            };
            Border border4 = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1)
            };
            Border border5 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            Border border6 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            Grid expectedGrid = new Grid
            {
                Margin = new Thickness(30),
                Background = Brushes.Beige,
                ColumnDefinitions = {column1, column2},
                RowDefinitions = {row1, row2, row3}
            };

            Grid.SetColumn(border1,0);
            Grid.SetRow(border1,0);

            Grid.SetColumn(border5, 0);
            Grid.SetRow(border5, 1);

            Grid.SetColumn(border2, 0);
            Grid.SetRow(border2, 2);

            Grid.SetColumn(border3, 1);
            Grid.SetRow(border3, 0);

            Grid.SetColumn(border6, 1);
            Grid.SetRow(border6, 1);

            Grid.SetColumn(border4, 1);
            Grid.SetRow(border4, 2);

            expectedGrid.Children.Add(border1);
            expectedGrid.Children.Add(border5);
            expectedGrid.Children.Add(border2);
            expectedGrid.Children.Add(border3);
            expectedGrid.Children.Add(border6);
            expectedGrid.Children.Add(border4);


            MainWindow board = new MainWindow(1,1,2,3);

            Grid actualGrid = board.CreateBoard();

            Assert.AreEqual(expectedGrid.RowDefinitions.Count, actualGrid.RowDefinitions.Count,"Board's rows not generated properly");
            Assert.AreEqual(expectedGrid.ColumnDefinitions.Count, actualGrid.ColumnDefinitions.Count, "Board's columns not generated properly");
            Assert.AreEqual(expectedGrid.Children.Count, actualGrid.Children.Count, "Borders not generated properly");
            Assert.AreEqual(border1.BorderBrush, (actualGrid.Children[0] as Border).BorderBrush, "Child 0 not generated properly");
            Assert.AreEqual(border5.BorderBrush, (actualGrid.Children[1] as Border).BorderBrush, "Child 1 not generated properly");
            Assert.AreEqual(border2.BorderBrush, (actualGrid.Children[2] as Border).BorderBrush, "Child 2 not generated properly");
            Assert.AreEqual(border3.BorderBrush, (actualGrid.Children[3] as Border).BorderBrush, "Child 3 not generated properly");
            Assert.AreEqual(border6.BorderBrush, (actualGrid.Children[4] as Border).BorderBrush, "Child 4 not generated properly");
            Assert.AreEqual(border4.BorderBrush, (actualGrid.Children[5] as Border).BorderBrush, "Child 5 not generated properly");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_ZeroParameters_NumberOFPlayers()
        {
            new MainWindow(0, 1, 2, 3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_ZeroParameters_GoalAreaHeight()
        {
            new MainWindow(1, 0, 2, 3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_ZeroParameters_BoardWidth()
        {
            new MainWindow(1, 1, 0, 3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_ZeroParameters_BoardHeight()
        {
            new MainWindow(1, 1, 2, 0);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateBoard_ZeroParameters_All()
        {
            new MainWindow(0,0,0,0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_NegativeParameters_NumberOFPlayers()
        {
            new MainWindow(-1,1,2,3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_NegativeParameters_GoalAreaHeight()
        {
            new MainWindow(1, -1, 2, 3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_NegativeParameters_BoardWidth()
        {
            new MainWindow(1, 1, -2, 3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_NegativeParameters_BoardHeight()
        {
            new MainWindow(1, 1, 2, -3);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MainWindow_NegativeParameters_All()
        {
            new MainWindow(-1, -1, -2, -3);
        }
    }
}
