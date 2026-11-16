<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Commandes.aspx.cs" Inherits="Bulma.Web.Vue.Pages.Commandes" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Mes commandes</title>
    <link href="~/CSS/Commandes.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">

        <div class="container">

            <div class="header">
                <h1>📦 Mes commandes</h1>

                <div class="menu">
                    <a href="Catalogue.aspx">Catalogue</a>
                    <a href="MonPanier.aspx">Mon panier</a>
                </div>
            </div>

            <asp:Label ID="lblMessage"
                runat="server"
                CssClass="message"
                ForeColor="Red" />

            <asp:GridView ID="gvCommandes"
                runat="server"
                CssClass="gridview"
                AutoGenerateColumns="false"
                EmptyDataText="Vous n'avez pas encore passé de commande.">

                <Columns>

                    <asp:BoundField
                        DataField="Id"
                        HeaderText="Commande" />

                    <asp:BoundField
                        DataField="Date"
                        HeaderText="Date" />

                    <asp:BoundField
                        DataField="Montant"
                        HeaderText="Montant (€)" />

                    <asp:BoundField
                        DataField="Statut"
                        HeaderText="Statut" />

                    <asp:HyperLinkField
                        HeaderText="Détails"
                        DataNavigateUrlFields="Id"
                        DataNavigateUrlFormatString="DetailCommande.aspx?id={0}"
                        Text="Afficher" />

                </Columns>

            </asp:GridView>

        </div>

    </form>
</body>
</html>
