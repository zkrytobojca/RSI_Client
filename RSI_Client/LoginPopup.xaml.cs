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
using RSI_Client.EventsService;
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
            try
            {
                var client = new EventsPortClient("EventsPortSoap11");
                loginRequest request = new loginRequest();
                request.username = TextBoxLogin.Text;
                request.password = TextBoxPassword.Password;
                loginResponse response = client.login(request);
                
                if(response.user == null)
                {
                    MessageBox.Show("Incorrect username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    TextBoxPassword.Password = "";
                }
                else
                {
                    LoggedUser = new User(response.user.username, response.user.password, response.user.admin);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
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
