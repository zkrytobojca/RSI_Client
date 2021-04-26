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
        public Book SelectedBook { get; set; }
        public Author SelectedAuthor { get; set; }
        public User SelectedUser { get; set; }
        public AdminWindowVM AdminWindowVM { get; set; }
        
        public AdminWindow(ObservableCollection<Author> authors, ObservableCollection<Book> books, ObservableCollection<User> users)
        {
            AdminWindowVM = new AdminWindowVM(authors,books,users);

            InitializeComponent();
            ComboBoxSearchType.ItemsSource = ApplicationConstants.userSearches;
            ComboBoxBookOwner.ItemsSource = AdminWindowVM.Users;
            CommandBinding commandBookAuthorAddBinding = new CommandBinding(
            CommandAddBookAuthor, ExecutedAddBookAuthor, CanExecuteAddBookAuthor);
            CommandBindings.Add(commandBookAuthorAddBinding);
            ButtonBookAddAuthor.Command = CommandAddBookAuthor;
            CommandBinding commandRemoveBookAuthorBinding = new CommandBinding(
            CommandRemoveBookAuthor, ExecutedRemoveBookAuthor, CanExecuteRemoveBookAuthor);
            CommandBindings.Add(commandRemoveBookAuthorBinding);
            ButtonRemoveBookAuthor.Command = CommandRemoveBookAuthor;
            CommandBinding commandRemoveBookBinding = new CommandBinding(
            CommandDeleteBook, ExecutedDeleteBook, CanExecuteDeleteBook);
            CommandBindings.Add(commandRemoveBookBinding);
            ButtonDeleteBook.Command = CommandDeleteBook;
            CommandBinding commandDeleteAuthorBinding = new CommandBinding(
            CommandDeleteAuthor, ExecutedDeleteAuthor, CanExecuteDeleteAuthor);
            CommandBindings.Add(commandDeleteAuthorBinding);
            ButtonDeleteAuthor.Command = CommandDeleteAuthor;
            CommandBinding commandAddAuthorBookBinding = new CommandBinding(
            CommandAddAuthorBook, ExecutedAddAuthorBook, CanExecuteAddAuthorBook);
            CommandBindings.Add(commandAddAuthorBookBinding);
            ButtonAuthorAddBook.Command = CommandAddAuthorBook;
            CommandBinding commandRemoveAuthorBookBinding = new CommandBinding(
            CommandRemoveAuthorBook, ExecutedRemoveAuthorBook, CanExecuteRemoveAuthorBook);
            CommandBindings.Add(commandRemoveAuthorBookBinding);
            ButtonAuthorRemoveBook.Command = CommandRemoveAuthorBook;
            CommandBinding commandDeleteUserBinding = new CommandBinding(
            CommandDeleteUser, ExecutedDeleteUser, CanExecuteDeleteUser);
            CommandBindings.Add(commandDeleteUserBinding);
            ButtonDeleteUser.Command = CommandDeleteUser;
            CommandBinding commandAddUserBookBinding = new CommandBinding(
            CommandAddUserBook, ExecutedAddUserBook, CanExecuteAddUserBook);
            CommandBindings.Add(commandAddUserBookBinding);
            ButtonAddUserBook.Command = CommandAddUserBook;
            CommandBinding commandRemoveUsersBookBinding = new CommandBinding(
            CommandRemoveUsersBook, ExecutedRemoveUsersBook, CanExecuteRemoveUsersBook);
            CommandBindings.Add(commandRemoveUsersBookBinding);
            ButtonUserRemoveBook.Command = CommandRemoveUsersBook;


        }
        #region BookButtons
        private void AddBookButtonClick(object sender, RoutedEventArgs e)
        {
            Book addedBook = new Book("new")
            {
                CurrentOwner = AdminWindowVM.Users[0]
            };
            AdminWindowVM.Users[0].RentedBooks.Add(addedBook);
            AdminWindowVM.Books.Add(addedBook);
            ListBoxAvailableBooks.SelectedIndex = AdminWindowVM.Books.Count - 1;
            TextBoxTitle.SelectAll();
            TextBoxTitle.Focus();

        }




        private void ButtonImagePickerClick(object sender, RoutedEventArgs e)
        {
            if(SelectedBook!=null)
            {
                Microsoft.Win32.OpenFileDialog FilePickerDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files (*.jpeg;*.png;*.jpg;*.gif)|*.jpeg;*.png;*.jpg;*.gif"
                };
                bool? result = FilePickerDialog.ShowDialog();
                if (result == true)
                {

                    string filename = FilePickerDialog.FileName;
                    SelectedBook.CoverImagePath = filename;
                }
            }
            else
            {
                MessageBox.Show("No book selected","Błąd",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        #endregion BookButtons
        private void BookSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxAvailableBooks.SelectedIndex >= 0 && ListBoxAvailableBooks.SelectedIndex < AdminWindowVM.Books.Count)
            {
                SelectedBook = AdminWindowVM.Books[ListBoxAvailableBooks.SelectedIndex];
                ListBoxBooksAuthors.ItemsSource = SelectedBook.AuthorList;
            }
            else
            {
                ListBoxBooksAuthors.ItemsSource = null;
            }


        }
        #region CommandAddBookAuthor
        public static RoutedCommand CommandAddBookAuthor = new RoutedCommand();
        private void ExecutedAddBookAuthor(object sender, ExecutedRoutedEventArgs e)
        {
            AuthorListPopup authorListPopup = new AuthorListPopup(AdminWindowVM.Authors.Except(SelectedBook.AuthorList).ToList());
            authorListPopup.Owner = this;
            authorListPopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (authorListPopup.ShowDialog() == true)
            {
                if (authorListPopup.ResultAuthor != null)
                {
                    SelectedBook.AuthorList.Add(authorListPopup.ResultAuthor);
                    authorListPopup.ResultAuthor.WrittenBooks.Add(SelectedBook);
                    ListBoxBooksAuthors.Items.Refresh();
                }
            }
        }
        private void CanExecuteAddBookAuthor(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableBooks.SelectedItem == null || SelectedBook.AuthorList.Count >= AdminWindowVM.Authors.Count)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }
        #endregion CommandAddBookAuthor
        #region CommandRemoveBookAuthor
        public static RoutedCommand CommandRemoveBookAuthor = new RoutedCommand();
        private void ExecutedRemoveBookAuthor(object sender, ExecutedRoutedEventArgs e)
        {
            Author tempRemoved = SelectedBook.AuthorList[ListBoxBooksAuthors.SelectedIndex];
            SelectedBook.AuthorList.Remove(tempRemoved);

            tempRemoved.WrittenBooks.Remove(SelectedBook);
            ListBoxBooksAuthors.Items.Refresh();
        }
        private void CanExecuteRemoveBookAuthor(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxBooksAuthors.SelectedIndex >= 0 && ListBoxBooksAuthors.SelectedIndex < SelectedBook.AuthorList.Count)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion CommandRemoveBookAuthor
        #region CommandRemoveBook
        public static RoutedCommand CommandDeleteBook = new RoutedCommand();
        private void ExecutedDeleteBook(object sender, ExecutedRoutedEventArgs e)
        {
            int tempIndex = ListBoxAvailableBooks.SelectedIndex;
            Book removed = AdminWindowVM.Books[tempIndex];
            AdminWindowVM.Books.Remove(removed);
            foreach (Author booksAuthors in removed.AuthorList)
            {
                booksAuthors.WrittenBooks.Remove(removed);
            }
            removed.CurrentOwner.RentedBooks.Remove(removed);
            ListBoxAuthorsBooks.Items.Refresh();
            ListBoxUsersBooks.Items.Refresh();
            if (AdminWindowVM.Books.Count <= 0)
            {
                ListBoxBooksAuthors.ItemsSource = null;
            }
            else
            {
                ListBoxAvailableBooks.SelectedIndex = tempIndex - 1;
            }

        }
        private void CanExecuteDeleteBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableBooks.SelectedIndex >= 0 && ListBoxAvailableBooks.SelectedIndex < AdminWindowVM.Books.Count)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion CommandRemoveBook
        #region CommandRemoveAuthor
        public static RoutedCommand CommandDeleteAuthor = new RoutedCommand();
        private void ExecutedDeleteAuthor(object sender, ExecutedRoutedEventArgs e)
        {
            int tempIndex = ListBoxAvailableAuthors.SelectedIndex;
            Author removed = AdminWindowVM.Authors[tempIndex];
            AdminWindowVM.Authors.Remove(removed);
            foreach (Book authorsBooks in removed.WrittenBooks)
            {
                authorsBooks.AuthorList.Remove(removed);
            }
            AdminWindowVM.Authors.Remove(removed);
            ListBoxBooksAuthors.Items.Refresh();
            if (AdminWindowVM.Authors.Count <= 0)
            {
                ListBoxAuthorsBooks.ItemsSource = null;
            }
            else
            {
                ListBoxAvailableAuthors.SelectedIndex = tempIndex - 1;
            }


        }
        private void CanExecuteDeleteAuthor(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableAuthors.SelectedIndex >= 0 && ListBoxAvailableAuthors.SelectedIndex < AdminWindowVM.Authors.Count)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion CommandRemoveAuthor
        private void AuthorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxAvailableAuthors.SelectedIndex >= 0 && ListBoxAvailableAuthors.SelectedIndex < AdminWindowVM.Authors.Count)
            {
                SelectedAuthor = AdminWindowVM.Authors[ListBoxAvailableAuthors.SelectedIndex];
                ListBoxAuthorsBooks.ItemsSource = SelectedAuthor.WrittenBooks;

            }
            else
            {
                ListBoxAuthorsBooks.ItemsSource = null;
            }
        }
        #region CommandAddAuthorBook
        public static RoutedCommand CommandAddAuthorBook = new RoutedCommand();
        private void ExecutedAddAuthorBook(object sender, ExecutedRoutedEventArgs e)
        {
            BookListPopup bookListPopup = new BookListPopup(AdminWindowVM.Books.Except(SelectedAuthor.WrittenBooks).ToList());
            bookListPopup.Owner = this;
            bookListPopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (bookListPopup.ShowDialog() == true)
            {
                if (bookListPopup.ResultBook != null)
                {
                    SelectedAuthor.WrittenBooks.Add(bookListPopup.ResultBook);
                    bookListPopup.ResultBook.AuthorList.Add(SelectedAuthor);
                    ListBoxAuthorsBooks.Items.Refresh();
                }
            }
        }
        private void CanExecuteAddAuthorBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableAuthors.SelectedItem == null || SelectedAuthor.WrittenBooks.Count >= AdminWindowVM.Books.Count)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }
        #endregion CommandAddAuthorBook
        #region CommandRemoveAuthorBook
        public static RoutedCommand CommandRemoveAuthorBook = new RoutedCommand();
        private void ExecutedRemoveAuthorBook(object sender, ExecutedRoutedEventArgs e)
        {
            Book tempRemoved = SelectedAuthor.WrittenBooks[ListBoxAuthorsBooks.SelectedIndex];
            SelectedAuthor.WrittenBooks.Remove(tempRemoved);

            tempRemoved.AuthorList.Remove(SelectedAuthor);
            ListBoxAuthorsBooks.Items.Refresh();
        }
        private void CanExecuteRemoveAuthorBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAuthorsBooks.SelectedIndex >= 0 && ListBoxAuthorsBooks.SelectedIndex < SelectedAuthor.WrittenBooks.Count)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion CommandRemoveAuthorBook
        private void ButtonAddAuthorClick(object sender, RoutedEventArgs e)
        {
            AdminWindowVM.Authors.Add(new Author("new"));
            ListBoxAvailableAuthors.SelectedIndex = AdminWindowVM.Authors.Count - 1;
            TextBoxAdminAuthorName.SelectAll();
            TextBoxAdminAuthorName.Focus();
        }

        private void BookOwnerChanged(object sender, EventArgs e)
        {
            if (SelectedBook != null)
            {
                if (SelectedBook.CurrentOwner != null)
                {
                    SelectedBook.CurrentOwner.RentedBooks.Remove(SelectedBook);
                }

                SelectedBook.CurrentOwner = (User)ComboBoxBookOwner.SelectedItem;
                if (SelectedBook.CurrentOwner != null)
                {
                    SelectedBook.CurrentOwner.RentedBooks.Add(SelectedBook);
                    ListBoxUsersBooks.Items.Refresh();
                }

            }

        }

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
                ListBoxUsersBooks.ItemsSource = SelectedUser.RentedBooks;

            }
            else
            {
                ListBoxUsersBooks.ItemsSource = null;
            }
        }
        #region CommandAddUserBook
        public static RoutedCommand CommandAddUserBook = new RoutedCommand();
        private void ExecutedAddUserBook(object sender, ExecutedRoutedEventArgs e)
        {
            if (AdminWindowVM.Users[0].RentedBooks.Count < 1)
            {
                MessageBox.Show("There are no free books", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                BookListPopup bookListPopup = new BookListPopup(AdminWindowVM.Users[0].RentedBooks);
                bookListPopup.Owner = this;
                bookListPopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (bookListPopup.ShowDialog() == true)
                {
                    if (bookListPopup.ResultBook != null)
                    {
                        SelectedUser.RentedBooks.Add(bookListPopup.ResultBook);
                        AdminWindowVM.Users[0].RentedBooks.Remove(bookListPopup.ResultBook);
                        bookListPopup.ResultBook.CurrentOwner = SelectedUser;
                        ListBoxUsersBooks.Items.Refresh();
                        ListBoxAvailableBooks.SelectedIndex = -1;
                    }
                }
            }

        }
        private void CanExecuteAddUserBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxAvailableUsers.SelectedIndex < 1 || AdminWindowVM.Users[0].RentedBooks.Count <1)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }
        #endregion CommandAddUserBook
        #region CommandRemoveUser
        public static RoutedCommand CommandDeleteUser = new RoutedCommand();
        private void ExecutedDeleteUser(object sender, ExecutedRoutedEventArgs e)
        {
            int tempIndex = ListBoxAvailableUsers.SelectedIndex;
            User removed = AdminWindowVM.Users[tempIndex];
            AdminWindowVM.Users.Remove(removed);
            foreach (Book usersBook in removed.RentedBooks)
            {
                usersBook.CurrentOwner = AdminWindowVM.Users[0];
            }
            if (AdminWindowVM.Users.Count <= 1)
            {
                ListBoxUsersBooks.ItemsSource = null;
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
        #region CommandRemoveAuthorBook
        public static RoutedCommand CommandRemoveUsersBook = new RoutedCommand();
        private void ExecutedRemoveUsersBook(object sender, ExecutedRoutedEventArgs e)
        {
            Book tempRemoved = SelectedUser.RentedBooks[ListBoxUsersBooks.SelectedIndex];
            SelectedUser.RentedBooks.Remove(tempRemoved);
            AdminWindowVM.Users[0].RentedBooks.Add(tempRemoved);
            tempRemoved.CurrentOwner = AdminWindowVM.Users[0];
            ListBoxUsersBooks.Items.Refresh();
            ListBoxAvailableBooks.SelectedIndex = -1;
        }
        private void CanExecuteRemoveUsersBook(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ListBoxUsersBooks.SelectedIndex >= 0 && ListBoxUsersBooks.SelectedIndex < SelectedUser.RentedBooks.Count)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        #endregion CommandRemoveAuthorBook

        

        private void FilterReset()
        {
            CollectionViewSource.GetDefaultView(ListBoxAvailableAuthors.ItemsSource).Filter = null;
            CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = null;
            CollectionViewSource.GetDefaultView(ListBoxAvailableBooks.ItemsSource).Filter = null;
        }
        private void AdminTabControlChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl&&IsActive)
            {
               
                if (TabItemBooks.IsSelected)
                {
                    ComboBoxSearchType.ItemsSource = ApplicationConstants.bookSearches;
                }
                else if(TabItemAuthors.IsSelected)
                {
                    ComboBoxSearchType.ItemsSource = ApplicationConstants.authorSearches;
                    
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
        private bool FilterUserBookTitle(Object item)
        {
            User user = (User)item;
            if (user == AdminWindowVM.Users[0])
            {
                return true;
            }
            foreach (Book b in user.RentedBooks)
            {
                if (b.Title.IndexOf(TextBoxAdminSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

            }
            return false;
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
            else if (TabItemAuthors.IsSelected)
            {
                FilterAuthor();
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
                        CollectionViewSource.GetDefaultView(ListBoxAvailableUsers.ItemsSource).Filter = FilterUserBookTitle;
                        break;
                    
                    }
                case 3:
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
        private void FilterAuthor()
        {
            switch (ComboBoxSearchType.SelectedIndex)
            {
                case 0:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableAuthors.ItemsSource).Filter = null;
                        break;
                    }
                case 1:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableAuthors.ItemsSource).Filter = FilterAuthorName;
                        break;
                    }
                case 2:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableAuthors.ItemsSource).Filter = FilterAuthorBookTitle;
                        break;

                    }
                default:
                    {
                        CollectionViewSource.GetDefaultView(ListBoxAvailableAuthors.ItemsSource).Filter = null;
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
                printDlg.PrintVisual(ListBoxUsersBooks, SelectedUser.Username+" books" );
            }
            
        }
    }
}
