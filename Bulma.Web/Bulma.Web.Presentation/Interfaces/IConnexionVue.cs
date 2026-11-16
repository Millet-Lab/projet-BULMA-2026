using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Interfaces
{
    public interface IConnexionVue
    {
        void AfficherErreur(string message);
        void OuvrirSession(int idClient);
    }
}
