using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class Adresse
    {
        private string _rue = string.Empty;
        private string _cp = string.Empty;
        private string _localite = string.Empty;

        public Adresse(string rue, string cp, string localite)
        {
            Rue = rue;
            CodePostal = cp;
            Localite = localite;
        }

        public Adresse(Adresse adresse)
        {
            if (adresse == null) 
            {
                throw new ArgumentNullException(nameof(adresse));
            }

            Rue = adresse.Rue;
            CodePostal = adresse.CodePostal;
            Localite = adresse.Localite;
        }

        public string Rue
        {
            get { return _rue; }
            set { _rue = ValiderTexte(value, 45, nameof(Rue)); }
        }

        public string CodePostal
        {
            get { return _cp; }
            set
            {
                if (value == null || value.Length != 4 || !value.All(char.IsDigit))
                {
                    throw new ArgumentException("Le code postal doit contenir 4 chiffres.", nameof(CodePostal));
                }

                _cp = value;
            }
        }

        public string Localite
        {
            get { return _localite; }
            set { _localite = ValiderTexte(value, 26, nameof(Localite)); }
        }

        private static string ValiderTexte(string valeur, int longueurMax, string nomChamp)
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
    }
}