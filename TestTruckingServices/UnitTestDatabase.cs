using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TruckingServices;
using System.Configuration;
using System.Xml;
using System.IO;
using TruckingServices.Models;
using TruckingServices.database;
namespace TestTruckingServices
{
    public class UnitTestDatabase : IDisposable
    {
        // Set up the connection string for your local database here
        string? _connectionString = "";
        public UnitTestDatabase()
        {
            initializePopulateDatabase();
        }

        // This accesses the App.config file for TruckingServices and extracts the connection string.
        // This is a kludgy way to get the connection string
        private string getAppDataFromTruckingServices()
        {
            string connStr = @"Data Source=LAPTOP-CMOUV9KC\SQLEXPRESS;Initial Catalog=Trucking;Persist Security Info=True;User ID=Tracking_Trucks;Password=waswas123";
            // string value;
            //bool result = File.Exists(@"..\..\..\..\TruckingServices\App.config");
            XmlReader xmlReader = XmlReader.Create(@"..\..\..\..\TruckingServices\App.config");
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "add"))
                {
                    if (xmlReader.HasAttributes)
                    {
                        //value = xmlReader.GetAttribute("name");
                        connStr = xmlReader.GetAttribute("connectionString");
                        break;
                    }
                }
            }
            return connStr;
        }

        // Make sure data is in the database to start with and that it's the data used for testing
        private void initializePopulateDatabase()
        {
            _connectionString = getAppDataFromTruckingServices();
            Database db = new Database(_connectionString);

            //clear out the database so that the data can be added anew
            db.deleteAllCustomers(true);

            Customer c1 = new Customer();
            Customer c2 = new Customer();
            Customer c3 = new Customer();
            c1.setValues(1, "Doc Savage", "Hidalgo Trading Co", "USA", "New York", "111 222 3333");
            c2.setValues(2, "Superman", "Fortress Of Solitude", "USA", "New York", "333 222 1111");
            c3.setValues(3, "Zaphod Bebblebrox", "Zilly Wig", "Andromeda", "Doubleheaded", "414 314 6372");

            // Add three customers to database
            db.addCustomer(c1, true);
            db.addCustomer(c2, true);
            db.addCustomer(c3, true);
        }

        [Fact]
        public void getCustomers_ShouldReturnProperCount()
        {
            int expectedCount = 3;

            Database db = new Database(_connectionString);
            //z1 = db.add(2, 2);
            List<Customer> customers = new List<Customer>();
            db.getCustomers(customers, true);

            int actualCount = customers.Count;

            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void getCustomerIDCards_ShouldBeInAscendingOrder()
        {
            List<int> idCards = new List<int>();
            bool result = true;
            bool expected = true;

            Database db = new Database(_connectionString);

            db.getCustomerIDCards(idCards, true);

            //Make sure the order is ascending.  result == false if it is not, else true
            for (int i = 0; i < idCards.Count - 1; i++)
            {
                if (idCards[i] > idCards[i + 1])
                {
                    result = false;
                    break;
                }
            }

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(1, false)]
        public void addCustomer_AddingCustomer(int idCard, bool expected)
        {
            Customer cst = new Customer();
            cst.setValues(idCard, "Under Dog", "Dog House", "The House", "Back Yard", "123 321 7734");
            Database db = new Database(_connectionString);
            bool actual = db.addCustomer(cst, true);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void updateCustomer_DoesACustomerUpdate()
        {
            bool actual = false;
            bool expected = true;
            List<Customer> lstCstms = new List<Customer>();

            Database db = new Database(_connectionString);
            // Get customer list
            db.getCustomers(lstCstms, true);

            //Append the City string
            string City = lstCstms[0].City;
            City += " City";
            lstCstms[0].City = City;

            //Update this value to the database
            db.updateCustomer(lstCstms[0], lstCstms[0].IDCard, true);


            // Get the customer list again
            lstCstms.Clear(); //Empty the list
            db.getCustomers(lstCstms, true);

            if (lstCstms[0].City == City)
            {
                actual = true;
            }
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void deleteCustomer_DeleteingCustomer()
        {
            bool expected = true;
            bool actual = true;

            List<Customer> lstCstms = new List<Customer>();

            Database db = new Database(_connectionString);
            Customer cst = new Customer();
            cst.setValues(123, "Under Dog", "Dog House", "The House", "Back Yard", "123 321 7734");
            db.addCustomer(cst, true);

            actual = db.deleteCustomer(123, true);

            if (actual)
            {
                db.getCustomers(lstCstms, true);

                for (int i = 0; i < lstCstms.Count; i++)
                {
                    // Should not find this IDCard, since the customer was deleted
                    if (lstCstms[i].IDCard == 123)
                    {
                        actual = false;
                    }
                }

            }
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void addCustomerOrder_AddingOrder()
        {
            List<Trucking> orders = new List<Trucking>();
            Trucking order = new Trucking();
            DateTime dt = new DateTime();
            bool result;
            bool expected = true;

            Database db = new Database(_connectionString);

            order.setValues(1, 1, dt, 123.45m, "Start Here", "End There", 987.65, 4);
            result = db.addCustomerOrder(order, true);

            db.getCustomersOrders(orders, 1, true);
            if (result)
            {
                bool foundOrder = false;
                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i].IDCard == 1 && orders[i].OrderID == order.OrderID)
                    {
                        foundOrder = true;
                        break;
                    }
                }
                result = foundOrder;
            }
            Assert.Equal(expected, result);
        }


        [Fact]
        public void updateCustomerOrder_UpdatingOrder()
        {
            bool result, expected = true;
            List<Trucking> orders = new List<Trucking>();
            Trucking order = new Trucking();
            DateTime dt = new DateTime();
            Database db = new Database(_connectionString);
            string startingPoint;
            int acquiredOrderId;

            // Clear all orders from the table
            db.deleteAllOrders(true);
            //db.getCustomersOrders()
            // Add to orders to test with
            order.setValues(2, 2, dt, 222.22m, "North Pole", "South Pole", 87.53, 2);
            db.addCustomerOrder(order, true);
            order.setValues(3, 2, dt, 111.11m, "East Coast", "South Pole", 22.32, 8);
            db.addCustomerOrder(order, true);

            acquiredOrderId = order.OrderID; // The OrderID is created by the database

            // Change order startingPoint from "East Coast" to "Godzilla Point", then update it in the database
            startingPoint = "Godzilla Point";
            order.StartingPoint = startingPoint;

            result = db.updateCustomerOrder(order, true);

            if (result)
            {
                result = false;
                //Check for results
                orders.Clear();
                db.getCustomersOrders(orders, 2, true);
                for (int i = 0; i < orders.Count; i++)
                {
                    // We should find the order and its updated value
                    if (orders[i].StartingPoint == startingPoint && orders[i].OrderID == acquiredOrderId && orders[i].IDCard == 2)
                    {
                        result = true;
                    }
                }
            }

            Assert.Equal(expected, result);
        }

        [Fact]
        public void getCustomersOrders_returnListOfCustomerOrders()
        {
            int expected = 2;
            List<Trucking> orders = new List<Trucking>();
            Trucking order = new Trucking();
            DateTime dt = new DateTime();
            Database db = new Database(_connectionString);

            // Clear all orders from the table
            db.deleteAllOrders(true);
            //db.getCustomersOrders()
            // Add to orders to test with
            order.setValues(2, 2, dt, 222.22m, "North Pole", "South Pole", 87.53, 2);
            db.addCustomerOrder(order, true);
            order.setValues(3, 2, dt, 111.11m, "East Coast", "South Pole", 22.32, 8);
            db.addCustomerOrder(order, true);

            // Now get the Customer's orders
            db.getCustomersOrders(orders, 2, true);

            //There should be two orders
            Assert.Equal(expected, orders.Count);
        }

        [Fact]
        public void deleteOrder_DeletingCustomerOrder()
        {
            bool expected = true, result;
            int acquiredOrderId;

            List<Trucking> orders = new List<Trucking>();
            Trucking order = new Trucking();
            DateTime dt = new DateTime();
            Database db = new Database(_connectionString);

            // Clear all orders from the table
            db.deleteAllOrders(true);
            order.setValues(2, 2, dt, 222.22m, "North Pole", "South Pole", 87.53, 2);
            db.addCustomerOrder(order, true);
            order.setValues(3, 2, dt, 111.11m, "East Coast", "South Pole", 22.32, 8);
            db.addCustomerOrder(order, true);
            acquiredOrderId = order.OrderID; // The OrderID is created by the database

            result = db.deleteOrder(acquiredOrderId, true);
            if (result)
            {
                result = false;
                // After deleting one of the two customer's orders, the result should be one order
                db.getCustomersOrders(orders, 2, true);
                if (orders.Count == 1)
                {
                    result = true;
                }
            }

            Assert.Equal(expected, result);
        }

        [Fact]
        public void deleteAllCustomersOrders_DeletingAllOrders()
        {
            bool expected = true, result;
            List<Trucking> orders = new List<Trucking>();
            Trucking order = new Trucking();
            DateTime dt = new DateTime();
            Database db = new Database(_connectionString);
            // Clear all orders from the table
            db.deleteAllOrders(true);
            order.setValues(2, 2, dt, 222.22m, "North Pole", "South Pole", 87.53, 2);
            db.addCustomerOrder(order, true);
            order.setValues(3, 2, dt, 111.11m, "East Coast", "South Pole", 22.32, 8);
            db.addCustomerOrder(order, true);
            order.setValues(3, 3, dt, 645.67m, "Tylyn House", "John House", 222.32, 20);
            db.addCustomerOrder(order, true);

            result = db.deleteAllCustomersOrders(2, true);
            if (result)
            {
                result = false;
                // After deleting all the customer's orders, the result should be zero orders
                db.getCustomersOrders(orders, 2, true);
                if (orders.Count == 0)
                {
                    result = true;
                }
            }

            Assert.Equal(expected, result);
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
