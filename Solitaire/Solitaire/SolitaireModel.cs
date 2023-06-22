using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Solitaire
{
    internal class SolitaireModel
    {
        int cardWidth = 252;
        double rowWidth;
        int[] boardColumns = new int[7];
        int boardTop;
        public MainWindow view;
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

        public void ReArrangeGameBoard(object sender, RoutedEventArgs e)
        {
            CalculateColumns();
            ReArrangeColumn();
            ReArrangeAceColumn();
        }

        public void Setup(object sender, RoutedEventArgs e)
        {
            List<Card> cards = CardContructor.GenerateCards();
            CalculateColumns();
            for (int column = 0; column < boardColumns.Length; column++)
            {
                //Canvas canvasColumn = (Canvas)CardGrid.Children[column];
                for (int row = 0; row <= column; row++)
                {
                    int margin = 0;
                    Card card = cards[0];
                    cards.RemoveAt(0);
                    card.HorizontalAlignment = HorizontalAlignment.Center;
                    margin = 50 * cardColumns[column].Count;
                    card.MouseMove += Card_MoveMouse;
                    card.MouseDoubleClick += MoveCardToAceStack;
                    card.column = column;
                    card.location = BoardLocation.Board;
                    Canvas.SetTop(card, margin + boardTop);
                    Canvas.SetLeft(card, boardColumns[column]);
                    cardColumns[column].Add(card);
                    view.GameBoard.Children.Add(card);
                }
            }
            foreach (var item in cardColumns)
            {
                item.Last().RevealCard();
            }
            for (int i = 3; i < boardColumns.Length; i++)
            {
                aceStacks[i - 3] = boardColumns[i];
            }
        }

        public void CalculateColumns()
        {
            double height = view.window.ActualHeight;
            double width = view.window.ActualWidth;

            Debug.Print(height + " " + width);

            rowWidth = width / 7;
            for (int i = 0; i < 7; i++)
            {
                boardColumns[i] = (int)(rowWidth * (i)) + (int)((rowWidth - cardWidth) / 2);
            }
            for (int i = 3; i < boardColumns.Length; i++)
            {
                aceStacks[i - 3] = boardColumns[i];
            }
            boardTop = (int)(height / 3);
        }

        public bool CanCardBeAdded(UIElementCollection canvasChildren, int cardColumn, Card card)
        {
            return true;
            if (canvasChildren.Count > 0)
            {
                if (canvasChildren[canvasChildren.Count - 1] is Card obj)
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
                        Canvas.SetTop(card, j + boardTop);
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

        public void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            if (sender is Card card)
                if (card.reveled && e.LeftButton == MouseButtonState.Pressed)
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
                    int top = boardTop - 125;
                    if (Canvas.GetTop(card) > top)
                    {
                        int column = boardColumns.Where(x => x < Canvas.GetLeft(card) + (card.Width / 2)).Count() - 1;
                        Canvas.SetLeft(card, boardColumns[column]);
                        ChangeCardColumn(card, column);
                        ReArrangeColumn();
                    }
                    else if (Canvas.GetTop(card) < top && aceStacks.Where(x => x < Canvas.GetLeft(card)).Any())
                    {
                        int column = aceStacks.Where(x => x < Canvas.GetLeft(card) + (card.Width / 2)).Count() - 1;
                        Canvas.SetLeft(card, aceStacks[column] + (int)((rowWidth - cardWidth) / 2));
                        Canvas.SetTop(card, aceTop);
                        ChangeAceColumn(card, column);
                    }
                    else
                    {
                        ReArrangeColumn();
                    }
                    //if (CanCardBeAdded(transferCanvas.Children, cardColumn, card))
                    //{
                    //}
                    //else
                    //{
                    //    Canvas.SetLeft(card, 0);
                    //    Canvas.SetTop(card, 0);
                    //    ReArrangeColumn();
                    //    card.IsHitTestVisible = true;
                    //}
                }
            }
        }

        public void ChangeCardColumn(Card card, int desiredColumn)
        {
            if (Canvas.GetTop(card) > boardTop - 125)
            {
                cardColumns[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
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
            if (card.location == BoardLocation.Board)
            {
                cardColumns[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
                card.column = desiredColumn;
                AceColumns[desiredColumn].Add(card);
                ReArrangeAceColumn();
            }
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
                    if (AceColumns[i].Count == 0)
                    {
                        cardColumns[card.column].Remove(card);
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
    }
}
