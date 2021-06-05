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
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization.Json;
using RSI_Client.EventsService;
using RSI_Client.Model;
using Newtonsoft.Json.Linq;


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
                var client = new RestClient("https://localhost:8443");
                client.Authenticator = new HttpBasicAuthenticator(TextBoxLogin.Text, TextBoxPassword.Password);
                var request = new RestRequest("user/login", Method.GET, RestSharp.DataFormat.Json);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var response = client.Get(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show("Incorrect username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    TextBoxPassword.Password = "";
                }
                else
                {
                    var response_json = JObject.Parse(response.Content);
                    LoggedUser = new User(response_json.GetValue("uconst").ToString(), TextBoxLogin.Text, TextBoxPassword.Password, Boolean.Parse(response_json.GetValue("admin").ToString()));
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
