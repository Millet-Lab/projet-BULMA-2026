using Bulma.WPF.Modele.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Affichages
{
    public class AjoutProduitEcran : BaseNotifiable
    {
        private string _typeChoisi = "ALCOOL";
        private string _nom = "", _prix = "", _gout = "", _stock = "";
        private string _degreAlcool = "", _provenance = "";
        private bool _estAlcoolisee = true;
        private Recipient _recipientChoisi = Recipient.Bouteille;
        private string _nomImage = "Aucune image choisie";
        private string _cheminApercu = "";
        private string _message = "";
        private bool _messageEstErreur;

        public IReadOnlyList<ChoixType> Types { get; } = new[]
        {
            new ChoixType("ALCOOL", "Alcool"),
            new ChoixType("BIERE", "Bière")
        };

        public IReadOnlyList<Recipient> Recipients { get; } = Enum.GetValues<Recipient>();

        public string TypeChoisi { get => _typeChoisi; set => Definir(ref _typeChoisi, value); }
        public string Nom { get => _nom; set => Definir(ref _nom, value); }
        public string Prix { get => _prix; set => Definir(ref _prix, value); }
        public string Gout { get => _gout; set => Definir(ref _gout, value); }
        public string Stock { get => _stock; set => Definir(ref _stock, value); }
        public string DegreAlcool { get => _degreAlcool; set => Definir(ref _degreAlcool, value); }
        public string Provenance { get => _provenance; set => Definir(ref _provenance, value); }
        public bool EstAlcoolisee { get => _estAlcoolisee; set => Definir(ref _estAlcoolisee, value); }
        public Recipient RecipientChoisi { get => _recipientChoisi; set => Definir(ref _recipientChoisi, value); }
        public string NomImage { get => _nomImage; set => Definir(ref _nomImage, value); }
        public string CheminApercu { get => _cheminApercu; set => Definir(ref _cheminApercu, value); }

        public string Message { get => _message; private set => Definir(ref _message, value); }
        public bool MessageEstErreur { get => _messageEstErreur; private set => Definir(ref _messageEstErreur, value); }

        public void Info(string message) { Message = message; MessageEstErreur = false; }
        public void Erreur(string message) { Message = message; MessageEstErreur = true; }

        // vide le formulaire (le type choisi est conservé pour enchaîner plusieurs ajouts)
        public void Reinitialiser()
        {
            Nom = Prix = Gout = Stock = DegreAlcool = Provenance = "";
            EstAlcoolisee = true;
            RecipientChoisi = Recipient.Bouteille;
            NomImage = "Aucune image choisie";
            CheminApercu = "";
        }
    }
}