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

            GameManager gameManager = new GameManager(gameMode,humanIsPlayingAsWhite); 
            string inputMove;
            while (gameManager.GameResult == null)
            {
                if (gameManager.WhiteToTurn)
                    Console.WriteLine("White to turn");
                else
                    Console.WriteLine("Black to turn");
                inputMove = Console.ReadLine();
                if (inputMove == "l")
                {
                    gameManager.Quit();
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
                gameManager.InputHumanMove(figure, startField, endField);
            }
            inputMove = Console.ReadLine();
            if (inputMove == "l")
            {
                gameManager.Quit();
                
            }
            Console.ReadLine();
        }
    }
}
