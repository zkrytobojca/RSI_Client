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
            CommandBinding commandRemoveBookBinding = new CommandBinding(
            CommandDeleteBook, ExecutedDeleteBook, CanExecuteDeleteEvent);
            CommandBindings.Add(commandRemoveBookBinding);
            ButtonDeleteBook.Command = CommandDeleteBook;
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
            if (ListBoxAvailableBooks.SelectedIndex >= 0 && ListBoxAvailableBooks.SelectedIndex < AdminWindowVM.Events.Count)
            {
                SelectedEvent = AdminWindowVM.Events[ListBoxAvailableBooks.SelectedIndex];
            }
            else
            {
                ListBoxBooksAuthors.ItemsSource = null;
            }
        }

        #region CommandRemoveEvent
        public static RoutedCommand CommandDeleteBook = new RoutedCommand();
        private void ExecutedDeleteBook(object sender, ExecutedRoutedEventArgs e)
        {
            int tempIndex = ListBoxAvailableBooks.SelectedIndex;
            Event removed = AdminWindowVM.Events[tempIndex];
            AdminWindowVM.Events.Remove(removed);
            if (AdminWindowVM.Events.Count <= 0)
            {
                ListBoxBooksAuthors.ItemsSource = null;
            }
            else
            {
                ListBoxAvailableBooks.SelectedIndex = tempIndex - 1;
            }

        }
        private void CanExecuteDeleteEvent(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableBooks.SelectedIndex >= 0 && ListBoxAvailableBooks.SelectedIndex < AdminWindowVM.Events.Count)
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
                ListBoxUsersEvents.ItemsSource = SelectedUser.SeenEvents;

            }
            else
            {
                ListBoxUsersEvents.ItemsSource = null;
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
                ListBoxUsersEvents.ItemsSource = null;
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
            CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = null;
        }
        private void AdminTabControlChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl&&IsActive)
            {
               
                if (TabItemBooks.IsSelected)
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
        #region BookFilters
        private bool FilterBookTitle(Object item)
        {
            Book book = (Book)item;
            return book.Title.IndexOf(TextBoxAdminSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool FilterDateOlder(Object item)
        {
            Book book = (Book)item;
            DateTime searchedDate;
            if (!DateTime.TryParseExact(TextBoxAdminSearch.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
            {
                return false;
            }
            if (DateTime.Compare(book.ReleaseDate,searchedDate) > 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        private bool FilterDateNewer(Object item)
        {
            Book book = (Book)item;
            DateTime searchedDate;
            if(!DateTime.TryParseExact(TextBoxAdminSearch.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture,DateTimeStyles.None, out searchedDate))
            {
                return false;
            }
            if (DateTime.Compare(book.ReleaseDate, searchedDate) < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool FilterBookAuthorName(Object item)
        {
            Book book = (Book)item;
            foreach(Author a in book.AuthorList)
            {
                if(a.FullName.IndexOf(TextBoxAdminSearch.Text,StringComparison.OrdinalIgnoreCase)>=0)
                {
                    return true;
                }

            }
            return false;
        }
        #endregion BookFilters
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
        #region AuthorFilters
        private bool FilterAuthorName(Object item)
        {
            Author author = (Author)item;
            return author.FullName.IndexOf(TextBoxAdminSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool FilterAuthorBookTitle(Object item)
        {
            Author author = (Author)item;
            foreach (Book b in author.WrittenBooks)
            {
                if (b.Title.IndexOf(TextBoxAdminSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

            }
            return false;
        }

        #endregion AuthorFilters
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
            else if(TabItemBooks.IsSelected)
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
                        CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = FilterBookTitle;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = FilterDateOlder;
                        break;
                    }
                case 3:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = FilterDateNewer;
                        break;
                    }
                case 4:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = FilterBookAuthorName;
                        break;
                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = null;
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

        private void ButtonPrintBooksClick(object sender, RoutedEventArgs e)
        {
            if(SelectedUser!=null)
            {
                PrintDialog printDlg = new PrintDialog();
                printDlg.PrintVisual(ListBoxUsersEvents, SelectedUser.Username + " events" );
            }
            
        }
    }
}
