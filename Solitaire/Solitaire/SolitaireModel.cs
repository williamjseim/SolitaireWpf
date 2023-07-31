using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
using Solitaire;
using System;

namespace Solitaire
{
    public class SolitaireModel
    {
        public MainWindow? view;
        BoardData board = new();
        public void ReArrangeGameBoard(object sender = null, RoutedEventArgs e = null)
        {
            CalculateGameBoard();
            ReArrangeColumn();
            ReArrangeAceColumn();
            ReArrangeCardStack();
            ReArrangeCardPool();
        }

        public void Setup(object sender, RoutedEventArgs e)
        {
            List<Card> cards = CardContructor.GenerateCards();
            CalculateGameBoard();
            for (int column = 0; column < board.BoardColumnPositions.Length; column++)
            {
                //Canvas canvasColumn = (Canvas)CardGrid.Children[column];
                for (int row = 0; row <= column; row++)
                {
                    int margin = 0;
                    Card card = cards[0];
                    //cardQueue.Add(card);
                    cards.RemoveAt(0);
                    card.HorizontalAlignment = HorizontalAlignment.Center;
                    margin = 50 * board.BoardColumns[column].Count;
                    //card.MouseLeftButtonDown += Card_Pickup;
                    card.MouseMove += Card_MoveMouse;
                    //card.MouseDoubleClick += MoveCardToAceStack;
                    //card.MouseEnter += HighLightCard;
                    //card.MouseLeave += DeHighLightCard;
                    //card.MouseRightButtonDown += CardToString;
                    card.column = column;
                    card.location = BoardLocation.Board;
                    Canvas.SetTop(card, margin + board.BoardMiddle);
                    Canvas.SetLeft(card, board.BoardColumnPositions[column]);
                    board.BoardColumns[column].Add(card);
                    view?.GameBoard.Children.Add(card);
                }
            }
            foreach (var card in board.BoardColumns)
            {
                card.Last().RevealCard();
            }
            foreach (Card card in cards)
            {
                card.MouseMove += Card_MoveMouse;
                card.MouseDoubleClick += MoveCardToAceStack;
                card.column = 0;
                card.location = BoardLocation.Deck;
                card.MouseRightButtonDown += CardToString;
                Canvas.SetLeft(card, board.BoardColumnPositions[0]);
                Canvas.SetTop(card, board.aceTop);
                card.Visibility = Visibility.Hidden;
                view?.GameBoard.Children.Add(card);
            }
            for (int i = 3; i < board.BoardColumnPositions.Length; i++)
            {
                board.aceStacks[i - 3] = board.BoardColumnPositions[i];
            }
            Canvas.SetLeft(board.cardStackPlaceHolder, board.BoardColumnPositions[0]);
            Canvas.SetTop(board.cardStackPlaceHolder, board.aceTop);
            view?.GameBoard.Children.Add(board.cardStackPlaceHolder);
            Canvas.SetZIndex(board.cardStackPlaceHolder, 1);
            board.cardStackPlaceHolder.MouseLeftButtonDown += TakeCardsFromStack;
            board.cardQueue = new Queue<Card>(cards);
        }

        void CardToString(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card)
                Debug.WriteLine(card.ToString());
        }

        public void CalculateGameBoard()//sets up card columns based on window width and height
        {
            double height = view.window.ActualHeight;
            double width = view.window.ActualWidth;
            board.aceTop = (int)view.window.ActualHeight / 20;

            board.rowWidth = width / 7;
            for (int i = 0; i < 7; i++)
            {
                board.BoardColumnPositions[i] = (int)(board.rowWidth * (i)) + (int)((board.rowWidth - board.cardWidth) / 2);
            }
            for (int i = 3; i < board.BoardColumnPositions.Length; i++)
            {
                board.aceStacks[i - 3] = board.BoardColumnPositions[i];
            }
            board.BoardMiddle = (int)(height / 3);
        }

