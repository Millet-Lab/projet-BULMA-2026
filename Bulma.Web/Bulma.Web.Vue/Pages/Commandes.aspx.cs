using Bulma.Web.Modele.CoucheAccesBD;
using Bulma.Web.Presentation.Affichages;
using Bulma.Web.Presentation.Interfaces;
using Bulma.Web.Presentation.Presenteur;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bulma.Web.Vue.Pages
{
    public partial class Commandes : PageClient, ICommandesVue
    {
        private AccesBD _bd;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["idClient"] == null)
            {
                Response.Redirect("Connexion.aspx", true);
                return;
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            _bd = new AccesBD(ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString);

            if (!IsPostBack)
            {
                new CommandesPresenteur(this, _bd, IdClientConnecte).Charger();
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (_bd != null)
            {
                _bd.Dispose();
            }
        }

        public void AfficherCommandes(List<CommandeAffichage> commandes)
        {
            gvCommandes.DataSource = commandes;
            gvCommandes.DataBind();
        }

        public void AfficherErreur(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}