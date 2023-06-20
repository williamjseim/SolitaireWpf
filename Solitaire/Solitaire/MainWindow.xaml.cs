using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        //List<Card>[] cardColumns = new List<Card>[]
        //{
        //    new List<Card>(),//1
        //    new List<Card>(),//2
        //    new List<Card>(),//3
        //    new List<Card>(),//4
        //    new List<Card>(),//5
        //    new List<Card>(),//6
        //    new List<Card>(),//7
        //};


        public MainWindow()
        {
            InitializeComponent();
            List<Card> cards = CardContructor.GenerateCards();
            
            GameBoard.DragOver += Canvas_DragOver;
            for (int column = 0; column < CardGrid.Children.Count; column++)
            {
                Canvas canvasColumn = (Canvas)CardGrid.Children[column];
                canvasColumn.Drop += Canvas_Drop;
                canvasColumn.AllowDrop = true;
                for (int row = 0; row <= column; row++)
                {
                    int margin = 0;
                    Card card = cards[0];
                    cards.RemoveAt(0);
                    card.HorizontalAlignment = HorizontalAlignment.Center;
                    margin = 50 * canvasColumn.Children.Count+1;
                    card.MouseMove += Card_MoveMouse;
                    Canvas.SetTop(card, margin);
                    Canvas.SetLeft(card, 50);
                    canvasColumn.Children.Add(card);
                }
            }
            foreach (var item in CardGrid.Children)
            {
                if (item is Canvas canvas)
                    ((Card)canvas.Children[canvas.Children.Count - 1]).RevealCard();
            }
        }

        bool CanCardBeAdded(UIElementCollection canvasChildren, int cardColumn, Card card)
        {
            return true;
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
                Canvas.SetLeft(child, 50);
                i += 50;
            }
        }

        private void Card_MoveMouse(object sender, MouseEventArgs e)
        {
            if(sender is Card card)
                if (card.reveled && e.LeftButton == MouseButtonState.Pressed)
                {
                    card.IsHitTestVisible = false;
                    Debug.Print(card.CardValue.ToString() + " : " + card.suit.ToString());
                    DragDrop.DoDragDrop(card, new DataObject(DataFormats.Serializable,card), DragDropEffects.Move);
                }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if(data is Card card)
            {
                Debug.Print(card.CardImage.Source.ToString()+"asdwasd");
                if(sender is Canvas transferCanvas)
                {
                    transferCanvas.Background = new SolidColorBrush(Color.FromRgb(250, 250, 0));
                    //int canvasColumn = Grid.GetColumn(transferCanvas);
                    int cardColumn = Grid.GetColumn((Canvas)card.Parent);
                    Canvas originCanvas = (Canvas)card.Parent;
                    if (CanCardBeAdded(transferCanvas.Children, cardColumn, card))
                    {
                        originCanvas.Children.Remove(card);
                        transferCanvas.Children.Add(card);
                        if (originCanvas.Children.Count > 0)
                            ((Card)originCanvas.Children[originCanvas.Children.Count - 1]).RevealCard();
                        Canvas.SetLeft(card, 0);
                        Canvas.SetTop(card, 0);
                        ReArangeColumn(transferCanvas.Children);
                        Debug.Print(card.CardValue.ToString() + " : " + card.suit.ToString()+"*");
                        card.IsHitTestVisible = true;
                    }
                    else
                    {
                        card.IsHitTestVisible = true;
                        Canvas.SetLeft(card, 0);
                        Canvas.SetTop(card, 0);
                        ReArangeColumn(originCanvas.Children);
                    }
                }
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if(data is Card card)
            {
                Point mouse = e.GetPosition((UIElement)card.Parent);
                Canvas.SetZIndex(card, 1);
                Canvas.SetLeft(card,mouse.X-card.Width/2);
                Canvas.SetTop(card,mouse.Y-card.Height/2);
            }
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            //if(e.OriginalSource is Canvas)
            //{
            //    object data = e.Data.GetData(DataFormats.Serializable);
            //    if (data is Card card)
            //    {
            //        ((Canvas)sender).Children.Remove(card);
            //    }
            //}
        }
    }
}
