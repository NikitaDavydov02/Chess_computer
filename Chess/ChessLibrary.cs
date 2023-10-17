using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chess
{
    class ChessLibrary
    {
        private static  List<Move> castlingMoves = new List<Move>();
        public static void Init()
        {
            castlingMoves = new List<Move>();
            castlingMoves.Add(new Move(new Vector(4, 0), new Vector(6, 0)));
            castlingMoves.Add(new Move(new Vector(4, 7), new Vector(6, 7)));
            castlingMoves.Add(new Move(new Vector(4, 0), new Vector(2, 0)));
            castlingMoves.Add(new Move(new Vector(4, 7), new Vector(2, 7)));
        }
        public static bool InputMove(Move move, int[,] board, ref bool WhiteToTurn, ref Result? GameResult, ref bool[] castlingPosibilityFromHistory)
        {
            Vector startField = move.start;
            Vector endField = move.end;
            int figure = board[startField.x, startField.y];
            if (GameResult != null)
                return false;
            int currentColorToMove = 1;
            if (!WhiteToTurn)
                currentColorToMove = -1;
            if (currentColorToMove * figure < 0)
            {
                Console.WriteLine("Wrong color to move");
                return false;
            }
            //OutputHumanMove(figure, startField, endField);
            //Check if there is a right figure at the start
            if (!ChessLibrary.IsThisMovePossible(startField, endField, board, castlingPosibilityFromHistory))
                return false;
            board[endField.x, endField.y] = figure;
            board[startField.x, startField.y] = 0;
            if ((figure == 1 && endField.y == 7) || (figure == -1 && endField.y == 0))
            {
                CreateNewFigureOnBoardAt(endField, WhiteToTurn, ref board);
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
            MoveType moveType = DetermineMoveType(figure, startField, endField, board);
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


            GameResult = CheckMate(!WhiteToTurn, board);
            if (GameResult != null)
            {
                if (GameResult == Result.Draw)
                    Console.WriteLine("Draw");
                else if (GameResult == Result.WhiteWon)
                    Console.WriteLine("White won!");
                else if (GameResult == Result.BloackWon)
                    Console.WriteLine("Black won!");
                OutputBoard(board);
                return true;
            }
            if (!ThereIsNoCheckInThisPosition(true, board))
                Console.WriteLine("White are checked");
            if (!ThereIsNoCheckInThisPosition(false, board))
                Console.WriteLine("Black are checked");
            OutputBoard(board);
            WhiteToTurn = !WhiteToTurn;

            return true;
        }
        private static void ReverseMove(int[,] position)
        {

        }
        public static void CreateNewFigureOnBoardAt(Vector v, bool white, ref int[,]board)
        {
            Console.WriteLine("Choose new figure");
            Console.WriteLine("N - knight");
            Console.WriteLine("B - bishop");
            Console.WriteLine("R - rock");
            Console.WriteLine("Q - queen");
            string input = Console.ReadLine();
            int figure = 2;
            switch (input)
            {
                case "N": figure = 2; break;
                case "B": figure = 3; break;
                case "R": figure = 5; break;
                case "Q": figure = 9; break;
            }
            if (!white)
                figure *= -1;
            board[v.x, v.y] = figure;

        }
        public static Result? CheckMate(bool forWhite, int[,] position)
        {
            List<Move> possibleMoves = FindAllPosibleMoves(forWhite, position, new bool[] { false, false, false, false });
            if (possibleMoves.Count == 0)
            {
                if (ThereIsNoCheckInThisPosition(forWhite, position))
                    return Result.Draw;
                else
                {
                    if (forWhite)
                        return Result.BloackWon;
                    else
                        return Result.WhiteWon;
                }
            }
            return null;
        }
        public static bool ThereIsNoCheckInThisPosition(bool checkIfWhiteWillBeChecked, int[,] position)
        {
            Vector kingPosition = new Vector(0, 0);
            int king = 0;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if ((checkIfWhiteWillBeChecked && position[i, j] == 10) || (!checkIfWhiteWillBeChecked && position[i, j] == -10))
                    {
                        kingPosition = new Vector(i, j);
                        king = position[kingPosition.x, kingPosition.y];
                        break;
                    }
                }
            List<Vector> minTranslations = new List<Vector>();
            minTranslations.Add(new Vector(0, 1));
            minTranslations.Add(new Vector(0, -1));
            minTranslations.Add(new Vector(1, 0));
            minTranslations.Add(new Vector(-1, 0));
            minTranslations.Add(new Vector(1, 1));
            minTranslations.Add(new Vector(1, -1));
            minTranslations.Add(new Vector(-1, 1));
            minTranslations.Add(new Vector(-1, -1));
            Vector trialField = kingPosition;
            for (int i = 0; i < minTranslations.Count; i++)
            {
                trialField = Vector.Sum(kingPosition, minTranslations[i]);
                int length = 1;
                while (trialField.x >= 0 && trialField.x < 8 && trialField.y >= 0 && trialField.y < 8)
                {
                    int figureAtTrialField = position[trialField.x, trialField.y];
                    if (figureAtTrialField * king < 0)
                    {
                        //Possible check
                        if (i < 4 && (Math.Abs(figureAtTrialField) == 5 || Math.Abs(figureAtTrialField) == 9))
                            return false;
                        if (i >= 4 && (Math.Abs(figureAtTrialField) == 3 || Math.Abs(figureAtTrialField) == 9))
                            return false;
                        if (Math.Abs(figureAtTrialField) == 10 && length == 1)
                            return false;
                    }
                    if (figureAtTrialField != 0)
                        break;
                    trialField = Vector.Sum(trialField, minTranslations[i]);
                    length++;
                }
            }
            //Knight check
            minTranslations = new List<Vector>();
            minTranslations.Add(new Vector(1, 2));
            minTranslations.Add(new Vector(1, -2));
            minTranslations.Add(new Vector(-1, 2));
            minTranslations.Add(new Vector(-1, -2));
            minTranslations.Add(new Vector(2, 1));
            minTranslations.Add(new Vector(2, -1));
            minTranslations.Add(new Vector(-2, 1));
            minTranslations.Add(new Vector(-2, -1));
            foreach (Vector move in minTranslations)
            {
                trialField = Vector.Sum(kingPosition, move);
                if (trialField.x > 7 || trialField.x < 0 || trialField.y > 7 || trialField.y < 0)
                    continue;
                if (position[trialField.x, trialField.y] * king == -30)
                    return false;
            }
            //Pawn check
            minTranslations = new List<Vector>();
            if (king == 10)
            {
                minTranslations.Add(new Vector(1, 1));
                minTranslations.Add(new Vector(-1, 1));
            }
            if (king == -10)
            {
                minTranslations.Add(new Vector(1, -1));
                minTranslations.Add(new Vector(-1, -1));
            }
            foreach (Vector move in minTranslations)
            {
                trialField = Vector.Sum(kingPosition, move);
                if (trialField.x > 7 || trialField.x < 0 || trialField.y > 7 || trialField.y < 0)
                    continue;
                if (position[trialField.x, trialField.y] * king == -10)
                    return false;
            }
            /*List<Move> opponentMoves = new List<Move>();
            opponentMoves = FindAllPosibleMoves(!checkIfWhiteWillBeChecked, position,new bool[] { false, false, false, false }, true);
            foreach (Move opponentMove in opponentMoves)
            {
                if (position[opponentMove.end.x, opponentMove.end.y] == 10 && checkIfWhiteWillBeChecked)
                    return false;
                if (position[opponentMove.end.x, opponentMove.end.y] == -10 && !checkIfWhiteWillBeChecked)
                    return false;
            }*/

            return true;
        }
        private static bool ThereIsNoCheckAfterThisMove(Move move, int[,] _position)
        {
            bool checkIfWhiteWillBeChecked = true;
            if (_position[move.start.x, move.start.y] < 0)
                checkIfWhiteWillBeChecked = false;
            int[,] position = _position.Clone() as int[,];
            position[move.end.x, move.end.y] = position[move.start.x, move.start.y];
            position[move.start.x, move.start.y] = 0;
            return ThereIsNoCheckInThisPosition(checkIfWhiteWillBeChecked, position);
        }
        public static List<Move> FindAllPosibleMoves(bool forWhite, int[,] position, bool[] castlingPosibilityFromHistory, bool withoutCheckOnCheck = false)
        {
            List<Move> output = new List<Move>();
            int color = 1;
            if (!forWhite)
                color = -1;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (position[i, j] * color > 0)
                    {
                        foreach (Move m in FindAllPosibleMovesForFigure(new Vector(i, j), position, castlingPosibilityFromHistory, withoutCheckOnCheck))
                            output.Add(m);
                    }
                }
            return output;
        }
        private static bool IsThisMovePossible(Vector startField, Vector endField, int[,] board, bool[] castlingPosibilityFromHistory)
        {
            int figure = board[startField.x, startField.y];
            if (board[startField.x, startField.y] != figure)
                return false;
            //Check if figure can make this move
            //  1. Direction
            if (!IfMoveCanDoneByFigureFormaly(figure, startField, endField, board, castlingPosibilityFromHistory))
                return false;
            //  2. No obsticles
            if (!IfThereIsNoObsticlesForMove(figure, startField, endField, board))
                return false;
            //Check if there is no check after this move
            if (!ThereIsNoCheckAfterThisMove(new Move(startField, endField), board))
                return false;
            return true;
        }
        public static string OutputHumanMove(Move move, int figure)
        {
            Vector start = move.start;
            Vector end = move.end;
            string figureLetter = NumberToFigure(figure);
            string startField = NumberToHorizontal(start.x) + (start.y + 1).ToString();
            string endField = NumberToHorizontal(end.x) + (end.y + 1).ToString();
            string output = figureLetter + startField + "-" + endField;
            return output;
        }
        public static int HorizontalToNumber(string hor)
        {
            switch (hor)
            {
                case "a": return 0;
                case "b": return 1;
                case "c": return 2;
                case "d": return 3;
                case "e": return 4;
                case "f": return 5;
                case "g": return 6;
                case "h": return 7;
                default: return -1;
            }
        }
        public static string NumberToHorizontal(int i)
        {
            switch (i)
            {
                case 0: return "a";
                case 1: return "b";
                case 2: return "c";
                case 3: return "d";
                case 4: return "e";
                case 5: return "f";
                case 6: return "g";
                case 7: return "h";
                default: return "s";
            }
        }
        public static string NumberToFigure(int figure)
        {
            switch (Math.Abs(figure))
            {
                case 1: return "";
                case 2: return "N";
                case 3: return "B";
                case 5: return "R";
                case 9: return "Q";
                case 10: return "K";
                default: return "Null";
            }
        }
        public static int FigureLetterToNumber(string figure, bool white)
        {
            int output = 0;
            switch (figure)
            {
                case "": output = 1; break;
                case "N": output = 2; break;
                case "B": output = 3; break;
                case "R": output = 5; break;
                case "Q": output = 9; break;
                case "K": output = 10; break;
                default: return 0;
            }
            if (!white)
                output *= -1;
            return output;

        }
        private static List<Move> FindAllPosibleMovesForFigure(Vector start, int[,] position, bool[] castlingPosibilityFromMovesHistory, bool withoutCheckOnCheck = false)
        {
            int figure = position[start.x, start.y];
            List<Move> formalyPossible = FindAllFormalyPosibleMovesForFigure(start, position, castlingPosibilityFromMovesHistory, withoutCheckOnCheck);
            //IfThere is a check
            List<Move> impossibleMoves = new List<Move>();
            for (int i = 0; i < formalyPossible.Count; i++)
            {
                if (withoutCheckOnCheck)
                    break;
                /*if (!ThereIsNoCheckAfterThisMove(formalyPossible[i], position))
                    impossibleMoves.Add(formalyPossible[i]);*/
                if (!ChessLibrary.IsThisMovePossible(formalyPossible[i].start, formalyPossible[i].end, position, castlingPosibilityFromMovesHistory))
                    impossibleMoves.Add(formalyPossible[i]);

            }
            for (int i = 0; i < impossibleMoves.Count; i++)
                formalyPossible.Remove(impossibleMoves[i]);
            return formalyPossible;
        }
        private static List<Move> FindAllFormalyPosibleMovesForLinearFigure(int figure, Vector start, int[,] position)
        {
            List<Move> output = new List<Move>();
            List<Vector> minTranslations = new List<Vector>();
            if (Math.Abs(figure) == 5 || Math.Abs(figure) == 9)
            {
                minTranslations.Add(new Vector(0, 1));
                minTranslations.Add(new Vector(0, -1));
                minTranslations.Add(new Vector(1, 0));
                minTranslations.Add(new Vector(-1, 0));
            }
            if (Math.Abs(figure) == 9 || Math.Abs(figure) == 3)
            {
                minTranslations.Add(new Vector(1, 1));
                minTranslations.Add(new Vector(1, -1));
                minTranslations.Add(new Vector(-1, 1));
                minTranslations.Add(new Vector(-1, -1));
            }
            foreach (Vector minTranslation in minTranslations)
            {
                Vector trialEnd = start;
                for (int i = 1; i <= 7; i++)
                {
                    trialEnd = Vector.Sum(trialEnd, minTranslation);
                    if (trialEnd.x < 0 || trialEnd.x > 7 || trialEnd.y < 0 || trialEnd.y > 7)
                        break;
                    if (position[trialEnd.x, trialEnd.y] * figure == 0)
                        output.Add(new Move(start, trialEnd));
                    else
                    {
                        if (position[trialEnd.x, trialEnd.y] * figure < 0)
                            output.Add(new Move(start, trialEnd));
                        break;
                    }
                }
            }
            return output;
        }
        private static List<Move> FindAllFormalyPosibleMovesForFigure(Vector start, int[,] position, bool[] castlingPosibilityFromHistory, bool withoutCheckOnCheck = false)
        {
            //Find all moves with right directions and without obsticles but does not include check check
            List<Move> output = new List<Move>();
            int figure = position[start.x, start.y];
            if (figure == 0)
                return output;
            Vector trialEnd;
            switch (Math.Abs(figure))
            {
                case 10:
                    int deltaX = 0;
                    int deltaY = 0;
                    for (deltaX = -1; deltaX <= 1; deltaX++)
                        for (deltaY = -1; deltaY <= 1; deltaY++)
                        {
                            trialEnd = new Vector(start.x + deltaX, start.y + deltaY);
                            if (trialEnd.x < 0 || trialEnd.x > 7 || trialEnd.y < 0 || trialEnd.y > 7)
                                continue;
                            if (position[trialEnd.x, trialEnd.y] * figure <= 0)
                                output.Add(new Move(start, trialEnd));
                        }
                    foreach (Move castlingMove in castlingMoves)
                        if (IfTheCastlingIsPossible(figure, castlingMove.start, castlingMove.end, position, castlingPosibilityFromHistory))
                            output.Add(new Move(castlingMove.start, castlingMove.end));
                    break;
                case 5:
                    return FindAllFormalyPosibleMovesForLinearFigure(figure, start, position);
                    break;
                case 3:
                    return FindAllFormalyPosibleMovesForLinearFigure(figure, start, position);
                    break;
                case 9:
                    return FindAllFormalyPosibleMovesForLinearFigure(figure, start, position);
                    break;
                case 2:
                    List<Vector> possibleTranslations = new List<Vector>();
                    possibleTranslations.Add(new Vector(1, 2));
                    possibleTranslations.Add(new Vector(-1, 2));
                    possibleTranslations.Add(new Vector(1, -2));
                    possibleTranslations.Add(new Vector(-1, -2));
                    possibleTranslations.Add(new Vector(2, 1));
                    possibleTranslations.Add(new Vector(2, -1));
                    possibleTranslations.Add(new Vector(-2, 1));
                    possibleTranslations.Add(new Vector(-2, -1));
                    foreach (Vector posibleTranslation in possibleTranslations)
                    {
                        trialEnd = Vector.Sum(start, posibleTranslation);
                        if (trialEnd.x < 0 || trialEnd.x > 7 || trialEnd.y < 0 || trialEnd.y > 7)
                            continue;
                        if (position[trialEnd.x, trialEnd.y] * figure <= 0)
                            output.Add(new Move(start, trialEnd));
                    }
                    break;
                case 1:
                    if (figure == 1)
                    {
                        if (position[start.x, start.y + 1] == 0 && !withoutCheckOnCheck)
                            output.Add(new Move(start, new Vector(start.x, start.y + 1)));
                        if (start.y == 1 && position[start.x, start.y + 2] == 0 && position[start.x, start.y + 1] == 0 && !withoutCheckOnCheck)
                            output.Add(new Move(start, new Vector(start.x, start.y + 2)));
                        if (start.x + 1 < 8 && position[start.x + 1, start.y + 1] < 0)
                            output.Add(new Move(start, new Vector(start.x + 1, start.y + 1)));
                        if (start.x - 1 >= 0 && position[start.x - 1, start.y + 1] < 0)
                            output.Add(new Move(start, new Vector(start.x - 1, start.y + 1)));
                    }
                    if (figure == -1)
                    {
                        if (position[start.x, start.y - 1] == 0 && !withoutCheckOnCheck)
                            output.Add(new Move(start, new Vector(start.x, start.y - 1)));
                        if (start.y == 6 && position[start.x, start.y - 2] == 0 && position[start.x, start.y - 1] == 0 && !withoutCheckOnCheck)
                            output.Add(new Move(start, new Vector(start.x, start.y - 2)));
                        if (start.x + 1 < 8 && position[start.x + 1, start.y - 1] > 0)
                            output.Add(new Move(start, new Vector(start.x + 1, start.y - 1)));
                        if (start.x - 1 >= 0 && position[start.x - 1, start.y - 1] > 0)
                            output.Add(new Move(start, new Vector(start.x - 1, start.y - 1)));
                    }
                    break;
                default: break;
            }
            return output;
        }
        private static bool IfThereIsNoObsticlesForMove(int figure, Vector start, Vector end, int[,] position)
        {
            if (Math.Abs(figure) == 2)
                return true;
            Vector move = Vector.Substract(end, start);
            int absX = Math.Abs(move.x);
            int absY = Math.Abs(move.y);
            int maxDelta = 0;
            if (absX > absY)
                maxDelta = absX;
            else
                maxDelta = absY;
            Vector minTranslation = new Vector(move.x / maxDelta, move.y / maxDelta);
            Vector trialEnd = Vector.Sum(start, minTranslation);
            for (int i = 1; i < maxDelta; i++)
            {
                if (position[trialEnd.x, trialEnd.y] != 0)
                    return false;
                trialEnd = Vector.Sum(trialEnd, minTranslation);
            }
            if (position[trialEnd.x, trialEnd.y] * figure > 0)
                return false;
            if (Math.Abs(figure) == 1 && Vector.Substract(end, start).x == 0 && position[end.x, end.y] != 0)
                return false;
            return true;
        }
        public static void OutputBoard(int[,] position, bool toConsole = true)
        {
            for (int i = 7; i >= 0; i--)
            {
                string line = "";
                for (int j = 0; j < 8; j++)
                {
                    int f = position[j, i];
                    if (f == 10)
                        line += " K" + " ";
                    else if (f == -10)
                        line += "-K" + " ";
                    else if (f < 0)
                        line += position[j, i].ToString() + " ";
                    else
                        line += " " + position[j, i].ToString() + " ";
                }
                if (toConsole)
                    Console.WriteLine(line);
                //else
                 //   writer.WriteLine(line);
            }
            Console.WriteLine();
        }
        private static void OutputAllPossibleMoves(bool forWhite, int[,] position, bool[] castlingPosibilityFromHistory, bool pawnsOnlyCapture = false)
        {
            List<Move> moves = FindAllPosibleMoves(forWhite, position, castlingPosibilityFromHistory, pawnsOnlyCapture);
            foreach (Move move in moves)
            {
                Vector end = move.end;
                Vector start = move.start;
                int endFigure = position[end.x, end.y];
                position[end.x, end.y] = position[start.x, start.y];
                position[start.x, start.y] = 0;
                OutputBoard(position);
                position[start.x, start.y] = position[end.x, end.y];
                position[end.x, end.y] = endFigure;
            }
        }
        public static MoveType DetermineMoveType(int figure, Vector start, Vector end, int[,] position)
        {
            Vector move = Vector.Substract(end, start);
            int absX = Math.Abs(move.x);
            int absY = Math.Abs(move.y);
            MoveType moveType = MoveType.Ordinary;
            if (Math.Abs(figure) == 10 && absX == 2)
                moveType = MoveType.Castling;
            if (Math.Abs(figure) == 1 && absX == 1 && position[end.x, end.y] == 0)
                moveType = MoveType.TakingOnThePass;
            if ((figure == 1 && end.y == 7) || (figure == -1 && end.y == 0))
                moveType = MoveType.PawnTransformation;
            return moveType;
        }
        private static bool IfMoveCanDoneByFigureFormaly(int figure, Vector start, Vector end, int[,] position, bool[] castlingPosibilityFromHistory)
        {

            Vector move = Vector.Substract(end, start);
            int absX = Math.Abs(move.x);
            int absY = Math.Abs(move.y);

            MoveType moveType = DetermineMoveType(figure, start, end, position);
            if (moveType == MoveType.Ordinary || moveType == MoveType.PawnTransformation)
            {
                switch (Math.Abs(figure))
                {
                    case 10:
                        if (absX <= 1 && absY <= 1)
                            return true;
                        break;
                    case 5:
                        if (absX == 0 || absY == 0)
                            return true;
                        break;
                    case 3:
                        if (absX == absY)
                            return true;
                        break;
                    case 9:
                        if (absX == absY)
                            return true;
                        if (absX == 0 || absY == 0)
                            return true;
                        break;
                    case 2:
                        if ((absX == 1 && absY == 2) || (absY == 1 && absX == 2))
                            return true;
                        break;
                    case 1:
                        if (figure == 1 && absX == 0 && move.y == 1)
                            return true;
                        if (figure == -1 && absX == 0 && move.y == -1)
                            return true;
                        if (figure == 1 && absX == 0 && move.y == 2 && start.y == 1)
                            return true;
                        if (figure == -1 && absX == 0 && move.y == -2 && start.y == 6)
                            return true;
                        if (figure == 1 && absX == 1 && move.y == 1 && position[start.x, start.y] * position[end.x, end.y] < 0)
                            return true;
                        if (figure == -1 && absX == 1 && move.y == -1 && position[start.x, start.y] * position[end.x, end.y] < 0)
                            return true;
                        break;
                    default: return false;
                }
            }
            else if (moveType == MoveType.Castling)
            {
                return IfTheCastlingIsPossible(figure, start, end, position, castlingPosibilityFromHistory);
            }
            else if (moveType == MoveType.TakingOnThePass)
            {

            }
            return false;

        }
        private static bool IfTheCastlingIsPossible(int figure, Vector start, Vector end, int[,] position, bool[] figuresPossibilityForastling)
        {
            if (!figuresPossibilityForastling[0] && !figuresPossibilityForastling[1] && !figuresPossibilityForastling[2] && !figuresPossibilityForastling[3])
                return false;
            //Check that space is free
            //Check that kings and rocks did not moved
            if (end.x == 6 && end.y == 0)
                if (position[5, 0] != 0 || position[6, 0] != 0 || !figuresPossibilityForastling[0])
                    return false;
            if (end.x == 6 && end.y == 7)
                if (position[5, 7] != 0 || position[6, 7] != 0 || !figuresPossibilityForastling[2])
                    return false;
            if (end.x == 2 && end.y == 0)
                if (position[3, 0] != 0 || position[1, 0] != 0 || position[2, 0] != 0 || !figuresPossibilityForastling[1])
                    return false;
            if (end.x == 2 && end.y == 7)
                if (position[3, 7] != 0 || position[1, 7] != 0 || position[2, 7] != 0 || !figuresPossibilityForastling[3])
                    return false;
            //Check that there is no check
            int horizontal = 0;
            if (figure == -10)
                horizontal = 6;
            if (!ThereIsNoCheckInThisPosition(true, position))
                return false;
            //return false;
            if (end.x == 6)
            {
                if (!ThereIsNoCheckAfterThisMove(new Move(new Vector(4, horizontal), new Vector(5, horizontal)), position))
                    return false;
                if (!ThereIsNoCheckAfterThisMove(new Move(new Vector(4, horizontal), new Vector(6, horizontal)), position))
                    return false;
            }
            if (end.x == 2)
            {
                if (!ThereIsNoCheckAfterThisMove(new Move(new Vector(4, horizontal), new Vector(3, horizontal)), position))
                    return false;
                if (!ThereIsNoCheckAfterThisMove(new Move(new Vector(4, horizontal), new Vector(2, horizontal)), position))
                    return false;
            }
            return true;
        }
        public static int[,] ReadPositionFromFile(string path)
        {
            int[,] output = new int[8, 8];
            using(StreamReader reader= new StreamReader(File.OpenRead(path)))
            {
                for(int i=0; i < 8; i++)
                {
                    string line = reader.ReadLine();
                    string[] lineSplited = line.Split(' ');
                    for(int j = 0; j < 8; j++)
                    {
                        int field = Convert.ToInt32(lineSplited[j]);
                        output[j, 7 - i] = field;
                    }
                }
            }
            return output;
        }
    }
}
