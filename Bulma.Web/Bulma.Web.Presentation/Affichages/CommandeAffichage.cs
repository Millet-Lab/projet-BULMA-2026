using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Affichages
{
    public class CommandeAffichage
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Montant { get; set; }
        public string Statut { get; set; }
    }
}