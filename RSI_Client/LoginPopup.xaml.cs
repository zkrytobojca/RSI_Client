using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using RSI_Client.Model;

namespace RSI_Client
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class LoginPopup : Window
    {
        public User LoggedUser { get; set; }
        public bool OpenRegister { get; set; }
        private List<User> Users { get; set; }
        public LoginPopup(List<User>users)
        {
            InitializeComponent();
            OpenRegister = false;
            Users = users;
            CommandBinding commandLoginBinding = new CommandBinding(
            CommandLogin, ExecutedLogin, CanExecuteLogin);
            CommandBindings.Add(commandLoginBinding);
            ButtonLogin.Command = CommandLogin;
        }

        private void LabelRegisterClick(object sender, MouseButtonEventArgs e)
        {
            OpenRegister = true;
            Close();
        }
        #region LoginCommand
        public static RoutedCommand CommandLogin = new RoutedCommand();
        private void ExecutedLogin(object sender, ExecutedRoutedEventArgs e)
        {
            TryLogin();

        }
        private void CanExecuteLogin(object sender, CanExecuteRoutedEventArgs e)
        {
            if (TextBoxLogin.Text.Length > 0 && TextBoxPassword.Password.Length > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion LoginCommand
        private void TryLogin()
        {
            LoggedUser = Users.FirstOrDefault(u => string.Compare(u.Username, TextBoxLogin.Text) == 0 && string.Compare(u.Password, TextBoxPassword.Password) == 0);
            if (LoggedUser != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Incorrect username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxPassword.Password = "";
            }
        }
        private void LoginEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TryLogin();
            }
        }
    }


}
