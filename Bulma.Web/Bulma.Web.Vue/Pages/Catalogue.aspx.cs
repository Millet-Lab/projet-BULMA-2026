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
    public partial class Catalogue : Page, ICatalogueVue
    {
        private AccesBD _bd;
        private CataloguePresenteur _presenteur;

        protected void Page_Load(object sender, EventArgs e)
        {
            string cs = ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString;
            bool connecte = Session["idClient"] != null;

            _bd = new AccesBD(cs);
            _presenteur = new CataloguePresenteur(this, _bd, Session["idClient"] as int?);

            if (!IsPostBack)
            {
                _presenteur.ChargerParType(ddlType.SelectedValue);
            }

            pnlNavVisiteur.Visible = !connecte;
            pnlNavClient.Visible = connecte;
            pnlInvite.Visible = !connecte;
        }

        protected void GvProduits_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Ajouter")
            {
                var ligne = (GridViewRow)((Control)e.CommandSource).NamingContainer;
                var txtQuantite = (TextBox)ligne.FindControl("txtQuantite");

                _presenteur.SurClicAjouter(int.Parse(e.CommandArgument.ToString()), txtQuantite.Text);
            }
        }

        protected void DdlType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _presenteur.ChargerParType(ddlType.SelectedValue);
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            _bd?.Dispose();
        }

        public void AfficherProduits(List<ProduitAffichage> produits, string titre)
        {
            lblTitre.Text = Server.HtmlEncode(titre);
            gvProduits.DataSource = produits;
            gvProduits.Columns[4].Visible = Session["idClient"] != null; // la colonne n'apparaît que pour un client connecté
            gvProduits.DataBind();
        }

        public void AfficherConfirmation(string message)
        {
            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = Server.HtmlEncode(message);
        }

        public void AfficherErreur(string message)
        {
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}