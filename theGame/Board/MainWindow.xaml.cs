using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        /// <summary>
        /// bool 2d array tells whether a cell is occupied by a player or not
        /// </summary>
        public bool [,]occupied = new bool[40, 40];
        /// <summary>
        /// bool 2d array tells whether a cell is has a piece or not
        /// </summary>
        public bool[,] pieaces = new bool[40, 40];
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

			Content = boardGrid;
		}

		

	}
}

            }
		    return boardGrid;

		/// <summary>
        /// check whether a cell is occupied or not
        /// </summary>
        /// <param name="i">index i</param>
        /// <param name="j">index j</param>
        /// <returns></returns>
        public bool IsOccupied(int i, int j)
        {
            return occupied[i, j];
        }
        /// <summary>
        /// check whether a cell has a piece or not
        /// </summary>
        /// <param name="i">row i</param>
        /// <param name="j">column j</param>
        /// <returns></returns>
        public bool IsPiece(int i, int j)
        {
            return pieaces[i, j];
        }