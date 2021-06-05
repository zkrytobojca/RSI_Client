using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSI_Client.Model
{
    [Serializable]
    public class Event : INotifyPropertyChanged
    {
        #region Fields
        public string Id { get; set; }
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("DisplayEvent");
            }

        }

        private RSI_Client.EventsService.type type;
        public RSI_Client.EventsService.type Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                OnPropertyChanged("DisplayEventType");
            }

        }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                OnPropertyChanged("DisplayEventDescription");
            }

        }

        private DateTime date;
        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                OnPropertyChanged("DisplayEventDate");
            }

        }
        public String DateShort
        {
            get
            {
                return date.ToLongDateString();
            }
        }

        private int week;
        public int Week
        {
            get
            {
                return week;
            }
            set
            {
                week = value;
                OnPropertyChanged("DisplayEventWeek");
            }

        }

        private int month;
        public int Month
        {
            get
            {
                return month;
            }
            set
            {
                month = value;
                OnPropertyChanged("DisplayEventMonth");
            }

        }

        private int year;
        public int Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
                OnPropertyChanged("DisplayEventYear");
            }

        }
        #endregion Fields

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(property));
        }
        public string DisplayEvent
        {
            get
            {
                return Name;
            }
        }

        #region Consctructors
        public Event()
        {

        }
        public Event(string id, string name, RSI_Client.EventsService.type type, DateTime date, int week, int month, int year, string description)
        {
            Id = id;
            Name = name;
            Type = type;
            Date = date;
            Week = week;
            Month = month;
            Year = year;
            Description = description;
        }
        #endregion Consctructors
    }
}
