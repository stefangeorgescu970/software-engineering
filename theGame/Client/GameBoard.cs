namespace Client
{
    public enum FieldContent
    {
        Empty, Item, FakeItem, Player
    }

    public enum FieldType
    {
        GoalArea, Board
    }
    public class Field
    {
        public FieldContent Content { get; set; }
        public FieldType Type { get; set; }
        public int PlayerID { get; set; }

        public Field()
        {
            Content = FieldContent.Empty;
            Type = FieldType.Board;
        }

    }
    public class BoardDimensions
    {
        public BoardDimensions(int width, int height, int goalAreaHeight)
        {
            Width = width;
            Height = height;
            GoalAreaHeight = goalAreaHeight;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int GoalAreaHeight { get; set; }

    }
    public class GameBoard
    {
        public Field[,] Board = new Field[1,1];
        public BoardDimensions BoardDim = new BoardDimensions(1,1,1);

        public int Width { get; set; }
        public int Height { get; set; }
        public int GoalAreaHeight { get; set; }

        public GameBoard()
        { }
        public GameBoard(int width, int height, int goalAreaHeight)
        {
            Width = width;
            Height = height;
            GoalAreaHeight = goalAreaHeight;
            BoardDim =new BoardDimensions(Width,Height,GoalAreaHeight);
            Board = new Field[width,height];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Board[i, j] = new Field();
                    

                    if (j <GoalAreaHeight || j >= (Height - GoalAreaHeight))
                        Board[i, j].Type = FieldType.GoalArea;
                }
            }
        }

        public bool IsOccupied(int xCoordinate, int yCoordinate)
        {
            if (Board[xCoordinate, yCoordinate].Content != FieldContent.Player)
                return false;
            else return true;
        }

    }
}