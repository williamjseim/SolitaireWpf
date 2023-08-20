using Solitaire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Solitaire
{
    public class SolitaireBoardData
    {
        public SolitaireBoardData()
        {
            StockPlaceHolder = new Card();
            stock = new();
        }

        public Stack<Move> moves = new Stack<Move>();

        #region BoardData

        public bool allCardsReveled = true;
        public int stockPull = 1;
        public LinkedList<Card>? waste = new();//the 3 cards besides the card stack
        public Card StockPlaceHolder;
        public LinkedList<Card> stock;//left over cards

        public int cardWidth = 252;
        public double rowWidth;

        public int[] pilePositions = new int[7];
        public int BoardMiddle;
        public int pileLength;

        public List<Card>[] piles = new List<Card>[]//the card columns/tableau
        {
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
        };

        public int foundationTop;

        public int[] foundationPositions = new int[4];

        public List<Card>[] foundations = new List<Card>[]//ace piles
        {
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
            new List<Card>(),
        };
        #endregion

        //changes the parent list of moved cards and updates the card so it reflects the move
        public bool ChangeCardLocation(Card[] cards, BoardLocation desiredLocation, int DesiredIndex)
        {
            if (cards.First().location == BoardLocation.stock)
            {
                if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> desiredPile) && CheckRules(desiredLocation, cards[0], desiredPile))
                {
                    this.moves.Push(new CardMoved(cards, BoardLocation.stock));
                    foreach (Card card in cards)
                    {
                        waste.Remove(card);
                        stock.Remove(card);
                        card.location = desiredLocation;
                        card.column = DesiredIndex;
                        desiredPile.Add(card);
                    }
                }
            }
            else if (CanCardBeAdded(cards[0], desiredLocation, DesiredIndex))
            {
                //removeCard
                if (GetCardPile(cards[0].location, cards[0].column, out ICollection<Card> originPile))
                {
                    //add card
                    if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> desiredPile) && CheckRules(desiredLocation, cards[0], desiredPile))
                    {
                        this.moves.Push(new CardMoved(cards, cards[0].location));
                        foreach (Card card in cards)
                        {
                            originPile.Remove(card);
                            card.location = desiredLocation;
                            card.column = DesiredIndex;
                            desiredPile.Add(card);
                        }
                    }
                    if (originPile.Count > 0)
                        originPile.Last().IsRevealed = true;
                }
                return true;
            }
            return false;
        }

        private bool CanCardBeAdded(Card card, BoardLocation desiredLocation, int DesiredIndex)
        {
            if (desiredLocation == BoardLocation.piles && desiredLocation >= 0)
            {
                if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> cards) && PileRules(card, cards))
                {
                    return true;
                }
            }
            else if (desiredLocation == BoardLocation.foundation && desiredLocation >= 0)
            {
                if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> cards) && FoundationRules(card, cards))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// trys to find the original pile for the card returns null if false
        /// </summary>
        /// <param name="location"></param>
        /// <param name="index"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        private bool GetCardPile(BoardLocation location, int index, out ICollection<Card> cards)
        {
            try
            {
                switch (location)
                {
                    case BoardLocation.foundation:
                        cards = foundations[index];
                        return true;
                    case BoardLocation.piles:
                        cards = piles[index];
                        return true;
                    default:
                        cards = null;
                        return false;
                }
            }
            catch 
            { 
                cards = null; 
                return false;
            }
        }

        //checks the rules based on card's desired location
        private bool CheckRules(BoardLocation location, Card card, ICollection<Card> cards)
        {
            if(location == BoardLocation.foundation)
            {
                return FoundationRules(card, cards);
            }
            if(location == BoardLocation.piles)
            {
                return PileRules(card, cards);
            }
            return false;
        }

        private bool PileRules(Card card, ICollection<Card> cards)
        {
            if (cards.Count == 0 && card.CardValue == CardValue.K)
            {
                return true;
            }
            else if (cards.Count > 0 && card.CardValue == cards.Last().CardValue - 1 && card.Color != cards.Last().Color)
            {
                return true;
            }
            return false;
        }
        
        private bool FoundationRules(Card card, ICollection<Card> cards)
        {
            if (cards.Count == 0 && card.CardValue == CardValue.A)
            {
                return true;
            }
            else if (cards.Count > 0 && card.CardValue == cards.Last().CardValue + 1 && card.suit == cards.First().suit)
            {
                return true;
            }
            return false;
        }

        public void SortBoard()
        {
            SortFoundation();
            SortPiles();
            SortStockAndWaste();
        }

        public void SortPiles()
        {
            bool allCardReveled = true;
            int j = 0;
            foreach (List<Card> list in piles)
            {
                if (list.Count > 0)
                {
                    int pileSpacing = Math.Clamp(list.Count > 0 ? BoardMiddle / list.Count : 0, 0, 50);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].IsRevealed != true)
                        {
                            allCardReveled = false;
                        }
                        Canvas.SetZIndex(list[i], i + 1);
                        list[i].IsHitTestVisible = list[i].IsRevealed;
                        Canvas.SetLeft(list[i], pilePositions[j]);
                        Canvas.SetTop(list[i], BoardMiddle + pileSpacing * i);
                    }
                }
                j++;
            }
            this.allCardsReveled = allCardReveled;
        }

        public void SortFoundation()
        {
            int j = 0;
            foreach (List<Card> list in foundations)
            {
                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].IsHitTestVisible = false;
                        Canvas.SetZIndex(list[i], 0);
                        Canvas.SetLeft(list[i], foundationPositions[j]);
                        Canvas.SetTop(list[i], foundationTop);
                    }
                    list.Last().IsHitTestVisible = true;
                    Canvas.SetZIndex(list.Last(), 1);
                    j++;
                }
            }
        }

        public void SortStockAndWaste()
        {
            int stockCount = waste.Count;
            foreach (Card card in stock)
            {
                card.Visibility = System.Windows.Visibility.Hidden;
                card.IsHitTestVisible = false;
                Canvas.SetLeft(card, pilePositions[0]);
                Canvas.SetTop(card, foundationTop);
                Panel.SetZIndex(card, 0);
            }
            if (waste != null && waste.Count > 0)
            {
                for (int i = stockCount - 1, j = 3; i >= stockCount - Math.Clamp(stockCount, 1, 3); i--, j--)
                {
                    Card card = waste.ElementAt(i);
                    card.IsRevealed = false;
                    Panel.SetZIndex(card, 0);
                    card.Visibility = Visibility.Visible;
                    card.IsRevealed = true;
                    card.IsHitTestVisible = true;
                    Panel.SetZIndex(card,j);
                    Canvas.SetLeft(card, (pilePositions[1]+75*j) - card.ActualWidth / 2);
                    Canvas.SetTop(card, foundationTop);

                }
            }
        }

        public void CheckIfGameIsWon()
        {
            if((waste.Count == 0 || waste == null) && (stock.Count == 0 || stock == null) && allCardsReveled)
            {
                //game won
            }
        }

        public void Undo(object sender, RoutedEventArgs e)
        {
            Debug.Print("asdwas"+moves.Count);
            if(this.moves.TryPop(out Move? obj))
            {
                if(obj is CardMoved cardMove)
                {
                    Card card = cardMove.movedCards[0];
                    if(cardMove.originLocation == BoardLocation.stock)
                    {
                        if(GetCardPile(card.location, card.column, out ICollection<Card> originPile))
                        {
                            foreach (Card pileCard in cardMove.movedCards)
                            {
                                originPile.Remove(pileCard);
                                waste.AddLast(pileCard);
                                pileCard.location = cardMove.originLocation;
                                pileCard.column = cardMove.topCardColumn;
                            }
                        }
                    }
                    else if(GetCardPile(card.location, card.column, out ICollection<Card> originPiles))
                    {
                        if(GetCardPile(cardMove.originLocation, cardMove.topCardColumn, out ICollection<Card> desiredPile))
                        {
                            foreach (Card pileCard in cardMove.movedCards)
                            {
                                originPiles.Remove(pileCard);
                                if(desiredPile.Count > 0 && cardMove.originLocation != BoardLocation.foundation)
                                {
                                    desiredPile.Last().HideCard();
                                }
                                desiredPile.Add(pileCard);
                                pileCard.location = cardMove.originLocation;
                                pileCard.column = cardMove.topCardColumn;
                            }
                        }
                    }
                }
                else if(obj is StockTurned stockTurn)
                {
                    if(waste.Count == 0)
                    {
                        waste = new LinkedList<Card>(stock);
                        stock.Clear();
                    }
                    else
                    {
                        Card card = waste.Last();
                        waste.RemoveLast();
                        stock.AddFirst(card);
                        card.Visibility = Visibility.Visible;
                        card.IsRevealed = true;
                    }
                }
            }
            SortBoard();
        }
    }
}

public class CardMoved : Move
{
    public CardMoved(Card[] cards, BoardLocation originLocation)
    {
        this.movedCards = cards;
        this.originLocation = originLocation;
        this.topCardColumn = cards[0].column;
    }

    public Card[] movedCards;
    public BoardLocation originLocation;
    public int topCardColumn;
}

public class StockTurned : Move
{
    
}

public abstract class Move
{

}