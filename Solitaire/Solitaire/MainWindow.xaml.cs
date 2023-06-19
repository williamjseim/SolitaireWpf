using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Card>[] cardColumns = new List<Card>[]
        {
            new List<Card>(),//1
            new List<Card>(),//2
            new List<Card>(),//3
            new List<Card>(),//4
            new List<Card>(),//5
            new List<Card>(),//6
            new List<Card>(),//7
        };


        public MainWindow()
        {
            InitializeComponent();
            List<Card> cards = CardContructor.GenerateCards();
            
            for (int column = 0; column < cardColumns.Length; column++)
            {
                Canvas canvasColumn = (Canvas)CardGrid.Children[column];
                canvasColumn.DragLeave += Canvas_DragLeave;
                canvasColumn.DragOver += Canvas_DragOver;
                canvasColumn.Drop += Card_Drop;
                for (int row = 0; row <= column; row++)
                {
                    int margin = 0;
                    Card card = cards[0];
                    cards.RemoveAt(0);
                    card.HorizontalAlignment = HorizontalAlignment.Center;
                    cardColumns[column].Add(card);
                    foreach (Card obj in cardColumns[column])
                    {
                        margin += 50;
                    }
                    
                    card.AllowDrop = true;
                    card.MouseMove += Card_MoveMouse;
                    card.MouseUp += Card_MouseUp;
                    Canvas.SetTop(card, margin);
                    Canvas.SetLeft(card, 50);
                    canvasColumn.Children.Add(card);
                    canvasColumn.DragEnter += MoveCard;
                }
            }
            foreach (var item in cardColumns)
            {
                item.Last().RevealCard();
            }
        }

        bool CanCardBeAdded(UIElementCollection canvasChildren, int cardColumn, Card card)
        {
            if(canvasChildren.Count > 0)
            {
                if (canvasChildren[canvasChildren.Count-1] is Card obj)
                {
                    if (card.suit == Suits.Clubs || card.suit == Suits.Spades)
                    {
                        if((int)obj.suit > 2)
                        {
                            if(obj.CardValue > card.CardValue)
                            {
                                return true;
                            }
                        }
                    }
                    else if (card.suit == Suits.Diamonds || card.suit == Suits.Hearts)
                    {
                        if((int)obj.suit < 2)
                        {
                            if (obj.CardValue > card.CardValue)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (card.CardValue == CardValue.K)
                {
                    return true;
                }
            }
            return false;
        }

        void ReArangeColumn(UIElementCollection cards)
        {
            int i = 0;
            foreach (UIElement child in cards)
            {
                Canvas.SetTop(child, i);
                i += 50;
            }
        }

        void MoveCard(object sender, DragEventArgs e)
        {

        }

        private void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            if(sender is Card card)
            if (card.reveled && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(card, new DataObject(DataFormats.Serializable,card), DragDropEffects.Move);
            }
        }

        private void Card_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Card_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if(data is Card card && sender is Canvas canvas)
            {
                Transform cardPosition = card.LayoutTransform;
                Debug.Print(cardPosition.Value.OffsetX.ToString()+"thick");
                int canvasColumn = Grid.GetColumn(canvas);
                int cardColumn = Grid.GetColumn((Canvas)card.Parent);
                Canvas cardCanvas = (Canvas)CardGrid.Children[cardColumn];

                if(CanCardBeAdded(((Canvas)CardGrid.Children[canvasColumn]).Children, cardColumn, card))
                {
                    cardCanvas.Children.Remove(card);
                    canvas.Children.Add(card);
                    if(cardCanvas.Children.Count > 0)
                    ((Card)cardCanvas.Children[cardCanvas.Children.Count - 1]).RevealCard();
                    ReArangeColumn(cardCanvas.Children);
                }
                else
                {
                    card.RenderTransform = new TranslateTransform(cardPosition.Value.OffsetX,cardPosition.Value.OffsetY);
                    Debug.Print(card.RenderTransform.Value.OffsetX.ToString()+"thick");
                }
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if(data is UserControl card)
            {
                Point drop = e.GetPosition((Canvas)card.Parent);
                Canvas.SetLeft(card, drop.X-card.Width/2);
                Canvas.SetTop(card, drop.Y-card.Height/2);
            }
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            if(e.OriginalSource is Canvas)
            {
                object data = e.Data.GetData(DataFormats.Serializable);
                if (data is Card card)
                {
                    ((Canvas)sender).Children.Remove(card);
                }
            }
        }
    }
}
