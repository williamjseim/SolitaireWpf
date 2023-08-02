using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solitaire.Actions
{
    public abstract class Action
    {
        public Card card { get; set; }

        public Action(Card card)
        {
            this.card = card;
        }
    }
}