        public void ReArrangeColumn()
        {
            for (int i = 0; i < board.BoardColumns.Length; i++)
            {
                try
                {
                    int j = 0;
                    foreach (Card card in board.BoardColumns[i])
                    {
                        Canvas.SetTop(card, j + board.BoardMiddle);
                        Canvas.SetLeft(card, board.BoardColumnPositions[i] + (int)((board.rowWidth - board.cardWidth) / 2));
                        j += 50;
                    }
                    board.BoardColumns[i].Last().IsHitTestVisible = true;
                }
                catch { }
            }
        }

        public void ReArrangeAceColumn()
        {
            for (int i = 0; i < board.AceColumns.Length; i++)
            {
                if (board.AceColumns[i].Count > 0)
                {
                    foreach (Card card in board.AceColumns[i])
                    {
                        Canvas.SetZIndex(card, 0);
                        Canvas.SetLeft(card, board.aceStacks[card.column] + (int)((board.rowWidth - board.cardWidth) / 2));
                        Canvas.SetTop(card, board.aceTop);
                    }
                    Canvas.SetZIndex(board.AceColumns[i].Last(), 1);
                }
            }
        }

        public void ReArrangeCardStack()
        {
            Canvas.SetLeft(board.cardStackPlaceHolder, board.BoardColumnPositions[0] + (int)((board.rowWidth - board.cardWidth) / 2));
            Canvas.SetTop(board.cardStackPlaceHolder, board.aceTop);
        }

        public void ReArrangeCardPool()
        {
            int cardPoolCount = board.cardPool.Count;
            if (board.cardPool.Count > 0)
            {
                for (int i = 0; i < board.cardPool.Count; i++)
                {
                    board.cardPool[i].IsHitTestVisible = false;
                    board.cardPool[i].Visibility = Visibility.Hidden;
                    Canvas.SetLeft(board.cardPool[i], board.BoardColumnPositions[0]);
                    Canvas.SetTop(board.cardPool[i], board.aceTop);
                    Panel.SetZIndex(board.cardPool[i], 0);
                }
                for (int i = cardPoolCount - 1, j = 3; i >= cardPoolCount - Math.Clamp(cardPoolCount, 1, 3); i--, j--)
                {
                    Card card = board.cardPool[i];
                    card.RevealCard(false);
                    Canvas.SetLeft(card, board.BoardColumnPositions[1] + ((board.cardWidth / 3 * j) - board.cardWidth / 3));
                    Panel.SetZIndex(card, j);
                    card.Visibility = Visibility.Visible;
                }
                board.cardPool.Last().RevealCard(true);
            }
            else
            {
                if (board.cardQueue != null)
                    foreach (Card card in board.cardQueue)
                    {
                        Panel.SetZIndex(card, 0);
                        card.Visibility = Visibility.Hidden;
                        Canvas.SetLeft(card, board.BoardColumnPositions[0]);
                        Canvas.SetTop(card, board.aceTop);
                    }
            }
        }

        //public void Card_Pickup(object sender, MouseEventArgs e)
        //{
        //    if (sender is Card card && card.location == BoardLocation.Board)
        //    {
        //        int i = board.BoardColumns[card.column].IndexOf(card);
        //        if (board.BoardColumns[card.column].Count > i)
        //        {
        //            for (int j = i; j < board.BoardColumns[card.column].Count; j++)
        //            {
        //                Card obj = board.BoardColumns[card.column][j - 1];

        //            }
        //        }
        //    }
        //}

