using Bulma.Web.Presentation.Affichages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Interfaces
{
    public interface ICommandesVue
    {
        void AfficherCommandes(List<CommandeAffichage> commandes);
        void AfficherErreur(string message);
    }
}
