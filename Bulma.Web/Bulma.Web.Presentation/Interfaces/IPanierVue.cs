using Bulma.Web.Presentation.Affichages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Interfaces
{
    public interface IPanierVue
    {
        void AfficherContenu(List<LigneAffichage> lignes, string sousTotal);
        void AfficherErreur(string message);
        void OuvrirDetailCommande(int idCommande);
    }
}