        public void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            if (sender is Card card)
                if (card.IsRevealed && e.LeftButton == MouseButtonState.Pressed)
                {
                    card.IsHitTestVisible = false;
                    Panel.SetZIndex(card, 50);
                    DataObject dataObject = new DataObject();
                    dataObject.SetData(DataFormats.Serializable, card);
                    DragDrop.DoDragDrop(sender as Card, dataObject, DragDropEffects.Move);
                }
        }

        public void Canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is Card card)
            {
                card.IsHitTestVisible = true;
                if (sender is Canvas transferCanvas)
                {
                    int top = board.BoardMiddle - 125;
                    if (Canvas.GetTop(card) > top)
                    {
                        int column = board.BoardColumnPositions.Where(x => x < Canvas.GetLeft(card) + (card.ActualWidth / 2)).Count() - 1;
                        TryChangeCardPosition(card, BoardLocation.Board, column);
                    }
                    else if (Canvas.GetTop(card) < top && board.aceStacks.Where(x => x < Canvas.GetLeft(card)).Any())
                    {
                        int column = board.aceStacks.Where(x => x < Canvas.GetLeft(card) + (card.ActualWidth / 2)).Count() - 1;
                        TryChangeCardPosition(card, BoardLocation.Ace, column);
                    }
                    else
                    {
                        ReArrangeColumn();
                    }
                }
            }
        }

        public void ChangeCardColumn(Card card, int desiredColumn)
        {
            if (Canvas.GetTop(card) > board.BoardMiddle - 125)
            {
                if (card.location == BoardLocation.Deck)
                {
                    card.location = BoardLocation.Board;
                    board.cardPool.Remove(card);
                    board.cardPool.Last().RevealCard();
                    card.MouseEnter += HighLightCard;
                    card.MouseLeave += DeHighLightCard;
                }
                else
                {
                    board.BoardColumns[card.column].Remove(card);
                    RevealLastCardInColumn(card.column);
                }
                board.BoardColumns[desiredColumn].Add(card);
                Canvas.SetZIndex(card, board.BoardColumns[desiredColumn].Count);
                card.column = desiredColumn;
                if (card.location != BoardLocation.Board)
                {
                    card.location = BoardLocation.Board;
                    ReArrangeAceColumn();
                }
            }
            else
            {
                ReArrangeColumn();
            }
        }

        public void ChangeAceColumn(Card card, int desiredColumn)
        {
            card.column = desiredColumn;
            if (card.location == BoardLocation.Deck)
            {
                board.cardPool.Remove(card);
                board.cardPool.Last().RevealCard();
                card.location = BoardLocation.Ace;
            }
            else if (card.location == BoardLocation.Board)
            {
                board.BoardColumns[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
                ReArrangeAceColumn();
            }
            board.AceColumns[desiredColumn].Add(card);
        }

        public void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is Card card)
            {
                Point mouse = e.GetPosition(view?.GameBoard);
                Canvas.SetLeft(card, mouse.X - card.ActualWidth / 2);
                Canvas.SetTop(card, mouse.Y - card.ActualHeight / 2);
            }
        }

        public void MoveCardToAceStack(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card && card.location != BoardLocation.Ace)
            {
                for (int i = 0; i < board.AceColumns.Length; i++)
                {
                    if (CanCardBeAddedToAceStack(card, i))
                    {
                        board.BoardColumns[card.column].Remove(card);
                        board.BoardColumns[card.column].Last().RevealCard();
                        RevealLastCardInColumn(card.column);
                        board.AceColumns[i].Add(card);
                        card.location = BoardLocation.Ace;
                        card.column = i;
                        Canvas.SetLeft(card, board.aceStacks[i]);
                        Canvas.SetTop(card, board.aceTop);
                        break;
                    }
                }
                ReArrangeAceColumn();
            }
        }

        public void RevealLastCardInColumn(int column)
        {
            if (board.BoardColumns[column].Count > 0)
            {
                board.BoardColumns[column].Last().RevealCard();
            }
        }

        bool CanCardBeAddedToAceStack(Card card, int desiredColumn)//checks if the dsired ace stack is empty or value i one above
        {
            if (card.CardValue == CardValue.A)
            {
                return true;
            }
            else if (board.AceColumns[desiredColumn].Count > 0 && board.AceColumns[desiredColumn].Last().suit == card.suit && board.AceColumns[desiredColumn].Last().CardValue == card.CardValue - 1)
            {
                return true;
            }
            return false;
        }

        void HighLightCard(object sender, MouseEventArgs e)
        {
            if (sender is Card card && card.IsRevealed)
            {
                Canvas.SetTop(card, Canvas.GetTop(card) - 50);
            }
        }

        void DeHighLightCard(object sender, MouseEventArgs e)
        {
            ReArrangeGameBoard();
        }

        void TakeCardsFromStack(object sender, MouseButtonEventArgs e)
        {
            if (board.cardQueue.Count > 0)
            {
                for (int i = 0; i < Math.Clamp(board.cardQueue.Count, 0, 3); i++)
                {
                    Card card = board.cardQueue.Dequeue();
                    board.cardPool.Add(card);
                    card.Visibility = Visibility.Visible;
                    card.RevealCard();
                }
                if (board.cardQueue.Count == 0)
                {
                    //cardStackPlaceHolder.Visibility = Visibility.Hidden;
                }
                ReArrangeCardPool();
            }
            else
            {
                board.cardQueue = new(board.cardPool);
                board.cardPool.Clear();
                ReArrangeGameBoard();
                //cardStackPlaceHolder.Visibility = Visibility.Visible;
            }
        }

        void TryChangeCardPosition(Card card, BoardLocation desiredLocation, int desiredColumn)
        {
            if (CanCardBeAdded(card, desiredLocation, desiredColumn))
            {
                if (RemoveCardFromParent(card))
                {
                    AddCardToNewParent(card, desiredLocation, desiredColumn);
                }
            }
            ReArrangeGameBoard();
        }

        bool CanCardBeAdded(Card card, BoardLocation desiredLocation, int desiredColumn)
        {
            if (desiredLocation == BoardLocation.Board && BoardColumnVerify(card, desiredColumn))
            {
                return true;
            }
            else if (desiredLocation == BoardLocation.Ace && AceColumnVerify(card, desiredColumn))
            {
                return true;
            }
            return false;
        }

        bool AddCardToNewParent(Card card, BoardLocation desiredLocation, int desiredColumn)
        {
            if (desiredLocation == BoardLocation.Board)
            {
                card.column = desiredColumn;
                board.BoardColumns[desiredColumn].Add(card);
            }
            else if (desiredLocation == BoardLocation.Ace)
            {
                card.column = desiredColumn;
                board.AceColumns[desiredColumn].Add(card);
            }
            return false;
        }

        bool RemoveCardFromParent(Card card)
        {
            switch (card.location)
            {
                case BoardLocation.Board:
                    board.BoardColumns[card.column].Remove(card);
                    RevealLastCardInColumn(card.column);
                    return true;
                case BoardLocation.Ace:
                    board.AceColumns[card.column].Remove(card);
                    if (board.AceColumns[card.column].Count > 0)
                        board.AceColumns[card.column].Last().RevealCard();
                    return true;
                case BoardLocation.Deck:
                    board.cardPool.Remove(card);
                    if (board.AceColumns[card.column].Count > 0)
                        board.cardPool.Last().RevealCard();
                    return true;
            }
            return false;
        }

        bool BoardColumnVerify(Card card, int desiredColumn)
        {
            if (board.BoardColumns[desiredColumn].Count > 0)
            {
                Card ParentCard = board.BoardColumns[desiredColumn].Last();
                if (ParentCard.Color != card.Color && ParentCard.CardValue == card.CardValue + 1)
                {
                    return true;
                }
            }
            else if (card.CardValue == CardValue.K)
            {
                return true;
            }
            return false;
        }

        bool AceColumnVerify(Card card, int desiredColumn)
        {
            if (board.AceColumns[desiredColumn].Count > 0)
            {
                Card parentCard = board.AceColumns[desiredColumn].Last();
                if (parentCard.suit == card.suit && parentCard.CardValue == card.CardValue - 1)
                {
                    return true;
                }
            }
            else if (card.CardValue == CardValue.A)
            {
                return true;
            }
            return false;
        }
    }

    public struct BoardData
    {
        public BoardData()
        {
            cardStackPlaceHolder = new Card();
            cardPool = new();
        }

        public Queue<Card>? cardQueue;
        public Card cardStackPlaceHolder;
        public List<Card> cardPool;

        public int cardWidth = 252;
        public double rowWidth;

        public int[] BoardColumnPositions = new int[7];
        public int BoardMiddle;
            
        public List<Card>[] BoardColumns = new List<Card>[]
        {
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
        };

        public int aceTop;

        public int[] aceStacks = new int[4];

        public List<Card>[] AceColumns = new List<Card>[]
        {
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
        };
    }
}
