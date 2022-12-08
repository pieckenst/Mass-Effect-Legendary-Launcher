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

namespace MASSEFFECTLauncher_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            base.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void lbl_progressBar_Click(object sender, RoutedEventArgs e)
        {
            Process me1game = new Process();
            Process me2game = new Process();
            Process me3game = new Process();
            Process lmexgame = new Process();
            Console.WriteLine();
            string gamePath;
            string legacygamePathme1;
            string legacygamePathme2;
            string legacygamePathme3;
            if(gameselector.SelectedValue == "Mass effect 1")
            {
                legacygamePathme1 = gamepatherwrite.Text;
            }
        }
    }
}
