using Bulma.Web.Modele.CoucheAccesBD;
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
    public class CommandesPresenteur
    {
        private static readonly CultureInfo Fr = new CultureInfo("fr-BE");

        private readonly ICommandesVue _vue;
        private readonly AccesBD _bd;
        private readonly int _idClient;

        public CommandesPresenteur(ICommandesVue vue, AccesBD bd, int idClient)
        {
            _vue = vue;
            _bd = bd;
            _idClient = idClient;
        }

        public void Charger()
        {
            try
            {
                List<CommandeAffichage> affichage = _bd.ListerCommandesPasseesParClient(_idClient)
                    .Select(c => new CommandeAffichage
                    {
                        Id = c.IdCommande,
                        Date = c.DateCommande.ToString("dd/MM/yyyy") + " à " + c.HeureCommande.ToString(@"hh\:mm"),
                        Montant = c.TotalMontant.ToString("C", Fr),
                        Statut = c.StatutLivraison == StatutLivraison.Livree ? "Livrée" : "Pas encore livrée"
                    })
                    .ToList();

                _vue.AfficherCommandes(affichage);
            }
            catch (ExceptionAccesBD)
            {
                _vue.AfficherErreur("Impossible de charger vos commandes pour le moment.");
            }
        }
    }
}