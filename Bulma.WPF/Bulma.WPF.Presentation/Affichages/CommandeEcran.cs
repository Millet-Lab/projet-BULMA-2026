using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Affichages
{
    public class CommandeEcran : BaseNotifiable
    {
        private ClientAffichage? _clientSelectionne;
        private CommandeAffichage? _commandeSelectionnee;
        private string _resumeClient = "";
        private string _enteteCommande = "Sélectionnez une commande pour voir ses produits.";
        private string _montantCommande = "";
        private bool _peutMarquerLivree;
        private string _message = "";
        private bool _messageEstErreur;

        public ObservableCollection<ClientAffichage> Clients { get; } = new();
        public ObservableCollection<CommandeAffichage> Commandes { get; } = new();
        public ObservableCollection<LigneAffichage> Lignes { get; } = new();

        public ClientAffichage? ClientSelectionne
        {
            get => _clientSelectionne;
            set => Definir(ref _clientSelectionne, value);
        }

        public CommandeAffichage? CommandeSelectionnee
        {
            get => _commandeSelectionnee;
            set => Definir(ref _commandeSelectionnee, value);
        }

        public string ResumeClient { get => _resumeClient; set => Definir(ref _resumeClient, value); }
        public string EnteteCommande { get => _enteteCommande; set => Definir(ref _enteteCommande, value); }
        public string MontantCommande { get => _montantCommande; set => Definir(ref _montantCommande, value); }
        public bool PeutMarquerLivree { get => _peutMarquerLivree; set => Definir(ref _peutMarquerLivree, value); }

        public string Message { get => _message; private set => Definir(ref _message, value); }
        public bool MessageEstErreur { get => _messageEstErreur; private set => Definir(ref _messageEstErreur, value); }

        public void Info(string message) { Message = message; MessageEstErreur = false; }
        public void Erreur(string message) { Message = message; MessageEstErreur = true; }
        public void EffacerMessage() { Message = ""; MessageEstErreur = false; }
    }
}
