using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
using Solitaire;
using System;
using System.Data.Common;

namespace Solitaire
{
    internal class SolitaireModel
    {
        public MainWindow? view;

        Queue<Card> cardQueue;
        Card cardStackPlaceHolder = new Card();
        List<Card> cardPool = new();


        int cardWidth = 252;
        double rowWidth;

        int[] boardColumns = new int[7];
        int BoardMiddle;

        List<Card>[] cardColumns = new List<Card>[]
        {
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
        };

        int aceTop = 50;

        int[] aceStacks = new int[4];

        List<Card>[] AceColumns = new List<Card>[]
        {
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
        };

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
            for (int column = 0; column < boardColumns.Length; column++)
            {
                //Canvas canvasColumn = (Canvas)CardGrid.Children[column];
                for (int row = 0; row <= column; row++)
                {
                    int margin = 0;
                    Card card = cards[0];
                    //cardQueue.Add(card);
                    cards.RemoveAt(0);
                    card.HorizontalAlignment = HorizontalAlignment.Center;
                    margin = 50 * cardColumns[column].Count;
                    card.MouseMove += Card_MoveMouse;
                    card.MouseDoubleClick += MoveCardToAceStack;
                    card.MouseLeftButtonDown += Test;
                    card.MouseEnter += HighLightCard;
                    card.MouseLeave += DeHighLightCard;
                    card.MouseRightButtonDown += CardToString;
                    card.column = column;
                    card.location = BoardLocation.Board;
                    Canvas.SetTop(card, margin + BoardMiddle);
                    Canvas.SetLeft(card, boardColumns[column]);
                    cardColumns[column].Add(card);
                    view.GameBoard.Children.Add(card);
                }
            }
            foreach (var card in cardColumns)
            {
                card.Last().RevealCard();
            }
            foreach (Card card in cards)
            {
                card.MouseMove += Card_MoveMouse;
                card.MouseDoubleClick += MoveCardToAceStack;
                card.MouseLeftButtonDown += Test;
                card.column = 0;
                card.location = BoardLocation.Deck;
                card.MouseRightButtonDown += CardToString;
            }
            for (int i = 3; i < boardColumns.Length; i++)
            {
                aceStacks[i - 3] = boardColumns[i];
            }
            Canvas.SetLeft(cardStackPlaceHolder, boardColumns[0]);
            Canvas.SetTop(cardStackPlaceHolder, aceTop);
            view.GameBoard.Children.Add(cardStackPlaceHolder);
            Canvas.SetZIndex(cardStackPlaceHolder, 1);
            foreach (Card card in cards)
            {
                Canvas.SetLeft(card, boardColumns[0]);
                Canvas.SetTop(card, aceTop);
                card.Visibility = Visibility.Hidden;
                view.GameBoard.Children.Add(card);
            }
            cardStackPlaceHolder.MouseLeftButtonDown += TakeCardsFromStack;
            cardQueue = new Queue<Card>(cards);
        }

