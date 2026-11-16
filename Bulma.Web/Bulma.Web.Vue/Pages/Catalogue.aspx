<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Catalogue.aspx.cs" Inherits="Bulma.Web.Vue.Pages.Catalogue" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Catalogue</title>
    <link href="~/CSS/Catalogue.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">

            <div class="entete">
                <h1><asp:Label ID="lblTitre" runat="server" /></h1>

                <%-- Visiteur : seulement Accueil --%>
                <asp:Panel ID="pnlNavVisiteur" runat="server" CssClass="menu">
                    <a class="btn-nav" href="Accueil.aspx">Accueil</a>
                </asp:Panel>

                <%-- Client connecté : son espace --%>
                <asp:Panel ID="pnlNavClient" runat="server" Visible="false" CssClass="menu">
                    <a class="btn-nav" href="Accueil.aspx">Accueil</a>
                    <a class="btn-nav" href="MonPanier.aspx">Mon panier</a>
                    <a class="btn-nav" href="Commandes.aspx">Mes commandes</a>
                    <a class="btn-nav" href="Deconnexion.aspx">Se déconnecter</a>
                </asp:Panel>
            </div>

            <asp:Panel ID="pnlInvite" runat="server" Visible="false" CssClass="message-invite">
                Vous consultez le catalogue en tant qu'invité.
                <a href="Connexion.aspx">Connectez-vous</a> ou <a href="Inscription.aspx">inscrivez-vous</a>
                pour ajouter des produits au panier.
            </asp:Panel>

            <div class="filtre">
                <asp:DropDownList
                    ID="ddlType"
                    runat="server"
                    CssClass="ddl"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="DdlType_SelectedIndexChanged">

                <asp:ListItem Value="ALCOOL" Text="🥃 Alcools" />
                <asp:ListItem Value="BIERE" Text="🍺 Bières" />

                </asp:DropDownList>
            </div>

            <asp:Label ID="lblMessage"
                runat="server"
                CssClass="message"
                ForeColor="Red" />

            <asp:GridView
                ID="gvProduits"
                runat="server"
                CssClass="grid"
                AutoGenerateColumns="false"
                OnRowCommand="GvProduits_RowCommand">

                <Columns>

                    <asp:BoundField DataField="Nom" HeaderText="Produit" />
                    <asp:BoundField DataField="Detail" HeaderText="Description" />
                    <asp:BoundField DataField="Prix" HeaderText="Prix (€)" />
                    <asp:TemplateField HeaderText="Stock">
                        <ItemTemplate>
                            <span class='<%# (bool)Eval("Stock") ? "bouton dispo" : "bouton indispo" %>'>
                                <%# (bool)Eval("Stock") ? "Disponible" : "Indisponible" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Ajouter au panier">
                        <ItemTemplate>

                            <asp:TextBox
                                ID="txtQuantite"
                                runat="server"
                                CssClass="quantite"
                                Text="1" />

                            <asp:Button
                                ID="btnAjouter"
                                runat="server"
                                CssClass="btnAjouter"
                                Text="Ajouter"
                                CommandName="Ajouter"
                                CommandArgument='<%# Eval("Id") %>' />

                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>
    </form>
</body>
</html>