using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Modele.Entites;
using Bulma.WPF.Modele.Enumerations;
using Bulma.WPF.Presentation.Affichages;
using Bulma.WPF.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Presenteur
{
    public class CommandePresenteur
    {
        private static readonly CultureInfo Fr = new("fr-BE");

        private readonly ICommandeVue _vue;
        private readonly AccesBD _accesBD;
        private List<Commande> _commandes = new();      // commandes du client sélectionné
        private bool _rechargement;                     // évite de réagir pendant qu'on remplit les listes

        private CommandeEcran Ecran => _vue.Ecran;

        public CommandePresenteur(ICommandeVue vue, AccesBD accesBD)
        {
            _vue = vue;
            _accesBD = accesBD;
            Ecran.PropertyChanged += SurChangementEcran;
        }

        public void Initialiser()
        {
            try
            {
                foreach (Client client in _accesBD.ListeClient())
                {
                    Ecran.Clients.Add(new ClientAffichage
                    {
                        Id = client.IdClient,
                        NomComplet = $"{client.Nom} {client.Prenom} ({client.Email})"
                    });
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur("Impossible de charger les clients. " + ex.Message);
            }
        }

        private void SurChangementEcran(object? sender, PropertyChangedEventArgs e)
        {
            if (_rechargement)
            {
                return;
            }

            if (e.PropertyName == nameof(CommandeEcran.ClientSelectionne))
            {
                Ecran.EffacerMessage();
                ChargerCommandes(null);
            }
            else if (e.PropertyName == nameof(CommandeEcran.CommandeSelectionnee))
            {
                ChargerLignes();
            }
        }

        public void SurClicMarquerLivree()
        {
            CommandeAffichage? selection = Ecran.CommandeSelectionnee;
            if (selection == null)
            {
                Ecran.Erreur("Sélectionnez d'abord une commande.");
                return;
            }

            if (!_vue.Confirmer($"Marquer la commande n° {selection.Id} comme livrée ?\nCette action est définitive."))
            {
                return;
            }

            try
            {
                Commande commande = _commandes.First(c => c.IdCommande == selection.Id);
                int resultat = _accesBD.ModifierStatutCommande(commande);

                ChargerCommandes(selection.Id);          // la base fait foi : on relit puis on réaffiche

                switch (resultat)
                {
                    case AccesBD.CommandeModifiee:
                        Ecran.Info($"La commande n° {selection.Id} est marquée comme livrée.");
                        break;
                    case AccesBD.CommandeDejaLivree:
                        Ecran.Erreur("Cette commande était déjà livrée.");
                        break;
                    default:
                        Ecran.Erreur("Commande introuvable.");
                        break;
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur(ex.Message);
            }
        }

        private void ChargerCommandes(int? idASelectionner)
        {
            _rechargement = true;

            try
            {
                Ecran.Commandes.Clear();
                Ecran.CommandeSelectionnee = null;
                _commandes = new List<Commande>();

                ClientAffichage? client = Ecran.ClientSelectionne;
                if (client == null)
                {
                    Ecran.ResumeClient = "";
                    return;
                }

                _commandes = _accesBD.ListerCommandesPasseesParClient(client.Id);

                foreach (Commande c in _commandes)
                {
                    Ecran.Commandes.Add(Convertir(c));
                }

                decimal total = _commandes.Sum(c => c.TotalMontant);
                Ecran.ResumeClient = _commandes.Count == 0
                    ? "Ce client n'a passé aucune commande."
                    : $"{_commandes.Count} commande(s) - montant total : {total.ToString("C", Fr)}";

                if (idASelectionner != null)
                {
                    Ecran.CommandeSelectionnee = Ecran.Commandes.FirstOrDefault(c => c.Id == idASelectionner);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur("Impossible de charger les commandes. " + ex.Message);
            }
            finally
            {
                _rechargement = false;
            }

            ChargerLignes();
        }

        private void ChargerLignes()
        {
            Ecran.Lignes.Clear();
            Ecran.PeutMarquerLivree = false;

            CommandeAffichage? selection = Ecran.CommandeSelectionnee;
            if (selection == null)
            {
                Ecran.EnteteCommande = "Sélectionnez une commande pour voir ses produits.";
                Ecran.MontantCommande = "";
                return;
            }

            try
            {
                Commande commande = _commandes.First(c => c.IdCommande == selection.Id);

                foreach (LigneCommande l in _accesBD.ListerProduitsCmdeClient(commande.IdCommande))
                {
                    Ecran.Lignes.Add(new LigneAffichage
                    {
                        Nom = l.ProduitLigneCommande.Nom,
                        Prix = l.ProduitLigneCommande.PrixUnitaire.ToString("C", Fr),
                        Quantite = l.QuantiteLigneCommande,
                        SousTotal = l.SousTotal().ToString("C", Fr),
                        Remarque = l.ProduitLigneCommande.EstSupprime ? "Produit retiré du catalogue" : ""
                    });
                }

                Ecran.EnteteCommande = $"Commande n° {commande.IdCommande} du {commande.DateCommande:dd/MM/yyyy} - "
                                       + (commande.StatutLivraison == StatutLivraison.Livree ? "livrée" : "pas encore livrée");
                Ecran.MontantCommande = "Montant de la commande : " + commande.TotalMontant.ToString("C", Fr);
                Ecran.PeutMarquerLivree = commande.StatutLivraison == StatutLivraison.NonLivree;
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur("Impossible de charger les produits de la commande. " + ex.Message);
            }
        }

        private static CommandeAffichage Convertir(Commande c)
        {
            bool livree = c.StatutLivraison == StatutLivraison.Livree;

            return new CommandeAffichage
            {
                Id = c.IdCommande,
                Date = $"{c.DateCommande:dd/MM/yyyy} à {c.HeureCommande:HH:mm}",
                Montant = c.TotalMontant.ToString("C", Fr),
                Statut = livree ? "Livrée" : "Pas encore livrée",
                EstLivree = livree
            };
        }
    }
}
