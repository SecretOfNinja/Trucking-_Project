using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using TruckingServices.Models;
namespace TruckingServices.database
{
    public class Database
    {
        string _connectionString;

        public Database()
        {

            _connectionString = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

        }

        public Database(string connectionString)
        {
        }

        // Log errors to file "trucking.log"
        private void logLineToFile(string data)
        {
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            StreamWriter? truckingWriter = null;
            truckingWriter = new StreamWriter("trucking.log", true);
            string strOut = "\nDate: " + dt.ToLongDateString() + ", Time: " + dt.ToShortTimeString() + ":\n " + data;
            truckingWriter.WriteLine(strOut);
            truckingWriter.Close();
        }

        // Get all of the IdCards in ascending order and pass them back
        //  in the List<int> parameter IdCards
        // Returns true if successful, else false
        public bool getCustomerIDCards(List<int> IdCards, bool unitTest = false)
        {
            bool result = true;

            string connectionString;
            SqlDataReader dataReader;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                transaction = connection.BeginTransaction("getCustomer");
                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Select IDCard From TestCustomerDetails Order By IDCard Ascending";
                    }
                    else
                    {
                        command.CommandText = "Select IDCard From CustomerDetails Order By IDCard Ascending";
                    }

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        Customer customer = new Customer();
                        int idCard;
                        idCard = (int)dataReader.GetValue(0);
                        IdCards.Add(idCard);

                    }
                    dataReader.Close();
                    command.Dispose();

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In getCustomerIDCards: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));
                    // Attempt to roll back the transaction.
                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Gets all customers from CustomerDetails_Table table and passes them back in the customers list
        // Returns true if successful, else false
        public bool getCustomers(List<Customer> customers, bool unitTest = false)
        {
            bool result = true;
            int idCard;
            string customerName, companyName, area, city, phoneNumber;

            string connectionString;
            SqlDataReader dataReader;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                transaction = connection.BeginTransaction("getCustomer");
                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Select * From TestCustomerDetails";
                    }
                    else
                    {
                        command.CommandText = "Select * From CustomerDetails";
                    }

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        Customer customer = new Customer();

                        idCard = (int)dataReader.GetValue(0);
                        customerName = (string)dataReader.GetValue(1);
                        companyName = (string)dataReader.GetValue(2);
                        //idCard = (string)dataReader.GetValue(3);
                        area = (string)dataReader.GetValue(3);
                        city = (string)dataReader.GetValue(4);
                        phoneNumber = (string)dataReader.GetValue(5);


                        customer.IDCard = idCard;
                        customer.CustomerName = customerName;
                        customer.CompanyName = companyName;
                        //customer.IDCard = idCard;
                        customer.Area = area;
                        customer.City = city;
                        customer.PhoneNumber = phoneNumber;
                        customers.Add(customer);

                    }
                    dataReader.Close();
                    command.Dispose();

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In getCustomer: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));
                    // Attempt to roll back the transaction.
                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Adds the passed in customer to the CustomerDetails_Table, and acquires the new customer ID
        // Returns true if successful, else false
        public bool addCustomer(Customer customer, bool unitTest = false)
        {
            bool result = true;
            string connectionString;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();

                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("addCustomer");


                command.Connection = connection;
                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText =
                            "Insert into TestCustomerDetails (IDCard, CustomerName, CompanyName, Area, City, PhoneNumber) VALUES (@IdCard, @CustomerName,@CompanyName,@Area,@City,@PhoneNumber)";
                    }
                    else
                    {
                        command.CommandText =
                            "Insert into CustomerDetails (IDCard, CustomerName, CompanyName, Area, City, PhoneNumber) VALUES (@IdCard, @CustomerName,@CompanyName,@Area,@City,@PhoneNumber)";
                    }

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@IdCard", customer.IDCard);
                    command.Parameters.AddWithValue("@CustomerName", customer.CustomerName);
                    command.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
                    command.Parameters.AddWithValue("@Area", customer.Area);
                    command.Parameters.AddWithValue("@City", customer.City);
                    command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In addCustomer: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));
                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.                        
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }


        // Updates the customer passed in to the CustomerDetails_Table table
        // if the existingIDCard value is different from the customer.IDCard value, this means the IDCard value is being changed
        // Returns true if successful, else false
        public bool updateCustomer(Customer customer, int existingIDCard = -1, bool unitTest = false)
        {
            bool result = true;
            string connectionString;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("updateCustomer");

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;


                    if (unitTest)
                    {
                        command.CommandText =
                            "Update TestCustomerDetails Set CustomerName=@customerName, CompanyName=@companyName, IdCard=@idCardNew, Area=@area, City=@city, PhoneNumber=@phoneNumber Where IdCard=@idCard";
                    }
                    else
                    {
                        command.CommandText =
                           "Update CustomerDetails Set CustomerName=@customerName, CompanyName=@companyName, IdCard=@idCardNew, Area=@area, City=@city, PhoneNumber=@phoneNumber Where IdCard=@idCard";
                    }

                    //int ID, string CustomerName, string CompanyName, string IdCard, string Area, string City, string PhoneNumber)
                    command.Parameters.Add("@idCardNew", SqlDbType.Int);
                    command.Parameters.Add("@idCard", SqlDbType.Int);
                    command.Parameters.Add("@customerName", SqlDbType.NVarChar);
                    command.Parameters.Add("@companyName", SqlDbType.NVarChar);
                    command.Parameters.Add("@area", SqlDbType.NVarChar);
                    command.Parameters.Add("@city", SqlDbType.NVarChar);
                    command.Parameters.Add("@phoneNumber", SqlDbType.NVarChar);


                    command.Parameters["@idCardNew"].Value = customer.IDCard; // This may not be a new card, but is if customer.IDCard != existingIDCard
                    command.Parameters["@idCard"].Value = existingIDCard;
                    command.Parameters["@customerName"].Value = customer.CustomerName;
                    command.Parameters["@companyName"].Value = customer.CompanyName;
                    command.Parameters["@area"].Value = customer.Area;
                    command.Parameters["@city"].Value = customer.City;
                    command.Parameters["@phoneNumber"].Value = customer.PhoneNumber;


                    command.ExecuteNonQuery();

                    transaction.Commit();
                    // logLineToFile("Selection from managers table worked.");
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In updateCustomer: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.                        
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Deletes the customer from the CustomerDetails table using the customer's IDCARD passed in via iDCard.
        // Returns true if successful, else false
        public bool deleteCustomer(int iDCard, bool unitTest = false)
        {
            bool result = true;
            string connectionString;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("deleteCustomer");

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Delete From TestCustomerDetails Where IDCard=@idCard";
                    }
                    else
                    {
                        command.CommandText = "Delete From CustomerDetails Where IDCard=@idCard";
                    }

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idCard", iDCard);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In deleteCustomer: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Deletes all customers from the TestCustomerDetails table. Only using for test.
        // I force unitTest true to make sure all orders aren't deleted inadvertantly.
        // Returns true if successful, else false
        public bool deleteAllCustomers(bool unitTest = false)
        {
            bool result = true;
            string connectionString;
            unitTest = true; // Force unitTest true, because this function is only meant for unit testing

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("deleteAllCustomers");

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Delete From TestCustomerDetails";
                    }
                    else
                    {
                        //command.CommandText = "Delete From CustomerDetails";
                    }

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In deleteAllCustomers: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Adds an order to the table TruckingOrders. The order is passed in via parameter order.   The
        // order contains all of the necessary information, such as the times and IDCard.
        // The orderID is determine during this transaction and placed in the order.
        // Returns true if successful, else false
        public bool addCustomerOrder(Trucking order, bool unitTest = false)
        {
            bool result = true;
            string connectionString;
            connectionString = _connectionString;
            SqlDataReader dataReader;
            int orderId = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();

                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("addCustomerOrder");


                command.Connection = connection;
                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;
                    string theDate;

                    if (unitTest)
                    {
                        command.CommandText =
                            "Insert into TestTruckingOrders (IDCard, TransportDate, TransportPrice, StartingPoint, Destination, ShippingWeight, NumberOfPallets) VALUES (@IDCard,@TransportDate,@TransportPrice,@StartingPoint,@Destination,@ShippingWeight,@NumberOfPallets)";
                    }
                    else
                    {
                        command.CommandText =
                            "Insert into TruckingOrders (IDCard, TransportDate, TransportPrice, StartingPoint, Destination, ShippingWeight, NumberOfPallets) VALUES (@IDCard,@TransportDate,@TransportPrice,@StartingPoint,@Destination,@ShippingWeight,@NumberOfPallets)";
                    }

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@IDCard", order.IDCard);
                    theDate = convertDateTimeToString(order.TransportDate);
                    command.Parameters.AddWithValue("@TransportDate", theDate);
                    command.Parameters.AddWithValue("@TransportPrice", order.TransportPrice);
                    command.Parameters.AddWithValue("@StartingPoint", order.StartingPoint);
                    command.Parameters.AddWithValue("@Destination", order.Destination);
                    command.Parameters.AddWithValue("@ShippingWeight", order.PriceOfPallets);
                    command.Parameters.AddWithValue("@NumberOfPallets", order.NumberOfPallets);
                    command.ExecuteNonQuery();

                    // Next, get the ID of the customer just added.
                    command = connection.CreateCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "select MAX(OrderID) from TestTruckingOrders";
                    }
                    else
                    {
                        command.CommandText = "select MAX(OrderID) from TruckingOrders";
                    }

                    command.CommandType = CommandType.Text;

                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        orderId = (int)dataReader.GetValue(0);
                    }
                    order.OrderID = orderId;
                    dataReader.Close();
                    command.Dispose();

                    transaction.Commit();

                    // MessageBox.Show("Selection from CustomerDetails_Table  worked.");
                }
                catch (Exception ex)
                {
                    result = false;

                    logLineToFile(String.Format("In addCustomerOrder: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Updates the customer passed in to the CustomerDetails_Table table
        // Returns true if successful, else false
        public bool updateCustomerOrder(Trucking order, bool unitTest = false)
        {
            bool result = true;
            string connectionString;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("updateCustomerOrder");

                try
                {
                    string theDate;
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    //IDCard, TransportDate, TransportPrice, StartingPoint, Destination, ShippingWeight, NumberOfPallets
                    if (unitTest)
                    {
                        command.CommandText =
                            "Update TestTruckingOrders Set IDCard=@idCard, TransportDate=@transportDate, TransportPrice=@transportPrice, StartingPoint=@startingPoint, Destination=@destination, ShippingWeight=@shippingWeight, NumberOfPallets=@numberOfPallets Where OrderID=@orderId";
                    }
                    else
                    {
                        command.CommandText =
                            "Update TruckingOrders Set IDCard=@idCard, TransportDate=@transportDate, TransportPrice=@transportPrice, StartingPoint=@startingPoint, Destination=@destination, ShippingWeight=@shippingWeight, NumberOfPallets=@numberOfPallets Where OrderID=@orderId";
                    }

                    command.Parameters.Add("@idCard", SqlDbType.Int);
                    command.Parameters.Add("@transportDate", SqlDbType.NVarChar);
                    command.Parameters.Add("@transportPrice", SqlDbType.Decimal);
                    command.Parameters.Add("@startingPoint", SqlDbType.NVarChar);
                    command.Parameters.Add("@destination", SqlDbType.NVarChar);
                    command.Parameters.Add("@shippingWeight", SqlDbType.Decimal);
                    command.Parameters.Add("@numberOfPallets", SqlDbType.Int);
                    command.Parameters.Add("@orderId", SqlDbType.Int);

                    command.Parameters["@idCard"].Value = order.IDCard;
                    theDate = convertDateTimeToString(order.TransportDate);
                    command.Parameters["@transportDate"].Value = theDate;
                    command.Parameters["@transportPrice"].Value = order.TransportPrice;
                    command.Parameters["@startingPoint"].Value = order.StartingPoint;
                    command.Parameters["@destination"].Value = order.Destination;
                    command.Parameters["@shippingWeight"].Value = order.PriceOfPallets;
                    command.Parameters["@numberOfPallets"].Value = order.NumberOfPallets;
                    command.Parameters["@orderId"].Value = order.OrderID;

                    command.ExecuteNonQuery();

                    transaction.Commit();
                    // logLineToFile("Selection from managers table worked.");
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In updateCustomerOrder: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // string will contain three numbers representing 
        // day, month and year in that order separated by a space
        private DateTime convertStringToDateTime(string date)
        {
            string[] theDate = date.Split(' ');
            int day, month, year;

            day = int.Parse(theDate[0]);
            month = int.Parse(theDate[1]);
            year = int.Parse(theDate[2]);
            DateTime dt = new DateTime(year, month, day);
            return dt;
        }

        private string convertDateTimeToString(DateTime? dtm)
        {
            if (dtm == null)
                return "";
            DateTime dt = (DateTime)dtm;
            string date = dt.Day.ToString() + " " + dt.Month.ToString() + " " + dt.Year.ToString();
            return date;
        }

        // Gets all orders for a customer, and passes it back as a list
        // The customer's ID is passed in.
        // Returns true if successful, else false
        public bool getCustomersOrders(List<Trucking> orders, int idCard, bool unitTest = false)
        {
            bool result = true;

            string connectionString;
            SqlDataReader dataReader;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;


                transaction = connection.BeginTransaction("getCustomersOrder");
                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Select * From TestTruckingOrders where IDCard=@idCard";
                    }
                    else
                    {
                        command.CommandText = "Select * From TruckingOrders where IDCard=@idCard";
                    }

                    command.Parameters.AddWithValue("@idCard", idCard);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        Trucking order = new Trucking();
                        decimal decVal;
                        string theDate;

                        order.OrderID = (int)dataReader.GetValue(0);
                        order.IDCard = (int)dataReader.GetValue(1);
                        theDate = (string)dataReader.GetValue(2);

                        order.TransportDate = convertStringToDateTime(theDate);
                        order.TransportPrice = (decimal)dataReader.GetValue(3);
                        order.StartingPoint = (string)dataReader.GetValue(4);
                        order.Destination = (string)dataReader.GetValue(5);
                        decVal = (decimal)dataReader.GetValue(6);
                        order.PriceOfPallets = Decimal.ToDouble(decVal);
                        order.NumberOfPallets = (int)dataReader.GetValue(7);

                        orders.Add(order);
                    }
                    dataReader.Close();
                    command.Dispose();

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In getCustomerOrders: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));
                    // Attempt to roll back the transaction.
                    try
                    {
                        // result = false;
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Deletes the order specified by the orderId from the TruckingOrders table.
        // Returns true if successful, else false
        public bool deleteOrder(int orderId, bool unitTest = false)
        {
            bool result = true;
            string connectionString;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("deleteCustomer");

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Delete From TestTruckingOrders Where OrderID=@orderId";
                    }
                    else
                    {
                        command.CommandText = "Delete From TruckingOrders Where OrderID=@orderId";
                    }

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@orderId", orderId);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In deleteOrder: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Deletes all orders of a particular customer.  Pass in the cusomter's IdCard.
        // Returns true if successful, else false
        public bool deleteAllCustomersOrders(int IdCard, bool unitTest = false)
        {
            bool result = true;
            string connectionString;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("deleteAllCustomersOrders");

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Delete From TestTruckingOrders Where IDCard=@IdCard";
                    }
                    else
                    {
                        command.CommandText = "Delete From TruckingOrders Where IDCard=@IdCard";
                    }

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@IdCard", IdCard);
                    command.ExecuteNonQuery();


                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In deleteAllCustomersOrders: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }

        // Deletes all orders from the table.  I force unitTest true to make sure all orders aren't deleted inadvertantly.
        // Returns true if successful, else false
        public bool deleteAllOrders(bool unitTest = false)
        {
            bool result = true;
            string connectionString;

            unitTest = true; // Force unitTest true, because this function is only meant for unit testing

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("deleteAllOrders");

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    command.Connection = connection;
                    command.Transaction = transaction;

                    if (unitTest)
                    {
                        command.CommandText = "Delete From TestTruckingOrders";
                    }
                    else
                    {
                        //command.CommandText = "Delete From TruckingOrders";
                    }

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();


                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    logLineToFile(String.Format("In deleteAllOrders: Commit Exception Type: {0}\n Message: {1}", ex.GetType(), ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        logLineToFile(String.Format("Rollback Exception Type: {0}\n Message: {1}", ex2.GetType(), ex2.Message));
                    }
                }
            }
            return result;
        }
    }
}