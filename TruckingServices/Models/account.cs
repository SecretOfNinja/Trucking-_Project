using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TruckingServices.Models
{
    public class account
    {
        private string Id;
        private string Username;
        private string Email;
        private string Password;
        private string CnfPassword;

        public string getId()
        {
            return Id;
        }
        public void setId(string id)
        {
            Id = id;
        }
        public string getUsername()
        {
            return Username;
        }

        public void setUsername(string username)
        {
            Username = username;
        }

        public string getEmail()
        {
            return Email;
        }

        public void setEmail(string email)
        {
            Email = email;
        }

        public string getPassword()
        {
            return Password;
        }

        public void setPassword(string password)
        {
            Password = password;
        }

        public string getCnfPassword()
        {
            return CnfPassword;
        }

        public void setCnfPassword(string cnfpassword)
        {
            CnfPassword = cnfpassword;
        }

        public account()
        {
            Id = "";
            Username = "";
            Email = "";
            Password = "";
            CnfPassword = "";
        }

        public account nameOfPersonCopyThis()
        {
            return this;
        }
    }
}

