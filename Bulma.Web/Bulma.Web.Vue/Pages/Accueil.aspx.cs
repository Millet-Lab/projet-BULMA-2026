using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bulma.Web.Vue.Pages
{
    public partial class Accueil : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // affichage seulement : on choisit quel panneau montrer selon la session
            bool connecte = Session["idClient"] != null;
            pnlVisiteur.Visible = !connecte;
            pnlClient.Visible = connecte;
        }
    }
}