using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Solitaire
{

    public enum CardColor
    {
        R,
        B
    }
    public enum BoardLocation
    {
        Deck,
        Board,
        Ace
    }
    public partial class Card : UserControl
    {
        public Suits suit { get; private set; }
        public CardValue CardValue { get; private set; }
        public BoardLocation location { get; set; }
        public int column { get; set; }

        public CardColor Color { get { return GetColor(); } }

        public bool IsRevealed = false;
        BitmapImage? frontSide;
        BitmapImage? BackSide;
        public BitmapImage? currentImage;

        CardColor GetColor()
        {
            return (int)suit <= 1 ? CardColor.B : CardColor.R;
        }

        public Card()
        {
            InitializeComponent();
            BackSide = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"../../../Extra/BackFace.png")));
            CardImage.Source = BackSide;
        }
        public Card(Uri cardFacePath, Suits cardSuit, CardValue cardValue) : this()
        {
            frontSide = new BitmapImage(cardFacePath);
            this.suit = cardSuit;
            this.CardValue = cardValue;
            this.MouseLeave += Mouse_Leave;
            this.MouseEnter += Mouse_Enter;
        }

        void Mouse_Enter(object sender, MouseEventArgs e)
        {
        }

        void Mouse_Leave(object sender, MouseEventArgs e)
        {
        }

        public void RevealCard(bool hitTestVisible = true)
        {
            IsRevealed = true;
            CardImage.Source = frontSide;
            IsHitTestVisible = hitTestVisible;
        }

        public override string ToString()
        {
            return $"value {CardValue} suit {suit} colar {GetColor()}";
        }
    }
}
