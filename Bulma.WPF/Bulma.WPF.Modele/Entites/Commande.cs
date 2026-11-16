using Bulma.WPF.Modele.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Modele.Entites
{
    public class Commande
    {
        private int _idCommande;
        private Client _clientCmde = null!;
        private DateOnly _dateCmde;
        private TimeOnly _heureCmde;
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
            DateCommande = DateOnly.FromDateTime(DateTime.Now);
            HeureCommande = TimeOnly.FromDateTime(DateTime.Now);
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
        public Commande(int idCmde, Client client, DateOnly dateCmde, TimeOnly heureCmde, decimal totalCmde, StatutLivraison statutLivraison)
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
            get => new (_clientCmde);
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                _clientCmde = new Client(value);
            }
        }

        public DateOnly DateCommande
        {
            get => _dateCmde;
            set
            {
                if (value > DateOnly.FromDateTime(DateTime.Today))
                {
                    throw new ArgumentException("La date de commande ne peut pas être dans le futur.", nameof(DateCommande));
                }

                _dateCmde = value;
            }
        }

        public TimeOnly HeureCommande
        {
            get => _heureCmde;
            set { _heureCmde = value; }
        }

        /// <summary>
        /// La valeur du total de la commande suit le format de 2 chiffres après la virgule
        /// </summary>
        public decimal TotalMontant
        {
            get => _totalMontant;
            set { _totalMontant = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
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
            ArgumentNullException.ThrowIfNull(lignes);

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

            decimal totalCalcule = Math.Round(lignes.Sum(ligne => ligne.SousTotal()), 2, MidpointRounding.AwayFromZero);

            if (_totalMontant != totalCalcule)
            {
                throw new InvalidOperationException($"Le montant total enregistré ({_totalMontant:F2} €) " +
                                                    $"ne correspond pas au montant calculé ({totalCalcule:F2} €).");
            }
        }
    }
}