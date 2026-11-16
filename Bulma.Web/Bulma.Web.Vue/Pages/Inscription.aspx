<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Inscription.aspx.cs" Inherits="Bulma.Web.Vue.Pages.Inscription" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Inscription</title>
    <link href="~/CSS/Inscription.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server" autocomplete="off">
    <div class="card">
        <a class="btn-fermer" href="Accueil.aspx" title="Retour à l'accueil" aria-label="Fermer et retourner à l'accueil">&times;</a>

        <h1>Créer un compte</h1>

        <div class="form-group">
            <label>Nom</label>
            <asp:TextBox ID="txtNom" runat="server" CssClass="input" MaxLength="25" />
        </div>

        <div class="form-group">
            <label>Prénom</label>
            <asp:TextBox ID="txtPrenom" runat="server" CssClass="input" MaxLength="25" />
        </div>

        <div class="form-group">
            <label>Rue</label>
            <asp:TextBox ID="txtRue" runat="server" CssClass="input" MaxLength="45" />
        </div>

        <div class="ligne">
            <div class="form-group demi">
                <label>Code postal</label>
                <asp:TextBox ID="txtCp" runat="server" CssClass="input" MaxLength="4" />
            </div>

            <div class="form-group demi">
                <label>Localité</label>
                <asp:TextBox ID="txtLocalite" runat="server" CssClass="input" MaxLength="26" />
            </div>
        </div>

        <div class="form-group">
            <label>Email</label>
            <asp:TextBox ID="txtEmail" runat="server"
                CssClass="input"
                TextMode="Email"
                MaxLength="100" />
        </div>

        <div class="form-group">
            <label>Mot de passe</label>
            <asp:TextBox ID="txtMotDePasse" runat="server"
                CssClass="input"
                TextMode="Password" />
        </div>

        <div class="form-group">
            <label>Confirmation</label>
            <asp:TextBox ID="txtConfirmation" runat="server"
                CssClass="input"
                TextMode="Password" />
        </div>

        <asp:Button ID="btnInscrire"
            runat="server"
            CssClass="btn"
            Text="S'inscrire"
            OnClick="BtnInscrire_Click" />

        <asp:Label ID="lblMessage" runat="server" CssClass="message" />

    </div>
</form>
</body>
</html>
