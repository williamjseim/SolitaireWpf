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

        bool reveled = false;
        BitmapImage? frontSide;
        BitmapImage? BackSide;
        public Card(Uri cardFacePath, Suits cardSuit, CardValue cardValue)
        {
            InitializeComponent();
            frontSide = new BitmapImage(cardFacePath);
            BackSide = new BitmapImage(new Uri("C:\\Users\\zbcwise\\source\\repos\\Solitaire\\Solitaire\\Extra\\PixelPlebes_V1_4x_Update002__Update001_13.png"));
            CardImage.Source = BackSide;
            this.suit = cardSuit;
            this.CardValue = cardValue;
            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject data = new();
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        public void RevelCard()
        {
            reveled = true;
            CardImage.Source = frontSide;
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            if(e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.Hand);
            }
            else if (e.Effects.HasFlag(DragDropEffects.Move))
            {
                Mouse.SetCursor(Cursors.Hand);
            }
            e.Handled = true;
        }

        private void CardImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(reveled && e.LeftButton == MouseButtonState.Pressed)
            {
                ((Canvas)this.Parent).Children.Remove(this);
            }
        }
    }
}
