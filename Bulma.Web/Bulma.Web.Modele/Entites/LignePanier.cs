using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class LignePanier
    {
        private readonly Panier _panier;
        private readonly Produit _produit;
        private int _quantite;

        public LignePanier(LignePanier lignePanier)
        {
            if (lignePanier == null)
            {
                throw new ArgumentNullException(nameof(lignePanier));
            }

            _panier = lignePanier._panier;
            _produit = lignePanier._produit;
            _quantite = lignePanier._quantite;
        }

        public LignePanier(Panier panier, Produit produit, int quantite)
        {
            _panier = panier ?? throw new ArgumentNullException(nameof(panier), "Le panier ne peut pas être nul.");
            _produit = produit ?? throw new ArgumentNullException(nameof(produit), "Le produit ne peut pas être nul.");
            Quantite = quantite;
        }

        public Panier Panier => _panier;

        public Produit Produit => _produit;

        public int Quantite
        {
            get { return _quantite; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("La ligne du panier doit avoir au moins un produit !", nameof(Quantite));
                }

                _quantite = value;
            }
        }

        public decimal SousTotal => _quantite * _produit.PrixUnitaire;
    }
}