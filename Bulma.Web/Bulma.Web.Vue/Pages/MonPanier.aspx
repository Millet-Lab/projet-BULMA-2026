<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MonPanier.aspx.cs" Inherits="Bulma.Web.Vue.Pages.MonPanier" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Mon panier</title>
    <link href="~/CSS/MonPanier.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">

        <div class="header">

            <h1>🛒 Mon panier</h1>

            <div class="menu">
                <a href="Catalogue.aspx">Catalogue</a>
                <a href="Commandes.aspx">Mes commandes</a>
            </div>

        </div>

        <asp:Label ID="lblMessage"
            runat="server"
            CssClass="message"
            ForeColor="Red" />

        <asp:GridView ID="gvPanier"
            runat="server"
            CssClass="grid"
            AutoGenerateColumns="False"
            EmptyDataText="Votre panier est vide.">

            <Columns>

                <asp:BoundField
                    DataField="Nom"
                    HeaderText="Produit" />

                <asp:BoundField
                    DataField="Prix"
                    HeaderText="Prix unitaire (€)"
                    DataFormatString="{0:C}" />

                <asp:BoundField
                    DataField="Quantite"
                    HeaderText="Quantité" />

                <asp:BoundField
                    DataField="SousTotal"
                    HeaderText="Sous-total (€)"
                    DataFormatString="{0:C}" />

                <asp:BoundField
                    DataField="Remarque"
                    HeaderText="Remarque" />

            </Columns>

        </asp:GridView>

        <div class="footer">

            <div class="total">
                Sous-total :
                <asp:Label ID="lblSousTotal" runat="server" />
            </div>

            <asp:Button
                ID="btnCommander"
                runat="server"
                CssClass="btnCommander"
                Text="Passer la commande"
                OnClick="BtnCommander_Click"
                UseSubmitBehavior="false"
                OnClientClick="this.disabled=true;" />

        </div>

    </div>
    </form>
</body>
</html>