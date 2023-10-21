using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Chess
{
    [Serializable]
    class Board
    {
        //-------------------PUBLIC----------------------------//
       // Stack<Board> boardHistory;
        public int[,] board { get; private set; }
        public bool HumanAsWhite { get; private set; } = true;
        public Result? GameResult
        {
            get { return gameResult; }
        }
        public bool WhiteToTurn
        {
            get { return whiteToTurn; }
        }
        //-------------------PRIVATE----------------------------//
        private bool whiteToTurn;
        private Result? gameResult = null;
        public GameMode GameMode { get; private set; }

        private ChessComputer chessComputer;
        //-------------------STAFF----------------------------//
        private Vector whiteKingPosition;
        private Vector blackKingPosition;

        private List<Vector> whiteFiguresPosition;
        private List<Vector> blackFiguresPosition;


        /*private List<Vector> whiteKnightsPosition;
        private List<Vector> blackKnightsPosition;

        private List<Vector> whitePawnsPosition;
        private List<Vector> blackPawnsPosition;*/

        private Dictionary<Vector, List<Move>> whitePossibleMovesWithoutCheckCheck;
        private Dictionary<Vector, List<Move>> blackPossibleMovesWithoutCheckCheck;

        public List<Move> whitePossibleMovesWithCheckCheck { get; private set; }
        public List<Move> blackPossibleMovesWithCheckCheck { get; private set; }

        private List<Vector> checkSourcesForWhite;
        private List<Vector> checkSourcesForBlack;

        List<Move> movesToRemove;

        Dictionary<Vector, List<Move>> newWhitePossibleMoves;
        Dictionary<Vector, List<Move>> newBlackPossibleMoves;

        //private Dictionary<Vector, int[]> hitMap;

        private CheckState? CheckState=null;

        //private bool[] castlingPosibilityFromHistory;
        //private List<Move> castlingMoves = new List<Move>();
        //short white
        //long white
        //short black
        //long white
        //StreamWriter writer;
        bool outputToLog = true;
        List<Vector> minTranslations;
        List<Vector> knightMinTranslations;
            
        
        public Board(GameMode gameMode, bool humanAsWhite = true)
        {
            
            chessComputer = new ChessComputer();
            Init(gameMode, humanAsWhite);
        }
        public void Quit()
        {
           // writer.Close();
            chessComputer.Quit();
        }
        private void Init(GameMode gameMode, bool humanAsWhite = true)
        {
            GameMode = gameMode;
            HumanAsWhite = humanAsWhite;
            //writer = new StreamWriter(File.Create("log_board3.txt"));
            whiteToTurn = true;
            gameResult = null;
            whitePossibleMovesWithoutCheckCheck = new Dictionary<Vector, List<Move>>();
            blackPossibleMovesWithoutCheckCheck = new Dictionary<Vector, List<Move>>();
            whitePossibleMovesWithCheckCheck = new List<Move>();
            blackPossibleMovesWithCheckCheck = new List<Move>();
            checkSourcesForWhite = new List<Vector>();
            checkSourcesForBlack = new List<Vector>();
            newWhitePossibleMoves = new Dictionary<Vector, List<Move>>();
            newBlackPossibleMoves = new Dictionary<Vector, List<Move>>();

            movesToRemove = new List<Move>();
            minTranslations = new List<Vector>();
            minTranslations.Add(new Vector(0, 1));
            minTranslations.Add(new Vector(0, -1));
            minTranslations.Add(new Vector(1, 0));
            minTranslations.Add(new Vector(-1, 0));
            minTranslations.Add(new Vector(1, 1));
            minTranslations.Add(new Vector(1, -1));
            minTranslations.Add(new Vector(-1, 1));
            minTranslations.Add(new Vector(-1, -1));
            knightMinTranslations = new List<Vector>();
            knightMinTranslations.Add(new Vector(2, 1));
            knightMinTranslations.Add(new Vector(2, -1));
            knightMinTranslations.Add(new Vector(1, 2));
            knightMinTranslations.Add(new Vector(-1, 2));
            knightMinTranslations.Add(new Vector(-2, -1));
            knightMinTranslations.Add(new Vector(-2, 1));
            knightMinTranslations.Add(new Vector(1, -2));
            knightMinTranslations.Add(new Vector(-1, -2));
            /*castlingPosibilityFromHistory = new bool[4];
            for (int i = 0; i < 4; i++)
                castlingPosibilityFromHistory[i] = true;*/

            /*castlingMoves = new List<Move>();
            castlingMoves.Add(new Move(new Vector(4, 0), new Vector(6, 0)));
            castlingMoves.Add(new Move(new Vector(4, 7), new Vector(6, 7)));
            castlingMoves.Add(new Move(new Vector(4, 0), new Vector(2, 0)));
            castlingMoves.Add(new Move(new Vector(4, 7), new Vector(2, 7)));*/

            board = new int[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = 0;
                    whitePossibleMovesWithoutCheckCheck.Add(new Vector(i, j), new List<Move>());
                    blackPossibleMovesWithoutCheckCheck.Add(new Vector(i, j), new List<Move>());
                    //whitePossibleMovesWithCheckCheck.Add(new Vector(i, j), new List<Move>());
                    //blackPossibleMovesWithCheckCheck.Add(new Vector(i, j), new List<Move>());
                }
                    
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
            board[2, 7] = -3;
            board[5, 7] = -3;

            board[3, 0] = 9;
            board[3, 7] = -9;

            board[4, 0] = 10;
            board[4, 7] = -10;

            whiteFiguresPosition = new List<Vector>();
            blackFiguresPosition = new List<Vector>();
            for (int i = 0; i < 8; i++)
            {
                board[i, 1] = 1;
                board[i, 6] = -1;
            }
            board = ChessLibrary.ReadPositionFromFile("checkmate1.txt");
            //ChessLibrary.ThereIsNoCheckInThisPosition(false, board);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("--------------BOARD----------------");
            Console.WriteLine("-----------------------------------");
            OutputBoard();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] > 0)
                        whiteFiguresPosition.Add(new Vector(i, j));
                    if (board[i, j] < 0)
                        blackFiguresPosition.Add(new Vector(i, j));
                    if (board[i, j] == 10)
                        whiteKingPosition = new Vector(i, j);
                    if (board[i, j] == -10)
                        blackKingPosition = new Vector(i, j);
                }
            //if (outputToLog)
            //    ChessLibrary.OutputBoard(board, false);
            InitialMoveFinding();
            //hitMap = new Dictionary<Vector, int[]>();
            ///UpdateHitmap();
            OutputAllPossibleMoves(true);
            OutputAllPossibleMoves(false);

            //boardHistory = new Stack<Board>();
            Console.WriteLine("Initialization completed");
            if (((whiteToTurn && !HumanAsWhite) || (!whiteToTurn && HumanAsWhite)) && GameMode == GameMode.AgainstComputer)
            {
                /*if (outputToLog)
                {
                    writer.WriteLine("------------------------------");
                    writer.WriteLine("Compters mive");
                }*/

                Move computersMove = chessComputer.FindTheBestMoveForPosition(this, whiteToTurn);
                Console.WriteLine(OutputHumanMove(computersMove));
                InputMove(computersMove);
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
            if (figure == 0 || startHorizontal < 0 || startHorizontal > 7 || startVertical < 0 || startVertical > 7 || finishVertical < 0 || finishVertical > 7 || finishHorizontal < 0 || finishHorizontal > 7)
            {
                Console.WriteLine("Invalid input");
                return false;
            }
            Vector start = new Vector(startVertical, startHorizontal);
            Vector end = new Vector(finishVertical, finishHorizontal);
            OutputHumanMove(new Move(start,end));
            if (!InputMove(new Move(start, end)))
            {
                Console.WriteLine("This move is imposible");
                return false;
            }

            if (((whiteToTurn && !HumanAsWhite) || (!whiteToTurn && HumanAsWhite)) && GameMode == GameMode.AgainstComputer)
            {
                Move computersMove = chessComputer.FindTheBestMoveForPosition(this, whiteToTurn);
                Console.WriteLine(OutputHumanMove(computersMove));
                InputMove(computersMove);

            }
            return true;
        }
        private string OutputHumanMove(Move move)
        {
            Vector start = move.start;
            Vector end = move.end;
            int figure = board[start.x, start.y];
            string figureLetter = ChessLibrary.NumberToFigure(figure);
            string startField = ChessLibrary.NumberToHorizontal(start.x) + (start.y + 1).ToString();
            string endField = ChessLibrary.NumberToHorizontal(end.x) + (end.y + 1).ToString();
            string output = figureLetter + startField + "-" + endField;
            return output;
        }
        public bool InputMove(Move move, int figureToCreate=9)
        {
            Vector startField = move.start;
            Vector endField = move.end;
            int figure = board[startField.x, startField.y];
            if (GameResult != null)
                return false;
            /*int currentColorToMove = 1;
            if (!WhiteToTurn)
                currentColorToMove = -1;
            if (currentColorToMove * figure < 0)
            {
                Console.WriteLine("Wrong color to move");
                return false;
            }*/
            //OutputHumanMove(figure, startField, endField);
            //Check if there is a right figure at the start
            //if (!ChessLibrary.IsThisMovePossible(startField, endField, board, castlingPosibilityFromHistory))
            //    return false;
            board[endField.x, endField.y] = figure;
            board[startField.x, startField.y] = 0;
            if (whiteToTurn)
            {
                whiteFiguresPosition.Remove(startField);
                whiteFiguresPosition.Add(endField);
                blackFiguresPosition.Remove(endField);
            }
            else
            {
                blackFiguresPosition.Remove(startField);
                blackFiguresPosition.Add(endField);
                whiteFiguresPosition.Remove(endField);
            }
            if (figure == 10)
                whiteKingPosition = endField;
            if (figure == -10)
                blackKingPosition = endField;
            if ((figure == 1 && endField.y == 7) || (figure == -1 && endField.y == 0))
            {
                CreateNewFigureOnBoardAt(endField, WhiteToTurn, figureToCreate);
            }
            //---------------------------------------------//
            //-------------BOARD IS CHANGED----------------//
            //---------------------------------------------//
            ChangeAllFormalyPossibleMovesWithoutCheckCorrectionAfterRecentMove(move, whiteToTurn);
            //Check correction
            ChangeAllPossibleMovesWithCheckCorrectionAfterRecentMove();
            OutputAllPossibleMoves(true);
            OutputAllPossibleMoves(false);
            //MoveType moveType = DetermineMoveType(figure, startField, endField, board);

            gameResult = CheckMate(WhiteToTurn);
            if (GameResult != null)
            {
                if (GameResult == Result.Draw)
                    Console.WriteLine("Draw");
                else if (GameResult == Result.WhiteWon)
                    Console.WriteLine("White won!");
                else if (GameResult == Result.BloackWon)
                    Console.WriteLine("Black won!");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("--------------BOARD----------------");
                Console.WriteLine("-----------------------------------");
                OutputBoard();
                return true;
            }
            if (CheckState==Chess.CheckState.WhiteAreChecked||CheckState==Chess.CheckState.WhiteAreDoubleChecked)
                Console.WriteLine("White are checked");
            if (CheckState == Chess.CheckState.BlackAreChecked || CheckState == Chess.CheckState.BlackAreDoubleChecked)
                Console.WriteLine("Black are checked");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("--------------BOARD----------------");
            Console.WriteLine("-----------------------------------");
            OutputBoard();
            whiteToTurn = !whiteToTurn;

            return true;
        }
        private void ChangeAllPossibleMovesWithCheckCorrectionAfterRecentMove()
        {
            //TO_DO Include check check when king is going to checked field not pins only. Ch\
            // Check processing
            //1. Если не под шахом, то:
            //Нельзя ходить под шах \/ (учтено при поиске новых ходов)
            //Нельзя ходить подставляя под шах  \/
            //2. Если под шахом
            //+ надо уходить от шаха
            whitePossibleMovesWithCheckCheck.Clear();
            foreach (Vector v in whitePossibleMovesWithoutCheckCheck.Keys)
            {
                foreach (Move m in whitePossibleMovesWithoutCheckCheck[v])
                    whitePossibleMovesWithCheckCheck.Add(m);
            }
            blackPossibleMovesWithCheckCheck.Clear();
            foreach (Vector v in blackPossibleMovesWithoutCheckCheck.Keys)
            {
                foreach (Move m in blackPossibleMovesWithoutCheckCheck[v])
                    blackPossibleMovesWithCheckCheck.Add(m);
            }
            //Нельзя ходить подставляя под шах  \/
            ExcludeForbidenMovesForKingsFromPossibleMovesWithCheckCheck();
            if (CheckState != null)
            {
                //Исключение ходов не избавляющих от шаха
                movesToRemove.Clear();
                //1. Hide
                //2. Destroy
                //3. Fence off
                if (CheckState == Chess.CheckState.WhiteAreChecked || CheckState == Chess.CheckState.WhiteAreDoubleChecked)
                {
                    //White are checked
                    foreach(Move move in whitePossibleMovesWithCheckCheck)
                    {
                        bool hide = false;
                        if (VectorMath.Equal(move.start, whiteKingPosition))
                            hide = true;
                        bool destroy = false;
                        if (VectorMath.Equal(move.end, checkSourcesForWhite[0]) && checkSourcesForWhite.Count == 0)
                            destroy = true;
                        bool fenceOff = false;
                        if (VectorMath.CBetweenAAndB(whiteKingPosition, checkSourcesForWhite[0], move.end) && checkSourcesForWhite.Count == 0)
                            fenceOff = false;
                        if (!(hide || destroy || fenceOff))
                            movesToRemove.Add(move);
                    }
                    foreach (Move m in movesToRemove)
                        whitePossibleMovesWithCheckCheck.Remove(m);
                }
                else
                {
                    //Black are checked
                    foreach (Move move in blackPossibleMovesWithCheckCheck)
                    {
                        bool hide = false;
                        if (VectorMath.Equal(move.start, blackKingPosition))
                            hide = true;
                        bool destroy = false;
                        if (VectorMath.Equal(move.end, checkSourcesForBlack[0]) && checkSourcesForBlack.Count == 0)
                            destroy = true;
                        bool fenceOff = false;
                        if (VectorMath.CBetweenAAndB(blackKingPosition, checkSourcesForBlack[0], move.end) && checkSourcesForBlack.Count == 0)
                            fenceOff = false;
                        if (!(hide || destroy || fenceOff))
                            movesToRemove.Add(move);
                    }
                    foreach (Move m in movesToRemove)
                        blackPossibleMovesWithCheckCheck.Remove(m);
                }
                
            }
        }
        private void ChangeAllFormalyPossibleMovesWithoutCheckCorrectionAfterRecentMove(Move move, bool whiteHadMoved)
        {
            newWhitePossibleMoves.Clear();
            newBlackPossibleMoves.Clear();
            //1. Change moves of figure that had moved
            if (whiteHadMoved)
            {
                whitePossibleMovesWithoutCheckCheck[move.start] = new List<Move>();
                whitePossibleMovesWithoutCheckCheck[move.end] = FindAllFormalyPosibleMovesForFigure(move.end, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
                //2. Change moves of figure that was eaten
                blackPossibleMovesWithoutCheckCheck[move.end] = new List<Move>();
            }
            else
            {
                blackPossibleMovesWithoutCheckCheck[move.start] = new List<Move>();
                blackPossibleMovesWithoutCheckCheck[move.end] = FindAllFormalyPosibleMovesForFigure(move.end, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
                //2. Change moves of figure that was eaten
                whitePossibleMovesWithoutCheckCheck[move.end] = new List<Move>();
            }
            //3. Update moves for figures that could go to start before move (special attention to pawns)
            UpdatePossibleMovesForFiguresThatCouldGoToParticularFieldBeforeRecentMove(move.start, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
            //4. Update moves for figures that could go to end before move (special attention to pawns)
            UpdatePossibleMovesForFiguresThatCouldGoToParticularFieldBeforeRecentMove(move.end, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
            
            //----------------------------------//
            //--------Check to check-----------//
            //----------------------------------//
            
            CheckState = null;
            checkSourcesForWhite.Clear();
            checkSourcesForBlack.Clear();
            if (whiteHadMoved)
            {
                foreach (List<Move> moves in newWhitePossibleMoves.Values)
                    foreach (Move newMove in moves)
                        if (board[newMove.end.x, newMove.end.y] == -10)
                        {
                            checkSourcesForBlack.Add(newMove.start);
                            if (CheckState == Chess.CheckState.BlackAreChecked)
                                CheckState = Chess.CheckState.BlackAreDoubleChecked;
                            else
                                CheckState = Chess.CheckState.BlackAreChecked;
                        }
            }
            else
            {
                foreach (List<Move> moves in newBlackPossibleMoves.Values)
                    foreach (Move newMove in moves)
                        if (board[newMove.end.x, newMove.end.y] == -10)
                        {
                            checkSourcesForWhite.Add(newMove.start);
                            if (CheckState == Chess.CheckState.WhiteAreChecked)
                                CheckState = Chess.CheckState.WhiteAreDoubleChecked;
                            else
                                CheckState = Chess.CheckState.WhiteAreChecked;
                        }
            }
        }
        private void UpdatePossibleMovesForFiguresThatCouldGoToParticularFieldBeforeRecentMove(Vector start, ref Dictionary<Vector, List<Move>> newWhitePossibleMoves, ref Dictionary<Vector, List<Move>> newBlackPossibleMoves)
        {
            Vector trialEnd = start;
            foreach (Vector direction in minTranslations)
            {
                trialEnd = VectorMath.Sum(start, direction);
                while (trialEnd.x < 8 && trialEnd.x >= 0 && trialEnd.y < 8 && trialEnd.y >= 0)
                {
                    if (board[trialEnd.x, trialEnd.y] < 0)
                    {
                        blackPossibleMovesWithoutCheckCheck[trialEnd] = FindAllFormalyPosibleMovesForFigure(trialEnd, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
                        break;
                    }
                    if (board[trialEnd.x, trialEnd.y] > 0)
                    {
                        whitePossibleMovesWithoutCheckCheck[trialEnd] = FindAllFormalyPosibleMovesForFigure(trialEnd, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
                        break;
                    }
                    trialEnd = VectorMath.Sum(trialEnd, direction);
                }
            }
            trialEnd = start;
            foreach (Vector direction in knightMinTranslations)
            {
                trialEnd = VectorMath.Sum(start, direction);
                if (trialEnd.x < 8 && trialEnd.x >= 0 && trialEnd.y < 8 && trialEnd.y >= 0)
                {
                    if (board[trialEnd.x, trialEnd.y] < 0)
                        blackPossibleMovesWithoutCheckCheck[trialEnd] = FindAllFormalyPosibleMovesForFigure(trialEnd, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
                    if (board[trialEnd.x, trialEnd.y] > 0)
                        whitePossibleMovesWithoutCheckCheck[trialEnd] = FindAllFormalyPosibleMovesForFigure(trialEnd, ref newWhitePossibleMoves, ref newBlackPossibleMoves);
                }
            }
        }
        public void OutputBoard(bool toConsole = true)
        {
            
            for (int i = 7; i >= 0; i--)
            {
                string line = "";
                for (int j = 0; j < 8; j++)
                {
                    int f = board[j, i];
                    if (f == 10)
                        line += " K" + " ";
                    else if (f == -10)
                        line += "-K" + " ";
                    else if (f < 0)
                        line += board[j, i].ToString() + " ";
                    else
                        line += " " + board[j, i].ToString() + " ";
                }
                if (toConsole)
                    Console.WriteLine(line);
                //else
                //   writer.WriteLine(line);
            }
            Console.WriteLine();
        }
        private void CreateNewFigureOnBoardAt(Vector v, bool white, int figure)
        {
            if (!white)
                figure = -Math.Abs(figure);
            else
                figure = Math.Abs(figure);
            board[v.x, v.y] = figure;

        }
        private Result? CheckMate(bool whiteHaveMoved)
        {
            if (whiteHaveMoved)
            {
                if ((CheckState == Chess.CheckState.BlackAreChecked || CheckState == Chess.CheckState.BlackAreDoubleChecked) && blackPossibleMovesWithCheckCheck.Count == 0)
                    return Result.WhiteWon;
                if (blackPossibleMovesWithCheckCheck.Count == 0)
                    return Result.Draw;
            }
            else
            {
                if ((CheckState == Chess.CheckState.WhiteAreChecked || CheckState == Chess.CheckState.WhiteAreDoubleChecked) && whitePossibleMovesWithCheckCheck.Count == 0)
                    return Result.BloackWon;
                if (whitePossibleMovesWithCheckCheck.Count == 0)
                    return Result.Draw;
            }
            return null;
        }
        private List<Move> FindAllFormalyPosibleMovesForLinearFigure(int figure, Vector start)
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
                    trialEnd = VectorMath.Sum(trialEnd, minTranslation);
                    if (trialEnd.x < 0 || trialEnd.x > 7 || trialEnd.y < 0 || trialEnd.y > 7)
                        break;
                    if (board[trialEnd.x, trialEnd.y] * figure == 0)
                        output.Add(new Move(start, trialEnd));
                    else
                    {
                        if (board[trialEnd.x, trialEnd.y] * figure < 0)
                            output.Add(new Move(start, trialEnd));
                        break;
                    }
                }
            }
            return output;
        }
        private bool IfThisFigureIsPinnedWithKing(Vector figurePos)
        {
            int figure = board[figurePos.x,figurePos.y];
            Vector vectorFromTheKing;
            Vector ownKingPosition;
            if (figure > 0)
                ownKingPosition = whiteKingPosition;
            else
                ownKingPosition = blackKingPosition;
            vectorFromTheKing = VectorMath.Substract(figurePos, ownKingPosition);
            for(int i = 0; i < minTranslations.Count; i++)
            {
                Vector direction = minTranslations[i];
                if (Math.Abs(VectorMath.Cos(direction, vectorFromTheKing) - 1) < 0.01)
                {
                    Vector trialEnd = VectorMath.Sum(figurePos, direction);
                    while (trialEnd.x < 8 && trialEnd.x >= 0 && trialEnd.y < 8 && trialEnd.y >= 0)
                    {
                        if (board[trialEnd.x, trialEnd.y]!= 0)
                        {
                            if (board[trialEnd.x, trialEnd.y] * figure < 0)
                            {
                                int enemyFigure = Math.Abs(board[trialEnd.x, trialEnd.y]);
                                if (i < 4 && (enemyFigure == 5 || enemyFigure == 9))
                                    return true;
                                if (i >= 4 && (enemyFigure == 3 || enemyFigure == 9))
                                    return true;
                            }
                            break;

                        }
                        trialEnd = VectorMath.Sum(trialEnd, direction);
                    }
                    break;
                }
            }
            return false;
        }
        private List<Move> FindAllFormalyPosibleMovesForFigure(Vector start, ref Dictionary<Vector, List<Move>> newWhiteMoves, ref Dictionary<Vector, List<Move>> newBlackMoves)
        {
            //Find all moves with right directions and without obsticles but does not include check check
            List<Move> output = new List<Move>();
            if (IfThisFigureIsPinnedWithKing(start))
                return output;
            int figure = board[start.x, start.y];
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
                            if (board[trialEnd.x, trialEnd.y] * figure <= 0)
                                output.Add(new Move(start, trialEnd));
                        }
                    //foreach (Move castlingMove in castlingMoves)
                    //    if (IfTheCastlingIsPossible(figure, castlingMove.start, castlingMove.end, position, castlingPosibilityFromHistory))
                    //        output.Add(new Move(castlingMove.start, castlingMove.end));
                    break;
                case 5:
                    output =  FindAllFormalyPosibleMovesForLinearFigure(figure, start);
                    break;
                case 3:
                    output = FindAllFormalyPosibleMovesForLinearFigure(figure, start);
                    break;
                case 9:
                    output = FindAllFormalyPosibleMovesForLinearFigure(figure, start);
                    break;
                case 2:
                    foreach (Vector posibleTranslation in knightMinTranslations)
                    {
                        trialEnd = VectorMath.Sum(start, posibleTranslation);
                        if (trialEnd.x < 0 || trialEnd.x > 7 || trialEnd.y < 0 || trialEnd.y > 7)
                            continue;
                        if (board[trialEnd.x, trialEnd.y] * figure <= 0)
                            output.Add(new Move(start, trialEnd));
                    }
                    break;
                case 1:
                    if (figure == 1)
                    {
                        if (board[start.x, start.y + 1] == 0)
                            output.Add(new Move(start, new Vector(start.x, start.y + 1)));
                        if (start.y == 1 && board[start.x, start.y + 2] == 0 && board[start.x, start.y + 1] == 0)
                            output.Add(new Move(start, new Vector(start.x, start.y + 2)));
                        if (start.x + 1 < 8 && board[start.x + 1, start.y + 1] < 0)
                            output.Add(new Move(start, new Vector(start.x + 1, start.y + 1)));
                        if (start.x - 1 >= 0 && board[start.x - 1, start.y + 1] < 0)
                            output.Add(new Move(start, new Vector(start.x - 1, start.y + 1)));
                    }
                    if (figure == -1)
                    {
                        if (board[start.x, start.y - 1] == 0)
                            output.Add(new Move(start, new Vector(start.x, start.y - 1)));
                        if (start.y == 6 && board[start.x, start.y - 2] == 0 && board[start.x, start.y - 1] == 0)
                            output.Add(new Move(start, new Vector(start.x, start.y - 2)));
                        if (start.x + 1 < 8 && board[start.x + 1, start.y - 1] > 0)
                            output.Add(new Move(start, new Vector(start.x + 1, start.y - 1)));
                        if (start.x - 1 >= 0 && board[start.x - 1, start.y - 1] > 0)
                            output.Add(new Move(start, new Vector(start.x - 1, start.y - 1)));
                    }
                    break;
                default: break;
            }
            if (figure > 0)
            {
                if(!newWhiteMoves.ContainsKey(start))
                    newWhiteMoves.Add(start, output);
            }
            else
            {
                if (!newBlackMoves.ContainsKey(start))
                    newBlackMoves.Add(start, output);
            }
            return output;
        }
        private void ExcludeForbidenMovesForKingsFromPossibleMovesWithCheckCheck()
        {
            List<Move> whiteKingPossibleMoves = whitePossibleMovesWithoutCheckCheck[whiteKingPosition];
            foreach(Move m in whiteKingPossibleMoves)
            {
                if (!ChessLibrary.ThereIsNoCheckAfterThisMove(m, board))
                    whitePossibleMovesWithCheckCheck.Remove(m);
            }
            List<Move> blackKingPossibleMoves = blackPossibleMovesWithoutCheckCheck[blackKingPosition];
            foreach (Move m in blackKingPossibleMoves)
            {
                if (!ChessLibrary.ThereIsNoCheckAfterThisMove(m, board))
                    blackPossibleMovesWithCheckCheck.Remove(m);
            }
        }
        private void OutputAllPossibleMoves(bool forWhite)
        {
            //List<Move> moves = FindAllPosibleMoves(forWhite, position, castlingPosibilityFromHistory, pawnsOnlyCapture);
            Console.WriteLine("/-----------------------------------------/");
            Console.WriteLine("/------------POSSIBLE MOVES----------------/");
            Console.WriteLine("/-----------------------------------------/");
            List<Move> list = new List<Move>();
            if (forWhite)
            {
                foreach (Move m in whitePossibleMovesWithCheckCheck)
                    list.Add(m);
            }
            else
            {
                foreach (Move m in blackPossibleMovesWithCheckCheck)
                    list.Add(m);
            }
            foreach (Move move in list)
            {
                Console.WriteLine(OutputHumanMove(move));
                Vector end = move.end;
                Vector start = move.start;
                int endFigure = board[end.x, end.y];
                board[end.x, end.y] = board[start.x, start.y];
                board[start.x, start.y] = 0;
                
                OutputBoard();
                board[start.x, start.y] = board[end.x, end.y];
                board[end.x, end.y] = endFigure;
            }
        }
        private void InitialMoveFinding()
        {
            List<Move> whiteInitialMoves = ChessLibrary.FindAllPosibleMoves(true, board, new bool[] { true, true, true, true });
            List<Move> blackInitialMoves = ChessLibrary.FindAllPosibleMoves(false, board, new bool[] { true, true, true, true });
            foreach(Move whiteMove in whiteInitialMoves)
            {
                whitePossibleMovesWithCheckCheck.Add(whiteMove);
                whitePossibleMovesWithoutCheckCheck[new Vector(whiteMove.start.x, whiteMove.start.y)].Add(whiteMove);
            }
            foreach (Move blackMove in blackInitialMoves)
            {
                blackPossibleMovesWithCheckCheck.Add(blackMove);
                blackPossibleMovesWithoutCheckCheck[new Vector(blackMove.start.x, blackMove.start.y)].Add(blackMove);
            }
            ChangeAllPossibleMovesWithCheckCorrectionAfterRecentMove();
        }
        /*public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }*/
    }
    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
