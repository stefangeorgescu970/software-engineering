using System.Windows;
using System.Windows.Controls;

namespace Board
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>


	public partial class MainWindow
	{
		public int PlayerNumber { get; set; }
		public int GoalWidth { get; set; }
		public int GoalHeight { get; set; }
		public int BoardWidth { get; set; }
		public int BoardHeight { get; set; }

		public MainWindow(int numberOfPlayers, int goalAreaW, int goalAreaH, int boardW, int boardH)
		{
			PlayerNumber = numberOfPlayers;
			GoalWidth = goalAreaW;
			GoalHeight = goalAreaH;
			BoardWidth = boardW;
			BoardHeight = boardH;

			InitializeComponent();
			CreateBoard(BoardWidth, BoardHeight);
		}

	    private void CreateBoard(int width, int height)
		{
			Grid boardGrid = new Grid
			{
				Margin = new Thickness(30),
				Background = System.Windows.Media.Brushes.CadetBlue
			};

			for (int i = 0; i < width; i++)
				boardGrid.ColumnDefinitions.Add(new ColumnDefinition());
			for (int i = 0; i < height; i++)
				boardGrid.RowDefinitions.Add(new RowDefinition());

			boardGrid.ShowGridLines = true;
			Content = boardGrid;
		}

		

	}
}
