using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using RSI_Client.Model;
using RSI_Client.ViewModels;

namespace RSI_Client
{
    public partial class MainWindow : Window
    {
        public Book SelectedBook { get; set; }
        public MainWindowVM MainWindowVM { get; set; }
        public MainWindow()
        {
            MainWindowVM = new MainWindowVM();
            InitializeComponent();
            LoadObjects();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CommandBinding commandBorrowBookBinding = new CommandBinding(
                CommandBorrowBook, ExecutedBorrowBook, CanExecuteBorrowBook);
            CommandBindings.Add(commandBorrowBookBinding);
            BorrowButton.Command = CommandBorrowBook;

            CommandBinding commandReturnBookBinding = new CommandBinding(
                CommandReturnBook, ExecutedReturnBook, CanExecuteReturnBook);
            CommandBindings.Add(commandReturnBookBinding);
            ReturnButton.Command = CommandReturnBook;
            ComboBoxSearchType.ItemsSource = ApplicationConstants.bookSearches;
        }

        private void RegisterButtonClick(object sender, RoutedEventArgs e)
        {
            List<User> checkedUsers = MainWindowVM.Users.ToList();
            checkedUsers.RemoveAt(0);
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
            // ---
            CountriesService.CountriesPortClient client = new CountriesService.CountriesPortClient("CountriesPortSoap11");
            CountriesService.getCountryRequest request = new CountriesService.getCountryRequest();
            request.name = "Poland";
            CountriesService.getCountryResponse country = client.getCountry(request);
            ButtonUserLogin.Content = country.country.capital;
            // ---


            List<User> checkedUsers = MainWindowVM.Users.ToList();
            checkedUsers.RemoveAt(0);
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
                    AdminWindow adminWindow = new AdminWindow(MainWindowVM.Authors, MainWindowVM.Books, MainWindowVM.Users)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    Visibility = Visibility.Collapsed;
                    adminWindow.ShowDialog();
                    if ((bool)adminWindow.DialogResult)
                    {
                        Visibility = Visibility.Visible;
                        ListOfAvailableBooks.Items.Refresh();
                        ListOfAvailableBooks.SelectedIndex = 0;

                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    MainWindowVM.LoggedUser = loginPopup.LoggedUser;
                    ListOfBorrowedBooks.ItemsSource = MainWindowVM.LoggedUser.RentedBooks;
                    ListOfAvailableBooks.SelectedIndex = 0;
                }
            }
            else
            if (loginPopup.OpenRegister)
            {
                RegisterButtonClick(sender, e);
            }
        }
        private void TESTOKNA(object sender, RoutedEventArgs e)
        {

            AdminWindow adminWindow = new AdminWindow(MainWindowVM.Authors, MainWindowVM.Books, MainWindowVM.Users)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            Visibility = Visibility.Collapsed;
            adminWindow.ShowDialog();
            if ((bool)adminWindow.DialogResult)
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                Close();
            }

        }


        private void LoadObjects()
        {
            FileStream fs = new FileStream("Data.dat", FileMode.OpenOrCreate);
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                MainWindowVM.Authors = (ObservableCollection<Author>)binaryFormatter.Deserialize(fs);
                MainWindowVM.Books = (ObservableCollection<Book>)binaryFormatter.Deserialize(fs);
                MainWindowVM.Users = (ObservableCollection<User>)binaryFormatter.Deserialize(fs);
                for (int i = 0; i < MainWindowVM.Authors.Count; i++)
                {
                    MainWindowVM.Authors[i].WrittenBooks = new List<Book>();
                }

                for (int i = 0; i < MainWindowVM.Books.Count; i++)
                {
                    MainWindowVM.Books[i].AuthorList = new List<Author>();
                    foreach (int authorId in MainWindowVM.Books[i].AuthorIds)
                    {
                        Author author = MainWindowVM.Authors.First(a => a.Id == authorId);
                        MainWindowVM.Books[i].AuthorList.Add(author);
                        author.WrittenBooks.Add(MainWindowVM.Books[i]);
                    }
                }
                for (int i = 0; i < MainWindowVM.Users.Count; i++)
                {
                    MainWindowVM.Users[i].RentedBooks = new List<Book>();
                    foreach (int bookId in MainWindowVM.Users[i].BookIds)
                    {
                        Book book = MainWindowVM.Books.First(b => b.Id == bookId);
                        MainWindowVM.Users[i].RentedBooks.Add(book);
                        book.CurrentOwner = MainWindowVM.Users[i];
                    }
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                MainWindowVM.Authors = new ObservableCollection<Author>();
                MainWindowVM.Books = new ObservableCollection<Book>();
                MainWindowVM.Users = new ObservableCollection<User>
                {
                    new User("(brak)"),new User
                {
                    Username="admin",
                    IsAdmin = true,
                    Password = "admin"
                }
            };

                //throw;
            }
            finally
            {
                fs.Close();

            }

        }
        private void SaveObjects()
        {
            for (int i = 0; i < MainWindowVM.Authors.Count; i++)
            {
                MainWindowVM.Authors[i].Id = i;
            }
            for (int i = 0; i < MainWindowVM.Books.Count; i++)
            {
                MainWindowVM.Books[i].Id = i;
                MainWindowVM.Books[i].AuthorIds = new List<int>();
                foreach (Author a in MainWindowVM.Books[i].AuthorList)
                {
                    MainWindowVM.Books[i].AuthorIds.Add(a.Id);
                }
            }
            for (int i = 0; i < MainWindowVM.Users.Count; i++)
            {
                MainWindowVM.Users[i].Id = i;
                MainWindowVM.Users[i].BookIds = new List<int>();
                foreach (Book b in MainWindowVM.Users[i].RentedBooks)
                {
                    MainWindowVM.Users[i].BookIds.Add(b.Id);
                }
            }
            FileStream fs = new FileStream("Data.dat", FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            try
            {
                binaryFormatter.Serialize(fs, MainWindowVM.Authors);
                binaryFormatter.Serialize(fs, MainWindowVM.Books);
                binaryFormatter.Serialize(fs, MainWindowVM.Users);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                //throw;
            }
            finally
            {
                fs.Close();
            }

        }
        public static RoutedCommand CommandBorrowBook = new RoutedCommand();
        private void ExecutedBorrowBook(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBook != null)
            {
                if (SelectedBook.CurrentOwner != null)
                {
                    SelectedBook.CurrentOwner.RentedBooks.Remove(SelectedBook);
                }
                SelectedBook.CurrentOwner = MainWindowVM.LoggedUser;
                if (SelectedBook.CurrentOwner != null)
                {
                    SelectedBook.CurrentOwner.RentedBooks.Add(SelectedBook);
                    ListOfAvailableBooks.Items.Refresh();
                }

            }

        }
        private void CanExecuteBorrowBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListOfAvailableBooks.SelectedIndex < 0 || ListOfAvailableBooks.SelectedIndex >= MainWindowVM.Users[0].RentedBooks.Count)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        public static RoutedCommand CommandReturnBook = new RoutedCommand();
        private void ExecutedReturnBook(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBook != null)
            {
                if (SelectedBook.CurrentOwner != null)
                {
                    SelectedBook.CurrentOwner.RentedBooks.Remove(SelectedBook);
                }
                SelectedBook.CurrentOwner = MainWindowVM.Users[0];
                if (SelectedBook.CurrentOwner != null)
                {
                    SelectedBook.CurrentOwner.RentedBooks.Add(SelectedBook);
                    ListOfAvailableBooks.Items.Refresh();
                    ListOfBorrowedBooks.Items.Refresh();
                }

            }

        }
        private void CanExecuteReturnBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (MainWindowVM.LoggedUser != null)
            {
                if (ListOfBorrowedBooks.SelectedIndex < 0 || ListOfBorrowedBooks.SelectedIndex >= MainWindowVM.LoggedUser.RentedBooks.Count)
                {
                    e.CanExecute = false;
                }
                else
                {
                    e.CanExecute = true;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void UserLogoutButtonClick(object sender, RoutedEventArgs e)
        {
            MainWindowVM.LoggedUser = null;
            TabControlBooks.SelectedItem = TabItemAvailableBooks;
            ListOfAvailableBooks.SelectedIndex = 0;
        }

        private void BookAvailableSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfAvailableBooks.SelectedIndex >= 0 && ListOfAvailableBooks.SelectedIndex < MainWindowVM.Users[0].RentedBooks.Count)
            {
                SelectedBook = MainWindowVM.Users[0].RentedBooks[ListOfAvailableBooks.SelectedIndex];
            }
        }

        private void BookBorrowedSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfBorrowedBooks.SelectedIndex >= 0 && ListOfBorrowedBooks.SelectedIndex < MainWindowVM.LoggedUser.RentedBooks.Count)
            {
                SelectedBook = MainWindowVM.LoggedUser.RentedBooks[ListOfBorrowedBooks.SelectedIndex];
            }
        }

        #region BookFilters
        private bool FilterBookTitle(Object item)
        {
            Book book = (Book)item;
            return book.Title.IndexOf(UserSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool FilterDateOlder(Object item)
        {
            Book book = (Book)item;
            DateTime searchedDate;
            if (!DateTime.TryParseExact(UserSearchTextBox.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
            {
                return false;
            }
            if (DateTime.Compare(book.ReleaseDate, searchedDate) > 0)
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
            if (!DateTime.TryParseExact(UserSearchTextBox.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchedDate))
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
            foreach (Author a in book.AuthorList)
            {
                if (a.FullName.IndexOf(UserSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

            }
            return false;
        }
        #endregion BookFilters

        private void DoFilter()
        {
            if (MainWindowVM.LoggedUser != null)
            {
                FilterBorrowedBooks();
            }
            FilterAvailableBooks();
        }

        private void SearchEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DoFilter();
            }
        }

        private void FilterAvailableBooks()
        {
            switch (ComboBoxSearchType.SelectedIndex)
            {
                case 0:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = FilterBookTitle;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = FilterDateOlder;
                        break;
                    }
                case 3:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = FilterDateNewer;
                        break;
                    }
                case 4:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = FilterBookAuthorName;
                        break;
                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = null;
                        break;
                    }

            }
        }

        private void FilterBorrowedBooks()
        {
            switch (ComboBoxSearchType.SelectedIndex)
            {
                case 0:
                    {
                        CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = FilterBookTitle;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = FilterDateOlder;
                        break;
                    }
                case 3:
                    {
                        CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = FilterDateNewer;
                        break;
                    }
                case 4:
                    {
                        CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = FilterBookAuthorName;
                        break;
                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = null;
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
            CollectionViewSource.GetDefaultView(ListOfAvailableBooks.ItemsSource).Filter = null;
            CollectionViewSource.GetDefaultView(ListOfBorrowedBooks.ItemsSource).Filter = null;
        }

        private void TabControlBooksChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl && IsActive)
            {
                ComboBoxSearchType.ItemsSource = ApplicationConstants.bookSearches;
                ComboBoxSearchType.SelectedIndex = 0;
                FilterReset();
            }
        }
    }
}
