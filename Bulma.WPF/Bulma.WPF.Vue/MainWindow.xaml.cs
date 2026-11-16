using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Vue.Vue;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bulma.WPF.Vue;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AccesBD? _accesBD;
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            _accesBD = new AccesBD(ConfigurationManager.ConnectionStrings["Bulma"].ConnectionString);
        }
        catch (ExceptionAccesBD ex)
        {
            MessageBox.Show("Connexion à la base impossible : " + ex.Message);
            Application.Current.Shutdown();
            return;
        }

        Closed += (s, e) => _accesBD.Dispose();
    }

    private void BtnProduits_Click(object sender, RoutedEventArgs e)
    {
        if(_accesBD != null)
        {
            new ProduitWindow(_accesBD)
            { 
                Owner = this 
            }.ShowDialog();
        }
    }

    private void BtnCommandes_Click(object sender, RoutedEventArgs e)
    {
        if (_accesBD != null)
        {
            new CommandeWindow(_accesBD) 
            { 
                Owner = this 
            }.ShowDialog();
        }
    }
}