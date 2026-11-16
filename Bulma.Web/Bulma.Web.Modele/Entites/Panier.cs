using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class Panier
    {
        private int _idPanier;
        private Client _clientPanier;

        public Panier(Panier panier)
        {
            if (panier == null)
            {
                throw new ArgumentNullException(nameof(panier));
            }

            _idPanier = panier._idPanier;
            _clientPanier = panier._clientPanier;
        }

        /// <summary>
        /// L'identifiant vaut 0 pour un nouveau panier
        /// </summary>
        /// <param name="clientPanier"></param>
        public Panier(Client clientPanier)
        {
            ClientPanier = clientPanier ?? throw new ArgumentNullException(nameof(clientPanier));
        }

        public Panier(int idPanier, Client clientPanier)
        {
            IdPanier = idPanier;
            ClientPanier = clientPanier;
        }

        public int IdPanier
        {
            get { return _idPanier; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("L'identifiant de panier invalide !", nameof(IdPanier));
                }

                _idPanier = value;
            }
        }

        public Client ClientPanier
        {
            get { return _clientPanier; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(ClientPanier));
                }

                _clientPanier = value;
            }
        }
    }
}