        void CardToString(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card)
                Debug.Write(card.ToString());
        }

        void Test(object sender, MouseEventArgs e)
        {
            if (sender is Card card)
                Debug.Write(card.ToString());
        }

        public void CalculateGameBoard()//sets up card columns based on window width and height
        {
            double height = view.window.ActualHeight;
            double width = view.window.ActualWidth;

            rowWidth = width / 7;
            for (int i = 0; i < 7; i++)
            {
                boardColumns[i] = (int)(rowWidth * (i)) + (int)((rowWidth - cardWidth) / 2);
            }
            for (int i = 3; i < boardColumns.Length; i++)
            {
                aceStacks[i - 3] = boardColumns[i];
            }
            BoardMiddle = (int)(height / 3);
        }

        public bool CanCardBeAdded(Card card, int desiredColumn, BoardLocation desiredLocation)
        {
            if (cardColumns[desiredColumn].Count > 0)
            {
                if (cardColumns[desiredColumn].Last() is Card obj)
                {
                    if (card.suit == Suits.Clubs || card.suit == Suits.Spades)
                    {
                        if ((int)obj.suit > 2)
                        {
                            if (obj.CardValue > card.CardValue)
                            {
                                return true;
                            }
                        }
                    }
                    else if (card.suit == Suits.Diamonds || card.suit == Suits.Hearts)
                    {
                        if ((int)obj.suit < 2)
                        {
                            if (obj.CardValue > card.CardValue)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (card.CardValue == CardValue.K)
                {
                    return true;
                }
            }
            return false;
        }

        public void ReArrangeColumn()
        {
            for (int i = 0; i < cardColumns.Length; i++)
            {
                try
                {
                    int j = 0;
                    foreach (Card card in cardColumns[i])
                    {
                        Canvas.SetTop(card, j + BoardMiddle);
                        Canvas.SetLeft(card, boardColumns[i] + (int)((rowWidth - cardWidth) / 2));
                        j += 50;
                    }
                    cardColumns[i].Last().IsHitTestVisible = true;
                }
                catch { }
            }
        }

        public void ReArrangeAceColumn()
        {
            for (int i = 0; i < AceColumns.Length; i++)
            {
                if (AceColumns[i].Count > 0)
                {
                    foreach (Card card in AceColumns[i])
                    {
                        Canvas.SetZIndex(card, 0);
                        Canvas.SetLeft(card, aceStacks[card.column] + (int)((rowWidth - cardWidth) / 2));
                        Canvas.SetTop(card, aceTop);
                    }
                    Canvas.SetZIndex(AceColumns[i].Last(), 1);
                }
            }
        }

        public void ReArrangeCardStack()
        {
            Canvas.SetLeft(cardStackPlaceHolder, boardColumns[0]+(int)((rowWidth - cardWidth) / 2));
            Canvas.SetTop(cardStackPlaceHolder, aceTop);
        }

        public void ReArrangeCardPool()
        {
            int cardPoolCount = cardPool.Count;
            if(cardPool.Count > 0)
            {
                for (int i = 0; i < cardPool.Count; i++)
                {
                    cardPool[i].IsHitTestVisible = false;
                    cardPool[i].Visibility = Visibility.Hidden;
                    Canvas.SetLeft(cardPool[i], boardColumns[0]);
                    Canvas.SetTop(cardPool[i], aceTop);
                    Panel.SetZIndex(cardPool[i], 0);
                }
                for ( int i = cardPoolCount - 1, j = 3; i >= cardPoolCount-Math.Clamp(cardPoolCount,1,3); i--, j--)
                {
                    Card card = cardPool[i];
                    card.RevealCard(true);//asdwawdwa
                    Canvas.SetLeft(card, boardColumns[1] + (cardWidth / 3 * j));
                    Panel.SetZIndex(card, j);
                    card.Visibility = Visibility.Visible;
                }
                cardPool.Last().RevealCard(true);
            }
            else
            {
                if(cardQueue != null)
                    foreach (Card card in cardQueue)
                    {
                            Panel.SetZIndex(card, 0);
                            card.Visibility = Visibility.Hidden;
                            Canvas.SetLeft(card, boardColumns[0]);
                            Canvas.SetTop(card, aceTop);
                    }
            }
        }

        public void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            if (sender is Card card)
                if (card.IsRevealed && e.LeftButton == MouseButtonState.Pressed)
                {
                    card.IsHitTestVisible = false;
                    Panel.SetZIndex(card, 50);
                    DragDrop.DoDragDrop(card, new DataObject(DataFormats.Serializable, card), DragDropEffects.Move);
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
                    int top = BoardMiddle - 125;
                    if (Canvas.GetTop(card) > top)
                    {
                        int column = boardColumns.Where(x => x < Canvas.GetLeft(card) + (card.Width / 2)).Count() - 1;
                        if(CanCardBeAdded(card, column))
                        {
                            Canvas.SetLeft(card, boardColumns[column]);
                            ChangeCardColumn(card, column);
                            ReArrangeColumn();
                        }
                        else
                        {
                            ReArrangeGameBoard();
                        }
                    }
                    else if (Canvas.GetTop(card) < top && aceStacks.Where(x => x < Canvas.GetLeft(card)).Any())
                    {
                        int column = aceStacks.Where(x => x < Canvas.GetLeft(card) + (card.Width / 2)).Count() - 1;
                        if (CanCardBeAddedToAceStack(card, column))
                        {
                            Canvas.SetLeft(card, aceStacks[column] + (int)((rowWidth - cardWidth) / 2));
                            Canvas.SetTop(card, aceTop);
                            ChangeAceColumn(card, column);
                        }
                        else
                        {
                            ReArrangeGameBoard();
                        }
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
            if (Canvas.GetTop(card) > BoardMiddle - 125)
            {
                if(card.location == BoardLocation.Deck)
                {
                    card.location = BoardLocation.Board;
                    cardPool.Remove(card);
                    cardPool.Last().RevealCard();
                    card.MouseEnter += HighLightCard;
                    card.MouseLeave += DeHighLightCard;
                }
                else
                {
                    cardColumns[card.column].Remove(card);
                    RevealLastCardInColumn(card.column);
                }
                cardColumns[desiredColumn].Add(card);
                Canvas.SetZIndex(card, cardColumns[desiredColumn].Count);
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
            if(card.location == BoardLocation.Deck)
            {
                cardPool.Remove(card);
                cardPool.Last().RevealCard();
                card.location = BoardLocation.Ace;
            }
            else if (card.location == BoardLocation.Board)
            {
                cardColumns[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
                ReArrangeAceColumn();
            }
            AceColumns[desiredColumn].Add(card);
        }

        public void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is Card card)
            {
                Point mouse = e.GetPosition((UIElement)card.Parent);
                Canvas.SetLeft(card, mouse.X - card.Width / 2);
                Canvas.SetTop(card, mouse.Y - card.Height / 2);
            }
        }

        public void MoveCardToAceStack(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card && card.location != BoardLocation.Ace)
            {
                for (int i = 0; i < AceColumns.Length; i++)
                {
                    if (CanCardBeAddedToAceStack(card, i))
                    {
                        cardColumns[card.column].Remove(card);
                        cardColumns[card.column].Last().RevealCard();
                        RevealLastCardInColumn(card.column);
                        AceColumns[i].Add(card);
                        card.location = BoardLocation.Ace;
                        card.column = i;
                        Canvas.SetLeft(card, aceStacks[i]);
                        Canvas.SetTop(card, aceTop);
                        break;
                    }
                }
                ReArrangeAceColumn();
            }
        }

        public void RevealLastCardInColumn(int column)
        {
            if (cardColumns[column].Count > 0)
            {
                cardColumns[column].Last().RevealCard();
            }
        }

        bool CanCardBeAddedToAceStack(Card card, int desiredColumn)//checks if the dsired ace stack is empty or value i one above
        {
            if(card.CardValue == CardValue.A)
            {
                return true;
            }
            else if (AceColumns[desiredColumn].Count > 0 && AceColumns[desiredColumn].Last().suit == card.suit && AceColumns[desiredColumn].Last().CardValue == card.CardValue-1)
            {
                return true;
            }
            return false;
        }

        void HighLightCard(object sender, MouseEventArgs e)
        {
            if(sender is Card card && card.IsRevealed)
            {
                Canvas.SetTop(card, Canvas.GetTop(card)-50);
            }
        }

        void DeHighLightCard(object sender, MouseEventArgs e)
        {
            ReArrangeGameBoard();
        }

        void TakeCardsFromStack(object sender, MouseButtonEventArgs e)
        {
            if(cardQueue.Count > 0)
            {
                for (int i = 0; i < Math.Clamp(cardQueue.Count,0,3); i++)
                {
                    Card card = cardQueue.Dequeue();
                    cardPool.Add(card);
                    card.Visibility = Visibility.Visible;
                    card.RevealCard();
                }
                if(cardQueue.Count == 0)
                {
                    //cardStackPlaceHolder.Visibility = Visibility.Hidden;
                }
                ReArrangeCardPool();
            }
            else
            {
                cardQueue = new(cardPool);
                cardPool.Clear();
                ReArrangeGameBoard();
                //cardStackPlaceHolder.Visibility = Visibility.Visible;
            }
        }

        void ChangeCardPosition(Card card, BoardLocation desiredLocation, int desiredColumn)
        {
            if(CanCardBeAdded(card,desiredColumn,desiredLocation))
            {
                switch (card.location)
                {
                    case BoardLocation.Deck:
                        cardPool.Remove(card);
                        break;
                    case BoardLocation.Board:
                        cardColumns[card.column].Remove(card);
                        break;
                    case BoardLocation.Ace:
                        AceColumns[card.column].Remove(card);
                        break;
                }

            }
        }

        bool AddCardToDesiredColumn(Card card, int desiredColumn, BoardLocation desiredLocation)
        {
            switch(desiredLocation)
            {
                case BoardLocation.Board:
                    if (cardColumns[desiredColumn].Last().Color != card.Color && cardColumns[desiredColumn].Last().CardValue == card.CardValue + 1)
                    {
                        return true;
                    }
                    break;
                case BoardLocation.Ace:
                    if(AceColumns[desiredColumn].Last().suit == card.suit && AceColumns[desiredColumn].Last().CardValue == card.CardValue-1)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
