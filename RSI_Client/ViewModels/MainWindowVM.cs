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
    public class MainWindowVM : INotifyPropertyChanged
    {
        public ObservableCollection<Author> Authors { get; set; }
        public ObservableCollection<Book> Books { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public MainWindowVM()
        {
            Authors = new ObservableCollection<Author>();
            Books = new ObservableCollection<Book>();
            Users = new ObservableCollection<User>();
        }
        private User loggedUser;
     
        public User LoggedUser {
            get
            {
                return loggedUser;
            }
            set {
                loggedUser = value;
                OnPropertyChanged("LoggedUser");
            } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(property));
        }
    }
}
