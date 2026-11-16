using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Affichages
{
    public abstract class BaseNotifiable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Definir<T>(ref T champ, T valeur, [CallerMemberName] string? nom = null)
        {
            if (EqualityComparer<T>.Default.Equals(champ, valeur))
            {
                return;
            }

            champ = valeur;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nom));
        }
    }
}