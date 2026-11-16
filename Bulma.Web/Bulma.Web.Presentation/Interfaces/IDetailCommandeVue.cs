using Bulma.Web.Presentation.Affichages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Interfaces
{
    public interface IDetailCommandeVue
    {
        void AfficherDetail(string entete, List<LigneAffichage> lignes, string montantTotal);
        void AfficherErreur(string message);
    }
}
