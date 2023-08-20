using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Solitaire
{
    public class SolitaireModel : GameModel
    {
        SolitaireBoardData board = new();
        List<Card> draggedCards = new();
        public override void ReArrangeGameBoard(object? sender = null, RoutedEventArgs? e = null)
        {
            CalculateGameBoard();
            board.SortBoard();
        }

        /// <summary>
        /// Gets a signal primarily from the mainwindow then its done loading so this can get the height and width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void Setup(object sender, RoutedEventArgs e)
        {
            view.GameBoard.DragOver += Canvas_DragOver;
            view.GameBoard.Drop += Canvas_Drop;
            List<Card> cards = CardContructor.GenerateCards();
            CalculateGameBoard();
            view.Undo.PreviewMouseLeftButtonDown += board.Undo;
            for (int column = 0; column < board.pilePositions.Length; column++)
            {
                //Canvas canvasColumn = (Canvas)CardGrid.Children[column];
                for (int row = 0; row <= column; row++)
                {
                    int margin = 0;
                    Card card = cards[0];
                    //cardQueue.Add(card);
                    cards.RemoveAt(0);
                    card.HorizontalAlignment = HorizontalAlignment.Center;
                    margin = 50 * board.piles[column].Count;
                    //card.MouseLeftButtonDown += Card_Pickup;
                    card.MouseLeftButtonDown += Card_Pickup;
                    //card.MouseDoubleClick += MoveCardToAceStack;
                    //card.MouseEnter += HighLightCard;
                    //card.MouseLeave += DeHighLightCard;
                    card.MouseRightButtonDown += CardToString;
                    card.column = column;
                    card.location = BoardLocation.piles;
                    Canvas.SetTop(card, margin + board.BoardMiddle);
                    Canvas.SetLeft(card, board.pilePositions[column]);
                    board.piles[column].Add(card);
                    view?.GameBoard.Children.Add(card);
                }
            }
            foreach (var card in board.piles)
            {
                RevealCard(card.Last());
            }
            foreach (Card card in cards)
            {
                card.MouseMove += Card_Pickup;
                //card.MouseDoubleClick += MoveCardToAceStack;
                card.column = 0;
                card.location = BoardLocation.stock;
                card.MouseRightButtonDown += CardToString;
                Canvas.SetLeft(card, board.pilePositions[0]);
                Canvas.SetTop(card, board.foundationTop);
                card.Visibility = Visibility.Hidden;
                view?.GameBoard.Children.Add(card);
            }
            for (int i = 3; i < board.pilePositions.Length; i++)
            {
                board.foundationPositions[i - 3] = board.pilePositions[i];
            }
            Canvas.SetLeft(board.StockPlaceHolder, board.pilePositions[0]);
            Canvas.SetTop(board.StockPlaceHolder, board.foundationTop);
            view?.GameBoard.Children.Add(board.StockPlaceHolder);
            Canvas.SetZIndex(board.StockPlaceHolder, 1);
            board.StockPlaceHolder.MouseLeftButtonDown += TakeCardsFromStack;
            board.stock = new LinkedList<Card>(cards);
        }

        #region Rearrange
        public void CalculateGameBoard()//sets up card columns based on window width and height
        {
            double height = view.GameBoard.ActualHeight;//hight of gameboard
            double width = view.GameBoard.ActualWidth;//width of gameboard
            int spacing = Math.Abs((int)((board.rowWidth - board.cardWidth) / 2));
            board.foundationTop = 0;

            board.rowWidth = width / 7;//spacing for each pile
            for (int i = 0; i < 7; i++)
            {
                board.pilePositions[i] = (int)(board.rowWidth * i) + spacing;
            }
            for (int i = 3; i < board.pilePositions.Length; i++)
            {
                board.foundationPositions[i - 3] = board.pilePositions[i];
            }
            board.BoardMiddle = (int)(height / 3);
            board.pileLength = (int)(height - board.BoardMiddle);
        }
        #endregion

        #region MoveCard
        public void Card_Pickup(object sender, MouseEventArgs e)
        {
            draggedCards.Clear();
            if (sender is Card card)
                if (card.IsRevealed && e.LeftButton == MouseButtonState.Pressed)
                {
                    if (card.location == BoardLocation.piles/* && board.BoardColumns[card.column].Last() != card*/)
                    {
                        int index = board.piles[card.column].IndexOf(card);
                        if(index != -1)
                        {
                            for (int i = index; i < board.piles[card.column].Count; i++)
                            {
                                draggedCards.Add(board.piles[card.column][i]);
                            }
                            foreach (var (value, i) in draggedCards.Select((value, i )=> (value, i)))
                            {
                                value.IsHitTestVisible = false;
                                Panel.SetZIndex(value, 50+i);
                                DataObject dataObject = new DataObject();
                                dataObject.SetData(DataFormats.Serializable, value);
                                DragDrop.DoDragDrop(value, dataObject, DragDropEffects.Move);
                            }
                        }
                    }
                    else
                    {
                        draggedCards.Add(card);
                        card.IsHitTestVisible = false;
                        Panel.SetZIndex(card, 50);
                        DataObject dataObject = new DataObject();
                        dataObject.SetData(DataFormats.Serializable, card);
                        DragDrop.DoDragDrop(sender as Card, dataObject, DragDropEffects.Move);
                    }
                }
        }

        //places the card if valid
        public void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (sender is Canvas)
            {
                Point mousePos = e.GetPosition(Application.Current.MainWindow);
                BoardLocation desiredLocation = GetDesiredLocation(mousePos, out int index);
                board.ChangeCardLocation(draggedCards.ToArray(), desiredLocation, index);
            }
            foreach (Card obj in draggedCards)
            {
                obj.IsHitTestVisible = true;
            }
            ReArrangeGameBoard();
        }

        //gets the desired pile based on mouse position
        private BoardLocation GetDesiredLocation(Point mousePos, out int index)
        {
            if(mousePos.Y < board.BoardMiddle)
            {
                index = board.foundationPositions.Where(x => x < mousePos.X).Count() - 1;
                return BoardLocation.foundation;
            }
            else
            {
                index = board.pilePositions.Where(x => x < mousePos.X).Count() - 1;
                return BoardLocation.piles;
            }
            
        }

        /// <summary>
        /// Controls the position of the dragged object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is Card card)
            {
                Point mouse = e.GetPosition(view?.GameBoard);
                if (draggedCards.Count != 0)
                {
                    foreach (var (value, i) in draggedCards.Select((value, i) => (value, i)))
                    {
                        Canvas.SetLeft(value, mouse.X - value.ActualWidth / 2);
                        Canvas.SetTop(value, (mouse.Y - value.ActualHeight / 2) - i * 50);
                    }
                }
                else
                {
                    Canvas.SetLeft(card, mouse.X - card.ActualWidth / 2);
                    Canvas.SetTop(card, mouse.Y - card.ActualHeight / 2);
                }
            }
        }

        //turn a card from the stock and puts it face up in the waste amount depending on stockpull setting
        void TakeCardsFromStack(object sender, MouseButtonEventArgs e)
        {
            if (board.stock.Count > 0)
            {
                //List<Card> cards = new List<Card>();
                for (int i = 0; i < Math.Clamp(board.stock.Count, 0, board.stockPull); i++)
                {
                    Card card = board.stock.First();
                    board.stock.RemoveFirst();
                    board.waste.AddLast(card);
                    card.Visibility = Visibility.Visible;
                    RevealCard(card);
                    //cards.Add(card);
                }
                //CreateCardPoolAction(cards.ToArray());
                if (board.waste.Count == 0)
                {
                    //cardStackPlaceHolder.Visibility = Visibility.Hidden;
                }
                board.SortStockAndWaste();
            }
            else
            {
                board.stock = new(board.waste);
                board.waste.Clear();
                ReArrangeGameBoard();
                //cardStackPlaceHolder.Visibility = Visibility.Visible;
            }
            board.moves.Push(new StockTurned());
        }
        #endregion

        //prints out card and which list its in
        void CardToString(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card)
            {
                Debug.WriteLine(card.ToString() + " " + Canvas.GetLeft(card) + " " + card.location + " zindex " + Panel.GetZIndex(card) + " column " + card.column);
                Debug.WriteLine($"Card location piles { board.piles.Where(i=>i.Contains(card)).Any()} foundation {board.foundations.Where(i => i.Contains(card)).Any()} stock {board.stock.Contains(card)} waste {board.waste?.Contains(card)}");
            }
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

        private void RevealCard(Card card, bool hitTest = true)
        {
            card.IsRevealed = hitTest;
            card.IsHitTestVisible = hitTest;
        }
    }
}
