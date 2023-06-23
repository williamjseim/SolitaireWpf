using System;
using System.Collections.Generic;
using System.IO;

namespace Solitaire
{
    public enum Suits
    {
        Clubs=0,
        Spades=1,
        Diamonds=2,
        Hearts=3,
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
                for (int i = 0; i < Directory.GetFiles(folders).Length; i++)
                {
                    cards.Add(new Card(new Uri(Path.GetFullPath(folders+$"/{i+1}.png")), (Suits)suit, (CardValue)type));
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
