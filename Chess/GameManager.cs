using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chess
{
    class GameManager
    {
        public int[,] board { get; private set; }
        private bool whiteToTurn;
        public bool HumanAsWhite { get; private set; } = true;
        public Result? GameResult
        {
            get { return gameResult; }
        }
        public bool WhiteToTurn { 
            get { return whiteToTurn; }
        }
        private Result? gameResult = null;
        public GameMode GameMode { get; private set; }

        private ChessComputer chessComputer;

        private Vector whiteKingPosition;
        private Vector blackKingPosition;

        private bool[] castlingPosibilityFromHistory;
        //private List<Move> castlingMoves = new List<Move>();
        //short white
        //long white
        //short black
        //long white
        StreamWriter writer;
        bool outputToLog = true;
        public GameManager(GameMode gameMode, bool humanAsWhite=true)
        {
            ChessLibrary.Init();
            chessComputer = new ChessComputer();
            Init(gameMode,humanAsWhite);
        }
        public void Quit()
        {
            writer.Close();
            chessComputer.Quit();
        }
        private void Init(GameMode gameMode, bool humanAsWhite = true)
        {
            GameMode = gameMode;
            HumanAsWhite = humanAsWhite;
            writer = new StreamWriter(File.Create("log.txt"));
            whiteToTurn = true;
            gameResult = null;
            castlingPosibilityFromHistory = new bool[4];
            for (int i = 0; i < 4; i++)
                castlingPosibilityFromHistory[i] = true;

            /*castlingMoves = new List<Move>();
            castlingMoves.Add(new Move(new Vector(4, 0), new Vector(6, 0)));
            castlingMoves.Add(new Move(new Vector(4, 7), new Vector(6, 7)));
            castlingMoves.Add(new Move(new Vector(4, 0), new Vector(2, 0)));
            castlingMoves.Add(new Move(new Vector(4, 7), new Vector(2, 7)));*/

            board = new int[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    board[i, j] = 0;
            board[0, 0] = 5;
            board[7, 0] = 5;
            board[0, 7] = -5;
            board[7, 7] = -5;

            board[1, 0] = 2;
            board[6, 0] = 2;
            board[1, 7] = -2;
            board[6, 7] = -2;

            board[2, 0] = 3;
            board[5, 0] = 3;
            board[2,7] = -3;
            board[5, 7] = -3;

            board[3, 0] = 9;
            board[3, 7] = -9;

            board[4, 0] = 10;
            board[4, 7] = -10;

            for (int i = 0; i < 8; i++)
            {
                board[i, 1] = 1;
                board[i, 6] = -1;
            }
            //board[0, 5] = 1;

            //board[2, 2] = -9;
            board = ChessLibrary.ReadPositionFromFile("checkmate3.txt");
            //ChessLibrary.ThereIsNoCheckInThisPosition(false, board);
            ChessLibrary.OutputBoard(board, true);
            if(outputToLog)
                ChessLibrary.OutputBoard(board, false);
            //OutputAllPossibleMoves(true,board,castlingPosibilityFromHistory);
            if (((whiteToTurn && !HumanAsWhite) || (!whiteToTurn && HumanAsWhite)) && GameMode == GameMode.AgainstComputer)
            {
                if (outputToLog)
                {
                    writer.WriteLine("------------------------------");
                    writer.WriteLine("Compters mive");
                }
              
                Move computersMove = chessComputer.FindTheBestMoveForPosition(board, whiteToTurn);
                Console.WriteLine(OutputHumanMove(computersMove));

                ChessLibrary.InputMove(computersMove, board,ref whiteToTurn,ref gameResult,ref castlingPosibilityFromHistory);
            }
        }
        public bool InputHumanMove(string figureType, string startField, string finishField)
        {
            int figure = ChessLibrary.FigureLetterToNumber(figureType, whiteToTurn);
            char[] splitedStartField = startField.ToCharArray();
            char[] splitedFinishField = finishField.ToCharArray();
            int startHorizontal = 0;
            int startVertical = 0;
            int finishHorizontal = 0;
            int finishVertical = 0;
            try
            {
                startHorizontal = Convert.ToInt32(splitedStartField[1].ToString()) - 1;
                startVertical = Convert.ToInt32(ChessLibrary.HorizontalToNumber(splitedStartField[0].ToString()));
                finishHorizontal = Convert.ToInt32(splitedFinishField[1].ToString()) - 1;
                finishVertical = Convert.ToInt32(ChessLibrary.HorizontalToNumber(splitedFinishField[0].ToString()));
            }
            catch
            {
                Console.WriteLine("Invalid input");
                return false;
            }
            if(figure==0||startHorizontal<0||startHorizontal>7|| startVertical < 0 || startVertical > 7 || finishVertical < 0 || finishVertical > 7 || finishHorizontal < 0 || finishHorizontal > 7)
            {
                Console.WriteLine("Invalid input");
                return false;
            }
            Vector start = new Vector(startVertical, startHorizontal);
            Vector end = new Vector(finishVertical, finishHorizontal);
            if (!ChessLibrary.InputMove(new Move(start,end),board,ref whiteToTurn,ref gameResult,ref castlingPosibilityFromHistory))
            {
                Console.WriteLine("This move is imposible");
                return false;
            }

            if (((whiteToTurn && !HumanAsWhite) || (!whiteToTurn && HumanAsWhite)) && GameMode == GameMode.AgainstComputer)
            {
                Move computersMove = chessComputer.FindTheBestMoveForPosition(board, whiteToTurn);
                Console.WriteLine(OutputHumanMove(computersMove));
                ChessLibrary.InputMove(computersMove, board, ref whiteToTurn, ref gameResult, ref castlingPosibilityFromHistory);
                
            }
            return true;
        }
        private string OutputHumanMove(Move move)
        {
            Vector start = move.start;
            Vector end = move.end;
            int figure = board[start.x, start.y];
            string figureLetter = ChessLibrary.NumberToFigure(figure);
            string startField = ChessLibrary.NumberToHorizontal(start.x) + (start.y+1).ToString();
            string endField = ChessLibrary.NumberToHorizontal(end.x) + (end.y + 1).ToString();
            string output = figureLetter + startField + "-" + endField;
            return output;
        }

        
        
    }
    public enum Result { 
        WhiteWon,
        Draw,
        BloackWon,
    }
    public struct Vector
    {
        public int x;
        public int y;
        public Vector(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    public class VectorMath
    {
        public static Vector Substract(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y);
        }
        public static Vector Sum(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y);
        }
        public static int Dot(Vector a, Vector b)
        {
            return (a.x*b.x+a.y*b.y);
        }
        public static double Cos(Vector a, Vector b)
        {
            return (double)(a.x * b.x + a.y * b.y)/(Math.Sqrt((double)(a.x * a.x + a.y * a.y)) * Math.Sqrt((double)(b.x * b.x + b.y * b.y)));
        }
        public static bool Equal(Vector a, Vector b)
        {
            return (a.x==b.x &&a.y==b.y);
        }
        public static bool CBetweenAAndB(Vector a, Vector b, Vector c)
        {
            int ac_x = c.x - a.x;
            int ac_y = c.y - a.y;
            int ab_x = b.x - a.x;
            int ab_y = b.y - a.y;
            int bc_x = c.x - b.x;
            int bc_y = c.y - b.y;
            double tan_ac = (double)ac_y / ((double)ac_x);
            double tan_ab = (double)ab_y / ((double)ab_x);
            if (Math.Abs(tan_ac - tan_ab) < 0.01 && ac_x * ab_x > 0 && bc_x * ab_x < 0)
                return true;
            return false;
        }
    }
    public struct Move
    {
        public Vector start;
        public Vector end;
        public Move(Vector _start, Vector _end)
        {
            start = _start;
            end = _end;
        }
    }
    public enum GameMode
    {
        TwoPlayers,
        AgainstComputer,
    }
    public enum CheckState
    {
        WhiteAreChecked,
        WhiteAreDoubleChecked,
        BlackAreChecked,
        BlackAreDoubleChecked,
    }
}
