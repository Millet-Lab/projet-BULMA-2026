using Bulma.WPF.Presentation.Affichages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Interfaces
{
    public interface ICommandeVue
    {
        CommandeEcran Ecran { get; }
        bool Confirmer(string question);
    }
}
