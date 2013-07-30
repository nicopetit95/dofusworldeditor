using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebSite
{
    public partial class Account : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] == null || Session["pass"] == null)
                Response.Redirect("Login.aspx");
            else
            {
                if (!Utilities.Managers.AccountsManager.isValidAccount(Session["user"].ToString(), Session["pass"].ToString()))
                {
                    Session["user"] = null;
                    Session["pass"] = null;

                    Response.Redirect("Login.aspx");
                }
            }
        }
    }
}