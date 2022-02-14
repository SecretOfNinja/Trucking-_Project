using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using TruckingServices.Models;
using TruckingServices.database;

namespace TruckingServices
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Database _theDatabase;
        private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();
        private GridViewColumnHeader? listViewSortCol = null;
        private Customer? _currentlySelectedCustomer = null;
        private ObservableCollection<Trucking> _currentOrders = new ObservableCollection<Trucking>();
        private Trucking? _currentOrder = null;
        private List<string> _printerPrintLines = new List<string>(); // Lines of text used to print to the printer
        private bool _languageEnglish = false; // true is English, False is Hebrew... type left-to-right/right-to-left
        public MainWindow()
        {
            InitializeComponent();
            _theDatabase = new Database();
            List<Customer> customers = new List<Customer>();
            _theDatabase.getCustomers(customers);
            customers.ToList().ForEach(_customers.Add);

            _lstVwCustomers.ItemsSource = _customers;
            setLanguageFlowDirection(FlowDirection.RightToLeft);
        }

        private void setLanguageFlowDirection(FlowDirection fd)
        {
            // ListView for customers
            _lstVwCustomers.FlowDirection = fd;

            // Customer Form
            _txtBxCustomerAddName.FlowDirection = fd;
            _txtBxCustomerAddCompanyName.FlowDirection = fd;
            _txtBxCustomerAddid.FlowDirection = fd;
            _txtBxCustomerAddArea.FlowDirection = fd;
            _txtBxCustomerAddCity.FlowDirection = fd;
            _txtBxCustomerAddnumberphone.FlowDirection = fd;

            // Orders Form
            _txtBxOrderNumber.FlowDirection = fd;
            _txtBxOrderCustomerName.FlowDirection = fd;
            _txtBxOrderCustomerID.FlowDirection = fd;
            _txtBxOrderStartingPoint.FlowDirection = fd;
            _txtBxOrderDestination.FlowDirection = fd;
            _txtBxOrderNumberOfPallets.FlowDirection = fd;
            _txtBxOrderPrice.FlowDirection = fd;
            _dtPkrTransport.FlowDirection = fd;

            // Print Form
            _txtBxPrintCustomerName.FlowDirection = fd;
            _txtBxPrintCustomerID.FlowDirection = fd;
            _dtPkrPrintFrom.FlowDirection = fd;
            _dtPkrPrintTo.FlowDirection = fd;

            _rTxtBxPrintOut.FlowDirection = fd;
        }

        private void Menu_Button_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;

            if (btn == null)
                return;

            _grpBoxPrintOrders.Visibility = Visibility.Hidden;
            _grpBoxCustomerForm.Visibility = Visibility.Hidden;
            _grpBoxCustomerOrderForm.Visibility = Visibility.Hidden;

            if (btn.Uid == "Print")
            {
                _grpBoxPrintOrders.Visibility = Visibility.Visible;
            }
            else if (btn.Uid == "Customers")
            {
                _grpBoxCustomerForm.Visibility = Visibility.Visible;
            }
            else if (btn.Uid == "Orders")
            {
                _grpBoxCustomerOrderForm.Visibility = Visibility.Visible;
            }

        }

        // Make sure phone number has only digits, and has between 7 and 11 
        // numbers
        private bool properPhoneNumber(string value)
        {
            int digitCount = 0;

            foreach (char ch in value)
            {
                if (Char.IsDigit(ch))
                {
                    digitCount++;
                }
                else if (ch != ' ')
                {
                    return false;
                }
            }

            if (digitCount >= 7 && digitCount <= 11)
                return true;

            return false;
        }

        // Returns false of length is zero, or if the characters aren't
        // alpha numeric or spaces.  This function assumes that there is atleast one
        // alpha numeric character and leading and trailing and extraneous
        // space have been removed
        private bool properEntryString(string value)
        {
            if (value.Length == 0)
            {
                return false;
            }

            foreach (char ch in value)
            {
                if (!(Char.IsLetterOrDigit(ch) || ch == ' ' || ch == ',' || ch == '.' || ch == '-'))
                {
                    return false;
                }

            }
            return true;
        }

        // Makes sure there are no leading or trailing spaces and that there is no more than one
        // space between characters. Pass in string to edit.  Returns the edited string
        private string deleteExtraSpaces(string value)
        {
            string edited = "";
            int spcCount = 0;
            int i = 0;
            int length;

            if (value.Length == 0)
            {
                return edited;
            }

            // ignore leading spaces
            while (value[i] == ' ' && i < value.Length)
            {
                i++;
            }

            // There are only spaces in this string, return an empty string
            if (i == value.Length)
            {
                return "";
            }

            length = value.Length;
            while (value[length - 1] == ' ')
            {
                length--;
            }
            // Make sure gaps are only one space
            while (i < length)
            {
                if (value[i] == ' ')
                {
                    if (spcCount == 0)
                    {
                        edited += value[i];
                    }
                    spcCount++;
                }
                else
                {
                    edited += value[i];
                    spcCount = 0;
                }
                i++;
            }

            return edited;
        }

        private bool customerDataSanityCheck(Customer customer)
        {
            // Eliminate any extranous spaces first
            //Customer name
            customer.CustomerName = deleteExtraSpaces(customer.CustomerName);
            _txtBxCustomerAddName.Text = customer.CustomerName;

            //company name
            customer.CompanyName = deleteExtraSpaces(customer.CompanyName);
            _txtBxCustomerAddCompanyName.Text = customer.CompanyName;

            // IDCard
            _txtBxCustomerAddid.Text = customer.IDCard.ToString();

            // Area
            customer.Area = deleteExtraSpaces(customer.Area);
            _txtBxCustomerAddArea.Text = customer.Area;

            // City
            customer.City = deleteExtraSpaces(customer.City);
            _txtBxCustomerAddCity.Text = customer.City;

            // phone number
            customer.PhoneNumber = deleteExtraSpaces(customer.PhoneNumber);
            _txtBxCustomerAddnumberphone.Text = customer.PhoneNumber;


            // Check if the data is entered correctly.

            //Customer name 
            if (!properEntryString(customer.CustomerName))
            {
                messageBoxOK("Customer Name Entered Incorrectly");
                return false;
            }

            //company name    
            if (!properEntryString(customer.CompanyName))
            {
                messageBoxOK("Company Name Entered Incorrectly");
                return false;
            }

            // Area
            if (!properEntryString(customer.Area))
            {
                messageBoxOK("Area Entered Incorrectly");
                return false;
            }

            // City
            if (!properEntryString(customer.City))
            {
                messageBoxOK("City Entered Incorrectly");
                return false;
            }

            // phone number
            if (!properPhoneNumber(customer.PhoneNumber))
            {
                messageBoxOK("Phone Number Entered Incorrectly");
                return false;
            }
            return true;
        }

        private Customer getCustomerDataFromForm()
        {
            Customer customer = new Customer();

            customer.CustomerName = _txtBxCustomerAddName.Text;
            customer.CompanyName = _txtBxCustomerAddCompanyName.Text;
            customer.IDCard = convertStringToInt(_txtBxCustomerAddid.Text);
            customer.Area = _txtBxCustomerAddArea.Text;
            customer.City = _txtBxCustomerAddCity.Text;
            customer.PhoneNumber = _txtBxCustomerAddnumberphone.Text;

            return customer;
        }

        private bool doesIdCardExist(int idCard)
        {
            if (idCard < 1)
            {
                return false;
            }

            foreach (Customer c in _customers)
            {
                if (c.IDCard == idCard)
                {
                    return true;
                }
            }
            return false;
        }

        // Find a free slot for a new IDCard value
        private int getNextFreeIdCardSlot()
        {
            List<int> idCards = new List<int>();

            // List returned is iin ascending order
            _theDatabase.getCustomerIDCards(idCards);


            // Look for gaps
            int id = 1;
            foreach (int c in idCards)
            {
                if (id < c)
                {
                    return id;
                }
                if (id == c)
                {
                    id++;
                }
            }

            return id;
        }

        private void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            Customer customer = getCustomerDataFromForm();


            // If the card id is less than 1, then find a free slot automatically
            if (customer.IDCard < 1)
            {
                customer.IDCard = getNextFreeIdCardSlot();
            }
            else // Check to see if the name entered by the user is available. If so, keep it; If not, generate a new card number.
            {
                if (doesIdCardExist(customer.IDCard))
                {
                    customer.IDCard = getNextFreeIdCardSlot();
                }
            }


            if (customerDataSanityCheck(customer))
            {
                if (_theDatabase.addCustomer(customer))
                {
                    _customers.Add(customer);
                }
                else
                {
                    messageBoxOK("Unable To Add Customer");
                }
            }

        }

        private void _lstVwCustomers_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int item;
            if (_lstVwCustomers.SelectedItems.Count == 0)
                return;

            item = _lstVwCustomers.Items.IndexOf(_lstVwCustomers.SelectedItems[0]);
            int i = 0;
            i++;
        }

        private void _lstVwCustomerColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader? column = sender as GridViewColumnHeader;

            if (column == null)
            {
                return;
            }

            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                //AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                _lstVwCustomers.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            listViewSortCol = column;
            _lstVwCustomers.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void clearOrderForm(bool removeCustomerNameAndId = true)
        {
            _txtBxOrderNumber.Text = "";
            if (removeCustomerNameAndId)
            {
                _txtBxOrderCustomerName.Text = "";
                _txtBxOrderCustomerID.Text = "";
            }
            _dtPkrTransport.SelectedDate = null;
            _txtBxOrderShippingWeight.Text = "";
            _txtBxOrderStartingPoint.Text = "";
            _txtBxOrderDestination.Text = "";
            _txtBxOrderNumberOfPallets.Text = "";
            _txtBxOrderPrice.Text = "";
        }

        // Display the customer clicked on in the ListView _lstVwCustomers
        private void _lstVwCustomersItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void _lstVwCustomersItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;

            if (item != null && item.IsSelected)
            {
                Customer? customer = _lstVwCustomers.SelectedItem as Customer;

                // Add selected customer info to textboxes
                if (customer != null)
                {
                    clearOrderForm();
                    // Customers Form
                    if (_grpBoxCustomerForm.Visibility == Visibility.Visible)
                    {
                        _txtBxCustomerAddName.Text = customer.CustomerName;
                        _txtBxCustomerAddCompanyName.Text = customer.CompanyName;
                        _txtBxCustomerAddid.Text = customer.IDCard.ToString();
                        _txtBxCustomerAddArea.Text = customer.Area;
                        _txtBxCustomerAddCity.Text = customer.City;
                        _txtBxCustomerAddnumberphone.Text = customer.PhoneNumber;
                        _currentlySelectedCustomer = customer;

                    } // Customer Orders Form
                    else if (_grpBoxCustomerOrderForm.Visibility == Visibility.Visible)
                    {

                        _txtBxOrderCustomerID.Text = customer.IDCard.ToString();
                        _txtBxOrderCustomerName.Text = customer.CustomerName;
                        List<Trucking> orders = new List<Trucking>();
                        if (_theDatabase.getCustomersOrders(orders, customer.IDCard))
                        {
                            _currentOrders.Clear();
                            orders.ToList().ForEach(_currentOrders.Add);
                        }
                        _lstVwOrders.ItemsSource = _currentOrders;
                        _currentOrder = null;
                    }
                    else if (_grpBoxPrintOrders.Visibility == Visibility.Visible)
                    {
                        _txtBxPrintCustomerName.Text = customer.CustomerName;
                        _txtBxPrintCustomerID.Text = customer.IDCard.ToString();
                        _rTxtBxPrintOut.Document.Blocks.Clear();
                    }
                }
            }
        }

        private void _lstVwOrdersItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;

            if (item != null && item.IsSelected)
            {
                Trucking? order = _lstVwOrders.SelectedItem as Trucking;

                if (order != null)
                {
                    _txtBxOrderNumber.Text = order.OrderID.ToString();
                    _dtPkrTransport.SelectedDate = order.TransportDate;
                    _txtBxOrderShippingWeight.Text = order.PriceOfPallets.ToString();
                    _txtBxOrderStartingPoint.Text = order.StartingPoint;
                    _txtBxOrderDestination.Text = order.Destination;
                    _txtBxOrderNumberOfPallets.Text = order.NumberOfPallets.ToString();
                    _txtBxOrderPrice.Text = order.TransportPrice.ToString("#.##");
                    _currentOrder = order; // set the current order
                }
            }
        }

        private int convertStringToInt(string strValue)
        {
            int result = -1;
            if (int.TryParse(strValue, out int val))
            {
                result = val;
            }

            return result;
        }

        // Get updated data from Customer form and place it in Customer object
        // return the customer
        private Customer getUpdatedDataFromForm()
        {
            Customer customer = new Customer();

            customer.CustomerName = _txtBxCustomerAddName.Text;
            customer.CompanyName = _txtBxCustomerAddCompanyName.Text;
            customer.IDCard = convertStringToInt(_txtBxCustomerAddid.Text);
            customer.Area = _txtBxCustomerAddArea.Text;
            customer.City = _txtBxCustomerAddCity.Text;
            customer.PhoneNumber = _txtBxCustomerAddnumberphone.Text;

            return customer;
        }

        // Send the updated data from the Customer's form to the database and update the
        // ListView _lstVwCustomers, by adding the update to the ObservableCollection _customers
        private void _btnUpdateCustomer_Click(object sender, RoutedEventArgs e)
        {
            Customer? customer = _currentlySelectedCustomer;
            if (customer == null)
            {
                messageBoxOK("No Customer To Update");
                return;
            }

            int currentIDCard = _currentlySelectedCustomer.IDCard;

            int IDCard = convertStringToInt(_txtBxCustomerAddid.Text);

            // IDCard value entered is different from current IDCard value
            if (currentIDCard != IDCard)
            {
                MessageBoxResult result = messageBoxYesNo("Do you want to change the user's IDCard?");
                if (result == MessageBoxResult.No)
                    return;

                if (doesCustomerExist(IDCard) == true)
                {
                    messageBoxOK("This IDCard is taken");
                    return;
                }
            }



            customer = getUpdatedDataFromForm();
            //getUpdatedDataFromForm();

            if (customerDataSanityCheck(customer))
            {
                //update the _currentlySelectedCustomer
                _currentlySelectedCustomer = customer;
                if (_theDatabase.updateCustomer(customer, currentIDCard))
                {
                    // Update observable collection using unique ID, which should update the Listivew _lstVwCustomers
                    for (int i = 0; i < _customers.Count(); i++)
                    {
                        if (_customers[i].IDCard == currentIDCard)
                        {
                            _customers[i] = customer;

                            //Update orders IDCard value if it was changed
                            if (currentIDCard != IDCard)
                            {
                                updateOrdersWhereIDCardChanged(currentIDCard, IDCard);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    messageBoxOK("Unable To Update Customer");
                }
            }
        }

        private void updateOrdersWhereIDCardChanged(int oldIDCard, int newIDCard)
        {
            List<Trucking> orders = new List<Trucking>();

            if (_theDatabase.getCustomersOrders(orders, oldIDCard))
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IDCard = newIDCard;
                    _theDatabase.updateCustomerOrder(orders[i]);
                }
            }
        }

        private void clearCustomersForm()
        {
            _txtBxCustomerAddName.Text = "";
            _txtBxCustomerAddCompanyName.Text = "";
            _txtBxCustomerAddid.Text = "";
            _txtBxCustomerAddArea.Text = "";
            _txtBxCustomerAddCity.Text = "";
            _txtBxCustomerAddnumberphone.Text = "";
        }


        private void messageBoxOK(string message)
        {
            string caption = "Trucking Services";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;

            MessageBox.Show(message, caption, button, icon, MessageBoxResult.OK);
        }

        private MessageBoxResult messageBoxYesNo(string message)
        {
            string caption = "Trucking Services";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;

            MessageBoxResult result = MessageBox.Show(message, caption, button, icon, MessageBoxResult.No);

            return result;
        }

        // Pass in the customer ID.  Check the ObservableCollection _customers to see
        // if he exists.  Return true if he does, else false
        private bool doesCustomerExist(int idCard)
        {
            // Does customer exist
            for (int i = 0; i < _customers.Count(); i++)
            {
                if (_customers[i].IDCard == idCard)
                {
                    return true;

                }
            }
            return false;
        }

        // Delete the currently selected customer
        private void _btnDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            Customer? customer = _currentlySelectedCustomer;
            if (customer == null)
            {
                messageBoxOK("No Customer Selected");
                return;
            }

            if (doesCustomerExist(customer.IDCard) == false)
            {
                messageBoxOK("This Customer Doesn't Exist");
                return;
            }

            if (_theDatabase.deleteCustomer(customer.IDCard))
            {
                // Remove the customer from the ObsevableCollection, which will update the ListView _lstVwCustomers                
                for (int i = 0; i < _customers.Count(); i++)
                {
                    if (_customers[i].IDCard == customer.IDCard)
                    {
                        _customers.RemoveAt(i);
                        clearCustomersForm();
                        break;
                    }
                }

                //Next, delete any orphaned orders for this customer
                if (_theDatabase.deleteAllCustomersOrders(customer.IDCard) == false)
                {
                    messageBoxOK("Unable To Delete Customer's Orders");
                }
            }
            else
            {
                messageBoxOK("Unable To Delete Customer");
            }
        }

        // Gets data from order form and does some sanity checking on
        // numerical values.  If a value is incorrectly set, it's set to -1 for
        // later rejection
        private Trucking getOrderFromForm()
        {
            Trucking order = new Trucking();

            if (int.TryParse(_txtBxOrderNumber.Text, out int val0))
            {
                order.OrderID = val0;
            }
            else
            {
                order.OrderID = -1; // Invalid Number
            }


            if (int.TryParse(_txtBxOrderCustomerID.Text, out int val1))
            {
                order.IDCard = val1;
            }
            else
            {
                order.IDCard = -1; // Invalid ID Card
            }

            order.TransportDate = _dtPkrTransport.SelectedDate;

            if (double.TryParse(_txtBxOrderShippingWeight.Text, out double val2))
            {
                order.PriceOfPallets = val2;
            }
            else
            {
                order.PriceOfPallets = -1.0; // Invalid weight
            }

            order.StartingPoint = _txtBxOrderStartingPoint.Text;
            order.Destination = _txtBxOrderDestination.Text;

            if (int.TryParse(_txtBxOrderNumberOfPallets.Text, out int val3))
            {
                order.NumberOfPallets = val3;
            }
            else
            {
                order.NumberOfPallets = -1; // Invalid Number
            }

            if (decimal.TryParse(_txtBxOrderPrice.Text, out decimal val4))
            {
                order.TransportPrice = val4;
            }
            else
            {
                order.TransportPrice = -1; // Invalid price
            }

            return order;
        }

        private bool customerOrderDataSanityCheck(Trucking order)
        {
            // Eliminate any extranous spaces first
            //Starting Point
            order.StartingPoint = deleteExtraSpaces(order.StartingPoint);
            _txtBxOrderStartingPoint.Text = order.StartingPoint;

            //Destination
            order.Destination = deleteExtraSpaces(order.Destination);
            _txtBxOrderDestination.Text = order.Destination;


            // Check if the data is entered correctly.

            // Transport Date?
            if (order.TransportDate == null)
            {
                messageBoxOK("Incorrect Date Entered");
                return false;
            }

            // Shipping Weight?
            if (order.PriceOfPallets == -1)
            {
                messageBoxOK("Incorrect Shipping Weight Entered");
                return false;
            }

            //Starting Point?
            if (!properEntryString(order.StartingPoint))
            {
                messageBoxOK("Order Starting Point Entered Incorrectly");
                return false;
            }

            //Destination?
            if (!properEntryString(order.Destination))
            {
                messageBoxOK("Order Destination Entered Incorrectly");
                return false;
            }

            // Number of trucks?
            if (order.NumberOfPallets == -1)
            {
                messageBoxOK("Number of Trucks Entered Incorrectly");
            }



            return true;
        }

        private Decimal payment(int numberOfPallets, double pay)
        {
            double mam = 1.17;
            decimal price = (decimal)((numberOfPallets * pay) * mam);
            return price;
        }

        private Decimal BeforeVAT(Decimal Before)
        {
            decimal vat = 1.17m;

            return Before / vat;
        }



        private void btnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            Trucking order = getOrderFromForm();

            if (order.IDCard == -1)
            {
                messageBoxOK("Invalid ID Card");
                return;
            }

            // Do sanity check on the entered data to make sure it is sensible
            if (customerOrderDataSanityCheck(order))
            {


                order.TransportPrice = payment(order.NumberOfPallets, order.PriceOfPallets);
                _txtBxOrderPrice.Text = order.TransportPrice.ToString("#.##");
                if (_theDatabase.addCustomerOrder(order))
                {
                    _currentOrders.Add(order);
                    //Retain the current order
                    _currentOrder = order;
                }
                else
                {
                    messageBoxOK("Unable To Add Order");
                }
            }

        }

        private void _btnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            //Customer? customer = _currentlySelectedCustomer;
            Trucking order = getOrderFromForm();
            if (order == null)
            {
                messageBoxOK("No Order Selected");
                return;
            }

            if (order.IDCard == -1)
            {
                messageBoxOK("Invalid ID Card");
                return;
            }

            if (doesOrderExist(order.OrderID) == false)
            {
                messageBoxOK("This Order Doesn't Exist");
                return;
            }

            if (_theDatabase.deleteOrder(order.OrderID))
            {
                // Remove the customer from the ObsevableCollection, which will update the ListView _lstVwCustomers                
                for (int i = 0; i < _currentOrders.Count(); i++)
                {
                    if (_currentOrders[i].OrderID == order.OrderID)
                    {
                        _currentOrders.RemoveAt(i);
                        clearOrderForm(false);
                        return;
                    }
                }
            }
            else
            {
                messageBoxOK("Unable To Delete Order");
            }
        }

        // Pass in the order ID.  Check the ObservableCollection _currentOrders to see
        // if he exists.  Return true if he does, else false
        private bool doesOrderExist(int orderId)
        {
            // Does order exist
            for (int i = 0; i < _currentOrders.Count(); i++)
            {
                if (_currentOrders[i].OrderID == orderId)
                {
                    return true;

                }
            }
            return false;
        }

        private void _btnUpdateOrder_Click(object sender, RoutedEventArgs e)
        {
            Trucking order = getOrderFromForm();

            if (order.IDCard == -1)
            {
                messageBoxOK("Invalid ID Card");
                return;
            }

            // Does orderId exist?
            if (doesOrderExist(order.OrderID) == false)
            {
                messageBoxOK("This Order Doesn't Exist");
                return;
            }

            if (customerOrderDataSanityCheck(order))
            {
                order.TransportPrice = payment(order.NumberOfPallets, order.PriceOfPallets);
                //decimal.Round(yourValue, 2, MidpointRounding.AwayFromZero);
                _txtBxOrderPrice.Text = order.TransportPrice.ToString("#.##");
                // Add the updated order to the database's TruckingOrders table
                if (_theDatabase.updateCustomerOrder(order))
                {
                    // update the order collection
                    for (int i = 0; i < _customers.Count(); i++)
                    {
                        if (_currentOrders[i].OrderID == order.OrderID)
                        {
                            _currentOrders[i] = order;
                            return;
                        }
                    }
                }
                else
                {
                    messageBoxOK("Unable To Update Order");
                    return;
                }
            }


        }


        // Create a TextBlock and return it
        private TextBlock createTextBlock(string text, SolidColorBrush color, double fontSize, double posX, double posY, string fontFamily = "Arial", bool italic = false, bool bold = false)
        {
            TextBlock txtb = new TextBlock();


            txtb.FontStretch = FontStretches.Expanded;
            txtb.TextWrapping = TextWrapping.Wrap;

            txtb.Text = text;
            txtb.Foreground = color;
            txtb.FontFamily = new FontFamily(fontFamily);
            txtb.FontSize = fontSize;

            txtb.RenderTransform = new TranslateTransform(posX, posY);

            if (italic)
            {
                txtb.FontStyle = FontStyles.Italic;
            }

            if (bold)
            {
                txtb.FontWeight = FontWeights.Bold;
            }

            // A thickness object defines how large the margin or padding should be. 

            txtb.Margin = new Thickness(25, 10, 5, 10);
            txtb.Padding = new Thickness(2);

            return txtb;
        }

        // The steps to create a printable document at this link:
        //https://medium.com/@therealchrisrutherford/c-wpf-and-fixed-documents-lets-talk-printing-e7742bfecae1
        //https://docs.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-save-load-and-print-richtextbox-content?view=netframeworkdesktop-4.8
        private void _btnPrintOrder_Click(object sender, RoutedEventArgs e)
        {
            // Create the print dialog object and set options.
            PrintDialog printDialog = new();

            // Display the dialog. This returns true if the user presses the Print button.
            bool? isPrinted = printDialog.ShowDialog();
            if (isPrinted != true)
                return;

            //var fontList = Fonts.SystemFontFamilies; // The list of available fonts

            FixedDocument doc = new FixedDocument();
            doc.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            FixedPage fp = new FixedPage();
            fp.Width = doc.DocumentPaginator.PageSize.Width;
            fp.Height = doc.DocumentPaginator.PageSize.Height;

            // Construct the print document with TextBlocks.  Use the data from _printerPrintLines;
            double nextLine = 14;
            double position = 5;
            int deltaOrder = 0;
            TextBlock txtblk;
            for (int j = 0; j < _printerPrintLines.Count; j++)
            {
                if (j == 0) // The first line, i.e. the header
                {
                    txtblk = createTextBlock(_printerPrintLines[j], Brushes.Blue, 15, 5, position);
                    position += 25;

                }
                else if (j == _printerPrintLines.Count - 1) // The last line, the price total
                {
                    txtblk = createTextBlock(_printerPrintLines[j], Brushes.Red, 12, 5, position);
                }
                else // The orders lines
                {
                    deltaOrder++;
                    txtblk = createTextBlock(_printerPrintLines[j], Brushes.Black, 12, 5, position);

                    //Each order takes up three lines
                    if (deltaOrder % 3 == 0)
                    {
                        position += nextLine; // Add an extra space between orders
                    }
                    position += nextLine;
                }
                fp.Children.Add(txtblk);
            }

            //Create the PageContent container
            PageContent pc = new PageContent();
            // Add the FixedPage to the PageContent.   A FixedPage is one page.  You can add several pages
            ((IAddChild)pc).AddChild(fp);
            // Add the PageContent, pc, to the FixedDocument, doc
            doc.Pages.Add(pc);

            try
            {
                // Send the document to the printer
                printDialog.PrintDocument(doc.DocumentPaginator, "Print Job Name");
            }
            catch
            {

            }
        }


        private void printOutMessage(string text)
        {

        }

        private string formatOrderData(Trucking order)
        {
            string dataFormatted = "";

            dataFormatted = "תאריך: " + order.DateString + ", מספר הזמנה: " + order.OrderID.ToString() + "\n";
            dataFormatted += "נקודת התחלה: " + order.StartingPoint + " נקודת סיום: " + order.Destination + "\n";
            dataFormatted += "מחיר משטח: " + order.PriceOfPallets + ",  מספר משטחים : " + order.NumberOfPallets + "\n";
            dataFormatted += "     מחיר: " + order.TransportPrice.ToString() + "      לפני מעמ: " + (order.TransportPrice / 1.17m).ToString("#.##") + "\n\n";
            return dataFormatted;
        }

        // Add print out formatted data to the passed in orderData list used for printer printou
        private void getFormatOrderDataLines(Trucking order, List<string> orderData)
        {

            string dataFormatted;

            dataFormatted = "תאריך: " + order.DateString + ", מספר הזמנה: " + order.OrderID.ToString();
            orderData.Add(dataFormatted);


            dataFormatted = "  נקודת התחלה: " + order.StartingPoint + "    יעד: " + order.Destination + "    מחיר משטח:" + order.PriceOfPallets + ",  מספר משטחים: " + order.NumberOfPallets;
            orderData.Add(dataFormatted);

            dataFormatted = ",      מחיר כולל מעמ " + order.TransportPrice.ToString() + "    לפני מעמ: " + (order.TransportPrice / 1.17m).ToString("#.##");
            orderData.Add(dataFormatted);


        }

        private void extractDataOverDateRange()
        {
            _rTxtBxPrintOut.Document.Blocks.Clear();
            _printerPrintLines.Clear();
            if (_dtPkrPrintFrom.SelectedDate == null || _dtPkrPrintTo.SelectedDate == null)
            {
                return;
            }


            DateTime dt1 = (DateTime)_dtPkrPrintFrom.SelectedDate;
            DateTime dt2 = (DateTime)_dtPkrPrintTo.SelectedDate;

            if (dt1.Ticks <= dt2.Ticks)
            {
                List<Trucking> orders = new List<Trucking>();
                int idCard;
                if (int.TryParse(_txtBxPrintCustomerID.Text, out int val))
                {
                    idCard = val;
                }
                else // Incorrect customer ID
                {
                    return;
                }

                if(_theDatabase.getCustomersOrders(orders, idCard) == false)
                {
                    return;
                }

                List<Trucking> sortedList = orders.OrderBy(o => o.DateAsTickCount).ToList(); // This uses Linq
                int startIdx = -1, endIdx;  // Get the indices that match the date range

                // Get start index
                for (int i = 0; i < sortedList.Count; i++)
                {
                    if (sortedList[i].DateAsTickCount >= dt1.Ticks)
                    {
                        startIdx = i;
                        break;
                    }
                }
                if (startIdx == -1)
                {
                    printOutMessage("No orders exist in this range");
                    return;
                }


                // Is the startIdx date greater than the end date?  If so, it's not in range
                if (sortedList[startIdx].DateAsTickCount > dt2.Ticks)
                {
                    printOutMessage("No orders exist in this range");
                    return;
                }


                endIdx = startIdx;
                // Get end index
                for (int i = startIdx; i < sortedList.Count; i++)
                {
                    if (sortedList[i].DateAsTickCount <= dt2.Ticks)
                    {
                        endIdx = i;
                    }
                }
                string printBoxData = " שם לקוח: " + _txtBxPrintCustomerName.Text + ",  תעודת זהות: " + _txtBxPrintCustomerID.Text;
                _printerPrintLines.Add(printBoxData);
                printBoxData += "\n\n";
                Decimal totalCost = 0.0m;
                // Now print out data over the range startIdx to endIdx
                for (int i = startIdx; i <= endIdx; i++)
                {
                    // For richtextbox output
                    printBoxData += formatOrderData(sortedList[i]);
                    totalCost += sortedList[i].TransportPrice;

                    // For printing to printer
                    getFormatOrderDataLines(sortedList[i], _printerPrintLines);
                }
                printBoxData += "\n";
                printBoxData += "        סך הכל כולל מעמ:   " + totalCost.ToString() + "     סך הכל : " + BeforeVAT(totalCost).ToString("#.##") + "      מעמ:" + (totalCost * 0.17m).ToString("#.##") + "\n";

                _rTxtBxPrintOut.Document.Blocks.Add(new Paragraph(new Run(printBoxData)));

                //For printer
                _printerPrintLines.Add("        סך הכל כולל מעמ:  " + totalCost.ToString() + "      סך הכל: " + BeforeVAT(totalCost).ToString("#.#") + "     מעמ:" + (totalCost * 0.17m).ToString("#.##"));


            }
            else
            {
                printOutMessage("Invalid Date Range");
            }

        }

        private void _dtPkrPrintFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //extractDataOverDateRange();
        }

        private void _dtPkrPrintTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //extractDataOverDateRange();
        }

        private void _dtPkrPrintFrom_CalendarClosed(object sender, RoutedEventArgs e)
        {
            extractDataOverDateRange();
        }

        private void _dtPkrPrintTo_CalendarClosed(object sender, RoutedEventArgs e)
        {
            extractDataOverDateRange();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_languageEnglish == true)
                {
                    setLanguageFlowDirection(FlowDirection.RightToLeft);
                    _languageEnglish = false;
                }
                else
                {
                    setLanguageFlowDirection(FlowDirection.LeftToRight);
                    _languageEnglish = true;
                }
            }
        }
    }
}
