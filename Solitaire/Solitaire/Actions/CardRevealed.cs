using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitaire.Actions
{
    internal class CardRevealed : Action, IUndo
    {
        public CardRevealed(Card[] card) : base(card)
        {
            
        }
        public void Undo()
        {
            
        }
    }
}
