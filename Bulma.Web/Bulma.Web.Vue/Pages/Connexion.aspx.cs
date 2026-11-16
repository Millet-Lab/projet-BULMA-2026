using Bulma.Web.Modele.CoucheAccesBD;
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
    public partial class Connexion : Page, IConnexionVue
    {
        private AccesBD _bd;
        private ConnexionPresenteur _presenter;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["idClient"] != null)
            {
                Response.Redirect("Catalogue.aspx", true);
                return;
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            _bd = new AccesBD(ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString);
            _presenter = new ConnexionPresenteur(this, _bd);
        }

        protected void BtnConnexion_Click(object sender, EventArgs e)
        {
            _presenter.SurClicConnexion(txtEmail.Text, txtMotDePasse.Text);
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (_bd != null)
            {
                _bd.Dispose();
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session["idClient"] != null)
            {
                Response.Redirect("Accueil.aspx", true);
            }
        }

        public void OuvrirSession(int idClient)
        {
            Session.Clear();                         // aucune donnée d'une session précédente
            Session["idClient"] = idClient;
            Response.Redirect("Catalogue.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        public void AfficherErreur(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}