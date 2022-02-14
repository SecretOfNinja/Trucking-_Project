using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace TruckingServices.Models
{
    public class Customer : INotifyPropertyChanged
    {
        private int idCard = -1;
        private string customerName = "";
        private string phoneNumber = "";
        private List<Trucking> _orders = new List<Trucking>();

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

        // public int ID { get; set; } = -1;
        public string CustomerName
        {
            get
            {
                return customerName;
            }

            set
            {
                customerName = value;
                NotifyPropertyChanged();
            }
        }

        public string CompanyName { get; set; } = "";

        public int IDCard
        {
            get
            {
                return idCard;
            }

            set
            {
                idCard = value;
                NotifyPropertyChanged();
            }
        }

        public string Area { get; set; } = "";
        public string City { get; set; } = "";
        public string PhoneNumber
        {
            get
            {
                return phoneNumber;
            }

            set
            {
                phoneNumber = value;
                NotifyPropertyChanged();
            }
        }

        public void setValues(int idCard, string customerName, string companyName, string area, string city, string phoneNumber)
        {
            IDCard = idCard;
            CustomerName = customerName;
            CompanyName = companyName;
            //IdCard = idCard;
            Area = area;
            City = city;
            PhoneNumber = phoneNumber;
        }

    }
}
