using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
        GameModel gameModel;
        public MainWindow()
        {
            gameModel = new SolitaireModel();
            InitializeComponent();
            gameModel.view = this;
            window.Loaded += gameModel.Setup;
            window.SizeChanged += gameModel.ReArrangeGameBoard;
            GameBoard.AllowDrop = true;
        }
    }
}
