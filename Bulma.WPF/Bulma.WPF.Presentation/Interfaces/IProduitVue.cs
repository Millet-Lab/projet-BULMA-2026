using Bulma.WPF.Presentation.Affichages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Vue.Interfaces
{
    public interface IProduitVue
    {
        ProduitEcran Ecran { get; }
        bool Confirmer(string question);
    }
}
