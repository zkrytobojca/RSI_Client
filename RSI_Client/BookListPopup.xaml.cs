using System;
using System.Collections.Generic;
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

namespace RSI_Client
{
    /// <summary>
    /// Logika interakcji dla klasy BookListPopup.xaml
    /// </summary>
    public partial class BookListPopup : Window
    {
        public BookListPopup(List<Book> books)
        {
            InitializeComponent();
            ListBoxAddedBookList.ItemsSource = books;

        }
        public Book ResultBook { get; set; }

        private void PopupAccept(object sender, RoutedEventArgs e)
        {
            ResultBook = (Book)ListBoxAddedBookList.SelectedItem;
            DialogResult = true;
        }

        private void PopupCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

        }
    }

}
