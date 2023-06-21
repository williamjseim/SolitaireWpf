using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Solitaire
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int[] boardColumns = new int[7];
        int boardTop;
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

        public MainWindow()
        {
            InitializeComponent();
            window.Loaded += Setup;
        }

        void Setup(object sender, RoutedEventArgs e)
        {
            CalculateColumns();
            List<Card> cards = CardContructor.GenerateCards();
            GameBoard.DragOver += Canvas_DragOver;
            GameBoard.Drop += Canvas_Drop;
            GameBoard.AllowDrop = true;
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
                    GameBoard.Children.Add(card);
                }
            }
            foreach (var item in cardColumns)
            {
                item.Last().RevealCard();
            }
            for (int i = 3; i < boardColumns.Length; i++)
            {
                aceStacks[i-3] = boardColumns[i];
            }
        }

        void CalculateColumns()
        {
            double height = window.ActualHeight;
            double width = window.ActualWidth;

            Debug.Print(height + " " + width);

            double rowWidth = width / 7;
            for (int i = 0; i < 7; i++)
            {
                boardColumns[i] = (int)(rowWidth * (i)) + 50;
            }
            boardTop = (int)(height / 3);
        }

        bool CanCardBeAdded(UIElementCollection canvasChildren, int cardColumn, Card card)
        {
            return true;
            if(canvasChildren.Count > 0)
            {
                if (canvasChildren[canvasChildren.Count-1] is Card obj)
                {
                    if (card.suit == Suits.Clubs || card.suit == Suits.Spades)
                    {
                        if((int)obj.suit > 2)
                        {
                            if(obj.CardValue > card.CardValue)
                            {
                                return true;
                            }
                        }
                    }
                    else if (card.suit == Suits.Diamonds || card.suit == Suits.Hearts)
                    {
                        if((int)obj.suit < 2)
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

        void ReArrangeColumn()
        {
            for (int i = 0; i < cardColumns.Length; i++)
            {
                int j = 0;
                foreach (Card card in cardColumns[i])
                {
                    Canvas.SetTop(card, j+boardTop);
                    Canvas.SetLeft(card, boardColumns[i]);
                    j += 50;
                }
            }
        }

        void ReArrangeAceColumn()
        {
            for (int i = 0; i < AceColumns.Length; i++)
            {
                if (AceColumns[i].Count > 0)
                {
                    foreach (Card card in AceColumns[i])
                    {
                        Canvas.SetZIndex(card, 0);
                    }
                    Canvas.SetZIndex(AceColumns[i].Last(), 1);
                }
            }
        }

        private void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            if(sender is Card card)
                if (card.reveled && e.LeftButton == MouseButtonState.Pressed)
                {
                    card.IsHitTestVisible = false;
                    Canvas.SetZIndex(card, 50);
                    DragDrop.DoDragDrop(card, new DataObject(DataFormats.Serializable,card), DragDropEffects.Move);
                }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if(data is Card card)
            {
                card.IsHitTestVisible = true;
                if(sender is Canvas transferCanvas)
                {
                    int top = boardTop - 125;
                    if (Canvas.GetTop(card) > top)
                    {
                        int column = boardColumns.Where(x => x < Canvas.GetLeft(card) + (card.Width/2)).Count()-1;
                        Canvas.SetLeft(card, boardColumns[column]);
                        ChangeCardColumn(card, column);
                        ReArrangeColumn();
                    }
                    else if (Canvas.GetTop(card) < top && aceStacks.Where(x=>x < Canvas.GetLeft(card)).Any())
                    {
                        int column = aceStacks.Where(x => x < Canvas.GetLeft(card) + (card.Width / 2)).Count()-1;
                        Canvas.SetLeft(card, aceStacks[column]);
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

        void ChangeCardColumn(Card card, int desiredColumn)
        {
            if(Canvas.GetTop(card) > boardTop-125)
            {
                cardColumns[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
                cardColumns[desiredColumn].Add(card);
                Canvas.SetZIndex(card, cardColumns[desiredColumn].Count);
                card.column = desiredColumn;
                if(card.location != BoardLocation.Board)
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

        void ChangeAceColumn(Card card, int desiredColumn)
        {
            if(card.location == BoardLocation.Board)
            {
                cardColumns[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
                AceColumns[desiredColumn].Add(card);
                ReArrangeAceColumn();
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if(data is Card card)
            {
                Point mouse = e.GetPosition((UIElement)card.Parent);
                Canvas.SetLeft(card,mouse.X-card.Width/2);
                Canvas.SetTop(card,mouse.Y-card.Height/2);
            }
        }

        private void MoveCardToAceStack(object sender, MouseButtonEventArgs e)
        {
            if(sender is Card card && card.location != BoardLocation.Ace)
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

        void RevealLastCardInColumn(int column)
        {
            if (cardColumns[column].Count > 0)
            {
                cardColumns[column].Last().RevealCard();
            }
        }
    }
}
