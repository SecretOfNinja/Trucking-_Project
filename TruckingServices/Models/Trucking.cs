using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TruckingServices.Models
{

    public class Trucking : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int orderId = -1;
        private DateTime? transportDate = new DateTime();
        private string? dateString = "";
        private long dateAsTickCount; // Used to sort date in order

        public int OrderID
        {
            get
            {
                return orderId;
            }
            set
            {
                orderId = value;
                NotifyPropertyChanged();
            }
        }

        public int IDCard { get; set; } = -1;

        public DateTime? TransportDate
        {
            get
            {
                return transportDate;
            }
            set
            {
                transportDate = value;
                DateString = "";
                if (transportDate != null)
                {
                    DateTime dt = (DateTime)transportDate;
                    DateString = dt.ToShortDateString();
                }

                NotifyPropertyChanged();
            }
        }

        public string? DateString
        {
            get
            {
                return dateString;
            }
            set
            {
                dateString = value;
                NotifyPropertyChanged();
            }
        }

        // Used to sort collection by date
        public long DateAsTickCount
        {
            get
            {
                if (transportDate != null)
                {
                    DateTime dt = (DateTime)transportDate;
                    dateAsTickCount = dt.Ticks;
                }
                else
                {
                    dateAsTickCount = 0;
                }
                return dateAsTickCount;
            }
        }

        public decimal TransportPrice { get; set; } = 0.0m;
        public string StartingPoint { get; set; } = "";
        public string Destination { get; set; } = "";
        public double PriceOfPallets { get; set; } = 0.0;
        public int NumberOfPallets { get; set; } = 0;




        public void setValues(int orderId, int idCard, DateTime transportDate, decimal transportPrice, string startingPoint,
            string destination, double priceOfPallets, int numberOfPallets)
        {
            OrderID = orderId;
            IDCard = idCard;
            TransportDate = transportDate;
            TransportPrice = transportPrice;
            StartingPoint = startingPoint;
            Destination = destination;
            PriceOfPallets = priceOfPallets;
            NumberOfPallets = numberOfPallets;
        }
    }
}
