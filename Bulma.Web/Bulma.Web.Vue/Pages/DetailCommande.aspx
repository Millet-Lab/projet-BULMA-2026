<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetailCommande.aspx.cs" Inherits="Bulma.Web.Vue.Pages.DetailCommande" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Détail de la commande</title>
    <link href="~/CSS/DetailCommande.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">

        <div class="header">

            <h1>
                <asp:Label ID="lblEntete"
                    runat="server"
                    Text="Détail de la commande" />
            </h1>

            <div class="menu">
                <a href="Commandes.aspx">Mes commandes</a>
                <a href="Catalogue.aspx">Catalogue</a>
            </div>

        </div>

        <asp:Label ID="lblMessage"
            runat="server"
            CssClass="message"
            ForeColor="Red" />

        <asp:GridView ID="gvLignes"
            runat="server"
            CssClass="grid"
            AutoGenerateColumns="False">

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

        <h2 class="total">
            Montant total :
            <span class="montant">
                <asp:Label ID="lblTotal" runat="server" />
            </span>
        </h2>

    </div>
    </form>
</body>
</html>