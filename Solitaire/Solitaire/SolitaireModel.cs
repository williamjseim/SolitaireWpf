using Solitaire.Actions;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
        int amountFromStack = 1;
        public override void ReArrangeGameBoard(object? sender = null, RoutedEventArgs? e = null)
        {
            CalculateGameBoard();
            //ReArrangeColumn();
            //ReArrangeAceColumn();
            //ReArrangeCardStack();
            //ReArrangeCardPool();
            board.SortBoard();
        }

        /// <summary>
        /// Gets a signal primarily from the mainwindow then its done loading so this can get the height and width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void Setup(object sender, RoutedEventArgs e)
        {
            List<Card> cards = CardContructor.GenerateCards();
            CalculateGameBoard();
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
                    card.MouseLeftButtonDown += Card_MoveMouse;
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
                card.MouseMove += Card_MoveMouse;
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
            board.stock = new List<Card>(cards);
        }

        #region Rearrange
        public void CalculateGameBoard()//sets up card columns based on window width and height
        {
            double height = view.GameBoard.ActualHeight;
            double width = view.GameBoard.ActualWidth;
            //board.aceTop = (int)view.window.ActualHeight / 20;
            int spacing = Math.Abs((int)((board.rowWidth - board.cardWidth) / 2));
            board.foundationTop = 0;

            board.rowWidth = width / 7;
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

        public void ReArrangeCardPool()
        {
            int cardPoolCount = board.stock.Count;
            if (board.stock.Count > 0)
            {
                for (int i = 0; i < board.stock.Count; i++)
                {
                    board.stock[i].IsHitTestVisible = false;
                    board.stock[i].Visibility = Visibility.Hidden;
                    Canvas.SetLeft(board.stock[i], board.pilePositions[0]);
                    Canvas.SetTop(board.stock[i], board.foundationTop);
                    Panel.SetZIndex(board.stock[i], 0);
                }
                for (int i = cardPoolCount - 1, j = 3; i >= cardPoolCount - Math.Clamp(cardPoolCount, 1, 3); i--, j--)
                {
                    Card card = board.stock[i];
                    RevealCard(card, false);
                    Canvas.SetLeft(card, board.pilePositions[1] + ((board.cardWidth / 3 * j) - board.cardWidth / 3));
                    Panel.SetZIndex(card, j);
                    card.Visibility = Visibility.Visible;
                }
                RevealCard(board.stock.Last());
            }
            else
            {
                if (board.waste != null)
                    foreach (Card card in board.waste)
                    {
                        Panel.SetZIndex(card, 0);
                        card.Visibility = Visibility.Hidden;
                        Canvas.SetLeft(card, board.pilePositions[0]);
                        Canvas.SetTop(card, board.foundationTop);
                    }
            }
        }
        #endregion

        #region MoveCard
        public void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            draggedCards.Clear();
            if (sender is Card card)
                if (card.IsRevealed && e.LeftButton == MouseButtonState.Pressed)
                {
                    if (card.location == BoardLocation.piles/* && board.BoardColumns[card.column].Last() != card*/)
                    {
                        int index = board.piles[card.column].IndexOf(card);
                        for (int i = index; i < board.piles[card.column].Count; i++)
                        {
                            draggedCards.Add(board.piles[card.column][i]);
                        }
                        foreach (Card obj in draggedCards)
                        {
                            obj.IsHitTestVisible = false;
                            Panel.SetZIndex(obj, 50);
                            DataObject dataObject = new DataObject();
                            dataObject.SetData(DataFormats.Serializable, obj);
                            DragDrop.DoDragDrop(obj, dataObject, DragDropEffects.Move);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("card");
                        draggedCards.Add(card);
                        card.IsHitTestVisible = false;
                        Panel.SetZIndex(card, 50);
                        DataObject dataObject = new DataObject();
                        dataObject.SetData(DataFormats.Serializable, card);
                        DragDrop.DoDragDrop(sender as Card, dataObject, DragDropEffects.Move);
                    }
                }
        }

        public void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (sender is Canvas)
            {
                Point mousePos = e.GetPosition(Application.Current.MainWindow);
                BoardLocation desiredLocation = GetDesiredLocation(mousePos, out int index);
                board.ChangeCardLocation(draggedCards.ToArray(), desiredLocation, index);
                //foreach (Card obj in draggedCards)
                //{
                //    obj.IsHitTestVisible = true;
                //}
                //Debug.Print("dragged" + draggedCards.Count);
                //BoardLocation newPosition = BoardLocation.piles;
                //int column = 0;
                //List<Card> cardsMoved = new List<Card>();
                //foreach (Card obj in draggedCards)
                //{
                //    int middle = board.BoardMiddle;
                //    if (mousePos.Y >= middle)
                //    {
                //        column = board.pilePositions.Where(x => x < mousePos.X/* + (obj.ActualWidth / 2)*/).Count() - 1;
                //        newPosition = BoardLocation.piles;
                //        if(TryChangeCardPosition(obj, BoardLocation.piles, column))
                //        cardsMoved.Add(obj);
                //    }
                //    else if (mousePos.Y <= middle && board.foundationPositions.Where(x => x < mousePos.X).Any())
                //    {
                //        Debug.WriteLine("ace");
                //        column = board.foundationPositions.Where(x => x < Canvas.GetLeft(obj) + (obj.ActualWidth / 2)).Count() - 1;
                //        newPosition = BoardLocation.foundation;
                //        if(TryChangeCardPosition(obj, BoardLocation.foundation, column))
                //        cardsMoved.Add(obj);
                //    }
                //}
                //CreateCardMovedAction(cardsMoved.ToArray(), newPosition, column);
            }
            foreach (Card obj in draggedCards)
            {
                obj.IsHitTestVisible = true;
            }
            ReArrangeGameBoard();
        }

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

        public void ChangeCardColumn(Card card, int desiredColumn)
        {
            if (Canvas.GetTop(card) > board.BoardMiddle - 125)
            {
                if (card.location == BoardLocation.stock)
                {
                    card.location = BoardLocation.piles;
                    board.stock.Remove(card);
                    RevealCard(board.stock.Last());
                    card.MouseEnter += HighLightCard;
                    card.MouseLeave += DeHighLightCard;
                }
                else
                {
                    board.piles[card.column].Remove(card);
                    RevealLastCardInColumn(card.column);
                }
                Debug.WriteLine(board.foundationPositions.Length + " asdwasdw");
                board.piles[desiredColumn].Add(card);
                Canvas.SetZIndex(card, board.piles[desiredColumn].Count);
                card.column = desiredColumn;
                if (card.location != BoardLocation.piles)
                {
                    card.location = BoardLocation.piles;
                    board.SortFoundation();
                }
            }
            else
            {
                board.SortPiles();
            }
        }

        public void ChangeAceColumn(Card card, int desiredColumn)
        {
            card.column = desiredColumn;
            if (card.location == BoardLocation.stock)
            {
                board.stock.Remove(card);
                RevealCard(board.stock.Last());
                card.location = BoardLocation.foundation;
            }
            else if (card.location == BoardLocation.piles)
            {
                board.piles[card.column].Remove(card);
                RevealLastCardInColumn(card.column);
                board.SortFoundation();
            }
            board.foundations[desiredColumn].Add(card);
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
                    int i = 0;
                    foreach (Card obj in draggedCards)
                    {
                        Canvas.SetLeft(obj, mouse.X - obj.ActualWidth / 2);
                        Canvas.SetTop(obj, (mouse.Y - obj.ActualHeight / 2) - i);
                        i += 50;
                    }
                }
                else
                {
                    Canvas.SetLeft(card, mouse.X - card.ActualWidth / 2);
                    Canvas.SetTop(card, mouse.Y - card.ActualHeight / 2);
                }
            }
        }

        public void RevealLastCardInColumn(int column)
        {
            if (board.piles[column].Count > 0)
            {
                RevealCard(board.piles[column].Last());
            }
        }

        void TakeCardsFromStack(object sender, MouseButtonEventArgs e)
        {
            if (board.stock.Count > 0)
            {
                //List<Card> cards = new List<Card>();
                for (int i = 0; i < Math.Clamp(board.stock.Count, 0, board.stockPull); i++)
                {
                    Card card = board.stock.First();
                    board.stock.RemoveAt(0);
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
        }
        #endregion

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

        #region Action


        //public void Undo(object sender, RoutedEventArgs e)
        //{
        //    Debug.Print(actions.Count + " actions");
        //    if (actions.Count > 0)
        //    {
        //        Debug.Print(actions.Peek().card.ToString() + "aasss");
        //        if (actions.Peek() is CardMoved obj)
        //        {
        //            foreach (Card card in obj.card)
        //            {
        //                RemoveCardFromParent(card);
        //                HideLastCard(obj.LastLocation, obj.lastColumn);
        //                AddCardToNewParent(card, obj.LastLocation, obj.lastColumn);
        //                ReArrangeGameBoard();
        //                actions.Pop();
        //            }
        //        }
        //        else if (actions.Peek() is CardRevealed revealedCard)
        //        {
        //            foreach (Card card in revealedCard.card)
        //            {
        //                card.IsRevealed = false;
        //                actions.Pop();
        //            }
        //        }
        //        else if (actions.Peek() is CardPoolAction cardTurned)
        //        {
        //            foreach (Card card in cardTurned.card)
        //            {
        //                if (board.stock.Contains(card))
        //                {
        //                    board.stock.Remove(card);
        //                    board.waste?.AddFirst(card);
        //                }
        //                else if (board.waste.Contains(card))
        //                {
        //                    board.waste.Remove(card);
        //                    board.stock.Add(card);
        //                }
        //            }
        //            actions.Pop();
        //            ReArrangeGameBoard();
        //        }
        //    }
        //}

        private void HideLastCard(BoardLocation location, int column)
        {
            if (location == BoardLocation.piles)
            {
                board.piles[column].Last().HideCard();
            }
            else if (location == BoardLocation.stock)
            {
                board.stock.Last().IsHitTestVisible = false;
            }
        }
        #endregion
    }
}
