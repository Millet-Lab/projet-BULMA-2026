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
    public partial class MonPanier : PageClient, IPanierVue
    {
        private AccesBD _bd;
        private PanierPresenteur _presenter;

        protected void Page_Load(object sender, EventArgs e)
        {
            _bd = new AccesBD(ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString);
            _presenter = new PanierPresenteur(this, _bd, IdClientConnecte);

            if (!IsPostBack)
            {
                _presenter.Charger();
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (_bd != null)
            {
                _bd.Dispose();
            }
        }

        public void AfficherContenu(List<LigneAffichage> lignes, string sousTotal)
        {
            gvPanier.DataSource = lignes;
            gvPanier.DataBind();
            lblSousTotal.Text = Server.HtmlEncode(sousTotal);
        }

        public void AfficherErreur(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
        }

        protected void BtnCommander_Click(object sender, EventArgs e)
        {
            _presenter.SurClicPasserCommande();
        }

        public void OuvrirDetailCommande(int idCommande)
        {
            Response.Redirect("DetailCommande.aspx?id=" + idCommande, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}