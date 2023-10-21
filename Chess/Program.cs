using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Choose game mode:");
            Console.WriteLine("1 - Two players");
            Console.WriteLine("2 - Against Computer");
            string gameModeInput = Console.ReadLine();
            GameMode gameMode = GameMode.TwoPlayers;
            if (gameModeInput == "1")
                gameMode = GameMode.TwoPlayers;
            if (gameModeInput == "2")
                gameMode = GameMode.AgainstComputer;
            bool humanIsPlayingAsWhite = true;
            if (gameMode == GameMode.AgainstComputer)
            {
                Console.WriteLine("Choose your color to play:");
                Console.WriteLine("1 - White");
                Console.WriteLine("-1 - Black");
                string colorInput = Console.ReadLine();
                if (colorInput == "-1")
                    humanIsPlayingAsWhite = false;
            }

            //GameManager gameManager = new GameManager(gameMode,humanIsPlayingAsWhite);
            Board board = new Board(gameMode, humanIsPlayingAsWhite);
            ChessComputer chessComputer = new ChessComputer();
            string inputMove;
            while (board.GameResult == null)
            {
                
                if (board.WhiteToTurn)
                    Console.WriteLine("White to turn");
                else
                    Console.WriteLine("Black to turn");
                if (((board.WhiteToTurn && !humanIsPlayingAsWhite)|| (!board.WhiteToTurn && humanIsPlayingAsWhite)) && gameMode == GameMode.AgainstComputer)
                {
                    //Computers move
                    Move computersMove = chessComputer.FindTheBestMoveForPosition(board, board.WhiteToTurn);
                    int f = board.board[computersMove.start.x, computersMove.start.y];
                    Console.WriteLine(ChessLibrary.OutputHumanMove(computersMove,f));
                    board.InputMove(computersMove);

                }
                else
                {
                    //Human Move
                    inputMove = Console.ReadLine();
                    if (inputMove == "l")
                    {
                        board.Quit();
                        chessComputer.Quit();
                        break;
                    }

                    List<char> inputMoveSplited = inputMove.ToList<char>();
                    string figure = "";
                    if (inputMoveSplited.Count > 5)
                    {
                        figure = inputMoveSplited[0].ToString();
                        inputMoveSplited.RemoveAt(0);
                    }
                    if (inputMoveSplited.Count < 5)
                    {
                        Console.WriteLine("Invalid input. Example: Qe2-e4");
                        continue;
                    }
                    string startField = inputMoveSplited[0].ToString() + inputMoveSplited[1].ToString();
                    string endField = inputMoveSplited[3].ToString() + inputMoveSplited[4].ToString();
                    board.InputHumanMove(figure, startField, endField);
                }
                board.OutputBoard();
                if (board.CheckState == Chess.CheckState.WhiteAreChecked || board.CheckState == Chess.CheckState.WhiteAreDoubleChecked)
                    Console.WriteLine("White are checked");
                if (board.CheckState == Chess.CheckState.BlackAreChecked || board.CheckState == Chess.CheckState.BlackAreDoubleChecked)
                    Console.WriteLine("Black are checked");
            }
            if (board.GameResult == Result.Draw)
                Console.WriteLine("Draw");
            else if (board.GameResult == Result.WhiteWon)
                Console.WriteLine("White won!");
            else if (board.GameResult == Result.BloackWon)
                Console.WriteLine("Black won!");
            string input = Console.ReadLine();
            if (input == "l")
            {
                board.Quit();
                chessComputer.Quit();
            }
            Console.ReadLine();
        }
    }

}
