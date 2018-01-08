using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Board
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public int PlayerNumber { get; set; }
        public int GoalHeight { get; set; }
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }

        public Dictionary<int,Image> PlayerIcons = new Dictionary<int, Image>();

        public MainWindow(int numberOfPlayers, int goalAreaH, int boardW, int boardH)
        {
            if (numberOfPlayers <= 0)
                throw new ArgumentException("Number of players zero", nameof(numberOfPlayers));
            if (goalAreaH <= 0)
                throw new ArgumentException("Goal area height zero", nameof(goalAreaH));
            if (boardW <= 0)
                throw new ArgumentException("Board width zero", nameof(boardW));
            if (boardH <= 0)
                throw new ArgumentException("Board height zero", nameof(boardH));

            PlayerNumber = numberOfPlayers;
            GoalHeight = goalAreaH;
            BoardWidth = boardW;
            BoardHeight = boardH;
            InitializeComponent();
            Content = CreateBoard();
        }

        public Grid CreateBoard()
        {
            Grid boardGrid = new Grid
            {
                Margin = new Thickness(30),
                Background = Brushes.Beige
            };

            for (int i = 0; i < BoardWidth; i++)
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < BoardHeight; i++)
                boardGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < BoardWidth; i++)
            {
                for (int j = 0; j < BoardHeight; j++)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    };

                    if (j < GoalHeight || j >= (BoardHeight - GoalHeight))
                        border.BorderBrush = Brushes.DarkGray;

                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                    boardGrid.Children.Add(border);
                }
            }
            return boardGrid;
        }
    }
}


