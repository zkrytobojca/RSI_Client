using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
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
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RSI_Client.EventsService;
using RSI_Client.Model;
using RSI_Client.ViewModels;

namespace RSI_Client
{
    /// <summary>
    /// Logika interakcji dla klasy AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public Event SelectedEvent { get; set; }
        public User SelectedUser { get; set; }
        public AdminWindowVM AdminWindowVM { get; set; }
        
        public AdminWindow(ObservableCollection<Event> events, ObservableCollection<User> users, User loggedUser)
        {
            AdminWindowVM = new AdminWindowVM(events, users);
            AdminWindowVM.LoggedUser = loggedUser;

            InitializeComponent();
            ComboBoxSearchType.ItemsSource = ApplicationConstants.userSearches;
            CommandBinding commandRemoveEventBinding = new CommandBinding(
            CommandDeleteEvent, ExecutedDeleteEvent, CanExecuteDeleteEvent);
            CommandBindings.Add(commandRemoveEventBinding);
            ButtonDeleteEvent.Command = CommandDeleteEvent;
            CommandBinding commandDeleteUserBinding = new CommandBinding(
            CommandDeleteUser, ExecutedDeleteUser, CanExecuteDeleteUser);
            CommandBindings.Add(commandDeleteUserBinding);
            ButtonDeleteUser.Command = CommandDeleteUser;
        }
        #region EventButtons
        private void AddEventButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                client.Authenticator = new HttpBasicAuthenticator(AdminWindowVM.LoggedUser.Username, AdminWindowVM.LoggedUser.Password);
                var request = new RestRequest("event", Method.POST, RestSharp.DataFormat.Json);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

                int week = 0;
                Int32.TryParse(TextBoxWeek.Text, out week);
                int month = 0;
                Int32.TryParse(TextBoxMonth.Text, out month);
                int year = 0;
                Int32.TryParse(TextBoxYear.Text, out year);
                var newEvent = new
                {
                    name = TextBoxName.Text,
                    type = TextBoxType.Text,
                    date = DatePickerReleaseDate.SelectedDate.Value,
                    week = week,
                    month = month,
                    year = year,
                    description = TextBoxDescription.Text
                };
                request.AddJsonBody(newEvent);

                var response = client.Post(request);

