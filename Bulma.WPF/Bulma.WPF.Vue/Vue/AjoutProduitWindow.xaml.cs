using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Presentation.Affichages;
using Bulma.WPF.Presentation.Interfaces;
using Bulma.WPF.Presentation.Presenteur;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Logique d'interaction pour AjoutProduitWindow.xaml
    /// </summary>
    public partial class AjoutProduitWindow : Window, IAjoutProduitVue
    {
        private readonly AjoutProduitPresenteur _presenteur;

        public AjoutProduitEcran Ecran { get; } = new();
        public AjoutProduitWindow(AccesBD accesBD)
        {
            InitializeComponent();

            DataContext = Ecran;
            string dossierImages = ConfigurationManager.AppSettings["DossierImages"] ?? "";
            _presenteur = new AjoutProduitPresenteur(this, accesBD, dossierImages);
        }

        public string? ChoisirFichierImage()
        {
            var dialogue = new OpenFileDialog
            {
                Title = "Choisir l'image du produit",
                Filter = "Images (*.jpg;*.jpeg;*.png;*.gif;*.webp)|*.jpg;*.jpeg;*.png;*.gif;*.webp"
            };

            return dialogue.ShowDialog(this) == true ? dialogue.FileName : null;
        }

        private void BtnParcourir_Click(object sender, RoutedEventArgs e) => _presenteur.SurClicParcourir();
        private void BtnAjouter_Click(object sender, RoutedEventArgs e) => _presenteur.SurClicAjouter();
        private void BtnFermer_Click(object sender, RoutedEventArgs e) => Close();
    }
}
