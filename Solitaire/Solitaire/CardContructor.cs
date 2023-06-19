using System;
using System.Collections.Generic;
using System.IO;

namespace Solitaire
{
    public enum Suits
    {
        Clubs,
        Spades,
        Diamonds,
        Hearts,
    }
    public enum CardValue
    {
        A,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        J,
        Q,
        K,
    }
    public static class CardContructor
    {
        public static List<Card> GenerateCards()
        {
            int suit = 0;
            int type = 0;
            List<Card> cards = new();
            foreach (string folders in Directory.GetDirectories(@"../../../Cards"))
            {
                foreach (string card in Directory.GetFiles(folders))
                {
                    cards.Add(new Card(new Uri(Path.GetFullPath(card)), (Suits)suit, (CardValue)type));
                    type++;
                }
                suit++;
                type = 0;
            }
            ShuffleMe(cards);
            return cards;
        }

        public static void ShuffleMe<T>(this IList<T> list)
        {
            Random random = new Random();
            int n = list.Count;

            for (int i = list.Count - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);

                T value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }
    }
}
