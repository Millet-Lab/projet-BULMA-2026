using Bulma.Web.Modele.CoucheAccesBD;
using Bulma.Web.Modele.Entites;
using Bulma.Web.Presentation.Affichages;
using Bulma.Web.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Presenteur
{
    public class PanierPresenteur
    {
        private static readonly CultureInfo Fr = new CultureInfo("fr-BE");

        private readonly IPanierVue _vue;
        private readonly AccesBD _bd;
        private readonly int _idClient;

        public PanierPresenteur(IPanierVue vue, AccesBD bd, int idClient)
        {
            _vue = vue;
            _bd = bd;
            _idClient = idClient;
        }

        public void Charger()
        {
            try
            {
                List<LignePanier> lignes = _bd.ContenuPanier(_idClient);
                var affichage = new List<LigneAffichage>();
                decimal sousTotal = 0m;

                foreach (LignePanier ligne in lignes)
                {
                    Produit produit = ligne.Produit;
                    string remarque = "";

                    if (produit.EstSupprime)
                    {
                        remarque = "Produit retiré du catalogue";
                    }
                    else if (produit.QuantiteEnStock < ligne.Quantite)
                    {
                        remarque = "Stock insuffisant (reste " + produit.QuantiteEnStock + ")";
                    }
                    else
                    {
                        sousTotal += ligne.SousTotal;    // seules les lignes commandables entrent dans le sous-total
                    }

                    affichage.Add(new LigneAffichage
                    {
                        Nom = produit.Nom,
                        Prix = produit.PrixUnitaire.ToString("C", Fr),
                        Quantite = ligne.Quantite,
                        SousTotal = ligne.SousTotal.ToString("C", Fr),
                        Remarque = remarque
                    });
                }

                _vue.AfficherContenu(affichage, sousTotal.ToString("C", Fr));
            }
            catch (ExceptionAccesBD)
            {
                _vue.AfficherErreur("Impossible de charger votre panier pour le moment.");
            }
        }

        public void SurClicPasserCommande()
        {
            try
            {
                Client client = _bd.InfoClient(_idClient);
                if (client == null)
                {
                    _vue.AfficherErreur("Client introuvable.");
                    return;
                }

                List<LignePanier> panier = _bd.ContenuPanier(_idClient);
                if (panier.Count == 0)
                {
                    _vue.AfficherErreur("Votre panier est vide.");
                    return;
                }

                var commande = new Commande(client);        // date, heure et statut fixés par le constructeur
                var lignes = new List<LigneCommande>();

                foreach (LignePanier lp in panier)
                {
                    Produit produit = lp.Produit;

                    if (produit.EstSupprime)
                    {
                        _vue.AfficherErreur(produit.Nom + " n'est plus disponible : la commande est refusée.");
                        return;
                    }

                    if (lp.Quantite > produit.QuantiteEnStock)
                    {
                        _vue.AfficherErreur("Stock insuffisant pour " + produit.Nom
                                            + " (reste " + produit.QuantiteEnStock + ").");
                        return;
                    }

                    lignes.Add(new LigneCommande(commande, produit, lp.Quantite));
                }

                commande.TotalMontant = lignes.Sum(l => l.SousTotal);
                commande.VerifierPourEnregistrement(lignes);

                int idCommande = _bd.AjouterCommande(commande, lignes);
                _vue.OuvrirDetailCommande(idCommande);
            }
            catch (InvalidOperationException ex)
            {
                _vue.AfficherErreur(ex.Message);
            }
            catch (ExceptionAccesBD ex)
            {
                _vue.AfficherErreur(ex.Message);
            }
        }
    }
}