using Bulma.Web.Modele.CoucheAccesBD;
using Bulma.Web.Modele.Entites;
using Bulma.Web.Presentation.Affichages;
using Bulma.Web.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Presenteur
{
    public class CataloguePresenteur
    {
        private static readonly CultureInfo Fr = new CultureInfo("fr-BE");

        private readonly ICatalogueVue _vue;
        private readonly AccesBD _bd;
        private readonly int? _idClient;

        public CataloguePresenteur(ICatalogueVue vue, AccesBD bd, int? idClient)
        {
            _vue = vue;
            _bd = bd;
            _idClient = idClient;
        }

        public void ChargerParType(string type)
        {
            try
            {
                List<Produit> produits = _bd.ListerProduits();
                IEnumerable<Produit> visibles = produits.Where(p => !p.EstSupprime);

                string titre;
                switch (type)
                {
                    case "ALCOOL":
                        visibles = visibles.Where(p => p is Alcool);
                        titre = "Alcools";
                        break;
                    case "BIERE":
                        visibles = visibles.Where(p => p is Biere);
                        titre = "Bières";
                        break;
                    default:
                        _vue.AfficherErreur("Type de produit inconnu.");
                        return;
                }

                _vue.AfficherProduits(visibles.Select(Convertir).ToList(), titre);
            }
            catch (ExceptionAccesBD ex)
            {
                _vue.AfficherErreur("Impossible de charger le catalogue. " + ex.Message);
            }
        }

        public void SurClicAjouter(int idProduit, string quantiteTexte)
        {
            if (_idClient == null)
            {
                _vue.AfficherErreur("Connectez-vous pour ajouter des produits au panier.");
                return;
            }

            int quantite;

            if (!int.TryParse(quantiteTexte, out quantite) || quantite < 1)
            {
                _vue.AfficherErreur("La quantité doit être un nombre entier supérieur à 0.");
                return;
            }

            try
            {
                if (_bd.AjouterAuPanier(_idClient.Value, idProduit, quantite) == 0)
                {
                    _vue.AfficherErreur("Ajout impossible : produit indisponible ou stock insuffisant pour cette quantité.");
                    return;
                }

                _vue.AfficherConfirmation("Produit ajouté au panier.");
            }
            catch (ExceptionAccesBD)
            {
                _vue.AfficherErreur("Ajout impossible pour le moment.");
            }
        }

        private static ProduitAffichage Convertir(Produit p)
        {
            string detail = "";

            if (p is Alcool a)
            {
                detail = a.DegreAlcool.ToString("0.0", Fr) + " % - " + a.Provenance;
            }
            else if (p is Biere b)
            {
                detail = b.Recipient + (b.EstAlcoolisee ? "" : " - sans alcool");
            }

            return new ProduitAffichage
            {
                Id = p.IdProduit,
                Nom = p.Nom,
                Detail = detail,
                Prix = p.PrixUnitaire.ToString("C", Fr),
                Stock = p.EstCommandable
            };
        }
    }
}