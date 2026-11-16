using Bulma.Web.Modele.CoucheAccesBD;
using Bulma.Web.Modele.Entites;
using Bulma.Web.Modele.Enumerations;
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
    public class DetailCommandePresenteur
    {
        private static readonly CultureInfo Fr = new CultureInfo("fr-BE");

        private readonly IDetailCommandeVue _vue;
        private readonly AccesBD _bd;
        private readonly int _idClient;

        public DetailCommandePresenteur(IDetailCommandeVue vue, AccesBD bd, int idClient)
        {
            _vue = vue;
            _bd = bd;
            _idClient = idClient;
        }

        public void Charger(string idTexte)
        {
            int idCommande;
            if (!int.TryParse(idTexte, out idCommande) || idCommande < 1)
            {
                _vue.AfficherErreur("Commande introuvable.");
                return;
            }

            try
            {
                // on ne cherche que parmi les commandes du client connecté :
                // impossible de lire celle d'un autre en changeant l'identifiant dans l'adresse
                Commande commande = _bd.ListerCommandesPasseesParClient(_idClient)
                                       .FirstOrDefault(c => c.IdCommande == idCommande);

                if (commande == null)
                {
                    _vue.AfficherErreur("Commande introuvable.");
                    return;
                }

                List<LigneAffichage> lignes = _bd.ListerProduitsCommande(commande)
                    .Select(l => new LigneAffichage
                    {
                        Nom = l.ProduitLigneCommande.Nom,
                        Prix = l.ProduitLigneCommande.PrixUnitaire.ToString("C", Fr),
                        Quantite = l.QuantiteLigneCommande,
                        SousTotal = l.SousTotal.ToString("C", Fr),
                        Remarque = l.ProduitLigneCommande.EstSupprime ? "Produit retiré du catalogue" : ""
                    })
                    .ToList();

                string entete = "Commande n° " + commande.IdCommande
                    + " du " + commande.DateCommande.ToString("dd/MM/yyyy")
                    + " - " + (commande.StatutLivraison == StatutLivraison.Livree ? "livrée" : "pas encore livrée");

                _vue.AfficherDetail(entete, lignes, commande.TotalMontant.ToString("C", Fr));
            }
            catch (ExceptionAccesBD)
            {
                _vue.AfficherErreur("Impossible de charger cette commande pour le moment.");
            }
        }
    }
}