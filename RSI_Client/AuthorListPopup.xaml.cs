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
using RSI_Client.Model;

namespace RSI_Client
{
    /// <summary>
    /// Logika interakcji dla klasy AuthorListPopup.xaml
    /// </summary>
    public partial class AuthorListPopup : Window
    {
        public AuthorListPopup(List<Author>authors)
        {
            InitializeComponent();
            ListBoxAddedAuthorList.ItemsSource = authors;
           
        }
        public Author ResultAuthor { get; set; }

        private void PopupAccept(object sender, RoutedEventArgs e)
        {
            ResultAuthor= (Author)ListBoxAddedAuthorList.SelectedItem;
            DialogResult = true;
        }

        private void PopupCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

        }
    }
}