                request = new RestRequest("event/all", Method.GET, RestSharp.DataFormat.Json);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                response = client.Get(request);


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JArray response_json = JArray.Parse(response.Content);
                    AdminWindowVM.Events.Clear();
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
                        AdminWindowVM.Events.Add(ev);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void ModifyEventButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                client.Authenticator = new HttpBasicAuthenticator(AdminWindowVM.LoggedUser.Username, AdminWindowVM.LoggedUser.Password);
                var request = new RestRequest("event/{eventId}", Method.PUT, RestSharp.DataFormat.Json).AddUrlSegment("eventId", SelectedEvent.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

                int week = 0;
                Int32.TryParse(TextBoxWeek.Text, out week);
                int month = 0;
                Int32.TryParse(TextBoxMonth.Text, out month);
                int year = 0;
                Int32.TryParse(TextBoxYear.Text, out year);
                var newEvent = new
                {
                    name = TextBoxName.Text,
                    type = TextBoxType.Text,
                    date = DatePickerReleaseDate.SelectedDate.Value,
                    week = week,
                    month = month,
                    year = year,
                    description = TextBoxDescription.Text
                };
                request.AddJsonBody(newEvent);

                var response = client.Put(request);

                request = new RestRequest("event/all", Method.GET, RestSharp.DataFormat.Json);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                response = client.Get(request);


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JArray response_json = JArray.Parse(response.Content);
                    AdminWindowVM.Events.Clear();
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
                        AdminWindowVM.Events.Add(ev);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        #endregion EventButtons

        private void EventSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxAvailableEvents.SelectedIndex >= 0 && ListBoxAvailableEvents.SelectedIndex < AdminWindowVM.Events.Count)
            {
                SelectedEvent = AdminWindowVM.Events[ListBoxAvailableEvents.SelectedIndex];
            }
        }

        #region CommandRemoveEvent
        public static RoutedCommand CommandDeleteEvent = new RoutedCommand();
        private void ExecutedDeleteEvent(object sender, ExecutedRoutedEventArgs e)
        {
            int tempIndex = ListBoxAvailableEvents.SelectedIndex;
            Event removed = AdminWindowVM.Events[tempIndex];

            try
            {
                var client = new RestClient("https://localhost:8443");
                client.Authenticator = new HttpBasicAuthenticator(AdminWindowVM.LoggedUser.Username, AdminWindowVM.LoggedUser.Password);
                var request = new RestRequest("event/{eventId}", Method.DELETE, RestSharp.DataFormat.Json).AddUrlSegment("eventId", removed.Id);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var response = client.Delete(request);

                request = new RestRequest("event/all", Method.GET, RestSharp.DataFormat.Json);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                response = client.Get(request);


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JArray response_json = JArray.Parse(response.Content);
                    AdminWindowVM.Events.Clear();
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
                        AdminWindowVM.Events.Add(ev);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            if (AdminWindowVM.Events.Count > 0)
            {
                ListBoxAvailableEvents.SelectedIndex = tempIndex - 1;
            }
        }
        private void CanExecuteDeleteEvent(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableEvents.SelectedIndex >= 0 && ListBoxAvailableEvents.SelectedIndex < AdminWindowVM.Events.Count)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion CommandRemoveEvent

        private void AddUserButtonClick(object sender, RoutedEventArgs e)
        {
            AdminWindowVM.Users.Add(new User("new"));
            ListBoxAvailableUsers.SelectedIndex = AdminWindowVM.Users.Count - 1;
            TextBoxAdminUsername.SelectAll();
            TextBoxAdminUsername.Focus();
        }

        private void UserSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxAvailableUsers.SelectedIndex >= 0 && ListBoxAvailableUsers.SelectedIndex < AdminWindowVM.Users.Count)
            {
                SelectedUser = AdminWindowVM.Users[ListBoxAvailableUsers.SelectedIndex];
                ListBoxAvailableEvents.ItemsSource = SelectedUser.SeenEvents;

            }
            else
            {
                ListBoxAvailableEvents.ItemsSource = null;
            }
        }
        #region CommandRemoveUser
        public static RoutedCommand CommandDeleteUser = new RoutedCommand();
        private void ExecutedDeleteUser(object sender, ExecutedRoutedEventArgs e)
        {
            int tempIndex = ListBoxAvailableUsers.SelectedIndex;
            User removed = AdminWindowVM.Users[tempIndex];
            AdminWindowVM.Users.Remove(removed);
            if (AdminWindowVM.Users.Count <= 1)
            {
                ListBoxAvailableEvents.ItemsSource = null;
            }
            else
            {
                ListBoxAvailableUsers.SelectedIndex = tempIndex - 1;
            }

        }
        private void CanExecuteDeleteUser(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableUsers.SelectedIndex >= 1 && ListBoxAvailableUsers.SelectedIndex < AdminWindowVM.Users.Count+1)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion CommandRemoveUser

        

        private void FilterReset()
        {
            CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = null;
            CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = null;
        }
        private void AdminTabControlChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl&&IsActive)
            {
                if (TabItemEvents.IsSelected)
                {
                    ComboBoxSearchType.ItemsSource = ApplicationConstants.eventSearches;
                }
                else if(TabItemUsers.IsSelected)
                {
                    ComboBoxSearchType.ItemsSource = ApplicationConstants.userSearches;
                }
                ComboBoxSearchType.SelectedIndex = 0;
                FilterReset();

            }
            
        }
        #region EventFilters
        private bool FilterNameEvent(Object item)
        {
            Event ev = (Event)item;
            return ev.Name.IndexOf(TextBoxAdminSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool FilterDay(Object item)
        {
            Event ev = (Event)item;
            DateTime searchedDate;
            if (!DateTime.TryParseExact(TextBoxAdminSearch.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
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
            if (Int32.TryParse(TextBoxAdminSearch.Text, out week))
            {
                return ev.Week == week;
            }
            else return false;
        }
        #endregion EventFilters
        #region UserFilters
        private bool FilterUserName(Object item)
        {
            User user = (User)item;
            if(user==AdminWindowVM.Users[0])
            {
                return true;
            }
            return user.Username.IndexOf(TextBoxAdminSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool FilterUserAdmin(Object item)
        {
            User user = (User)item;
            if (user == AdminWindowVM.Users[0])
            {
                return true;
            }
            return user.IsAdmin;
        }
        #endregion UserFilters
        #region FilterMethods
        private void SearchSelected(object sender, EventArgs e)
        {
            DoFilter();
        }
        private void DoFilter()
        {
            if(TabItemUsers.IsSelected)
            {
                FilterUser();
            }
            else if(TabItemEvents.IsSelected)
            {
                //FilterEvent();
                FilterEventViaService();
            }
            
        }

        private void SearchEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DoFilter();
            }
        }
        private void FilterUser()
        {
            switch (ComboBoxSearchType.SelectedIndex)
            {
                case 0:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = FilterUserName;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = FilterUserAdmin;
                        break;
                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = null;
                        break;
                    }
            }

        }
        private void FilterEvent()
        {
            switch (ComboBoxSearchType.SelectedIndex)
            {
                case 0:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = FilterNameEvent;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = FilterDay;
                        break;
                    }
                case 3:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = FilterWeek;
                        break;
                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = null;
                        break;
                    }
            }
        }

        private void FilterEventViaService()
        {
            try
            {
                var client = new RestClient("https://localhost:8443");
                client.Authenticator = new HttpBasicAuthenticator(AdminWindowVM.LoggedUser.Username, AdminWindowVM.LoggedUser.Password);

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
                                AdminWindowVM.Events.Clear();
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
                                    AdminWindowVM.Events.Add(ev);
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = FilterNameEvent;
                            break;
                        }
                    case 2:
                        {
                            DateTime searchedDate;
                            if (!DateTime.TryParseExact(TextBoxAdminSearch.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
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
                                AdminWindowVM.Events.Clear();
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
                                    AdminWindowVM.Events.Add(ev);
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            int week;
                            if (!Int32.TryParse(TextBoxAdminSearch.Text, out week))
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
                                AdminWindowVM.Events.Clear();
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
                                    AdminWindowVM.Events.Add(ev);
                                }
                            }
                            break;
                        }
                    default:
                        {
                            CollectionViewSource.GetDefaultView(ListBoxAvailableEvents.ItemsSource).Filter = null;
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        #endregion FilterMethods
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            DoFilter();
        }

        private void AdminLogout(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
