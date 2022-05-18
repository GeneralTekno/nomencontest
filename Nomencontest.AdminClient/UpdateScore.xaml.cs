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
using System.Windows.Shapes;

namespace Nomencontest.Clients.Admin
{
    /// <summary>
    /// Interaction logic for UpdateScore.xaml
    /// </summary>
    public partial class UpdateScore : Window
    {
        public int Player1Score;
        public int Player2Score;
        public bool Result = false;

        public UpdateScore(int player1Score, int player2Score)
        {
            Player1Score = player1Score;
            Player2Score = player2Score;
            Result = false;
            InitializeComponent();
            Player1Text.Text = Player1Score.ToString();
            Player2Text.Text = Player2Score.ToString();
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            int outputValue = 0;
            if (int.TryParse(Player1Text.Text, out outputValue))
            {
                Player1Score = outputValue;
                if (int.TryParse(Player2Text.Text, out outputValue))
                {
                    Player2Score = outputValue;
                    Result = true;
                    Close();
                }
            }
        }
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
