using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
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
using RSI_Client.EventsService;
using RSI_Client.Model;
using RSI_Client.ViewModels;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;

namespace RSI_Client
{
    public partial class MainWindow : Window
    {
        static string KEYSTORE_PATH = "C:/Users/Piotrek/Desktop/Piotrek/Studia/8_Semestr/RSI/Projekt1/SOAP_ws/src/main/resources/myKeyStore.p12";
        static string PASSWORD = "123456";

        public Event SelectedEvent { get; set; }
        public MainWindowVM MainWindowVM { get; set; }
        public MainWindow()
        {
            MainWindowVM = new MainWindowVM();
            InitializeComponent();
            LoadObjects();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ComboBoxSearchType.ItemsSource = ApplicationConstants.eventSearches;
        }

        private void RegisterButtonClick(object sender, RoutedEventArgs e)
        {
            List<User> checkedUsers = MainWindowVM.Users.ToList();
            RegisterPopup registerPopup = new RegisterPopup(checkedUsers)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            registerPopup.ShowDialog();
            if ((bool)registerPopup.DialogResult)
            {
                MainWindowVM.Users.Add(registerPopup.RegisteredUser);
            }
            else
            if (registerPopup.OpenLogin)
            {
                LoginButtonClick(sender, e);
            }
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            List<User> checkedUsers = MainWindowVM.Users.ToList();
            LoginPopup loginPopup = new LoginPopup(checkedUsers)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            loginPopup.ShowDialog();
            if ((bool)loginPopup.DialogResult)
            {
                if (loginPopup.LoggedUser.IsAdmin)
                {
                    AdminWindow adminWindow = new AdminWindow(MainWindowVM.Events, MainWindowVM.Users, loginPopup.LoggedUser)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    Visibility = Visibility.Collapsed;
                    adminWindow.ShowDialog();
                    if ((bool)adminWindow.DialogResult)
                    {
                        Visibility = Visibility.Visible;
                        ListOfAvailableEvents.Items.Refresh();
                        ListOfAvailableEvents.SelectedIndex = 0;
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    MainWindowVM.LoggedUser = loginPopup.LoggedUser;
                    ListOfAvailableEvents.SelectedIndex = 0;
                    RefreshEvent(false);
                }
            }
            else
            if (loginPopup.OpenRegister)
            {
                RegisterButtonClick(sender, e);
            }
        }
        private void LoadObjects()
        {
            try
            {
                X509Certificate2 cert = new X509Certificate2(KEYSTORE_PATH, PASSWORD);
                ServicePointManager.ServerCertificateValidationCallback +=
                     (mender, certificate, chain, sslPolicyErrors) => true;

                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/all", Method.GET, RestSharp.DataFormat.Json);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var response = client.Get(request);

                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JArray response_json = JArray.Parse(response.Content);
                    MainWindowVM.Events.Clear();
                    foreach (JObject item in response_json)
                    {
                        type type;
                        Enum.TryParse<type>(item.GetValue("type").ToString(), out type);
                        Event ev = new Event(
                            item.GetValue("econst").ToString(),
                            item.GetValue("name").ToString(),
                            type,
                            DateTime.Parse(item.GetValue("date").ToString()),
                            Int32.Parse(item.GetValue("week").ToString()),
                            Int32.Parse(item.GetValue("month").ToString()),
                            Int32.Parse(item.GetValue("year").ToString()),
                            item.GetValue("description").ToString()
                            );
                        MainWindowVM.Events.Add(ev);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        private void SaveObjects()
        {
            
        }

        private void UserLogoutButtonClick(object sender, RoutedEventArgs e)
        {
            MainWindowVM.LoggedUser = null;
            ListOfAvailableEvents.SelectedIndex = 0;
        }

        private void EventAvailableSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfAvailableEvents.SelectedIndex >= 0 && ListOfAvailableEvents.SelectedIndex < MainWindowVM.Events.Count)
            {
                SelectedEvent = MainWindowVM.Events[ListOfAvailableEvents.SelectedIndex];
            }

            try
            {
                var client = new RestClient("https://localhost:8443");
                if (MainWindowVM.LoggedUser != null)
                {
                    client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                }
                else
                {
                    client.Authenticator = new HttpBasicAuthenticator("admin", "admin");
                }
                var request = new RestRequest("event/{eventId}/rating", Method.GET, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                var response = client.Get(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    double rating = double.Parse(response.Content, CultureInfo.InvariantCulture);
                    SetOverallStars(rating);
                }
                else
                {
                    SetOverallStars();
                }

                var request2 = new RestRequest("event/{eventId}/rating/{userId}", Method.GET, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id).AddUrlSegment("userId", MainWindowVM.LoggedUser.Id);
                var response2 = client.Get(request2);
                if (response2.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    double rating = double.Parse(response2.Content, CultureInfo.InvariantCulture);
                    SetUserStars(rating);
                }
                else
                {
                    SetUserStars();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        #region EventFilters
        private bool FilterNameEvent(Object item)
        {
            Event ev = (Event)item;
            return ev.Name.IndexOf(UserSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool FilterDay(Object item)
        {
            Event ev = (Event)item;
            DateTime searchedDate;
            if (!DateTime.TryParseExact(UserSearchTextBox.Text, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
            {
                return false;
            }
            if (DateTime.Compare(ev.Date, searchedDate) > 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        private bool FilterWeek(Object item)
        {
            Event ev = (Event)item;
            int week = 0;
            if (Int32.TryParse(UserSearchTextBox.Text, out week))
            {
                return ev.Week == week;
            }
            else return false;
        }
        #endregion EventFilters

        private void DoFilter()
        {
            //FilterAvailableEvents();
            FilterAvailableEventsViaService();
        }

        private void SearchEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DoFilter();
            }
        }

        private void FilterAvailableEvents()
        {
            switch (ComboBoxSearchType.SelectedIndex)
            {
                case 0:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = FilterNameEvent;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = FilterDay;
                        break;
                    }
                case 3:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = FilterWeek;
                        break;
                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = null;
                        break;
                    }

            }
        }

        private void FilterAvailableEventsViaService()
        {
            try
            {
                var client = new RestClient("https://localhost:8443");

                switch (ComboBoxSearchType.SelectedIndex)
                {
                    case 0:
                        {
                            var request = new RestRequest("event/all", Method.GET, RestSharp.DataFormat.Json);
                            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                            var response = client.Get(request);


                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                JArray response_json = JArray.Parse(response.Content);
                                MainWindowVM.Events.Clear();
                                foreach (JObject item in response_json)
                                {
                                    type type;
                                    Enum.TryParse<type>(item.GetValue("type").ToString(), out type);
                                    Event ev = new Event(
                                        item.GetValue("econst").ToString(),
                                        item.GetValue("name").ToString(),
                                        type,
                                        DateTime.Parse(item.GetValue("date").ToString()),
                                        Int32.Parse(item.GetValue("week").ToString()),
                                        Int32.Parse(item.GetValue("month").ToString()),
                                        Int32.Parse(item.GetValue("year").ToString()),
                                        item.GetValue("description").ToString()
                                        );
                                    MainWindowVM.Events.Add(ev);
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = FilterNameEvent;
                            break;
                        }
                    case 2:
                        {
                            DateTime searchedDate;
                            if (!DateTime.TryParseExact(UserSearchTextBox.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
                            {
                                break;
                            }

                            var request = new RestRequest("event", Method.GET, RestSharp.DataFormat.Json);
                            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                            string searchValue = searchedDate.Year.ToString() + "-" 
                                + (searchedDate.Month > 9 ? searchedDate.Month.ToString() : "0" + searchedDate.Month.ToString()) + "-" 
                                + (searchedDate.Day > 9 ? searchedDate.Day.ToString() : "0" + searchedDate.Day.ToString());
                            request.AddQueryParameter("date", searchValue);
                            var response = client.Get(request);


                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                JArray response_json = JArray.Parse(response.Content);
                                MainWindowVM.Events.Clear();
                                foreach (JObject item in response_json)
                                {
                                    type type;
                                    Enum.TryParse<type>(item.GetValue("type").ToString(), out type);
                                    Event ev = new Event(
                                        item.GetValue("econst").ToString(),
                                        item.GetValue("name").ToString(),
                                        type,
                                        DateTime.Parse(item.GetValue("date").ToString()),
                                        Int32.Parse(item.GetValue("week").ToString()),
                                        Int32.Parse(item.GetValue("month").ToString()),
                                        Int32.Parse(item.GetValue("year").ToString()),
                                        item.GetValue("description").ToString()
                                        );
                                    MainWindowVM.Events.Add(ev);
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            int week;
                            if (!Int32.TryParse(UserSearchTextBox.Text, out week))
                            {
                                break;
                            }
                            var request = new RestRequest("event", Method.GET, RestSharp.DataFormat.Json);
                            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                            request.AddQueryParameter("week", week.ToString());
                            var response = client.Get(request);


                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                JArray response_json = JArray.Parse(response.Content);
                                MainWindowVM.Events.Clear();
                                foreach (JObject item in response_json)
                                {
                                    type type;
                                    Enum.TryParse<type>(item.GetValue("type").ToString(), out type);
                                    Event ev = new Event(
                                        item.GetValue("econst").ToString(),
                                        item.GetValue("name").ToString(),
                                        type,
                                        DateTime.Parse(item.GetValue("date").ToString()),
                                        Int32.Parse(item.GetValue("week").ToString()),
                                        Int32.Parse(item.GetValue("month").ToString()),
                                        Int32.Parse(item.GetValue("year").ToString()),
                                        item.GetValue("description").ToString()
                                        );
                                    MainWindowVM.Events.Add(ev);
                                }
                            }
                            break;
                        }
                    default:
                        {
                            CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = null;
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            DoFilter();
        }

        private void SearchSelected(object sender, EventArgs e)
        {
            DoFilter();
        }

        private void ApplicationClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveObjects();
        }

        private void FilterReset()
        {
            CollectionViewSource.GetDefaultView(ListOfAvailableEvents.ItemsSource).Filter = null;
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshEvent(true);
        }

        private void RefreshEvent(bool withMessageBox)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/{eventId}", Method.GET, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);

                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var response = client.Get(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response_json = JObject.Parse(response.Content);
                    type type;
                    Enum.TryParse<type>(response_json.GetValue("type").ToString(), out type);
                    SelectedEvent.Id = response_json.GetValue("econst").ToString();
                    SelectedEvent.Name = response_json.GetValue("name").ToString();
                    SelectedEvent.Type = type;
                    SelectedEvent.Date = DateTime.Parse(response_json.GetValue("date").ToString());
                    SelectedEvent.Year = Int32.Parse(response_json.GetValue("year").ToString());
                    SelectedEvent.Month = Int32.Parse(response_json.GetValue("month").ToString());
                    SelectedEvent.Week = Int32.Parse(response_json.GetValue("week").ToString());
                    SelectedEvent.Description = response_json.GetValue("description").ToString();
                }

                var request2 = new RestRequest("event/{eventId}/rating", Method.GET, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                var response2 = client.Get(request2);
                if (response2.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    double rating = double.Parse(response2.Content, CultureInfo.InvariantCulture);
                    SetOverallStars(rating);
                }
                else
                {
                    SetOverallStars();
                }

                var request3 = new RestRequest("event/{eventId}/rating/{userId}", Method.GET, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id).AddUrlSegment("userId", MainWindowVM.LoggedUser.Id);
                var response3 = client.Get(request3);
                if (response3.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    double rating = double.Parse(response3.Content, CultureInfo.InvariantCulture);
                    SetUserStars(rating);
                }
                else
                {
                    SetUserStars();
                }
                if(withMessageBox) MessageBox.Show("Event updated", "Event info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void PdfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/to-pdf", Method.GET);
                request.AddHeader("Accept", "application/pdf");
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                var response = client.Get(request);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "LisOfEvents"; // Default file name
                saveFileDialog.DefaultExt = ".pdf"; // Default file extension
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllBytes(saveFileDialog.FileName, client.DownloadData(request));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void Rate1Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/{eventId}/rating", Method.POST, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                var newRating = new
                {
                    rating = 1
                };
                request.AddJsonBody(newRating);
                var response = client.Post(request);
                SetUserStars(1);
                MessageBox.Show("Thank you for your rating!", "Rating info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        private void Rate2Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/{eventId}/rating", Method.POST, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                var newRating = new
                {
                    rating = 2
                };
                request.AddJsonBody(newRating);
                var response = client.Post(request);
                SetUserStars(2);
                MessageBox.Show("Thank you for your rating!", "Rating info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        private void Rate3Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/{eventId}/rating", Method.POST, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                var newRating = new
                {
                    rating = 3
                };
                request.AddJsonBody(newRating);
                var response = client.Post(request);
                SetUserStars(3);
                MessageBox.Show("Thank you for your rating!", "Rating info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        private void Rate4Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/{eventId}/rating", Method.POST, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                var newRating = new
                {
                    rating = 4
                };
                request.AddJsonBody(newRating);
                var response = client.Post(request);
                SetUserStars(4);
                MessageBox.Show("Thank you for your rating!", "Rating info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        private void Rate5Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                var request = new RestRequest("event/{eventId}/rating", Method.POST, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                client.Authenticator = new HttpBasicAuthenticator(MainWindowVM.LoggedUser.Username, MainWindowVM.LoggedUser.Password);
                var newRating = new
                {
                    rating = 5
                };
                request.AddJsonBody(newRating);
                var response = client.Post(request);
                SetUserStars(5);
                MessageBox.Show("Thank you for your rating!", "Rating info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void SetOverallStars(double rating = -1)
        {
            BitmapImage empty = new BitmapImage();
            empty.BeginInit();
            empty.UriSource = new Uri("Icons/PNG/star.png", UriKind.Relative);
            empty.EndInit();

            BitmapImage half = new BitmapImage();
            half.BeginInit();
            half.UriSource = new Uri("Icons/PNG/star_half.png", UriKind.Relative);
            half.EndInit();

            BitmapImage filled = new BitmapImage();
            filled.BeginInit();
            filled.UriSource = new Uri("Icons/PNG/star_filled.png", UriKind.Relative);
            filled.EndInit();

            if (rating < 0.5) OverallStar1.Source = empty;
            else if(rating < 1) OverallStar1.Source = half;
            else OverallStar1.Source = filled;

            if (rating < 1.5) OverallStar2.Source = empty;
            else if (rating < 2) OverallStar2.Source = half;
            else OverallStar2.Source = filled;

            if (rating < 2.5) OverallStar3.Source = empty;
            else if (rating < 3) OverallStar3.Source = half;
            else OverallStar3.Source = filled;

            if (rating < 3.5) OverallStar4.Source = empty;
            else if (rating < 4) OverallStar4.Source = half;
            else OverallStar4.Source = filled;

            if (rating < 4.5) OverallStar5.Source = empty;
            else if (rating < 5) OverallStar5.Source = half;
            else OverallStar5.Source = filled;

            if (rating == -1) OverallRatingTextBlock.Text = "Unknown";
            else OverallRatingTextBlock.Text = rating.ToString();
        }

        private void SetUserStars(double rating = -1)
        {
            BitmapImage empty = new BitmapImage();
            empty.BeginInit();
            empty.UriSource = new Uri("Icons/PNG/star.png", UriKind.Relative);
            empty.EndInit();

            BitmapImage half = new BitmapImage();
            half.BeginInit();
            half.UriSource = new Uri("Icons/PNG/star_half.png", UriKind.Relative);
            half.EndInit();

            BitmapImage filled = new BitmapImage();
            filled.BeginInit();
            filled.UriSource = new Uri("Icons/PNG/star_filled.png", UriKind.Relative);
            filled.EndInit();

            if (rating < 0.5) UserStar1.Source = empty;
            else if (rating < 1) UserStar1.Source = half;
            else UserStar1.Source = filled;

            if (rating < 1.5) UserStar2.Source = empty;
            else if (rating < 2) UserStar2.Source = half;
            else UserStar2.Source = filled;

            if (rating < 2.5) UserStar3.Source = empty;
            else if (rating < 3) UserStar3.Source = half;
            else UserStar3.Source = filled;

            if (rating < 3.5) UserStar4.Source = empty;
            else if (rating < 4) UserStar4.Source = half;
            else UserStar4.Source = filled;

            if (rating < 4.5) UserStar5.Source = empty;
            else if (rating < 5) UserStar5.Source = half;
            else UserStar5.Source = filled;

            if(rating == -1) UserRatingTextBlock.Text = "Unknown";
            else UserRatingTextBlock.Text = rating.ToString();
        }
    }
}
