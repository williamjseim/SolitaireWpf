using System;
using System.Collections.Generic;
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
    public partial class Card : UserControl
    {
        public Suits suit { get; private set; }
        public CardValue CardValue { get; private set; }
        public TranslateTransform position { get; set; }

        public bool reveled = false;
        BitmapImage? frontSide;
        BitmapImage? BackSide;

        
        public Card(Uri cardFacePath, Suits cardSuit, CardValue cardValue)
        {
            InitializeComponent();
            frontSide = new BitmapImage(cardFacePath);
            BackSide = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"../../../Extra/BackFace.png")));
            CardImage.Source = BackSide;
            this.suit = cardSuit;
            this.CardValue = cardValue;
        }

        public void RevealCard()
        {
            reveled = true;
            CardImage.Source = frontSide;
        }
    }
}
