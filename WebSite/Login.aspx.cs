using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebSite
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] != null && Session["pass"] != null)
            {
                if (Utilities.Managers.AccountsManager.isValidAccount(Session["user"].ToString(), Session["pass"].ToString()))
                    Response.Redirect("Account.aspx");
                else
                {
                    Session["user"] = null;
                    Session["pass"] = null;
                }
            }
        }

        protected void connectwithinfos_Click(object sender, EventArgs e)
        {
            if (Utilities.Managers.AccountsManager.isValidAccount(username.Text, password.Text))
            {
                Session["user"] = username.Text;
                Session["pass"] = password.Text;

                Response.Redirect("Account.aspx");
            }
        }

        protected void connectwithkey_Click(object sender, EventArgs e)
        {
            if (Utilities.Managers.AccountsManager.isValidKey(new string[] { licence1.Text, licence2.Text, licence3.Text }))
            {
                var infos = Utilities.Managers.AccountsManager.GetUser(new string[] { licence1.Text, licence2.Text, licence3.Text }).Split('|');

                Session["user"] = infos[0];
                Session["pass"] = infos[1];

                Response.Redirect("Account.aspx");
            }
        }
    }
}