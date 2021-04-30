using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
        
        public AdminWindow(ObservableCollection<Event> events, ObservableCollection<User> users)
        {
            AdminWindowVM = new AdminWindowVM(events, users);

            InitializeComponent();
            ComboBoxSearchType.ItemsSource = ApplicationConstants.userSearches;
            CommandBinding commandRemoveEventBinding = new CommandBinding(
            CommandDeleteEvent, ExecutedDeleteEvent, CanExecuteDeleteEvent);
            CommandBindings.Add(commandRemoveEventBinding);
            ButtonDeleteBook.Command = CommandDeleteEvent;
            CommandBinding commandDeleteUserBinding = new CommandBinding(
            CommandDeleteUser, ExecutedDeleteUser, CanExecuteDeleteUser);
            CommandBindings.Add(commandDeleteUserBinding);
            ButtonDeleteUser.Command = CommandDeleteUser;
        }
        #region EventButtons
        private void AddEventButtonClick(object sender, RoutedEventArgs e)
        {

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
            AdminWindowVM.Events.Remove(removed);
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
                FilterBook();
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
        private void FilterBook()
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
