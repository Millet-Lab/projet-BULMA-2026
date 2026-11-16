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
    public partial class DetailCommande : PageClient, IDetailCommandeVue
    {
        private AccesBD _bd;

        protected void Page_Load(object sender, EventArgs e)
        {
            _bd = new AccesBD(ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString);

            if (!IsPostBack)
            {
                new DetailCommandePresenteur(this, _bd, IdClientConnecte).Charger(Request.QueryString["id"]);
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (_bd != null)
            {
                _bd.Dispose();
            }
        }

        public void AfficherDetail(string entete, List<LigneAffichage> lignes, string montantTotal)
        {
            lblEntete.Text = Server.HtmlEncode(entete);
            gvLignes.DataSource = lignes;
            gvLignes.DataBind();
            lblTotal.Text = Server.HtmlEncode(montantTotal);
        }

        public void AfficherErreur(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}