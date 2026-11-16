using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Presentation.Affichages;
using Bulma.WPF.Presentation.Interfaces;
using Bulma.WPF.Presentation.Presenteur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bulma.WPF.Vue.Vue
{
    /// <summary>
    /// Logique d'interaction pour CommandeWindow.xaml
    /// </summary>
    public partial class CommandeWindow : Window, ICommandeVue
    {
        private readonly CommandePresenteur _presenteur;

        public CommandeEcran Ecran { get; } = new();
        public CommandeWindow(AccesBD accesBD)
        {
            InitializeComponent();

            DataContext = Ecran;
            _presenteur = new CommandePresenteur(this, accesBD);
            _presenteur.Initialiser();
        }

        public bool Confirmer(string question)
        {
            return MessageBox.Show(question, "Confirmation",
                                   MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private void BtnLivrer_Click(object sender, RoutedEventArgs e) => _presenteur.SurClicMarquerLivree();
    }
}
