using System;
using System.Collections.Generic;
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
                    Canvas.SetTop(card, margin);
                    Canvas.SetLeft(card, 50);
                    ((Canvas)CardGrid.Children[column]).Children.Add(card);
                }
            }
            foreach (var item in cardColumns)
            {
                item.Last().RevelCard();
            }
        }
        
        void SetupGameBoard()
        {
            
        }

    }
}
