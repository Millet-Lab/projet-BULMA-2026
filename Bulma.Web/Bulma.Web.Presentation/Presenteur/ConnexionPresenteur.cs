using Bulma.Web.Modele.CoucheAccesBD;
using Bulma.Web.Modele.Entites;
using Bulma.Web.Modele.Securite;
using Bulma.Web.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Presenteur
{
    public class ConnexionPresenteur
    {
        // hash d'un mot de passe bidon : on vérifie quand même un hash quand l'email est inconnu,
        // pour que le temps de réponse ne révèle pas si le compte existe
        private static readonly string HashFactice = HacheurMotDePasse.Hacher("mot de passe factice");

        private readonly IConnexionVue _vue;
        private readonly AccesBD _bd;

        public ConnexionPresenteur(IConnexionVue vue, AccesBD bd)
        {
            _vue = vue;
            _bd = bd;
        }

        public void SurClicConnexion(string email, string motDePasse)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(motDePasse))
            {
                _vue.AfficherErreur("L'adresse email et le mot de passe sont obligatoires.");
                return;
            }

            try
            {
                Client client = _bd.GetClientEmail(email.Trim().ToLowerInvariant());

                string hash = client != null ? client.MotDePasseHash : HashFactice;
                bool motDePasseValide = HacheurMotDePasse.Verifier(motDePasse, hash);

                // même message pour un email inconnu et un mauvais mot de passe : on ne révèle pas lequel
                if (client == null || !motDePasseValide)
                {
                    _vue.AfficherErreur("Adresse email ou mot de passe incorrect.");
                    return;
                }

                _vue.OuvrirSession(client.IdClient);
            }
            catch (ExceptionAccesBD ex)
            {
                _vue.AfficherErreur($"Connexion impossible pour le moment. Réessayez plus tard : {ex}");
            }
        }
    }
}