﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DayPuzzle
{
    class Program
    {
        private class BingoCard
        {
            public int?[,] Card { get; set; }

            public bool[,] CardValidator { get; set; }
        }

        private static List<int?[,]> BingoCardPass2 = new List<int?[,]>();

        private static List<BingoCard> BingoCards = new List<BingoCard>();

        private static List<int> draw = new List<int>();

        private static int lastNumberCalled;
        private static int sumOfUnmarkedNumbers;

        static void Main(string[] args)
        {
            try
            {
                if (args.Count() == 1)
                {
                    string filename = args[0];

                    Program.Run(filename).Wait();

                }
                else
                {
                    Console.WriteLine("Invalid Arguments. Please specify input filename only.");
                    Console.WriteLine("   DailyPuzzle input.txt");
                    return;
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        private async static Task Run(string filename)
        {
            //Step 1: Read in the file
            ReadFile(filename);

            #region Part 1

            PlayBingoGame();

	        Console.WriteLine("First Solution - Part 1: Sum of Unmarked Cards in Winner Board ({0}) * Last Number Called ({1}): {2}", sumOfUnmarkedNumbers, lastNumberCalled, sumOfUnmarkedNumbers * lastNumberCalled);

            #endregion

            #region Part 2

            ResetBingoValidation();

            PlayBingoGame(true);

            Console.WriteLine("First Solution - Part 2: Sum of Unmarked Cards in Last Winning Board ({0}) * Last Number Called ({1}): {2}", sumOfUnmarkedNumbers, lastNumberCalled, sumOfUnmarkedNumbers * lastNumberCalled);

            //Restart and perform function with Lambda Queries
            //BingoCards.Clear();
            //ReadFile(filename);
            //PlayBingoGameLinqOrLambdaStyle();

            #endregion
        }

        private static void ReadFile(string filename)
        {
            try
            {
                string line;
                using (TextReader reader = File.OpenText(filename))
                {
                    //read the first line to create the draw number Array
                    line = reader.ReadLine();
                    draw = Array.ConvertAll(line.Split(','), s => int.Parse(s)).ToList();

                    while ((line = reader.ReadLine()) != null)
                    {

                        if (line.Length == 0)
                        {
                            //skip processing, we are in between bingo cards
                            continue;
                        }

                        int?[,] newBoard = new int?[5, 5];
                        bool[,] newBoardValidator = new bool[5, 5];

                        int rowIndex = 0;

                        while (rowIndex < 5)
                        {
                            //strip whitespace from middle sequence
                            string[] integers = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ');
                            int[] row = Array.ConvertAll(integers, s => int.Parse(s));

                            //process next five lines into one board listing
                            for (int col = 0; col < 5; col++)
                            {
                                newBoard[rowIndex, col] = row[col];
                                newBoardValidator[rowIndex, col] = false;
                            }

                            rowIndex = rowIndex + 1;

                            //read next line
                            line = reader.ReadLine();
                        }

                        BingoCard card = new BingoCard();

                        card.Card = newBoard;
                        card.CardValidator = newBoardValidator;

                        BingoCards.Add(card);
                        BingoCardPass2.Add(newBoard);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ResetBingoValidation()
        {
            foreach (BingoCard card in BingoCards)
            {
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        card.CardValidator[row, col] = false;
                    }
                }
            }
        }

        private static void PlayBingoGame(bool lastToWin = false)
        {
            bool bingo;
            BingoCard card;

            foreach (int number in draw)
            {
                (bingo, card) = PlayAndCheckBingo(number, lastToWin);

                if (bingo)
                {
                    sumOfUnmarkedNumbers = GetSumBingoCard(card);
                    lastNumberCalled = number;
                    break;
                }
            }
        }

        private static (bool, BingoCard) PlayAndCheckBingo(int number, bool lastToWin)
        {
            bool bingo = false;
            BingoCard winningCard = null;

            foreach (BingoCard card in BingoCards)
            {
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        if (card.Card[row, col] == number)
                        {
                            card.CardValidator[row, col] = true;
                        }
                    }
                }
            }

            List<BingoCard> cardsToRemove = new List<BingoCard>();

            foreach (BingoCard card in BingoCards)
            {
                (bingo, winningCard) = CheckForBingo(card);

                if (bingo && lastToWin)
                {
                    cardsToRemove.Add(winningCard);
                }
                else if (bingo)
                    break;
            }

            if (BingoCards.Count > 1 && lastToWin && cardsToRemove.Count > 0)
            {
                foreach(BingoCard card in cardsToRemove)
                {
                    BingoCards.Remove(card);
                    bingo = false;
                    winningCard = null;
                }

                cardsToRemove.Clear();
            }

            return (bingo, winningCard);
        }
        private static (bool, BingoCard) CheckForBingo(BingoCard card)
        {
            //check each row
            for (int row = 0; row < 5; row++)
            {

                if (   card.CardValidator[row,0] 
                    && card.CardValidator[row,1] 
                    && card.CardValidator[row,2] 
                    && card.CardValidator[row,3] 
                    && card.CardValidator[row,4])
                    
                    return (true, card);
            }

            //check the columns
            for (int col = 0; col < 5; col++)
            {
                if (   card.CardValidator[0,col] 
                       && card.CardValidator[1,col] 
                       && card.CardValidator[2,col] 
                       && card.CardValidator[3,col] 
                       && card.CardValidator[4,col])
                    
                    return (true, card);
            }

            return (false, null);
        }
        private static int GetSumBingoCard(BingoCard card)
        {
            int sum = 0;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (card.CardValidator[row,col] == false)
                    {
                        sum += card.Card[row,col] ?? 0;
                    }
                }
            }

            return sum;
        }

        #region Trial code with Lambda Expressions (incomplete)

        //private static void PlayBingoGameLinqOrLambdaStyle()
        //{
        //    bool firstWin = false;

        //    foreach (int number in draw)
        //    {
        //        BingoCardPass2.
        //    }

        //}


        //private static (bool, int [,]) CheckForBingoLambdaStyle(int [,] card)
        //{

        //    return (false, null);
        //}

        //private static int GetSumBingoCardLambdaStyle(int[,] card)
        //{
        //    int sum = 0;

        //    return sum;
        //}


        #endregion

    }

}
