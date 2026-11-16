<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Connexion.aspx.cs" Inherits="Bulma.Web.Vue.Pages.Connexion" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Connexion</title>
    <link href="~/CSS/Connexion.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            <a class="btn-fermer" href="Accueil.aspx" title="Retour à l'accueil" aria-label="Fermer et retourner à l'accueil">&times;</a>
            <h1>Connexion</h1>

            <div class="form-group">
                <label>Email</label>
                <asp:TextBox ID="txtEmail"
                    runat="server"
                    CssClass="input"
                    TextMode="Email" />
            </div>

            <div class="form-group">
                <label>Mot de passe</label>
                <asp:TextBox ID="txtMotDePasse"
                    runat="server"
                    CssClass="input"
                    TextMode="Password" />
            </div>

            <asp:Button ID="btnConnexion"
                runat="server"
                Text="Se connecter"
                CssClass="btn"
                OnClick="BtnConnexion_Click" />

            <asp:Label ID="lblMessage"
                runat="server"
                CssClass="message"
                ForeColor="Red" />

            <div class="inscription">
                <a href="Inscription.aspx">
                    Pas encore de compte ? S'inscrire
                </a>
            </div>

        </div>
    </form>
</body>
</html>