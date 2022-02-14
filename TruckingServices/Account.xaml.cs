using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using TruckingServices.Models;


namespace TruckingServices
{
    /// <summary>
    /// Interaction logic for Account.xaml
    /// </summary>
    public partial class Account : Window
    {
        string _connectionString;

        


        public int count_Cust = 0;

        public List<account> _account = new List<account>();
        public Account()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
            InitializeComponent();
            loadManagersFromDatabase();

        }



        private void LoginButton(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            string user_name;
            string password;

            foreach (account manager in _account)
            {
                user_name = manager.getUsername();
                password = manager.getPassword();

                if (user_name == User_name.Text && password == User_Passwordbox.Password)
                {
                    mainWindow.Show();
                    Close();
                }
            }
        }




        private void Close_ProgramBtn(object sender, RoutedEventArgs e)
        {

        }

      





        private void loadManagersFromDatabase()
        {
            string Username, Email, Password;

            string connectionString;
            SqlDataReader dataReader;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "SelectManagers");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From AccountValidity_Table";

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {

                        account acc = new account();

                        Username = (string)dataReader.GetValue(0);
                        Email = (string)dataReader.GetValue(1);
                        Password = (string)dataReader.GetValue(2);

                        acc.setUsername(Username);
                        acc.setEmail(Email);
                        acc.setPassword(Password);
                        _account.Add(acc);

                    }

                    dataReader.Close();
                    command.Dispose();

                    transaction.Commit();
                   // MessageBox.Show("Selection from managers table worked.");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

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
                        Console.WriteLine("Rollback Exception Type: {0 }", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }

            if (_account.Count > 0)
            {
                Buttonhides.Visibility = Visibility.Hidden;
            }
            else
            {
                Buttonhides.Visibility = Visibility.Visible;
            }
        }








        // Register---------------------------


        // Checking if the Email have the request
        private bool validateEmail(string email)
        {
            //
            if (email.Length < 5)
            {
                //Email has to have a pattern like chr_@_chr_.chr, at minimum 5 characters
                return false;
            }

            string[] parts = email.Split('@');
            if (parts.Length != 2)
            {
                return false;
            }
            int pos = parts[1].IndexOf('.');
            if (pos == 0 || pos == parts[1].Length - 1)
            {
                return false;
            }

            return true;
        }



        //--------------------------------------------

        // Creating account
        private void BtnCreateAccount(object sender, RoutedEventArgs e)
        {
            string error = "";
            //Make sure that the information is correct
            if (txtbxName.Text.Length == 0)
            {
                error = "No First Name Entered";
            }
            if (validateEmail(txtbxEmail.Text) == false)
            {
                error = "Invalid Email Address";
            }
            else if (txtbxPassword.Password.Length == 0)
            {
                error = "No Passwored Entered";
            }
            else if (txtbxPasswordConfirm.Password.Length == 0)
            {
                error = "No Confirmation Password Entered";
            }
            if (error != "")
            {
                MessageBox.Show(error, "try again");
                return;
            }

            else
            {
                //confirm password
                if (txtbxPassword.Password.Length < 8)
                {
                    MessageBox.Show(error, "Password must be at lest 8 characters long");

                    return;
                }
                if (txtbxPassword.Password != txtbxPasswordConfirm.Password)
                {
                    MessageBox.Show(error, "Invaid password confirmation");

                    return;
                }
            }



            account acc = new account();
            acc.setUsername(txtbxName.Text);
            acc.setEmail(txtbxEmail.Text);
            acc.setPassword(txtbxPassword.Password);
            acc.setCnfPassword(txtbxPasswordConfirm.Password);
            addNewManagerAccountToDatabase(acc.getUsername(), acc.getEmail(), acc.getPassword(), acc.getCnfPassword());
            _account.Add(acc);
            RegisterBorder.Visibility = Visibility.Hidden;
            LoginBorder.Visibility = Visibility.Visible;
            Buttonhides.Visibility = Visibility.Hidden;

        }


        private void addNewManagerAccountToDatabase(string Username, string Email, string Password, string CnfPassword)
        {
            string connectionString;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "InsertManager");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {

                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText =
                    "Insert into AccountValidity_Table (Username,Email, password, cnfpassword) VALUES (@Username,@Email,@password,@cnfpassword)";


                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Username", Username);
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@password", Password);
                    command.Parameters.AddWithValue("@cnfpassword", CnfPassword);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                   // MessageBox.Show("Selection from AccountValidity table worked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

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
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }





        private void To_Login(object sender, RoutedEventArgs e)
        {
            LoginBorder.Visibility = Visibility.Visible;
            RegisterBorder.Visibility = Visibility.Hidden;
         
        }

        private void To_register(object sender, RoutedEventArgs e)
        {
            RegisterBorder.Visibility = Visibility.Visible;
            LoginBorder.Visibility = Visibility.Hidden;
        }

        private void Begin()
        {

        }
    }
}