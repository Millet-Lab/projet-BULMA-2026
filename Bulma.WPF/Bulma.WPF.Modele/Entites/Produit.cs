using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Modele.Entites
{
    public abstract class Produit
    {
        private int _idProduit;
        private string _nom = string.Empty;
        private string _image = string.Empty;
        private decimal _prixUnitaire;
        private string _gout = string.Empty;
        private int _quantiteEnStock;
        private bool _estSupprime;

        // Contraintes de classe Produit en relation avec la base de données
        private const int NomLongueurMax = 30;      // varchar(30)
        private const int ImageLongueurMax = 255;   // varchar(255)
        private const int GoutLongueurMax = 30;     // varchar(30)
        private const decimal PrixMax = 999.99m;    // numeric(5,2)

        /// <summary>
        /// Constructeur d'un nouveau produit par copie d'un ancien produit existant.
        /// La validation n'est pas necessaire ici, car déjà vérifié à la création de l'ancien produit.
        /// </summary>
        /// <param name="produit"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected Produit(Produit produit)
        {
            ArgumentNullException.ThrowIfNull(produit);

            _idProduit = produit._idProduit;
            _nom = produit._nom;
            _image = produit._image;
            _prixUnitaire = produit._prixUnitaire;
            _gout = produit._gout;
            _quantiteEnStock = produit._quantiteEnStock;
            _estSupprime = produit._estSupprime;
        }

        /// <summary>
        /// Constructeur d'un nouveau produit avec IdProduit vallant 0 jusqu'à l'enregistrement dans la BD
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="image"></param>
        /// <param name="prixUnitaire"></param>
        /// <param name="gout"></param>
        /// <param name="quantiteEnStock"></param>
        protected Produit(string nom, string image, decimal prixUnitaire, string gout, int quantiteEnStock)
        {
            Nom = nom;
            Image = image;
            PrixUnitaire = prixUnitaire;
            Gout = gout;
            _quantiteEnStock = ValiderStock(quantiteEnStock);
        }

        /// <summary>
        /// Constructeur d'un nouveau produit avec lecture depuis la base de données
        /// </summary>
        /// <param name="idProduit"></param>
        /// <param name="nom"></param>
        /// <param name="image"></param>
        /// <param name="prixUnitaire"></param>
        /// <param name="gout"></param>
        /// <param name="quantiteEnStock"></param>
        /// <param name="estSupprime"></param>
        protected Produit(int idProduit, string nom, string image, decimal prixUnitaire, string gout, int quantiteEnStock, bool estSupprime)
            : this(nom, image, prixUnitaire, gout, quantiteEnStock)
        {
            IdProduit = idProduit;
            _estSupprime = estSupprime;
        }

        public int IdProduit
        {
            get { return _idProduit; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("L'identifiant ne peut pas être négatif.", nameof(IdProduit));
                }

                _idProduit = value;
            }
        }

        public string Nom
        {
            get { return _nom; }
            set { _nom = ValiderNom(value, NomLongueurMax, nameof(Nom)); }
        }

        public string Image
        {
            get { return _image; }
            set { _image = ValiderNom(value, ImageLongueurMax, nameof(Image)); }
        }

        public decimal PrixUnitaire
        {
            get { return _prixUnitaire; }
            set
            {
                if (value <= 0 || value > PrixMax)
                {
                    throw new ArgumentException("Le prix doit être supérieur à 0.", nameof(PrixUnitaire));
                }

                _prixUnitaire = Math.Round(value, 2); // garantit 2 chiffres après la virgule pour les centimes
            }
        }

        public string Gout
        {
            get { return _gout; }
            set { _gout = ValiderNom(value, GoutLongueurMax, nameof(Gout)); }
        }

        public int QuantiteEnStock
        {
            get { return _quantiteEnStock; }
            set
            {
                if (_estSupprime)
                {
                    throw new InvalidOperationException("Le stock d'un produit supprimé ne peut plus être modifié.");
                }

                _quantiteEnStock = ValiderStock(value);
            }
        }

        /// <summary>
        /// Suppression logique : le produit reste lié aux commandes passées
        /// Ceci est utile pour retracer l'historique d'un produit
        /// </summary>
        public void Supprimer()
        {
            _estSupprime = true;
        }

        /// <summary>
        /// Valeur du discriminant en BD (type_produit) : 'ALCOOL' ou 'BIERE'
        /// </summary>
        public abstract string TypeProduit { get; }

        public bool EstSupprime
        {
            get { return _estSupprime; }
        }

        /// <summary>
        /// Methode permettant de verifier si un produit est commandable, c'est-a-dire
        /// non supprimé et en stock
        /// </summary>
        public bool EstCommandable
        {
            get { return !_estSupprime && _quantiteEnStock > 0; }
        }

        /// <summary>
        /// Méthode partagée avec la classe fille Alcool pour la validation de la provenance
        /// </summary>
        /// <param name="valeur"></param>
        /// <param name="longueurMax"></param>
        /// <param name="nomChamp"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected static string ValiderNom(string valeur, int longueurMax, string nomChamp)
        {
            if (string.IsNullOrWhiteSpace(valeur))
            {
                throw new ArgumentException(nomChamp + " est obligatoire.", nomChamp);
            }

            valeur = valeur.Trim();

            if (valeur.Length > longueurMax)
            {
                throw new ArgumentException(nomChamp + " ne peut pas dépasser " + longueurMax + " caractères.", nomChamp);
            }

            return valeur;
        }

        private static int ValiderStock(int valeur)
        {
            if (valeur < 0)
            {
                throw new ArgumentException("La quantité en stock ne peut pas être négative.", nameof(valeur));
            }

            return valeur;
        }
    }
}
