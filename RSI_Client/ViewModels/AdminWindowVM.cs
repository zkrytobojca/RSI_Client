using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSI_Client.Model;

namespace RSI_Client.ViewModels
{
    public class AdminWindowVM: INotifyPropertyChanged
    {
        public ObservableCollection<Author> Authors { get; set; }
        public ObservableCollection<Book> Books { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public AdminWindowVM()
        {
            Authors = new ObservableCollection<Author>();
            Books = new ObservableCollection<Book>();
            Users = new ObservableCollection<User>();
        }
        public AdminWindowVM(ObservableCollection<Author>authors,ObservableCollection<Book>books,ObservableCollection<User>users)
        {
            Authors = authors;
            Books = books;
            Users = users;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(property));
        }
    }
}
