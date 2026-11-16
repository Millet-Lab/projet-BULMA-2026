using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Modele.Entites
{
    public class LigneCommande
    {
        private Commande _comandeLC = null!;
        private Produit _produitLC = null!;
        private int _quantiteLC;

        /// <summary>
        /// Constructeur d'une nouvelle ligne de commande par copie d'une ancienne ligne existante
        /// </summary>
        /// <param name="ligneCmde"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LigneCommande(LigneCommande ligneCmde)
        {
            ArgumentNullException.ThrowIfNull(ligneCmde);

            CommandeLigneCommande = ligneCmde.CommandeLigneCommande;
            ProduitLigneCommande = ligneCmde.ProduitLigneCommande;
            QuantiteLigneCommande = ligneCmde.QuantiteLigneCommande;
        }

        /// <summary>
        /// Constructeur d'une nouvelle ligne de commande à partir des données lues depuis la base de données
        /// </summary>
        /// <param name="commandeLC"></param>
        /// <param name="produitLC"></param>
        /// <param name="quantiteLC"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LigneCommande(Commande commandeLC, Produit produitLC, int quantiteLC)
        {
            ArgumentNullException.ThrowIfNull(commandeLC);
            ArgumentNullException.ThrowIfNull(produitLC);

            if (quantiteLC < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantiteLC), "La quantité doit être supérieure à zéro.");
            }

            CommandeLigneCommande = commandeLC;
            ProduitLigneCommande = produitLC;
            QuantiteLigneCommande = quantiteLC;
        }

        public Commande CommandeLigneCommande
        {
            get { return _comandeLC; }
            set
            {
                _comandeLC = value ?? throw new ArgumentNullException(nameof(CommandeLigneCommande));
            }
        }

        public Produit ProduitLigneCommande
        {
            get { return _produitLC; }
            set
            {
                _produitLC = value ?? throw new ArgumentNullException(nameof(ProduitLigneCommande));

            }
        }

        public int QuantiteLigneCommande
        {
            get { return _quantiteLC; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("La quantité de la ligne doit être supérieure à zéro.", nameof(QuantiteLigneCommande));
                }

                _quantiteLC = value;
            }
        }

        public decimal SousTotal()
        {
            return QuantiteLigneCommande * _produitLC.PrixUnitaire;
        }
    }
}
