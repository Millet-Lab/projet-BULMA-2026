using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Interfaces
{
    public interface IInscriptionVue
    {
        void AfficherErreur(string message);
        void AfficherConfirmation(string message);
    }
}
