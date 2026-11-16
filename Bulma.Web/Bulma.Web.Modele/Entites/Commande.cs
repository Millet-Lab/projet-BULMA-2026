using Bulma.Web.Modele.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class Commande
    {
        private int _idCommande;
        private Client _clientCmde = null!;
        private DateTime _dateCmde;
        private TimeSpan _heureCmde;
        private decimal _totalMontant;
        private StatutLivraison _statutLivraison = StatutLivraison.NonLivree;

        /// <summary>
        /// Constructeur de commande par copie d'une ancienne commande existante
        /// </summary>
        /// <param name="commande"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Commande(Commande commande)
        {
            if (commande == null)
            {
                throw new ArgumentNullException(nameof(commande));
            }

            IdCommande = commande.IdCommande;
            ClientCommande = commande.ClientCommande;
            DateCommande = commande.DateCommande;
            HeureCommande = commande.HeureCommande;
            TotalMontant = commande.TotalMontant;
            StatutLivraison = commande.StatutLivraison;
        }

        /// <summary>
        /// Constructeur de commande à partir d'un client existant.
        /// _idCmde initialisé 0 pour une nouvelle commande, ainsi que NonLivree.
        /// date, heure et statut sont fixées à la date et heure courante de la commande
        /// </summary>
        /// <param name="clientCmde"></param>
        public Commande(Client clientCmde)
        {
            ClientCommande = clientCmde;
            DateCommande = DateTime.Now;
            HeureCommande = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Constructeur d'une commande à partir des données lues depuis la base de données
        /// </summary>
        /// <param name="idCmde"></param>
        /// <param name="client"></param>
        /// <param name="dateCmde"></param>
        /// <param name="heureCmde"></param>
        /// <param name="totalCmde"></param>
        /// <param name="statutLivraison"></param>
        public Commande(int idCmde, Client client, DateTime dateCmde, TimeSpan heureCmde, decimal totalCmde, StatutLivraison statutLivraison)
        {
            IdCommande = idCmde;
            ClientCommande = client;
            DateCommande = dateCmde;
            HeureCommande = heureCmde;
            TotalMontant = totalCmde;
            StatutLivraison = statutLivraison;
        }

        public int IdCommande
        {
            get => _idCommande;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("L'identifiant ne peut pas être négatif.", nameof(IdCommande));
                }

                _idCommande = value;
            }
        }

        public Client ClientCommande
        {
            get => new Client(_clientCmde);
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(ClientCommande));
                }

                _clientCmde = new Client(value);
            }
        }

        public DateTime DateCommande
        {
            get => _dateCmde;
            set
            {
                if (value.Date > DateTime.Today)
                {
                    throw new ArgumentException("La date de commande ne peut pas être dans le futur.", nameof(DateCommande));
                }

                _dateCmde = value.Date;
            }
        }

        public TimeSpan HeureCommande
        {
            get => _heureCmde;
            set { _heureCmde = value; }
        }

        /// <summary>
        /// La valeur du total de la commande suit le format de 2 chiffres qprès la virgule
        /// </summary>
        public decimal TotalMontant
        {
            get => _totalMontant;
            set { _totalMontant = Math.Round(value, 2); }
        }

        public StatutLivraison StatutLivraison
        {
            get => _statutLivraison;
            set { _statutLivraison = value; }
        }

        public void MarquerCommandeLivree()
        {
            _statutLivraison = StatutLivraison.Livree;
        }

        /// <summary>
        /// Contrôle qu'une commande et ses lignes peuvent être enregistrées.
        /// Lève une InvalidOperationException dont le message est affichable à l'utilisateur.
        /// </summary>
        public void VerifierPourEnregistrement(IReadOnlyCollection<LigneCommande> lignes)
        {
            if (lignes == null)
            {
                throw new ArgumentNullException(nameof(lignes));
            }

            if (_idCommande != 0)
            {
                throw new InvalidOperationException("Cette commande est déjà enregistrée.");
            }

            if (_clientCmde.IdClient == 0)
            {
                throw new InvalidOperationException("Le client de la commande n'est pas enregistré.");
            }

            if (_statutLivraison != StatutLivraison.NonLivree)
            {
                throw new InvalidOperationException("Une nouvelle commande doit être non livrée.");
            }

            if (lignes.Count == 0)
            {
                throw new InvalidOperationException("Une commande doit contenir au moins un produit.");
            }

            var produitsVus = new HashSet<int>();

            foreach (LigneCommande ligne in lignes)
            {
                if (!ReferenceEquals(ligne.CommandeLigneCommande, this))
                {
                    throw new InvalidOperationException("Une ligne appartient à une autre commande.");
                }

                Produit produit = ligne.ProduitLigneCommande;

                if (produit.IdProduit == 0)
                {
                    throw new InvalidOperationException("Le produit " + produit.Nom + " n'est pas enregistré.");
                }

                if (!produitsVus.Add(produit.IdProduit))
                {
                    throw new InvalidOperationException(
                        "Le produit " + produit.Nom + " figure plusieurs fois dans la commande.");
                }

                if (produit.EstSupprime)
                {
                    throw new InvalidOperationException(
                        "Le produit " + produit.Nom + " n'est plus disponible à la commande.");
                }

                if (ligne.QuantiteLigneCommande > produit.QuantiteEnStock)
                {
                    throw new InvalidOperationException(
                        "Stock insuffisant pour " + produit.Nom + " (reste " + produit.QuantiteEnStock + ").");
                }
            }

            if (_totalMontant != lignes.Sum(l => l.QuantiteLigneCommande * l.ProduitLigneCommande.PrixUnitaire))
            {
                throw new InvalidOperationException("Le montant total ne correspond pas aux lignes de la commande.");
            }
        }
    }
}