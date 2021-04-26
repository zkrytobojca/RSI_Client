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
    public class Author:INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string fullName;
        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {
                fullName = value;
                OnPropertyChanged("DisplayAuthor");
            }
        }
        public virtual List<Book> WrittenBooks { get; set; }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(property));
        }
        public string DisplayAuthor
        {
            get
            {
                return fullName;
            }
        }
        public Author()
        {


        }
        public Author(string name)
        {
            FullName = name;
            WrittenBooks = new List<Book>();
        }
    }
}
