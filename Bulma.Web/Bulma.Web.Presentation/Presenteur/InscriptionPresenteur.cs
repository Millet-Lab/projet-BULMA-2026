using Bulma.Web.Modele.CoucheAccesBD;
using Bulma.Web.Modele.Entites;
using Bulma.Web.Modele.Securite;
using Bulma.Web.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Presenteur
{
    public class InscriptionPresenteur
    {
        private const int LongueurMinMotDePasse = 8;
        private readonly IInscriptionVue _vue;
        private readonly AccesBD _bd;

        public InscriptionPresenteur(IInscriptionVue vue, AccesBD bd)
        {
            _vue = vue;
            _bd = bd;
        }

        public void SurClicInscription(string nom, string prenom, string rue, string cp, string localite,
                                       string email, string motDePasse, string confirmation)
        {
            // règles sur le mot de passe : elles se contrôlent AVANT le hachage
            if (string.IsNullOrEmpty(motDePasse) || motDePasse.Length < LongueurMinMotDePasse)
            {
                _vue.AfficherErreur("Le mot de passe doit contenir au moins " + LongueurMinMotDePasse + " caractères.");
                return;
            }

            if (motDePasse != confirmation)
            {
                _vue.AfficherErreur("Le mot de passe et sa confirmation sont différents.");
                return;
            }

            try
            {
                // les constructeurs valident chaque champ et lèvent une ArgumentException
                var adresse = new Adresse(rue, cp, localite);
                var client = new Client(nom, prenom, adresse, email, HacheurMotDePasse.Hacher(motDePasse));

                if (_bd.ExisteClientAvecEmail(client.Email))
                {
                    _vue.AfficherErreur("Cette adresse email est déjà utilisée.");
                    return;
                }

                _bd.AjouterClient(client);
                _vue.AfficherConfirmation("Votre compte a été créé. Vous pouvez maintenant vous connecter.");
            }
            catch (ArgumentException ex)
            {
                _vue.AfficherErreur(PremiereLigne(ex.Message));
            }
            catch (ExceptionAccesBD ex)
            {
                _vue.AfficherErreur(ex.Message);
            }
        }

        // ArgumentException.Message ajoute une 2e ligne « Nom du paramètre : ... » inutile à l'utilisateur
        private static string PremiereLigne(string message)
        {
            return message.Split('\n')[0].Trim();
        }
    }
}