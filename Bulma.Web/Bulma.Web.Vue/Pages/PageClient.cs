using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Bulma.Web.Vue.Pages
{
    public class PageClient : Page
    {
        protected int IdClientConnecte
        {
            get { return (int)Session["idClient"]; }
        }

        protected override void OnInit(EventArgs e)
        {
            if (Session["idClient"] == null)
            {
                Response.Redirect("Connexion.aspx", true);   // termine la requête
            }

            base.OnInit(e);
        }
    }
}