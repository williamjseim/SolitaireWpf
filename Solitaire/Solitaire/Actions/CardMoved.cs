namespace Solitaire.Actions
{
    public class CardMoved : Action, IUndo
    {
        public int lastColumn { get; set; }
        public BoardLocation LastLocation { get; set; }
        public CardMoved(Card card, BoardLocation oldLocation, int oldColumn) : base(card)
        {
            this.lastColumn = oldColumn;
            this.LastLocation = oldLocation;
        }

        public void Undo()
        {
            
        }
    }
}
