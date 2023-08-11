﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Solitaire
{
    public class SolitaireBoardData
    {
        public SolitaireBoardData()
        {
            StockPlaceHolder = new Card();
            stock = new();
        }

        #region BoardData
        public int stockPull = 1;
        public LinkedList<Card>? waste;//the 3 cards besides the card stack
        public Card StockPlaceHolder;
        public List<Card> stock;//left over cards

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
        public bool ChangeCardLocation(Card[] cards, BoardLocation desiredLocation, int DesiredIndex)
        {
            if (cards.First().location == BoardLocation.stock)
            {
                if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> desiredPile))
                {
                    foreach (Card card in cards)
                    {
                        waste?.Remove(card);
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
                    if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> desiredPile))
                    {
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
                Debug.Print("ass");
                if (GetCardPile(desiredLocation, DesiredIndex, out ICollection<Card> cards) && PileRules(card, cards))
                {
                    return true;
                }
            }
            else if (desiredLocation == BoardLocation.foundation && desiredLocation >= 0)
            {
                Debug.Print("foundation");
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
            catch { cards = null; return false; }
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
            else if (cards.Count > 0 && card.CardValue == cards.Last().CardValue + 1)
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
            int j = 0;
            foreach (List<Card> list in piles)
            {
                if (list.Count > 0)
                {
                    int pileSpacing = Math.Clamp(list.Count > 0 ? BoardMiddle / list.Count : 0, 0, 50);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Canvas.SetZIndex(list[i], i + 1);
                        list[i].IsHitTestVisible = list[i].IsRevealed;
                        Canvas.SetLeft(list[i], pilePositions[j]);
                        Canvas.SetTop(list[i], BoardMiddle + pileSpacing * i);
                    }
                }
                j++;
            }
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
            int cardPoolCount = stock.Count;
            foreach (Card card in stock)
            {
                card.Visibility = System.Windows.Visibility.Hidden;
                card.IsHitTestVisible = false;
                Canvas.SetLeft(card, pilePositions[0]);
                Canvas.SetTop(card, foundationTop);
                Panel.SetZIndex(card, 0);
            }
            if (waste != null)
            {
                for (int i = cardPoolCount - 1, j = 3; i >= cardPoolCount - Math.Clamp(cardPoolCount, 1, 3); i--, j--)
                {
                    Card card = stock[i];
                    card.IsRevealed = false;
                    Panel.SetZIndex(card, 0);
                    card.Visibility = Visibility.Visible;
                    card.IsRevealed = true;
                    card.IsHitTestVisible = true;
                    Panel.SetZIndex(card,j);
                    Canvas.SetLeft(card, pilePositions[1]+50*i);
                    Canvas.SetTop(card, foundationTop);

                }
            }
        }
    }
}
