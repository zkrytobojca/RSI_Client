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
        public ObservableCollection<Event> Events { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public AdminWindowVM()
        {
            Events = new ObservableCollection<Event>();
            Users = new ObservableCollection<User>();
        }
        public AdminWindowVM(ObservableCollection<Event> events, ObservableCollection<User>users)
        {
            Events = events;
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
