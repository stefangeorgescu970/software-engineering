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
        /// <summary>
        /// Number of players per team.
        /// </summary>
		public int PlayerNumber { get; set; }
        /// <summary>
        /// Height of a goal area.
        /// </summary>
		public int GoalHeight { get; set; }
        /// <summary>
        /// Width of the board.
        /// </summary>
        public int BoardWidth { get; set; }
        /// <summary>
        /// Height of the board.
        /// </summary>
		public int BoardHeight { get; set; }

        /// <summary>
        /// Creates MainWindow with board of the game.
        /// </summary>
        /// <param name="numberOfPlayers">Number of players per team.</param>
        /// <param name="goalAreaH">Height of a goal area</param>
        /// <param name="boardW">Width of a board</param>
        /// <param name="boardH">Height of a board</param>
		public MainWindow(int numberOfPlayers, int goalAreaH, int boardW, int boardH)
		{
			PlayerNumber = numberOfPlayers;
			GoalHeight = goalAreaH;
			BoardWidth = boardW;
			BoardHeight = boardH;

			InitializeComponent();
			CreateBoard(BoardWidth, BoardHeight, GoalHeight);
		}

        /// <summary>
        /// Creates board of the game.
        /// </summary>
        /// <param name="width">Widht of a board.</param>
        /// <param name="height">Height of a board.</param>
        /// <param name="gheight">Height of a goal area.</param>
	    private void CreateBoard(int width, int height, int gheight)
		{
			Grid boardGrid = new Grid
			{
				Margin = new Thickness(30),
				Background = Brushes.Beige
			};

			for (int i = 0; i < width; i++)
				boardGrid.ColumnDefinitions.Add(new ColumnDefinition());
			for (int i = 0; i < height; i++)
				boardGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    };

                    if (j < gheight || j >= (height - gheight))
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
