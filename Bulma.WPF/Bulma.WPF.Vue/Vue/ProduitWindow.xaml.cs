using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Presentation.Affichages;
using Bulma.WPF.Presentation.Presenteur;
using Bulma.WPF.Vue.Interfaces;
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
    /// Logique d'interaction pour ProduitWindow.xaml
    /// </summary>
    public partial class ProduitWindow : Window, IProduitVue
    {
        private readonly ProduitPresenteur _presenteur;
        private readonly AccesBD _accesBD;
        public ProduitEcran Ecran { get; } = new();
        public ProduitWindow(AccesBD accesBD)
        {
            InitializeComponent();

            _accesBD = accesBD;
            DataContext = Ecran;
            _presenteur = new ProduitPresenteur(this, accesBD);
            _presenteur.Initialiser();

            // abonné après l'initialisation, pour ne pas recharger deux fois au démarrage
            cmbType.SelectionChanged += (s, e) => _presenteur.SurChangementType();
        }

        public bool Confirmer(string question)
        {
            return MessageBox.Show(question, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private void BtnModifierStock_Click(object sender, RoutedEventArgs e) => _presenteur.SurClicModifierStock();

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e) => _presenteur.SurClicSupprimer();

        private void BtnAjouterProduit_Click(object sender, RoutedEventArgs e)
        {
            new AjoutProduitWindow(_accesBD) { Owner = this }.ShowDialog();
            _presenteur.Rafraichir();
        }
    }
}