using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Affichages
{
    public record ChoixType(string Code, string Libelle);
    public class ProduitEcran : BaseNotifiable
    {
        private string _typeChoisi = "ALCOOL";
        private ProduitAffichage? _produitSelectionne;
        private string _nouveauStock = "";
        private string _message = "";
        private bool _messageEstErreur;

        public IReadOnlyList<ChoixType> Types { get; } = new[]
        {
            new ChoixType("ALCOOL", "Alcools"),
            new ChoixType("BIERE", "Bières")
        };

        // liste affichée : on la modifie (Clear/Add), la Vue suit automatiquement
        public ObservableCollection<ProduitAffichage> Produits { get; } = new();

        public string TypeChoisi
        {
            get => _typeChoisi;
            set => Definir(ref _typeChoisi, value);
        }

        public ProduitAffichage? ProduitSelectionne
        {
            get => _produitSelectionne;
            set => Definir(ref _produitSelectionne, value);
        }

        public string NouveauStock
        {
            get => _nouveauStock;
            set => Definir(ref _nouveauStock, value);
        }

        public string Message
        {
            get => _message;
            private set => Definir(ref _message, value);
        }

        public bool MessageEstErreur
        {
            get => _messageEstErreur;
            private set => Definir(ref _messageEstErreur, value);
        }

        public void Info(string message)
        {
            Message = message;
            MessageEstErreur = false;
        }

        public void Erreur(string message)
        {
            Message = message;
            MessageEstErreur = true;
        }
    }
}