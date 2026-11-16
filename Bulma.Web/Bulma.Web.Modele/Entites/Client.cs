using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class Client
    {
        private int _idClient;
        private string _nom = string.Empty;
        private string _prenom = string.Empty;
        private Adresse _adresseClient = null!;
        private string _email = string.Empty;
        private string _motDePasseHash = string.Empty;

        // Contraintes de classe Client en relation avec la base de données
        private const int NomPrenomLongueurMax = 30;   // varchar(30)
        private const int EmailLongueurMax = 100;      // varchar(100)
        private const int HashLongueurMax = 255;       // varchar(255)

        private static readonly Regex FormatNomPrenom = new Regex(@"^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$");
        private static readonly Regex FormatEmail = new Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");

        public Client(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            IdClient = client.IdClient;
            Nom = client.Nom;
            Prenom = client.Prenom;
            AdresseClient = client.AdresseClient;
            Email = client.Email;
            MotDePasseHash = client.MotDePasseHash;
        }

        /// <summary>
        /// Nouveau client pas encore enregistré : IdClient vaut 0
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="prenom"></param>
        /// <param name="adresseClient"></param>
        /// <param name="email"></param>
        /// <param name="motDePasseHash"></param>
        public Client(string nom, string prenom, Adresse adresseClient, string email, string motDePasseHash)
        {
            Nom = nom;
            Prenom = prenom;
            AdresseClient = adresseClient;
            Email = email;
            MotDePasseHash = motDePasseHash;
        }

        /// <summary>
        /// Client déjà enregistré et lu depuis la base de données
        /// </summary>
        /// <param name="idClient"></param>
        /// <param name="nom"></param>
        /// <param name="prenom"></param>
        /// <param name="adresseClient"></param>
        /// <param name="email"></param>
        /// <param name="motDePasseHash"></param>
        public Client(int idClient, string nom, string prenom, Adresse adresseClient, string email, string motDePasseHash) : this(nom, prenom, adresseClient, email, motDePasseHash)
        {
            IdClient = idClient;
        }

        public int IdClient
        {
            get { return _idClient; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("L'identifiant ne peut pas être négatif.", nameof(IdClient));
                }

                _idClient = value;
            }
        }

        public string Nom
        {
            get { return _nom; }
            set { _nom = ValiderNomPrenom(value, nameof(Nom)); }
        }

        public string Prenom
        {
            get { return _prenom; }
            set { _prenom = ValiderNomPrenom(value, nameof(Prenom)); }
        }

        public Adresse AdresseClient
        {
            get { return new Adresse(_adresseClient); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(AdresseClient));
                }

                _adresseClient = new Adresse(value);
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("L'adresse email est obligatoire.", nameof(Email));
                }

                value = value.Trim().ToLowerInvariant();

                if (value.Length > EmailLongueurMax || !FormatEmail.IsMatch(value))
                {
                    throw new ArgumentException("Format d'adresse email invalide.", nameof(Email));
                }

                _email = value;
            }
        }

        public string MotDePasseHash
        {
            get { return _motDePasseHash; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length > HashLongueurMax)
                {
                    throw new ArgumentException("Hash du mot de passe invalide.", nameof(MotDePasseHash));
                }

                _motDePasseHash = value;
            }
        }

        private static string ValiderNomPrenom(string valeur, string nomChamp)
        {
            if (string.IsNullOrWhiteSpace(valeur))
            {
                throw new ArgumentException(nomChamp + " est obligatoire.", nomChamp);
            }

            valeur = valeur.Trim();

            if (valeur.Length < 2 || valeur.Length > NomPrenomLongueurMax)
            {
                throw new ArgumentException(nomChamp + " doit contenir entre 2 et " + NomPrenomLongueurMax + " caractères.", nomChamp);
            }

            if (!FormatNomPrenom.IsMatch(valeur))
            {
                throw new ArgumentException(
                    nomChamp + " ne peut contenir que des lettres, espaces et tirets.", nomChamp);
            }

            return valeur;
        }
    }
}
