using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitaire.Actions
{
    public class CardPoolAction : IUndo
    {
        public Card[] cards;

        public CardPoolAction(Card[] pulledCards)
        {
            cards = pulledCards;
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }
    }
}
