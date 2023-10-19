using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chess
{
    class ChessComputer
    {
        public ChessComputer()
        {

        }
        Random random = new Random();
        static StreamWriter streamWriter = new StreamWriter(File.Create("computations.txt"));
        //public bool WhiteToTurn = true;
        //public Result? GameResult = null;

        /*private Vector whiteKingPosition;
        private Vector blackKingPositi*/
        public void Quit()
        {
            streamWriter.Close();
        }
        public Move FindTheBestMoveForPosition(int[,]position, bool forWhite)
        {
            /*List<Move> possibleMoves = ChessLibrary.FindAllPosibleMoves(forWhite, position, castlingPossibilityHistory);
            int index = random.Next(possibleMoves.Count);
            return possibleMoves[index];*/
            Move bestMove;
            streamWriter.WriteLine("------------------------------------------------------");
            streamWriter.WriteLine("------------------------------------------------------");
            streamWriter.WriteLine("------------------------------------------------------");
            //EstimatePosition(position, 0, forWhite, castlingPossibilityHistory, out bestMove);
            double limit;
            if (forWhite)
                limit = 10;
            else
                limit = -10;
            DateTime start = DateTime.Now;
            FindMaxOrMinMove(position, 0, forWhite, limit, out bestMove);
            DateTime end = DateTime.Now;
            TimeSpan dif = end - start;
            Console.WriteLine(dif);
            return bestMove;

        }
        private double FindMaxOrMinMove(int[,] position, int depth, bool max, double limitEstimation, out Move bestMove, bool[] _castlingPosibilityFromHistory=null)
        {
            depth++;
            string indent = "";
            for (int i = 0; i < depth; i++)
                indent += "    ";
            List<Move> possibleMoves = ChessLibrary.FindAllPosibleMoves(max, position, _castlingPosibilityFromHistory);
            bestMove = possibleMoves[0];
            if (depth == 6)
            {
                //Final estimation

                return MaterialEstimation(position);
            }
            //List<Move> possibleMoves = ChessLibrary.FindAllPosibleMoves(forWhite, position, _castlingPosibilityFromHistory);

            double totalEstimation = 0;
            double maxEstimation = -10;
            double minEstimation = 10;
            for (int i = 0; i < possibleMoves.Count; i++)
            {
                int[,] board = position.Clone() as int[,];
                bool[] castlingPosibilityFromHistory = _castlingPosibilityFromHistory.Clone() as bool[];
                Vector endField = possibleMoves[i].end;
                Vector startField = possibleMoves[i].start;
                int figure = board[startField.x, startField.y];
                int u = 0;
                if (startField.x == 1 && startField.y == 0 && endField.x == 1 && endField.y == 7)
                    u=1;
                board[endField.x, endField.y] = figure;
                board[startField.x, startField.y] = 0;
                if ((figure == 1 && endField.y == 7) || (figure == -1 && endField.y == 0))
                {
                    ChessLibrary.CreateNewFigureOnBoardAt(endField, max, ref board, false);
                }
                if (figure == 10)
                {
                    castlingPosibilityFromHistory[0] = false;
                    castlingPosibilityFromHistory[1] = false;
                }
                if (figure == -10)
                {
                    castlingPosibilityFromHistory[2] = false;
                    castlingPosibilityFromHistory[3] = false;
                }
                if (figure == 5)
                {
                    if (startField.x == 0)
                        castlingPosibilityFromHistory[1] = false;
                    if (startField.x == 7)
                        castlingPosibilityFromHistory[0] = false;
                }
                if (figure == -5)
                {
                    if (startField.x == 0)
                        castlingPosibilityFromHistory[3] = false;
                    if (startField.x == 7)
                        castlingPosibilityFromHistory[2] = false;
                }
                MoveType moveType = ChessLibrary.DetermineMoveType(figure, startField, endField, board);
                if (moveType == MoveType.Castling)
                {
                    if (endField.x == 6 && endField.y == 0)
                    {
                        board[5, 0] = 5;
                        board[7, 0] = 0;
                    }
                    if (endField.x == 2 && endField.y == 0)
                    {
                        board[3, 0] = 5;
                        board[0, 0] = 0;
                    }
                    if (endField.x == 6 && endField.y == 7)
                    {
                        board[5, 7] = -5;
                        board[7, 7] = 0;
                    }
                    if (endField.x == 2 && endField.y == 7)
                    {
                        board[3, 7] = -5;
                        board[0, 7] = 0;
                    }
                }
                Result? GameResult = ChessLibrary.CheckMate(!max, board);
                double moveEstimation = 0;
                if (GameResult != null)
                {
                    if (GameResult == Result.Draw)
                    {
                        moveEstimation = 0;
                    }
                    else if (GameResult == Result.WhiteWon)
                    {
                        moveEstimation = 100;
                    }
                    else if (GameResult == Result.BloackWon)
                    {
                        moveEstimation = -100;
                    }
                    //OutputBoard(board);
                }
                else
                {
                    Move bestContinuation;

                    double limitForNextStep;
                    if (max)
                        limitForNextStep = maxEstimation;
                    else
                        limitForNextStep = minEstimation;
                    //moveEstimation = FindMaxOrMinMove(board, depth, !max, castlingPosibilityFromHistory,limitForNextStep, out bestContinuation);

                }
                if (moveEstimation > maxEstimation)
                {
                    maxEstimation = moveEstimation;
                    if (max)
                    {
                        bestMove = possibleMoves[i];
                        if (moveEstimation > limitEstimation)
                        {
                            string logString2 = ChessLibrary.OutputHumanMove(possibleMoves[i], figure);
                            streamWriter.WriteLine(indent + logString2 + "(" + moveEstimation.ToString() + ")*");
                            return moveEstimation;

                        }
                    }
                }
                if (moveEstimation < minEstimation)
                {
                    minEstimation = moveEstimation;
                    if (!max)
                    {
                        bestMove = possibleMoves[i];
                        if (moveEstimation < limitEstimation)
                        {
                            string logString2 = ChessLibrary.OutputHumanMove(possibleMoves[i], figure);
                            streamWriter.WriteLine(indent + logString2 + "(" + moveEstimation.ToString() + ")*");
                            return moveEstimation;
                        }
                        
                    }
                }
                //totalEstimation += moveEstimation;
                string logString = ChessLibrary.OutputHumanMove(possibleMoves[i], figure);
                streamWriter.WriteLine(indent + logString + "(" + moveEstimation.ToString() + ")");
            }
            if (max)
                return maxEstimation;
            else
                return minEstimation;
        }

        private double EstimatePosition(int[,]position, int depth, bool forWhite, bool[] _castlingPosibilityFromHistory, out Move bestMove)
        {
            depth++;
            string indent = "";
            for (int i = 0; i < depth; i++)
               indent += "    ";
            List<Move> possibleMoves = ChessLibrary.FindAllPosibleMoves(forWhite, position, _castlingPosibilityFromHistory);
            bestMove = possibleMoves[0];
            if (depth == 4)
            {
                //Final estimation
                
                return MaterialEstimation(position);
            }
            //List<Move> possibleMoves = ChessLibrary.FindAllPosibleMoves(forWhite, position, _castlingPosibilityFromHistory);

            double totalEstimation = 0;
            double maxEstimation = -10;
            double minEstimation = 10;
            for(int i=0;i<possibleMoves.Count;i++)
            {
                int[,] board = position.Clone() as int[,];
                bool[] castlingPosibilityFromHistory = _castlingPosibilityFromHistory.Clone() as bool[];
                Vector endField = possibleMoves[i].end;
                Vector startField = possibleMoves[i].start;
                int figure = board[startField.x, startField.y];
                board[endField.x, endField.y] = figure;
                board[startField.x, startField.y] = 0;
                if ((figure == 1 && endField.y == 7) || (figure == -1 && endField.y == 0))
                {
                    ChessLibrary.CreateNewFigureOnBoardAt(endField, forWhite, ref board, false);
                }
                if (figure == 10)
                {
                    castlingPosibilityFromHistory[0] = false;
                    castlingPosibilityFromHistory[1] = false;
                }
                if (figure == -10)
                {
                    castlingPosibilityFromHistory[2] = false;
                    castlingPosibilityFromHistory[3] = false;
                }
                if (figure == 5)
                {
                    if (startField.x == 0)
                        castlingPosibilityFromHistory[1] = false;
                    if (startField.x == 7)
                        castlingPosibilityFromHistory[0] = false;
                }
                if (figure == -5)
                {
                    if (startField.x == 0)
                        castlingPosibilityFromHistory[3] = false;
                    if (startField.x == 7)
                        castlingPosibilityFromHistory[2] = false;
                }
                MoveType moveType = ChessLibrary.DetermineMoveType(figure, startField, endField, board);
                if (moveType == MoveType.Castling)
                {
                    if (endField.x == 6 && endField.y == 0)
                    {
                        board[5, 0] = 5;
                        board[7, 0] = 0;
                    }
                    if (endField.x == 2 && endField.y == 0)
                    {
                        board[3, 0] = 5;
                        board[0, 0] = 0;
                    }
                    if (endField.x == 6 && endField.y == 7)
                    {
                        board[5, 7] = -5;
                        board[7, 7] = 0;
                    }
                    if (endField.x == 2 && endField.y == 7)
                    {
                        board[3, 7] = -5;
                        board[0, 7] = 0;
                    }
                }
                Result? GameResult = ChessLibrary.CheckMate(!forWhite, board);
                double moveEstimation = 0;
                if (GameResult != null)
                {
                    if (GameResult == Result.Draw)
                    {
                        moveEstimation = 0;
                    }
                    else if (GameResult == Result.WhiteWon)
                    {
                        moveEstimation = 10;
                    }
                    else if (GameResult == Result.BloackWon)
                    {
                        moveEstimation = -10;
                    }
                    //OutputBoard(board);
                }
                else
                {
                    Move bestContinuation;
                    moveEstimation = EstimatePosition(board, depth, !forWhite, castlingPosibilityFromHistory, out bestContinuation);
                    
                }
                if (moveEstimation > maxEstimation)
                {
                    maxEstimation = moveEstimation;
                    if (forWhite)
                        bestMove = possibleMoves[i];
                }
                if (moveEstimation < minEstimation)
                {
                    minEstimation = moveEstimation;
                    if (!forWhite)
                        bestMove = possibleMoves[i];
                }
                totalEstimation += moveEstimation;
                string logString = ChessLibrary.OutputHumanMove(possibleMoves[i], figure);
                streamWriter.WriteLine(indent + logString + "(" + moveEstimation.ToString() + ")");
            }
            totalEstimation /= possibleMoves.Count;
            return totalEstimation;
        }
        private double MaterialEstimation(int[,] position)
        {
            double output = 0;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    output += position[i, j];
            return output;
        }
    }
    public enum MoveType
    {
        Ordinary,
        PawnTransformation,
        Castling,
        TakingOnThePass,
    }
}
