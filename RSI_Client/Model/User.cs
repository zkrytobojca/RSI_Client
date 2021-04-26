using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSI_Client.Model
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string username;
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                OnPropertyChanged("DisplayUser");
            }
        }
        public string Password { get; set; }
        public List<int> BookIds { get; set; }
        public virtual List<Book> RentedBooks { get; set; }
        private bool isAdmin;
        public bool IsAdmin
        {
            get
            {
                return isAdmin;
            }
            set
            {
                isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }
        public User()
        {
            RentedBooks = new List<Book>();
        }
        public User(string name)
        {
            Username = name;
            RentedBooks = new List<Book>();
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(property));
        }
        public string DisplayUser
        {
            get
            {
                return username;
            }
        }
    }
}
