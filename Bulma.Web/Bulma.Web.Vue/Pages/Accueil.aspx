<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Accueil.aspx.cs" Inherits="Bulma.Web.Vue.Pages.Accueil" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Bulma - Accueil</title>
    <link href="~/CSS/Accueil.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="accueil">
            <header class="barre">
                <div class="marque">B<span>U</span>LMA</div>
            </header>

            <main class="scene">

                <%-- Visiteur non connecté --%>
                <asp:Panel ID="pnlVisiteur" runat="server">
                    <div class="tuiles">

                        <div class="element">
                            <a class="tuile" href="Catalogue.aspx" aria-label="Parcourir le catalogue en tant qu'invité">
                                <svg viewBox="0 0 64 64" fill="none" stroke="currentColor" stroke-width="2.5" aria-hidden="true">
                                    <circle cx="32" cy="20" r="11" />
                                    <path d="M10 58c0-13 9-21 22-21s22 8 22 21z" />
                                </svg>
                            </a>
                            <div class="libelle">Invité</div>
                            <details class="info">
                                <summary>i</summary>
                                <p>Parcourez le catalogue sans compte. L'ajout au panier n'est pas possible en mode invité.</p>
                            </details>
                        </div>

                        <div class="element principal">
                            <a class="tuile" href="Connexion.aspx" aria-label="Se connecter à son compte">
                                <svg viewBox="0 0 64 64" fill="currentColor" aria-hidden="true">
                                    <circle cx="32" cy="20" r="11" />
                                    <path d="M10 58c0-13 9-21 22-21s22 8 22 21z" />
                                </svg>
                            </a>
                            <div class="libelle">Connexion</div>
                            <details class="info">
                                <summary>i</summary>
                                <p>Connectez-vous avec votre adresse email et votre mot de passe pour commander.</p>
                            </details>
                        </div>

                        <div class="element">
                            <a class="tuile" href="Inscription.aspx" aria-label="Créer un compte">
                                <svg viewBox="0 0 64 64" fill="none" stroke="currentColor" stroke-width="2.5" aria-hidden="true">
                                    <circle cx="28" cy="22" r="11" stroke-dasharray="2 4" />
                                    <path d="M6 58c0-13 9-21 22-21s22 8 22 21z" stroke-dasharray="2 4" />
                                    <path d="M52 6v16M44 14h16" />
                                </svg>
                            </a>
                            <div class="libelle">S'inscrire</div>
                            <details class="info">
                                <summary>i</summary>
                                <p>Créez un compte gratuit pour remplir un panier et passer commande.</p>
                            </details>
                        </div>

                    </div>
                </asp:Panel>

                <%-- Client connecté --%>
                <asp:Panel ID="pnlClient" runat="server" Visible="false">
                    <div class="tuiles">
                        <div class="element principal">
                            <a class="tuile" href="Catalogue.aspx"><span>Catalogue</span></a>
                        </div>
                        <div class="element">
                            <a class="tuile" href="MonPanier.aspx"><span>Mon panier</span></a>
                        </div>
                        <div class="element">
                            <a class="tuile" href="Commandes.aspx"><span>Mes commandes</span></a>
                        </div>
                        <div class="element">
                            <a class="tuile" href="Deconnexion.aspx"><span>Déconnexion</span></a>
                        </div>
                    </div>
                </asp:Panel>

            </main>

            <footer class="pied">
                L'abus d'alcool est dangereux pour la santé. À consommer avec modération.
            </footer>
        </div>
    </form>
</body>
</html>
