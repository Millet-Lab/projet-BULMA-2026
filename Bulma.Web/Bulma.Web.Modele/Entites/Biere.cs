using Bulma.Web.Modele.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Entites
{
    public class Biere : Produit
    {
        public Biere(Biere biere) : base(biere)
        {
            EstAlcoolisee = biere.EstAlcoolisee;
            Recipient = biere.Recipient;
        }

        // construction de l'objet bière avec identifiant vallant 0
        public Biere(string nom, string image, decimal prixUnitaire, string gout, int quantiteStock, bool estAlcoolisee, Recipient recipient)
            : base(nom, image, prixUnitaire, gout, quantiteStock)
        {
            EstAlcoolisee = estAlcoolisee;
            Recipient = recipient;
        }

        // construction de l'objet bière avec lecture depuis la BD
        public Biere(int idProduit, string nom, string image, decimal prixUnitaire, string gout, int quantiteStock, bool estSupprime, bool estAlcoolisee, Recipient recipient)
            : base(idProduit, nom, image, prixUnitaire, gout, quantiteStock, estSupprime)
        {
            EstAlcoolisee = estAlcoolisee;
            Recipient = recipient;
        }

        public override string TypeProduit
        {
            get { return "BIERE"; }
        }

        public bool EstAlcoolisee { get; set; }

        public Recipient Recipient { get; set; }
    }
}