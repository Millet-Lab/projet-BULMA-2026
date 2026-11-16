#nullable enable
using Bulma.Web.Modele.Entites;
using Bulma.Web.Modele.Enumerations;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.CoucheAccesBD
{
    public class AccesBD : IDisposable
    {
        private bool _estDispose;
        private readonly string _chaineConnexion = @"Server=localhost;
                                                     Port=5433;
                                                     Database=bulma_dev;
                                                     User ID=postgres;
                                                     Password=millet"
        ;
        private NpgsqlTransaction? transaction;

        public NpgsqlConnection SqlConn { get; private set; }

        public AccesBD(string chaineConnexion)
        {
            if (string.IsNullOrWhiteSpace(chaineConnexion))
            {
                throw new ArgumentException("La chaîne de connexion ne peut pas être vide.", nameof(chaineConnexion));
            }

            _chaineConnexion = chaineConnexion;

            try
            {
                SqlConn = new NpgsqlConnection(_chaineConnexion);
                SqlConn.Open();
            }
            catch (NpgsqlException ex)
            {
                SqlConn?.Dispose();
                throw new ExceptionAccesBD("Erreur PostgreSQL lors de la connexion à la base de données.", ex);
            }
            catch (Exception e)
            {
                SqlConn?.Dispose();
                throw new ExceptionAccesBD("Erreur inconnue lors de la tentative de connexion à la base de données.", e);
            }


        }

        /// <summary>
        /// Propriété calculée : Renvoie VRAI si l'objet n'est pas détruit ET que 
        /// la connexion réseau avec SQL est active
        /// </summary>
        public bool EstConnecte => !_estDispose && SqlConn.State == ConnectionState.Open;

        /// <summary>
        /// On indique au Garbage Collector que l'objet a déjà été nettoyé manuellement
        /// en faisant l'ajout crucial de GC.SuppressFinalize(this).
        /// Sécurité : Si l'objet a déjà été nettoyé, on ne fait rien.
        /// </summary>
        public void Dispose()
        {
            if (_estDispose)
            {
                return;
            }

            SqlConn.Dispose();          // Coupe la connexion et libère la mémoire de l'objet PostgreSQL
            _estDispose = true;         // Marque l'objet comme définitivement détruit
            GC.SuppressFinalize(this);  // Indique au Garbage Collector de sauter l'étape du destructeur automatique
        }

        public string GetChaineConnexion => _chaineConnexion;

        /// <summary>
        /// Cette méthode de commande d'ouverture instancie une nouvelle connexion si SqlConn est nul.
        /// Et si la connexion n'est pas déjà ouverte, on l'ouvre.
        /// </summary>
        public void Ouvrir()
        {
            SqlConn ??= new NpgsqlConnection(_chaineConnexion);

            if (SqlConn.State != System.Data.ConnectionState.Open)
                SqlConn.Open();
        }

        /// <summary>
        /// Cette méthode de commande de fermeture vérifie si l'objet existe et que 
        /// le canal est ouvert, le ferme le cas échéant.
        /// </summary>
        public void Fermer()
        {
            if (SqlConn != null && SqlConn.State == System.Data.ConnectionState.Open)
                SqlConn.Close();
        }

        /// <summary>
        /// Cette méthode démarre un bloc transactionnel PostgreSQL en assurant au préalable que
        /// la connexion est active.
        /// </summary>
        public void DebuterTransaction()
        {
            Ouvrir();   // S'assure que la connexion est active
            transaction = SqlConn.BeginTransaction();
        }

        /// <summary>
        /// Cette méthode valide définitivement toutes les écritures SQL en attente si une 
        /// transaction existe. Ensuite, réinitialise la variable transaction.
        /// </summary>
        public void ValiderTransaction()
        {
            transaction?.Commit();
            transaction = null;
        }

        /// <summary>
        /// Cette méthode annule toutes les modifications faites depuis DebuterTransaction() si
        /// un problème survient. Ensuite, réinitialise la variable transaction.
        /// </summary>
        public void AnnulerTransaction()
        {
            transaction?.Rollback();
            transaction = null;
        }

        /// <summary>
        /// Cette méthode exécute la requête et renvoie le nombre de lignes modifiées
        /// pour les ordres INSERT, UPDATE, DELETE
        /// </summary>
        /// <param name="sqlRequete"> la requête textuelle à envoyer à PostgreSQL </param>
        /// <param name="parametres"></param>
        /// <returns></returns>
        public int ExecuterNonQuery(string sqlRequete, NpgsqlParameter[] parametres)
        {
            Ouvrir();
            using var cmd = new NpgsqlCommand(sqlRequete, SqlConn, transaction);
            if (parametres != null) cmd.Parameters.AddRange(parametres);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Cette méthode récupère une valeur unique (ex: un COUNT(*), un MAX(),
        /// ou un ID généré via un RETURNING).
        /// </summary>
        /// <param name="sqlRequete"> la requête textuelle à envoyer à PostgreSQL </param>
        /// <param name="parametres"></param>
        /// <returns></returns>
        public object? ExecuterScalar(string sqlRequete, NpgsqlParameter[] parametres)
        {
            Ouvrir();
            using var cmd = new NpgsqlCommand(sqlRequete, SqlConn, transaction);

            if (parametres != null)
                cmd.Parameters.AddRange(parametres);

            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Cette méthode renvoie un curseur de données permettant de lire les lignes de 
        /// résultats une par une pour les ordres SELECT.
        /// </summary>
        /// <param name="sqlRequete">  la requête textuelle à envoyer à PostgreSQL </param>
        /// <param name="parametres"></param>
        /// <returns></returns>
        public NpgsqlDataReader ExecuterReader(string sqlRequete, NpgsqlParameter[] parametres)
        {
            Ouvrir();
            var cmd = new NpgsqlCommand(sqlRequete, SqlConn, transaction);

            if (parametres != null)
                cmd.Parameters.AddRange(parametres);

            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Cetre méthode prpopre à la classe AccesBD permet d'obtenir le type Adresse valide
        /// à partir d'un texte:
        /// </summary>
        /// <param name="adresse"> une chaîne de caractères </param>
        /// <returns> un type Adresse valide </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        private static Adresse TexteVersAdresse(string adresse)
        {
            if (string.IsNullOrWhiteSpace(adresse))
                throw new ArgumentException("L'adresse ne peut pas être nulle.", nameof(adresse));

            adresse = adresse.Trim('(', ')');
            string[] morceaux = adresse.Split(',');

            if (morceaux.Length != 3)
                throw new FormatException("Format d'adresse invalide.");

            return new Adresse(morceaux[0].Trim(), morceaux[1].Trim(), morceaux[2].Trim());
        }

        private static Recipient LireRecipient(string texte, int idProduit)
        {
            if (!Enum.TryParse(texte, true, out Recipient recipient)
                || !Enum.IsDefined(typeof(Recipient), recipient))
            {
                throw new ExceptionAccesBD($"Récipient inconnu en base. Produit {idProduit} : valeur « {texte} »");
            }

            return recipient;
        }

        public int AjouterProduit(Produit nouveauProduit)
        {
            if (nouveauProduit == null)
            {
                throw new ArgumentNullException(nameof(nouveauProduit));
            }

            if (nouveauProduit.IdProduit != 0)
            {
                throw new ExceptionAccesBD("Ajout du produit impossible. Le produit est déjà enregistré.");
            }

            if (nouveauProduit.EstSupprime)
            {
                throw new ExceptionAccesBD("Ajout du produit impossible. Un produit supprimé ne peut pas être ajouté.");
            }

            const string requeteSqlProduit = @"INSERT INTO produit (nom, 
                                                                 image, 
                                                                 prix_unitaire,
                                                                 gout,
                                                                 quantite_stock,
                                                                 type_produit
                                                                ) 
                                                        VALUES (@nom
                                                                @image,
                                                                @prixUnitaire,
                                                                @gout,
                                                                @quantiteStock,
                                                                @typeProduit
                                                               ) 
                                                   RETURNING id;"
            ;

            try
            {
                DebuterTransaction();
                string typeProduit = nouveauProduit is Alcool ? "ALCOOL" : "BIERE";

                var paramProduit = new NpgsqlParameter[]
                {
                    new("@nom", nouveauProduit.Nom),
                    new("@image", nouveauProduit.Image),
                    new("@prixUnitaire", nouveauProduit.PrixUnitaire),
                    new("@gout", nouveauProduit.Gout),
                    new("@quantiteStock", nouveauProduit.QuantiteEnStock),
                    new("@typeProduit", typeProduit)
                };

                int idProduit = Convert.ToInt32(ExecuterScalar(requeteSqlProduit, paramProduit));

                switch (nouveauProduit)
                {
                    case Alcool alcool:
                        InsererAlcool(idProduit, typeProduit, alcool);
                        break;
                    case Biere biere:
                        InsererBiere(idProduit, typeProduit, biere);
                        break;
                    default:
                        throw new ExceptionAccesBD("Type de produit non géré : " + nouveauProduit.GetType().Name);
                }

                ValiderTransaction();
                nouveauProduit.IdProduit = idProduit;
                return idProduit;
            }
            catch (NpgsqlException ex)
            {
                AnnulerTransaction();
                string details = ex is PostgresException pg ? pg.MessageText : ex.Message;
                throw new ExceptionAccesBD($"Erreur lors de l'ajout du produit : {details}", ex);
            }
        }

        private void InsererAlcool(int idProduit, string typeProduit, Alcool alcool)
        {
            const string requeteSqlAlcool = @"INSERT INTO alcool (
                                                                alcool_id, 
                                                                degre_alcool, 
                                                                provenance)
                                                       VALUES (
                                                                @idProduit, 
                                                                @degreAlcool, 
                                                                @provenance
                                                              )"
            ;

            var paramAlcool = new NpgsqlParameter[]
            {
                new("@idProduit", idProduit),
                new("@degreAlcool", alcool.DegreAlcool),
                new("@provenance", alcool.Provenance),
                new("@typeProduit", typeProduit)
            };

            ExecuterNonQuery(requeteSqlAlcool, paramAlcool);
        }

        private void InsererBiere(int idProduit, string typeProduit, Biere biere)
        {
            const string requeteSqlBiere = @"INSERT INTO biere (
                                                              biere_id, 
                                                              est_alcoolisee, 
                                                              recipient)
                                                     VALUES (
                                                              @idProduit, 
                                                              @alcoolisee, 
                                                              @recipient::recipient
                                                            )"
            ;

            var param = new NpgsqlParameter[]
            {
                new("@id", idProduit),
                new("@estAlc", biere.EstAlcoolisee),
                new("@recip", biere.Recipient.ToString()), // 'Bouteille' ou 'Canette' 
                new("@type", typeProduit)
            };

            ExecuterNonQuery(requeteSqlBiere, param);
        }

        /// <summary>
        /// Cette méthode permet de récupérer la liste des produits disponibles et avec un stock positif
        /// </summary>
        /// <returns> Liste de produits commandables </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public List<Produit> ListerProduits()
        {
            const string requeteSql = @"
                SELECT p.produit_id, 
                       p.nom,
                       p.image,
                       p.prix_unitaire,
                       p.gout,
                       p.quantite_stock,
                       p.est_supprime,
                       p.type_produit,

                       a.degre_alcool,
                       a.provenance as prov_alcool,

                       b.est_alcoolisee,
                       b.recipient

                FROM produit p
                LEFT JOIN alcool a 
                     ON a.alcool_id = p.produit_id
                LEFT JOIN biere b 
                     ON b.biere_id = p.produit_id
                WHERE p.est_supprime = FALSE AND p.quantite_stock > 0
                ORDER BY CASE
                            WHEN a.alcool_id IS NOT NULL THEN 1
                            WHEN b.biere_id IS NOT NULL THEN 2
                            END,
                         p.nom;"
            ;

            List<Produit> listeProduit = new();

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            using var reader = cmd.ExecuteReader();

            // parent : Produit
            int idxIdProduit = reader.GetOrdinal("produit_id");
            int idxNom = reader.GetOrdinal("nom");
            int idxImage = reader.GetOrdinal("image");
            int idxPrixUnitaire = reader.GetOrdinal("prix_unitaire");
            int idxGout = reader.GetOrdinal("gout");
            int idxQuantiteEnStock = reader.GetOrdinal("quantite_stock");
            int idxEstSupprime = reader.GetOrdinal("est_supprime");
            int idxTypeProduit = reader.GetOrdinal("type_produit");

            // enfant : Alcool
            int idxDegreAlcool = reader.GetOrdinal("degre_alcool");
            int idxProvenance = reader.GetOrdinal("prov_alcool");

            // enfant :Biere
            int idxEstAlcoolisee = reader.GetOrdinal("est_alcoolisee");
            int idxRecipient = reader.GetOrdinal("recipient");
            
            while (reader.Read())
            {
                int idProduit = reader.GetInt32(idxIdProduit);
                string nom = reader.GetString(idxNom);
                string image = reader.IsDBNull(idxImage) ? "" : reader.GetString(idxImage);
                decimal prixUnitaire = reader.GetDecimal(idxPrixUnitaire);
                string gout = reader.GetString(idxGout);
                int quantiteEnStock = reader.GetInt32(idxQuantiteEnStock);
                bool estSupprime = reader.GetBoolean(idxEstSupprime);
                string typeProduit = reader.GetString(idxTypeProduit);

                switch (typeProduit)
                {
                    case "ALCOOL":
                        decimal degreAlcool = reader.GetDecimal(idxDegreAlcool);
                        string provenance = reader.GetString(idxProvenance);

                        listeProduit.Add(new Alcool(
                                                    idProduit,
                                                    nom,
                                                    image,
                                                    prixUnitaire,
                                                    gout,
                                                    quantiteEnStock,
                                                    estSupprime,
                                                    degreAlcool,
                                                    provenance
                                                   )
                        );
                        break;
                    case "BIERE":
                        bool estAlcoolisee = reader.GetBoolean(idxEstAlcoolisee);
                        Recipient recipient = LireRecipient(reader.GetString(idxRecipient), idProduit);

                        listeProduit.Add(new Biere(
                                                   idProduit,
                                                   nom,
                                                   image,
                                                   prixUnitaire,
                                                   gout,
                                                   quantiteEnStock,
                                                   estSupprime,
                                                   estAlcoolisee,
                                                   recipient
                                                  )
                        );
                        break;
                    default:
                        throw new InvalidOperationException($"Type de produit inconnu : {typeProduit}");
                }
            }

            return listeProduit;
        }

        public List<Produit> ListerProduitsParType(string typeProduit)
        {
            if (string.IsNullOrWhiteSpace(typeProduit))
                throw new ArgumentException("Type produit requis");

            typeProduit = typeProduit.ToUpper();

            return ListerProduits().Where(p => p.TypeProduit == typeProduit).ToList();
        }

        /// <summary>
        /// Cette méthode liste tous les produits faisant partie d'une commande spécifique
        /// </summary>
        /// <param name="idCmde"> l'identifiant de la commande spécifique </param>
        /// <returns> Une liste contenant tous les produits de la commande spécifique </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public List<LigneCommande> ListerProduitsCmdeClient(int idCmde)
        {
            if (idCmde < 1)
                throw new ArgumentException("Id valide requis");

            string requeteSql = @"
                SELECT c.client_id,
                       c.nom                AS nom_client,
                       c.prenom             AS prenom_client,
                       (c.adresse).rue      AS rue,
                       (c.adresse).cp       AS cp,
                       (c.adresse).localite AS localite,
                       c.email,
                       c.mot_de_passe_hash,

                       co.commande_id,
                       co.date_commande,
                       co.heure_commande,
                       co.montant_total,
                       co.statut,

                       lc.quantite_commande,
                       
                       p.produit_id,
                       p.nom                AS nom_produit,
                       p.image,
                       p.prix_unitaire,
                       p.gout,
                       p.quantite_stock,
                       p.type_produit,
                       p.est_supprime,

                       a.degre_alcool,
                       a.provenance         AS prov_alcool,

                       b.est_alcoolisee,
                       b.recipient
                FROM commande co
                INNER JOIN client c ON c.client_id = co.client_id
                INNER JOIN ligne_commande lc ON lc.commande_id = co.commande_id
                INNER JOIN produit p ON p.produit_id = lc.produit_id
                LEFT JOIN alcool a ON a.produit_id = p.produit_id
                LEFT JOIN biere b ON b.produit_id = p.produit_id
                WHERE co.commande_id = @idCmde;"
            ;

            List<LigneCommande> listeProduitCmdeClient = new();

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("idCmde", idCmde);
            using var reader = cmd.ExecuteReader();

            // Client
            int idxClientId = reader.GetOrdinal("client_id");
            int idxNomClient = reader.GetOrdinal("nom_client");
            int idxPrenomClient = reader.GetOrdinal("prenom_client");
            int idxRue = reader.GetOrdinal("rue");
            int idxCp = reader.GetOrdinal("cp");
            int idxLocalite = reader.GetOrdinal("localite");
            int idxEmail = reader.GetOrdinal("email");
            int idxMotDePasseHash = reader.GetOrdinal("mot_de_passe_hash");

            // Commande
            int idxCommandeId = reader.GetOrdinal("commande_id");
            int idxDateCommande = reader.GetOrdinal("date_commande");
            int idxHeureCommande = reader.GetOrdinal("heure_commande");
            int idxMontantTotal = reader.GetOrdinal("montant_total");
            int idxStatut = reader.GetOrdinal("statut");

            // LigneCommande
            int idxQuantiteCommande = reader.GetOrdinal("quantite_commande");

            // Produit
            int idxProduitId = reader.GetOrdinal("produit_id");
            int idxNomProduit = reader.GetOrdinal("nom_produit");
            int idxImage = reader.GetOrdinal("image");
            int idxPrixUnitaire = reader.GetOrdinal("prix_unitaire");
            int idxGout = reader.GetOrdinal("gout");
            int idxQuantiteStock = reader.GetOrdinal("quantite_stock");
            int idxTypeProduit = reader.GetOrdinal("type_produit");
            int idxEstSupprime = reader.GetOrdinal("est_supprime");

            // Alcool
            int idxDegreAlcool = reader.GetOrdinal("degre_alcool");
            int idxProvAlcool = reader.GetOrdinal("prov_alcool");

            // Bière
            int idxEstAlcoolisee = reader.GetOrdinal("est_alcoolisee");
            int idxRecipient = reader.GetOrdinal("recipient");

            while (reader.Read())
            {
                // lecture du client
                int idClient = reader.GetInt32(idxClientId);
                string nomClient = reader.GetString(idxNomClient);
                string prenomClient = reader.GetString(idxPrenomClient);

                string rue = reader.GetString(idxRue);
                string cp = reader.GetString(idxCp);
                string localite = reader.GetString(idxLocalite);
                Adresse adresse = new(
                                        rue,
                                        cp,
                                        localite
                                      )
                ;

                string email = reader.GetString(idxEmail);
                string motPasseHash = reader.GetString(idxMotDePasseHash);

                var clientLC = new Client(
                                            idClient,
                                            nomClient,
                                            prenomClient,
                                            adresse,
                                            email,
                                            motPasseHash
                                         )
                ;

                // lecture de la commande spécifique
                int idCommande = reader.GetInt32(idxCommandeId);
                DateTime dateCommande = reader.GetFieldValue<DateTime>(idxDateCommande);
                TimeSpan heureCommande = reader.GetFieldValue<TimeSpan>(idxHeureCommande);
                decimal montantTotal = reader.GetDecimal(idxMontantTotal);
                StatutLivraison statut = (StatutLivraison)reader.GetInt32(idxStatut);

                var commandeLC = new Commande(
                                                idCommande,
                                                clientLC,
                                                dateCommande,
                                                heureCommande,
                                                montantTotal,
                                                statut
                                             )
                ;

                // lecture du produit de la ligne de commande spécifique
                int idProduit = reader.GetInt32(idxProduitId);
                string nomProduit = reader.GetString(idxNomProduit);
                string image = reader.GetString(idxImage);
                decimal prixUnitaire = reader.GetDecimal(idxPrixUnitaire);
                string gout = reader.GetString(idxGout);
                int quantiteStock = reader.GetInt32(idxQuantiteStock);
                string typeProduit = reader.GetString(idxTypeProduit);
                bool estSupprime = reader.GetBoolean(idxEstSupprime);

                Produit produitLC;

                switch (typeProduit)
                {
                    case "ALCOOL":
                        decimal degreAlcool = reader.GetDecimal(idxDegreAlcool);
                        string provenance = reader.GetString(idxProvAlcool);

                        produitLC = new Alcool(idProduit,
                                               nomProduit,
                                               image,
                                               prixUnitaire,
                                               gout,
                                               quantiteStock,
                                               estSupprime,
                                               degreAlcool,
                                               provenance
                                              )
                        ;
                        break;
                    case "BIERE":
                        bool estAlcoolisee = reader.GetBoolean(idxEstAlcoolisee);
                        string valeurRecipient = reader.GetString(idxRecipient);
                        Recipient recipient = LireRecipient(valeurRecipient, idProduit);

                        produitLC = new Biere(idProduit,
                                              nomProduit,
                                              image,
                                              prixUnitaire,
                                              gout,
                                              quantiteStock,
                                              estSupprime,
                                              estAlcoolisee,
                                              recipient
                                             )
                        ;
                        break;
                    default:
                        throw new InvalidOperationException($"Type de produit inconnu " + $"{idProduit} : {typeProduit}");
                }

                // lecture de la quantité de la ligne commande
                int qteCmdeLC = reader.GetInt32(reader.GetOrdinal("quantite_commande"));

                var ligneCommande = new LigneCommande(
                                                        commandeLC,
                                                        produitLC,
                                                        qteCmdeLC
                                                     )
                ;

                listeProduitCmdeClient.Add(new LigneCommande(commandeLC, produitLC, qteCmdeLC));
            }

            return listeProduitCmdeClient;
        }

        // Code retour UPDATE des méthodes de modification de la base des données
        private const int CommandeModifiee = 1;
        private const int CommandeDejaLivree = 0;
        private const int CommandeIntrouvable = -1;

        public int ModifierStatutCommande(Commande commande)
        {
            if (commande == null)
            {
                throw new ArgumentNullException(nameof(commande));
            }

            if (commande.IdCommande < 1)
            {
                throw new ArgumentException("La commande n'est pas enregistrée.", nameof(commande));
            }

            const string requeteSql = @"
                                        UPDATE commande
                                        SET statut = 'livree'
                                        WHERE commande_id = @idCmde AND statut = 'non_livree'
                                      ;"
            ;

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
                cmd.Parameters.AddWithValue("idCmde", commande.IdCommande);

                int lignesModifiees = cmd.ExecuteNonQuery();

                if (lignesModifiees == 1)
                {
                    if (commande.StatutLivraison != StatutLivraison.Livree)
                    {
                        commande.MarquerCommandeLivree();
                    }

                    return CommandeModifiee;
                }

                // aucune ligne modifiée : soit la commande n'existe pas, soit elle est déjà livrée
                return ExisteCommande(commande.IdCommande) ? CommandeDejaLivree : CommandeIntrouvable;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la modification du statut de la commande :  {ex.Message}", ex);
            }

        }

        public int ModifierQuantiteEnStock(Produit produit, int nouveauStock)
        {
            if (produit == null)
            {
                throw new ArgumentNullException(nameof(produit));
            }

            if (produit.IdProduit < 1)
            {
                throw new ArgumentException("Le produit n'est pas enregistrée.", nameof(produit));
            }

            if (nouveauStock < 0)
            {
                throw new ArgumentException("Quantité positive requise !", nameof(nouveauStock));
            }

            const string requeteSql = @"
                                        UPDATE produit
                                        SET quantite_stock = @nouveauStock
                                        WHERE produit_id = @idProduit AND est_supprime = FALSE
                                      ;"
            ;
            NpgsqlParameter[] param = [new("idProduit", produit.IdProduit), new("nouveauStock", nouveauStock)];

            int lignesModifiees = ExecuterNonQuery(requeteSql, param);

            if (lignesModifiees == 0)
            {
                throw new ArgumentException("Le produit est introuvable ou a été supprimé.", nameof(produit));
            }

            return lignesModifiees;
        }

        /// <summary>
        /// Crée le client ET son panier vide dans la même transaction
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ExceptionAccesBD"></exception>
        public int AjouterClient(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (client.IdClient != 0)
            {
                throw new ExceptionAccesBD("Ajout du client impossible : le client est déjà enregistré !");
            }

            const string requeteClient = @"INSERT INTO client (
                                                                nom, 
                                                                prenom, 
                                                                adresse,
                                                                email, 
                                                                mot_de_passe_hash
                                                              )
                                                       VALUES (
                                                                @nom, 
                                                                @prenom, 
                                                                ROW(@rue, @cp, @localite), 
                                                                @email, 
                                                                @hash
                                                              )
                                            RETURNING client_id;"
            ;
            
            const string requetePanier = "INSERT INTO panier (client_id) VALUES (@idClient);";

            try
            {
                Ouvrir();
                using (NpgsqlTransaction transaction = SqlConn.BeginTransaction())
                {
                    int idClient;
                    Adresse adresse = client.AdresseClient;

                    using (var cmd = new NpgsqlCommand(requeteClient, SqlConn, transaction))
                    {
                        cmd.Parameters.AddWithValue("nom", client.Nom);
                        cmd.Parameters.AddWithValue("prenom", client.Prenom);
                        cmd.Parameters.AddWithValue("rue", adresse.Rue);
                        cmd.Parameters.AddWithValue("cp", adresse.CodePostal);
                        cmd.Parameters.AddWithValue("localite", adresse.Localite);
                        cmd.Parameters.AddWithValue("email", client.Email);
                        cmd.Parameters.AddWithValue("hash", client.MotDePasseHash);
                        idClient = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (var cmd = new NpgsqlCommand(requetePanier, SqlConn, transaction))
                    {
                        cmd.Parameters.AddWithValue("idClient", idClient);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    client.IdClient = idClient;
                    return idClient;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // deux inscriptions simultanées avec le même email : la contrainte unique de la base tranche
                throw new ExceptionAccesBD($"Cette adresse email est déjà utilisée : {ex.MessageText}", ex);
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de l'enregistrement du client : {ex.Message}", ex);
            }
        }

        private const string ColonnesClient = @"client_id, 
                                                nom, 
                                                prenom,
                                                (adresse).rue AS rue, 
                                                (adresse).cp AS cp, 
                                                (adresse).localite AS localite,
                                                email, 
                                                mot_de_passe_hash"
        ;

        private static Client LireClient(NpgsqlDataReader reader)
        {
            var adresse = new Adresse(reader.GetString(reader.GetOrdinal("rue")),
                                      reader.GetString(reader.GetOrdinal("cp")).Trim(),
                                      reader.GetString(reader.GetOrdinal("localite")))
            ;

            return new Client(reader.GetInt32(reader.GetOrdinal("client_id")),
                              reader.GetString(reader.GetOrdinal("nom")),
                              reader.GetString(reader.GetOrdinal("prenom")),
                              adresse,
                              reader.GetString(reader.GetOrdinal("email")),
                              reader.GetString(reader.GetOrdinal("mot_de_passe_hash")))
            ;
        }

        public Client GetClientEmail(string email)
        {
            return ChercherClient("email = @valeur", email);
        }

        public Client InfoClient(int idClient)
        {
            return ChercherClient("client_id = @valeur", idClient);
        }

        private Client ChercherClient(string condition, object valeur)
        {
            string requete = "SELECT " + ColonnesClient + " FROM client WHERE " + condition + ";";

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand(requete, SqlConn);
                cmd.Parameters.AddWithValue("valeur", valeur);

                using var reader = cmd.ExecuteReader();
                    
                return reader.Read() ? LireClient(reader) : null!;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la recherche du client : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idClient"></param>
        /// <param name="idProduit"></param>
        /// <param name="quantite"></param>
        /// <returns>Retourne 1 si le produit est ajouté ou sa quantité cumulée, 0 s'il est supprimé,
        /// inexistant ou si la quantité totale dépasse le stock
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ExceptionAccesBD"></exception>
        public int AjouterAuPanier(int idClient, int idProduit, int quantite)
        {
            if (idClient < 1 || idProduit < 1 || quantite < 1)
            {
                throw new ArgumentException("Paramètres invalides pour AjouterAuPanier().");
            }

            const string requeteSql = @"INSERT INTO panier_produit (panier_id, 
                                                                    produit_id, 
                                                                    quantite_produits_panier
                                                                   )
                                            SELECT pa.panier_id, p.produit_id, @quantite
                                            FROM panier pa CROSS JOIN produit p
                                            WHERE pa.client_id = @idClient 
                                                AND p.produit_id = @idProduit
                                                AND NOT p.est_supprime 
                                                AND p.quantite_stock >= @quantite
                                            ON CONFLICT (panier_id, produit_id) DO UPDATE
                                            SET quantite_produits_panier = panier_produit.quantite_produits_panier + EXCLUDED.quantite_produits_panier
                                            WHERE panier_produit.quantite_produits_panier + EXCLUDED.quantite_produits_panier <=
                                            (SELECT quantite_stock FROM produit WHERE produit_id = EXCLUDED.produit_id);"
            ;

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand(requeteSql, SqlConn);
                
                cmd.Parameters.AddWithValue("idClient", idClient);
                cmd.Parameters.AddWithValue("idProduit", idProduit);
                cmd.Parameters.AddWithValue("quantite", quantite);
                return cmd.ExecuteNonQuery();
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de l'ajout au panier : {ex.Message}", ex);
            }
        }

        private Panier ObtenirPanier(int idClient)
        {
            Client client = InfoClient(idClient) ?? throw new ExceptionAccesBD($"Client introuvable : idClient = {idClient}");

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand("SELECT panier_id FROM panier WHERE client_id = @c;", SqlConn);
                
                cmd.Parameters.AddWithValue("c", idClient);
                object id = cmd.ExecuteScalar()!;

                return id == null ? throw new ExceptionAccesBD($"Panier introuvable : idClient = ") : new Panier(Convert.ToInt32(id), client);
                
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la lecture du panier : {ex.Message}", ex);
            }
        }

        public List<LignePanier> ContenuPanier(int idClient)
        {
            if (idClient < 1)
            {
                throw new ArgumentException("Identifiant de client invalide.", nameof(idClient));
            }

            Panier panier = ObtenirPanier(idClient);

            const string requeteSql = @"SELECT pp.quantite_produits_panier,
                                            p.produit_id, 
                                            p.nom, 
                                            p.image, 
                                            p.prix_unitaire, 
                                            p.gout,
                                            p.quantite_stock, 
                                            p.type_produit::text AS type_produit, 
                                            p.est_supprime,
               
                                            a.degre_alcool, 
                                            a.provenance AS prov_alcool,

                                            b.est_alcoolisee, 
                                            b.recipient::text AS recipient
                                     FROM panier_produit pp JOIN produit p ON p.produit_id = pp.produit_id
                                     LEFT JOIN alcool a ON a.alcool_id  = p.produit_id
                                     LEFT JOIN biere b  ON b.biere_id   = p.produit_id
                                     WHERE pp.panier_id = @idPanier
                                     ORDER BY p.nom;"
            ;

            var lignes = new List<LignePanier>();

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand(requeteSql, SqlConn);
                
                cmd.Parameters.AddWithValue("idPanier", panier.IdPanier);

                using var reader = cmd.ExecuteReader();
                    
                while (reader.Read())
                {
                    Produit produit = ConstruireProduit(reader);
                    int quantite = reader.GetInt32(reader.GetOrdinal("quantite_produits_panier"));
                    lignes.Add(new LignePanier(panier, produit, quantite));
                }
                
                return lignes;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la lecture du contenu du panier : {ex.Message}", ex);
            }
        }

        private static Produit ConstruireProduit(NpgsqlDataReader reader)
        {
            int id = reader.GetInt32(reader.GetOrdinal("produit_id"));
            string nom = reader.GetString(reader.GetOrdinal("nom"));
            string image = reader.GetString(reader.GetOrdinal("image"));
            decimal prix = reader.GetDecimal(reader.GetOrdinal("prix_unitaire"));
            string gout = reader.GetString(reader.GetOrdinal("gout"));
            int stock = reader.GetInt32(reader.GetOrdinal("quantite_stock"));
            bool supprime = reader.GetBoolean(reader.GetOrdinal("est_supprime"));
            string type = reader.GetString(reader.GetOrdinal("type_produit"));

            return type switch
            {
                "ALCOOL" => new Alcool(id, 
                                       nom, 
                                       image, 
                                       prix, 
                                       gout, 
                                       stock, 
                                       supprime, 
                                       reader.GetDecimal(reader.GetOrdinal("degre_alcool")), 
                                       reader.GetString(reader.GetOrdinal("prov_alcool"))),

                "BIERE" => new Biere(id, 
                                     nom, 
                                     image, 
                                     prix, 
                                     gout, 
                                     stock, 
                                     supprime, 
                                     reader.GetBoolean(reader.GetOrdinal("est_alcoolisee")), 
                                     LireRecipient(reader.GetString(reader.GetOrdinal("recipient")), id)),

                _ => throw new ExceptionAccesBD($"Type de produit inconnu : {type}"),
            };
        }

        private static string StatutVersTexte(StatutLivraison statut)
        {
            return statut switch
            {
                StatutLivraison.Livree => "livree",
                StatutLivraison.NonLivree => "non_livree",
                _ => throw new ArgumentOutOfRangeException(nameof(statut))
            };
        }

        private static StatutLivraison TexteVersStatut(string texte)
        {
            if(texte == null)
            {
                throw new ArgumentNullException(nameof(texte));
            }

            return texte.ToLowerInvariant() switch
            {
                "livree" => StatutLivraison.Livree,
                "non_livree" => StatutLivraison.NonLivree,
                _ => throw new ArgumentOutOfRangeException(nameof(texte), $"Statut inconnu : {texte}")
            };
        }

        public int AjouterCommande(Commande commande, List<LigneCommande> lignes)
        {
            if (commande == null) { throw new ArgumentNullException(nameof(commande)); }
            if (lignes == null) { throw new ArgumentNullException(nameof(lignes)); }

            if (commande.IdCommande != 0)
            {
                throw new ExceptionAccesBD("Ajout de la commande impossible : La commande est déjà enregistrée.");
            }

            if (lignes.Count == 0)
            {
                throw new ExceptionAccesBD("Ajout de la commande impossible : Une commande doit contenir au moins un produit.");
            }

            const string sqlCommande = @"INSERT INTO commande (
                                                                client_id, 
                                                                date_commande, 
                                                                heure_commande, 
                                                                montant_total, 
                                                                statut
                                                              )
                                                       VALUES (
                                                                @client, 
                                                                @date, 
                                                                @heure, 
                                                                @montant, 
                                                                @statut::statut_livraison
                                                              )
                                                       RETURNING commande_id;"
            ;

            // le WHERE garantit que le stock ne devient jamais négatif, même avec deux commandes simultanées
            const string sqlStock = @"UPDATE produit 
                                      SET quantite_stock = quantite_stock - @q
                                      WHERE produit_id = @prod AND quantite_stock >= @q AND NOT est_supprime;";

            const string sqlLigne = @"INSERT INTO ligne_commande (
                                                                    commande_id, 
                                                                    produit_id, 
                                                                    quantite_commande
                                                                 )
                                                          VALUES (
                                                                    @cmd, 
                                                                    @prod, 
                                                                    @q
                                                                 );"
            ;

            const string sqlViderPanier = @"DELETE FROM panier_produit
                                            WHERE panier_id = (SELECT panier_id FROM panier WHERE client_id = @client);"
            ;

            try
            {
                Ouvrir();
                using (NpgsqlTransaction tx = SqlConn.BeginTransaction())
                {
                    int idCommande;

                    using (var cmd = new NpgsqlCommand(sqlCommande, SqlConn, tx))
                    {
                        cmd.Parameters.AddWithValue("client", commande.ClientCommande.IdClient);
                        cmd.Parameters.Add(new NpgsqlParameter("date", NpgsqlDbType.Date) { Value = commande.DateCommande });
                        cmd.Parameters.Add(new NpgsqlParameter("heure", NpgsqlDbType.Time) { Value = commande.HeureCommande });
                        cmd.Parameters.AddWithValue("montant", commande.TotalMontant);
                        cmd.Parameters.AddWithValue("statut", StatutVersTexte(commande.StatutLivraison));
                        idCommande = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // ordre fixe (par produit) : évite les verrous croisés entre deux commandes simultanées
                    foreach (LigneCommande ligne in lignes.OrderBy(l => l.ProduitLigneCommande.IdProduit))
                    {
                        using (var cmd = new NpgsqlCommand(sqlStock, SqlConn, tx))
                        {
                            cmd.Parameters.AddWithValue("q", ligne.QuantiteLigneCommande);
                            cmd.Parameters.AddWithValue("prod", ligne.ProduitLigneCommande.IdProduit);

                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                // la transaction est annulée à la sortie du using : rien n'est enregistré
                                throw new ExceptionAccesBD("Stock insuffisant ou produit indisponible : "
                                    + ligne.ProduitLigneCommande.Nom + ". produit_id = " + ligne.ProduitLigneCommande.IdProduit);
                            }
                        }

                        using (var cmd = new NpgsqlCommand(sqlLigne, SqlConn, tx))
                        {
                            cmd.Parameters.AddWithValue("cmd", idCommande);
                            cmd.Parameters.AddWithValue("prod", ligne.ProduitLigneCommande.IdProduit);
                            cmd.Parameters.AddWithValue("q", ligne.QuantiteLigneCommande);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var cmd = new NpgsqlCommand(sqlViderPanier, SqlConn, tx))
                    {
                        cmd.Parameters.AddWithValue("client", commande.ClientCommande.IdClient);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    commande.IdCommande = idCommande;
                    return idCommande;
                }
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de l'enregistrement de la commande : {ex.Message}", ex);
            }
        }

        // Commandes d'un client, la plus récente d'abord. Le client est chargé une seule fois
        public List<Commande> ListerCommandesPasseesParClient(int idClient)
        {
            if (idClient < 1)
            {
                throw new ArgumentException("Identifiant de client invalide.", nameof(idClient));
            }

            Client client = InfoClient(idClient) ?? throw new ExceptionAccesBD("Client introuvable. idClient = " + idClient);

            const string requeteSql = @"SELECT commande_id, 
                                            date_commande, 
                                            heure_commande, 
                                            montant_total, 
                                            statut::text AS statut
                                    FROM commande
                                    WHERE client_id = @c
                                    ORDER BY date_commande DESC, 
                                             heure_commande DESC, 
                                             commande_id DESC;"
            ;

            var commandes = new List<Commande>();

            try
            {
                Ouvrir();
                using (var cmd = new NpgsqlCommand(requeteSql, SqlConn))
                {
                    cmd.Parameters.AddWithValue("c", idClient);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            commandes.Add(new Commande(
                                reader.GetInt32(reader.GetOrdinal("commande_id")),
                                client,
                                reader.GetDateTime(reader.GetOrdinal("date_commande")),
                                reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("heure_commande")),
                                reader.GetDecimal(reader.GetOrdinal("montant_total")),
                                TexteVersStatut(reader.GetString(reader.GetOrdinal("statut")))));
                        }
                    }
                }

                return commandes;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la lecture des commandes {ex.Message}", ex);
            }
        }

        // Aucun filtre sur est_supprime : un produit supprimé reste visible dans les commandes déjà passées
        public List<LigneCommande> ListerProduitsCommande(Commande commande)
        {
            if (commande == null) { throw new ArgumentNullException(nameof(commande)); }

            const string requete = @"SELECT lc.quantite_commande,
               
                                            p.produit_id, 
                                            p.nom, 
                                            p.image, 
                                            p.prix_unitaire, 
                                            p.gout,
                                            p.quantite_stock, 
                                            p.type_produit::text AS type_produit, 
                                            p.est_supprime,

                                            a.degre_alcool, 
                                            a.provenance AS prov_alcool,

                                            b.est_alcoolisee, 
                                            b.recipient::text AS recipient
                                    FROM ligne_commande lc
                                    JOIN produit p ON p.produit_id = lc.produit_id
                                    LEFT JOIN alcool a ON a.alcool_id = p.produit_id
                                    LEFT JOIN biere b  ON b.biere_id = p.produit_id
                                    WHERE lc.commande_id = @idCmde
                                    ORDER BY p.nom_produit;"
            ;

            var lignes = new List<LigneCommande>();

            try
            {
                Ouvrir();
                using (var cmd = new NpgsqlCommand(requete, SqlConn))
                {
                    cmd.Parameters.AddWithValue("idCmde", commande.IdCommande);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lignes.Add(new LigneCommande(
                                commande,
                                ConstruireProduit(reader),
                                reader.GetInt32(reader.GetOrdinal("quantite_commande"))));
                        }
                    }
                }

                return lignes;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la lecture des produits de la commande : {ex.Message}", ex);
            }
        }

        public int SupprimerProduit(Produit produit)
        {
            if (produit == null)
            {
                throw new ArgumentNullException(nameof(produit));
            }

            if (!ExisteProduit(produit.IdProduit))
            {
                throw new ArgumentException("Erreur : Le produit n'existe pas dans la base de données !", nameof(produit));
            }

            produit.Supprimer(); // suppression logique pour la base de données

            string requeteSql = @"
                                    UPDATE produit
                                    SET est_supprime = TRUE
                                    WHERE produit_id = @idProduit AND est_supprime = FALSE
                                ;"
            ;

            NpgsqlParameter[] param = [new("idProduit", produit.IdProduit)];

            int lignesModifiees = ExecuterNonQuery(requeteSql, param);

            if (lignesModifiees == 0)
            {
                throw new InvalidOperationException("Produit introuvable ou déjà supprimé.");
            }

            return lignesModifiees;
        }

        public bool ExisteClientAvecID(int idClient)
        {
            if (idClient <= 0)
                return false; // O : identifiant de table nouvellement créée et non existante en BD

            const string requeteSql = @"
                                        SELECT EXISTS (
                                                        SELECT 1
                                                        FROM client
                                                        WHERE client_id = @idClient
                                                      )
                                      ;"
            ;

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("idClient", idClient);
            object? estTrouve = cmd.ExecuteScalar();

            return estTrouve is bool existe && existe;
        }

        public bool ExisteClientAvecEmail(string email)
        {
            if (email == null)
                return false;

            const string requeteSql = @"SELECT EXISTS (
                                            SELECT 1
                                            FROM client
                                            WHERE email = @email
                                                      );"
            ;

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand(requeteSql, SqlConn);
                cmd.Parameters.AddWithValue("email", email);
                
                return (bool)cmd.ExecuteScalar()!;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la vérification de l'adresse email : {ex.Message}", ex);
            }
        }

        public bool ExisteCommande(int idCmde)
        {
            if (idCmde <= 0)
                return false; // O : représente un identifiant qui n'a pas encore été attribué par la BD

            const string requeteSql = @"
                                        SELECT EXISTS (
                                                        SELECT 1
                                                        FROM commande
                                                        WHERE commande_id = @idCmde
                                                      )
                                      ;"
            ;

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("idCmde", idCmde);
            object? estTrouve = cmd.ExecuteScalar();

            return estTrouve is bool existe && existe;
        }

        public bool ExisteProduit(int idProduit)
        {
            if (idProduit <= 0)
                return false; // O : représente un identifiant qui n'a pas encore été attribué par la BD

            const string requeteSql = @"
                                        SELECT EXISTS (
                                                        SELECT 1
                                                        FROM produit
                                                        WHERE produit_id = @idProduit
                                                      )
                                      ;"
            ;

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("idProduit", idProduit);
            object? estTrouve = cmd.ExecuteScalar();

            return estTrouve is bool existe && existe;
        }
    }
}