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
        public string Id { get; set; }
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
        public virtual List<Event> SeenEvents { get; set; }

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
            SeenEvents = new List<Event>();
        }
        public User(string username)
        {
            Username = username;
            SeenEvents = new List<Event>();
        }
        public User(string id, string username, string password, bool isAdmin)
        {
            Id = id;
            Username = username;
            Password = password;
            IsAdmin = isAdmin;
            SeenEvents = new List<Event>();
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
