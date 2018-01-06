using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Board
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class GameBoard
    {
        public GameBoard(int width, int height, int goalAreaHeight)
        {
            Width = width;
            Height = height;
            GoalAreaHeight = goalAreaHeight;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int GoalAreaHeight { get; set; }

    }

    public partial class MainWindow
	{
		public int PlayerNumber { get; set; }
	    public GameBoard GameBoard;
        /// <summary>
        /// bool 2d array tells whether a cell is occupied by a player or not
        /// </summary>
        public bool[,] Occupied = new bool[40, 40];
        /// <summary>
        /// bool 2d array tells whether a cell is has a piece or not
        /// </summary>
        public bool[,] Pieaces = new bool[40, 40];
        public MainWindow(int numberOfPlayers, int goalAreaH, int boardW, int boardH)
		{
		    if (numberOfPlayers <= 0 )
		        throw new ArgumentException("Number of players zero", nameof(numberOfPlayers));
            if (goalAreaH <= 0)
		        throw new ArgumentException("Goal area height zero", nameof(goalAreaH));
		    if (boardW <= 0)
		        throw new ArgumentException("Board width zero", nameof(boardW));
		    if (boardH <= 0)
		        throw new ArgumentException("Board height zero", nameof(boardH));

            PlayerNumber = numberOfPlayers;
		    GameBoard = new GameBoard(boardW, boardH, goalAreaH);
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

	        for (int i = 0; i < GameBoard.Width; i++)
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition());
			for (int i = 0; i < GameBoard.Height; i++)
				boardGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < GameBoard.Width; i++)
            {
                for (int j = 0; j < GameBoard.Height; j++)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    };

                    if (j < GameBoard.GoalAreaHeight || j >= (GameBoard.Height - GameBoard.GoalAreaHeight))
                        border.BorderBrush = Brushes.DarkGray;

                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                    boardGrid.Children.Add(border);
                }
            }

			return boardGrid;
		}



	    /// <summary>
	    /// check whether a cell is occupied or not
	    /// </summary>
	    /// <param name="i">index i</param>
	    /// <param name="j">index j</param>
	    /// <returns></returns>
	    public bool IsOccupied(int i, int j)
	    {
	        return Occupied[i, j];
	    }
	    /// <summary>
	    /// check whether a cell has a piece or not
	    /// </summary>
	    /// <param name="i">row i</param>
	    /// <param name="j">column j</param>
	    /// <returns></returns>
	    public bool IsPiece(int i, int j)
	    {
	        return Pieaces[i, j];
	    }

    }
}


		