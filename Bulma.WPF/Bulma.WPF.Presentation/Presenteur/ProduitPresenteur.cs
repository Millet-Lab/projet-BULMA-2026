using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Modele.Entites;
using Bulma.WPF.Presentation.Affichages;
using Bulma.WPF.Vue.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Presenteur
{
    public class ProduitPresenteur
    {
        private static readonly CultureInfo Fr = new("fr-BE");

        private readonly IProduitVue _vue;
        private readonly AccesBD _accesBD;
        private Dictionary<int, Produit> _produits = new();    // produits affichés, par identifiant

        private ProduitEcran Ecran => _vue.Ecran;

        public ProduitPresenteur(IProduitVue vue, AccesBD accesBD)
        {
            _vue = vue;
            _accesBD = accesBD;
            Ecran.PropertyChanged += SurChangementEcran;
        }

        public void Initialiser()
        {
            Charger();
        }

        public void SurChangementType()
        {
            Charger();
        }

        // le Présentateur réagit à ce qui change dans l'écran : à la sélection, il propose le stock actuel
        private void SurChangementEcran(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProduitEcran.ProduitSelectionne) && Ecran.ProduitSelectionne != null)
            {
                Ecran.NouveauStock = Ecran.ProduitSelectionne.Stock.ToString(Fr);
            }
        }

        public void SurClicModifierStock()
        {
            ProduitAffichage? selection = Ecran.ProduitSelectionne;

            if (selection == null)
            {
                Ecran.Erreur("Sélectionnez d'abord un produit.");
                return;
            }

            if (!int.TryParse(Ecran.NouveauStock, NumberStyles.Integer, Fr, out int stock) || stock < 0)
            {
                Ecran.Erreur("Le stock doit être un nombre entier supérieur ou égal à 0.");
                return;
            }

            try
            {
                Produit produit = _produits[selection.Id];
                produit.QuantiteEnStock = stock;

                if (_accesBD.ModifierQuantiteEnStock(produit, produit.QuantiteEnStock) == 0)
                {
                    Charger();
                    Ecran.Erreur("Modification refusée : produit supprimé ou introuvable.");
                    return;
                }

                Charger();
                Ecran.Info($"Stock de « {produit.Nom} » mis à jour : {stock}.");
            }
            catch (InvalidOperationException ex)
            {
                Ecran.Erreur(ex.Message);
            }
            catch (ArgumentException ex)
            {
                Ecran.Erreur(ex.Message);
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur(ex.Message);
            }
        }

        public void SurClicSupprimer()
        {
            ProduitAffichage? selection = Ecran.ProduitSelectionne;

            if (selection == null)
            {
                Ecran.Erreur("Sélectionnez d'abord un produit.");
                return;
            }

            if (!_vue.Confirmer($"Supprimer « {selection.Nom} » du catalogue ?\n"
                                + "Il restera visible dans les commandes déjà passées, mais ne pourra plus être commandé."))
            {
                return;
            }

            try
            {
                Produit produit = _produits[selection.Id];
                produit.Supprimer();

                if (_accesBD.SupprimerProduit(produit) == 0)
                {
                    Charger();
                    Ecran.Erreur("Suppression impossible : produit déjà supprimé ou introuvable.");
                    return;
                }

                Charger();
                Ecran.Info($"« {produit.Nom} » a été supprimé du catalogue.");
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur(ex.Message);
            }
        }

        // source de vérité = la base : on relit après chaque opération
        private void Charger()
        {
            try
            {
                string type = Ecran.TypeChoisi;

                List<Produit> visibles = _accesBD.ListerProduits()
                    .Where(p => !p.EstSupprime && p.TypeProduit == type)
                    .ToList();

                _produits = visibles.ToDictionary(p => p.IdProduit);

                Ecran.Produits.Clear();
                foreach (Produit p in visibles)
                {
                    Ecran.Produits.Add(Convertir(p));
                }

                Ecran.NouveauStock = "";
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur("Impossible de charger les produits. " + ex.Message);
            }
        }

        public void Rafraichir()
        {
            Charger();
        }

        private static ProduitAffichage Convertir(Produit p)
        {
            string detail = p switch
            {
                Alcool a => $"{a.DegreAlcool.ToString("0.0", Fr)} % - {a.Provenance}",
                Biere b => b.Recipient + (b.EstAlcoolisee ? "" : " - sans alcool"),
                _ => ""
            };

            return new ProduitAffichage
            {
                Id = p.IdProduit,
                Nom = p.Nom,
                Detail = detail,
                Prix = p.PrixUnitaire.ToString("C", Fr),
                Stock = p.QuantiteEnStock
            };
        }
    }
}