using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Presentation.Affichages
{
    public class ProduitAffichage
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Detail { get; set; }
        public string Prix { get; set; }
        public bool Stock { get; set; }
    }
}
