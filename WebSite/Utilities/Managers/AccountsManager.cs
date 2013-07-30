using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSite.Utilities.Managers
{
    public class AccountsManager
    {
        public static bool isAdmin(string user)
        {
            return false;
        }

        public static bool isValidAccount(string user, string pass)
        {
            return false;
        }

        public static bool isValidKey(string[] key)
        {
            return true;
        }

        public static string GetUser(string[] key)
        {
            return "Ghost|trololol";
        }
    }
}