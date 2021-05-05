using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace RSI_Client
{
    public partial class MainWindow : Window
    {
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
                    AdminWindow adminWindow = new AdminWindow(MainWindowVM.Events, MainWindowVM.Users)
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
                var client = new EventsPortClient("EventsPortSoap11");
                getAllEventsRequest request = new getAllEventsRequest();
                @event[] events = client.getAllEvents(request);

                foreach (@event ev in events)
                {
                    MainWindowVM.Events.Add(new Event(ev));
                }

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            MainWindowVM.Users.Add(new User("admin", "admin", true));
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
            if (ListOfAvailableEvents.SelectedIndex >= 0 && ListOfAvailableEvents.SelectedIndex < MainWindowVM.Users[0].SeenEvents.Count)
            {
                SelectedEvent = MainWindowVM.Users[0].SeenEvents[ListOfAvailableEvents.SelectedIndex];
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
            if (!DateTime.TryParseExact(UserSearchTextBox.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
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
            FilterAvailableEvents();
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
    }
}
