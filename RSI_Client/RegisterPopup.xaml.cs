using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using RestSharp;
using RestSharp.Authenticators;

namespace RSI_Client
{
    /// <summary>
    /// Logika interakcji dla klasy RegisterPopup.xaml
    /// </summary>
    public partial class RegisterPopup : Window
    {
        public User RegisteredUser { get; set; }
        public bool OpenLogin { get; set; }
        public List<User> Users { get; set; }
        public string ValidatedLogin { get; set; }
        public string ValidatedPassword { get; set; }

        public RegisterPopup(List<User> users)
        {
            InitializeComponent();
            OpenLogin = false;
            Users = users;

            CommandBinding commandRegisterBinding = new CommandBinding(
            CommandRegister, ExecutedRegister, CanExecuteRegister);
            CommandBindings.Add(commandRegisterBinding);
            ButtonRegister.Command = CommandRegister;
        }

        private void LabelLoginClick(object sender, MouseButtonEventArgs e)
        {
            OpenLogin = true;
            Close();
        }

        private void TextBoxLoginPreview(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPasswordPreview(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z0-9!#$%&_^]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxDisableSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void ClearPassword()
        {
            TextBoxRegisterPassword.Text = "";
            TextBoxRepeatPassword.Password = "";
        }
        #region RegisterCommand
        public static RoutedCommand CommandRegister = new RoutedCommand();

        private void ExecutedRegister(object sender, ExecutedRoutedEventArgs e)
        {

            TryRegister();

        }
        private void CanExecuteRegister(object sender, CanExecuteRoutedEventArgs e)
        {
            if (TextBoxRegisterLogin.Text.Length > 3 && TextBoxRegisterPassword.Text.Length > 3 && TextBoxRepeatPassword.Password.Length > 3)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion RegisterCommand
        private void TryRegister()
        {
            if (TextBoxRegisterPassword.Text != TextBoxRepeatPassword.Password)
            {
                MessageBox.Show("Passwords do not match", "Password error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ClearPassword();
            }
            else
            {
                try
                {
                    var client = new RestClient("https://localhost:8443");
                    var request = new RestRequest("user/register", Method.POST, RestSharp.DataFormat.Json);
                    request.RequestFormat = RestSharp.DataFormat.Json;
                    var param = new { username = TextBoxRegisterLogin.Text, password = TextBoxRegisterPassword.Text };
                    request.AddJsonBody(param);
                    var response = client.Post(request);

                    if (response.ResponseStatus == ResponseStatus.Error)
                    {
                        MessageBox.Show("User with that username already exists", "Username taken", MessageBoxButton.OK, MessageBoxImage.Error);
                        ClearPassword();
                    }
                    else if(response.ResponseStatus == ResponseStatus.Completed)
                    {
                        RegisteredUser = new User
                        {
                            Username = TextBoxRegisterLogin.Text,
                            IsAdmin = false,
                            Password = TextBoxRegisterPassword.Text
                        };
                        DialogResult = true;
                        MessageBox.Show("Account successfully created", "Registration success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }
        }
        private void RegisterEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if(ButtonRegister.IsEnabled)
                {
                    TryRegister();
                }
            }
        }
    }

}
