using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class Alcool : Produit
    {
        private decimal _degreAlcool;
        private string _provenance = string.Empty;
        private const int ProvenanceLongueurMax = 30;   // varchar(30) : contraintes en relation avec la base de données

        public Alcool(Alcool alcool) : base(alcool)
        {
            DegreAlcool = alcool.DegreAlcool;
            Provenance = alcool.Provenance;
        }

        // construction de l'objet alcool avec un identifiant vallant 0.
        public Alcool(string nom, string image, decimal prixUnitaire, string gout, int stock, decimal degreAlcool, string provenance)
            : base(nom, image, prixUnitaire, gout, stock)
        {
            DegreAlcool = degreAlcool;
            Provenance = provenance;
        }

        // construction de l'objet alcool avec lecture des tables depuis la BD
        public Alcool(int idProduit, string nom, string image, decimal prixUnitaire, string gout, int stock, bool estSupprime, decimal degreAlcool, string provenance)
            : base(idProduit, nom, image, prixUnitaire, gout, stock, estSupprime)
        {
            DegreAlcool = degreAlcool;
            Provenance = provenance;
        }

        public override string TypeProduit
        {
            get { return "ALCOOL"; }
        }

        public decimal DegreAlcool
        {
            get { return _degreAlcool; }
            set
            {
                if (value <= 0 || value > 100)
                {
                    throw new ArgumentException("Le degré d'alcool doit être compris entre 0 (exclu) et 100.", nameof(DegreAlcool));
                }

                _degreAlcool = Math.Round(value, 1);   // numeric(4,1)
            }
        }

        public string Provenance
        {
            get { return _provenance; }
            set { _provenance = ValiderNom(value, ProvenanceLongueurMax, nameof(Provenance)); }
        }
    }
}
