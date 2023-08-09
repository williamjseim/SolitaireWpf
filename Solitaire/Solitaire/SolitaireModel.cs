using Solitaire.Actions;
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
        BoardData board = new();
        List<Card> draggedCards = new();
        int amountFromStack = 1;
        public override void ReArrangeGameBoard(object? sender = null, RoutedEventArgs? e = null)
        {
            CalculateGameBoard();
            ReArrangeColumn();
            ReArrangeAceColumn();
            ReArrangeCardStack();
            ReArrangeCardPool();
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
                    card.MouseLeftButtonDown += Card_MoveMouse;
                    //card.MouseDoubleClick += MoveCardToAceStack;
                    //card.MouseEnter += HighLightCard;
                    //card.MouseLeave += DeHighLightCard;
                    card.MouseRightButtonDown += CardToString;
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
                RevealCard(card.Last());
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
            board.cardQueue = new LinkedList<Card>(cards);
        }

        #region Rearrange
        public void CalculateGameBoard()//sets up card columns based on window width and height
        {
            double height = view.GameBoard.ActualHeight;
            double width = view.GameBoard.ActualWidth;
            //board.aceTop = (int)view.window.ActualHeight / 20;
            int spacing = Math.Abs((int)((board.rowWidth - board.cardWidth) / 2));
            board.aceTop = 0;

            board.rowWidth = width / 7;
            for (int i = 0; i < 7; i++)
            {
                board.BoardColumnPositions[i] = (int)(board.rowWidth * i) + spacing;
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
                    int cardSpacing = Math.Max((int)(view.GameBoard.Height - board.BoardMiddle) / board.BoardColumns[i].Count, 50);
                    int j = 0;
                    int zindex = 0;
                    foreach (Card card in board.BoardColumns[i])
                    {
                        Canvas.SetTop(card, j + board.BoardMiddle);
                        Canvas.SetLeft(card, board.BoardColumnPositions[i]/* + (int)((board.rowWidth - board.cardWidth) / 2)*/);
                        Canvas.SetZIndex(card, zindex);
                        j += cardSpacing;
                        zindex++;
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
                    RevealCard(card, false);
                    Canvas.SetLeft(card, board.BoardColumnPositions[1] + ((board.cardWidth / 3 * j) - board.cardWidth / 3));
                    Panel.SetZIndex(card, j);
                    card.Visibility = Visibility.Visible;
                }
                RevealCard(board.cardPool.Last());
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
        #endregion

        #region MoveCard
        public void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            draggedCards.Clear();
            if (sender is Card card)
                if (card.IsRevealed && e.LeftButton == MouseButtonState.Pressed)
                {
                    if(card.location == BoardLocation.Board/* && board.BoardColumns[card.column].Last() != card*/)
                    {
                        int index = board.BoardColumns[card.column].IndexOf(card);
                        for (int i = index; i < board.BoardColumns[card.column].Count; i++)
                        {
                            draggedCards.Add(board.BoardColumns[card.column][i]);
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
            object data = e.Data.GetData(DataFormats.Serializable);
            if (sender is Canvas)
            {
                var mousePos = e.GetPosition(Application.Current.MainWindow);
                foreach (Card obj in draggedCards)
                {
                    obj.IsHitTestVisible = true;
                }
                Debug.Print("dragged"+draggedCards.Count);
                foreach (Card obj in draggedCards)
                {
                    Debug.Print(obj.ToString()+ mousePos);
                    int middle = board.BoardMiddle;
                    if (mousePos.Y >= middle)
                    {
                        int column = board.BoardColumnPositions.Where(x => x < mousePos.X/* + (obj.ActualWidth / 2)*/).Count() - 1;
                        TryChangeCardPosition(obj, BoardLocation.Board, column);
                    }
                    else if (mousePos.Y <= middle && board.aceStacks.Where(x => x < mousePos.X).Any())
                    {
                        Debug.WriteLine("ace");
                        int column = board.aceStacks.Where(x => x < Canvas.GetLeft(obj) + (obj.ActualWidth / 2)).Count() - 1;
                        TryChangeCardPosition(obj, BoardLocation.Ace, column);
                    }
                    else
                    {
                        ReArrangeColumn();
                    }
                }
                //else
                //{
                //    Card card = data as Card;
                //    Debug.WriteLine("asdwasd" + draggedCards.Count);
                //    int top = board.BoardMiddle - 125;
                //    if (Canvas.GetTop(card) > top)
                //    {
                //        int column = board.BoardColumnPositions.Where(x => x < Canvas.GetLeft(card) + (card.ActualWidth / 2)).Count() - 1;
                //        TryChangeCardPosition(card, BoardLocation.Board, column);
                //    }
                //    else if (Canvas.GetTop(card) < top && board.aceStacks.Where(x => x < Canvas.GetLeft(card)).Any())
                //    {
                //        int column = board.aceStacks.Where(x => x < Canvas.GetLeft(card) + (card.ActualWidth / 2)).Count() - 1;
                //        TryChangeCardPosition(card, BoardLocation.Ace, column);
                //    }
                //    else
                //    {
                //        ReArrangeColumn();
                //    }
                //}
            }
            foreach (Card obj in draggedCards)
            {
                obj.IsHitTestVisible = true;
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
                    RevealCard(board.cardPool.Last());
                    card.MouseEnter += HighLightCard;
                    card.MouseLeave += DeHighLightCard;
                }
                else
                {
                    board.BoardColumns[card.column].Remove(card);
                    RevealLastCardInColumn(card.column);
                }
                Debug.WriteLine(board.aceStacks.Length + " asdwasdw");
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
                RevealCard(board.cardPool.Last());
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
                if(draggedCards.Count != 0)
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

        public void MoveCardToAceStack(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card && card.location != BoardLocation.Ace)
            {
                for (int i = 0; i < board.AceColumns.Length; i++)
                {
                    if (CanCardBeAddedToAceStack(card, i))
                    {
                        board.BoardColumns[card.column].Remove(card);
                        RevealCard(board.BoardColumns[card.column].Last());
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
                RevealCard(board.BoardColumns[column].Last());
            }
        }
        void TakeCardsFromStack(object sender, MouseButtonEventArgs e)
        {
            if (board.cardQueue?.Count > 0)
            {
                for (int i = 0; i < Math.Clamp(board.cardQueue.Count, 0, amountFromStack); i++)
                {
                    Card card = board.cardQueue.First();
                    board.cardQueue.RemoveFirst();
                    board.cardPool.Add(card);
                    card.Visibility = Visibility.Visible;
                    RevealCard(card);
                    CreateCardPoolAction(card);
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
        #endregion

        #region ChangeParent

        bool AddCardToNewParent(Card card, BoardLocation desiredLocation, int desiredColumn)
        {
            if (desiredLocation == BoardLocation.Board)
            {
                card.column = desiredColumn;
                board.BoardColumns[desiredColumn].Add(card);
                card.location = BoardLocation.Board;
                return true;
            }
            else if (desiredLocation == BoardLocation.Ace)
            {
                card.column = desiredColumn;
                board.AceColumns[desiredColumn].Add(card);
                card.location = BoardLocation.Ace;
                return true;
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
                        RevealCard(board.AceColumns[card.column].Last());
                    return true;
                case BoardLocation.Deck:
                    board.cardPool.Remove(card);
                    if (board.AceColumns[card.column].Count > 0)
                        if(board.cardPool.Count > 0)
                            RevealCard(board.cardPool.Last());
                    return true;
            }
            return false;
        }
        #endregion

        #region Verify
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

        void TryChangeCardPosition(Card card, BoardLocation desiredLocation, int desiredColumn)
        {
            //Debug.WriteLine("trychange " + card.ToString());
            if (CanCardBeAdded(card, desiredLocation, desiredColumn))
            {
            //Debug.WriteLine("canadd " + card.ToString());
                if (RemoveCardFromParent(card))
                {
                    Debug.WriteLine("remove" + card.ToString());
                    CreateCardMovedAction(card, card.location, card.column);
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
        #endregion

        void CardToString(object sender, MouseButtonEventArgs e)
        {
            if (sender is Card card)
            {
                Debug.WriteLine(card.ToString() + " " + Canvas.GetLeft(card)+" "+card.location+" zindex "+Panel.GetZIndex(card)+ " column "+ card.column);
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

        Queue<Actions.Action> actions = new();

        public void Undo(object sender, RoutedEventArgs e)
        {
            Debug.Print(actions.Count+" actions");
            if(actions.Count > 0)
            {
                if(actions.Peek() is CardMoved obj)
                {
                    RemoveCardFromParent(obj.card);
                    HideLastCard(obj.LastLocation, obj.lastColumn);
                    AddCardToNewParent(obj.card, obj.LastLocation, obj.lastColumn);
                    ReArrangeGameBoard();
                    actions.Dequeue();
                }
                else if(actions.Peek() is CardRevealed revealedCard)
                {
                    revealedCard.card.IsRevealed = false;
                    actions.Dequeue();
                }
                else if(actions.Peek() is CardPoolAction cardTurned)
                {
                    if (board.cardPool.Contains(cardTurned.card))
                    {
                        board.cardPool.Remove(cardTurned.card);
                        board.cardQueue?.AddFirst(cardTurned.card);
                    }
                    else if(board.cardQueue.Contains(cardTurned.card))
                    {
                        board.cardQueue.Remove(cardTurned.card);
                        board.cardPool.Add(cardTurned.card);
                    }
                    actions.Dequeue();
                    ReArrangeGameBoard();
                }
            }
        }

        private void HideLastCard(BoardLocation location, int column)
        {
            if(location == BoardLocation.Board)
            {
                board.BoardColumns[column].Last().HideCard();
            }
            else if(location == BoardLocation.Deck)
            {
                board.cardPool.Last().IsHitTestVisible = false;
            }
        }

        private void CreateCardPoolAction(Card card)
        {
            actions.Enqueue(new CardPoolAction(card));
        }

        public void CreateCardMovedAction(Card card, BoardLocation oldLocation, int oldColumn)
        {
            actions.Enqueue(new CardMoved(card, oldLocation, oldColumn));
        }

        #endregion
    }

    public struct BoardData
    {
        public BoardData()
        {
            cardStackPlaceHolder = new Card();
            cardPool = new();
        }

        public LinkedList<Card>? cardQueue;
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
