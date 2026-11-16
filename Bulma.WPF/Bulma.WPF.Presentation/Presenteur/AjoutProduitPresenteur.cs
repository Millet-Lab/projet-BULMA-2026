using Bulma.WPF.Modele.CoucheAccesBD;
using Bulma.WPF.Modele.Entites;
using Bulma.WPF.Presentation.Affichages;
using Bulma.WPF.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bulma.WPF.Presentation.Presenteur
{
    public class AjoutProduitPresenteur
    {
        private static readonly CultureInfo Fr = new("fr-BE");
        private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long TailleMax = 2 * 1024 * 1024;       // 2 Mo

        private readonly IAjoutProduitVue _vue;
        private readonly AccesBD _accesBD;
        private readonly string _dossierImages;
        private string? _cheminSource;

        private AjoutProduitEcran Ecran => _vue.Ecran;

        public AjoutProduitPresenteur(IAjoutProduitVue vue, AccesBD accesBD, string dossierImages)
        {
            _vue = vue;
            _accesBD = accesBD;
            _dossierImages = dossierImages;
        }

        public void SurClicParcourir()
        {
            string? chemin = _vue.ChoisirFichierImage();
            if (chemin == null)
            {
                return;
            }

            _cheminSource = chemin;
            Ecran.NomImage = Path.GetFileName(chemin);
            Ecran.CheminApercu = chemin;
        }

        public void SurClicAjouter()
        {
            string? fichierCopie = null;

            try
            {
                // 1. nombres saisis
                if (!TryDecimal(Ecran.Prix, out decimal prix))
                {
                    Ecran.Erreur("Le prix doit être un nombre (par exemple 39,90).");
                    return;
                }

                if (!int.TryParse(Ecran.Stock, NumberStyles.Integer, Fr, out int stock) || stock < 0)
                {
                    Ecran.Erreur("Le stock doit être un nombre entier supérieur ou égal à 0.");
                    return;
                }

                // 2. image
                if (_cheminSource == null)
                {
                    Ecran.Erreur("Choisissez une image pour le produit.");
                    return;
                }

                string extension = Path.GetExtension(_cheminSource).ToLowerInvariant();
                if (!Extensions.Contains(extension))
                {
                    Ecran.Erreur("Format d'image non accepté (jpg, png, gif ou webp).");
                    return;
                }

                var fichier = new FileInfo(_cheminSource);
                if (!fichier.Exists)
                {
                    Ecran.Erreur("Le fichier image est introuvable.");
                    return;
                }

                if (fichier.Length > TailleMax)
                {
                    Ecran.Erreur("L'image est trop volumineuse (2 Mo maximum).");
                    return;
                }

                if (!Directory.Exists(_dossierImages))
                {
                    Ecran.Erreur("Le dossier des images est introuvable : vérifiez « DossierImages » dans App.config.");
                    return;
                }

                // 3. produit : les constructeurs valident chaque champ (ArgumentException)
                string nomFichier = Guid.NewGuid().ToString("N") + extension;
                Produit produit = Ecran.TypeChoisi == "ALCOOL"
                    ? CreerAlcool(nomFichier, prix, stock)
                    : CreerBiere(nomFichier, prix, stock);

                // 4. copie de l'image, puis enregistrement ; si la base refuse, la copie est supprimée
                string destination = Path.Combine(_dossierImages, nomFichier);
                File.Copy(_cheminSource, destination, overwrite: false);
                fichierCopie = destination;

                _accesBD.AjouterProduit(produit);
                fichierCopie = null;                      // enregistré : l'image reste

                Ecran.Info($"« {produit.Nom} » a été ajouté au catalogue.");
                Ecran.Reinitialiser();
                _cheminSource = null;
            }
            catch (ArgumentException ex)
            {
                Ecran.Erreur(Nettoyer(ex.Message));
            }
            catch (ExceptionAccesBD ex)
            {
                Ecran.Erreur(ex.Message);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Ecran.Erreur("Copie de l'image impossible : " + ex.Message);
            }
            finally
            {
                if (fichierCopie != null)
                {
                    try { File.Delete(fichierCopie); } catch (IOException) { }
                }
            }
        }

        private Produit CreerAlcool(string image, decimal prix, int stock)
        {
            if (!TryDecimal(Ecran.DegreAlcool, out decimal degre))
            {
                throw new ArgumentException("Le degré d'alcool doit être un nombre (par exemple 40,5).");
            }

            return new Alcool(Ecran.Nom, image, prix, Ecran.Gout, stock, degre, Ecran.Provenance);
        }

        private Produit CreerBiere(string image, decimal prix, int stock)
        {
            return new Biere(Ecran.Nom, image, prix, Ecran.Gout, stock, Ecran.EstAlcoolisee, Ecran.RecipientChoisi);
        }

        // accepte la virgule et le point comme séparateur décimal
        private static bool TryDecimal(string texte, out decimal valeur)
        {
            return decimal.TryParse((texte ?? "").Trim().Replace('.', ','), NumberStyles.Number, Fr, out valeur);
        }

        // retire le suffixe technique « (Parameter 'xxx') » que .NET ajoute aux ArgumentException
        private static string Nettoyer(string message)
        {
            return Regex.Replace(message, @"\s*\([^()]*'[^']*'\)\s*$", "").Trim();
        }
    }
}
