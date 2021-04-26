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
    public class Book:INotifyPropertyChanged
    {
        #region Fields
        public int Id { get; set; }
        private string title;
        public string Title {
            get
            {
                return title;
            }
            set
            {
                title = value;
                OnPropertyChanged("DisplayBook");
            }

        }
        public DateTime ReleaseDate { get; set; }
        public string DisplayDate
        {
            get
            {
                return "Released "+ReleaseDate.ToString("dd/MM/yyyy");
            }
        }
        public List<int> AuthorIds { get; set; }
        private List<Author> authorList;
        public  List<Author>AuthorList
        {
            get
            {
                return authorList;
            }
            set
            {
                authorList = value;
                OnPropertyChanged("AuthorList");
            }
        }
        public virtual User CurrentOwner { get; set; }
        private string coverImagePath;
        public string CoverImagePath
        {
            get
            {
                return coverImagePath;
            }
            set
            {
                coverImagePath = value;
                OnPropertyChanged("CoverImagePath");
            }
        }
        #endregion Fields
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(property));
        }
        public string DisplayBook
        {
            get
            {
                return Title;
            }
        }
        public string DisplayAuthors
        {
            get
            {
                if (authorList.Count == 0) return "-- --"; 
                string result = "";
                foreach(Author author in authorList)
                {
                    result += author.DisplayAuthor + " ";
                }
                return result;
            }
        }
        #region Consctructors
        public Book()
        {

        }
        public Book(string title)
        {
            Title = title;
            ReleaseDate = DateTime.Today;
            AuthorList = new List<Author>();
        }
        #endregion Consctructors
    }
}
