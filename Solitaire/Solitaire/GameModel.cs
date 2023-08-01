using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Solitaire
{
    public abstract class GameModel
    {
        public MainWindow? view;
        public abstract void Setup(object sender, RoutedEventArgs e);
        public abstract void ReArrangeGameBoard(object? sender = null, RoutedEventArgs? e = null);
    }
}
