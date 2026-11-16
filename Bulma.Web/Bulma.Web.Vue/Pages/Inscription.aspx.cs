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
    public partial class Inscription : Page, IInscriptionVue
    {
        private AccesBD _bd;
        private InscriptionPresenteur _presenter;

        protected void Page_Load(object sender, EventArgs e)
        {
            _bd = new AccesBD(ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString);
            _presenter = new InscriptionPresenteur(this, _bd);
        }

        protected void BtnInscrire_Click(object sender, EventArgs e)
        {
            _presenter.SurClicInscription(
                txtNom.Text, txtPrenom.Text, txtRue.Text, txtCp.Text, txtLocalite.Text,
                txtEmail.Text, txtMotDePasse.Text, txtConfirmation.Text);
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            _bd?.Dispose();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session["idClient"] != null)
            {
                Response.Redirect("Accueil.aspx", true);
            }
        }

        public void AfficherErreur(string message)
        {
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Text = Server.HtmlEncode(message);
        }

        public void AfficherConfirmation(string message)
        {
            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}