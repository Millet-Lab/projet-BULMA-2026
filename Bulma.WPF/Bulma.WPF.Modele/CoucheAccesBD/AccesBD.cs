using Bulma.WPF.Modele.Entites;
using Bulma.WPF.Modele.Enumerations;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.WPF.Modele.CoucheAccesBD
{
    public class AccesBD : IDisposable
    {
        private bool _estDispose;

        // Code retour UPDATE des méthodes de modification de la base des données
        public const int CommandeModifiee = 1;
        public const int CommandeDejaLivree = 0;
        public const int CommandeIntrouvable = -1;

        private readonly string _chaineConnexion;
        
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

            if (parametres != null) 
                cmd.Parameters.AddRange(parametres);

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

        private static string RecipientVersTexte(Recipient recipient)
        {
            return recipient switch
            {
                Recipient.Bouteille => "bouteille",
                Recipient.Canette => "canette",
                _ => throw new ArgumentOutOfRangeException(nameof(recipient))
            };
        }

        public int AjouterProduit(Produit nouveauProduit)
        {
            ArgumentNullException.ThrowIfNull(nouveauProduit);

            if (nouveauProduit.IdProduit < 0)
            {
                throw new ExceptionAccesBD("Ajout du produit impossible. Le produit est déjà enregistré.");
            }

            if (nouveauProduit.EstSupprime)
            {
                throw new ExceptionAccesBD("Ajout du produit impossible. Un produit supprimé ne peut pas être ajouté.");
            }

            const string requeteSqlProduit = @"INSERT INTO produit (
                                                                      nom, 
                                                                      image, 
                                                                      prix_unitaire,
                                                                      gout,
                                                                      quantite_stock,
                                                                      type_produit
                                                                   ) 
                                                            VALUES (
                                                                      @nom,
                                                                      @image,
                                                                      @prixUnitaire,
                                                                      @gout,
                                                                      @quantiteStock,
                                                                      @typeProduit
                                                                   ) 
                                                            RETURNING produit_id;"
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
                                                                    provenance
                                                                 )
                                                          VALUES (
                                                                    @idProduit, 
                                                                    @degreAlcool, 
                                                                    @provenance
                                                                 );"
            ;

            var paramAlcool = new NpgsqlParameter[]
            {
                new("@idProduit", idProduit),
                new("@typeProduit", typeProduit),
                new("@degreAlcool", alcool.DegreAlcool),
                new("@provenance", alcool.Provenance)
            };

            ExecuterNonQuery(requeteSqlAlcool, paramAlcool);
        }

        private void InsererBiere(int idProduit, string typeProduit, Biere biere)
        {
            const string requeteSqlBiere = @"INSERT INTO biere (
                                                                  biere_id, 
                                                                  est_alcoolisee, 
                                                                  recipient
                                                               )
                                                        VALUES (
                                                                  @idProduit, 
                                                                  @estAlcoolisee, 
                                                                  @recipient::recipient
                                                               );"
            ;

            var param = new NpgsqlParameter[]
            {
                new("@idProduit", idProduit),
                new("@typeProduit", typeProduit),
                new("@estAlcoolisee", biere.EstAlcoolisee),
                new("@recipient", RecipientVersTexte(biere.Recipient)) // 'Bouteille' ou 'Canette'
            };

            ExecuterNonQuery(requeteSqlBiere, param);
        }

        private static Recipient LireRecipient(string texte, int idProduit)
        {
            if (!Enum.TryParse(texte, true, out Recipient recipient) || !Enum.IsDefined(recipient))
            {
                throw new ExceptionAccesBD($"Récipient inconnu en base. Produit {idProduit} : valeur « {texte} »");
            }

            return recipient;
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

            List<Produit> listeProduit = [];

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
            ArgumentOutOfRangeException.ThrowIfLessThan(idCmde, 1, "Id valide requis");

            if (!ExisteCommande(idCmde))
                throw new KeyNotFoundException($"La commande ayant l'identifiant {idCmde} " + "n'existe pas dans la base de données.");

            const string requeteSql = @"
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
                LEFT JOIN alcool a ON a.alcool_id = p.produit_id
                LEFT JOIN biere b ON b.biere_id = p.produit_id
                WHERE co.commande_id = @idCmde
                ORDER BY p.nom, p.produit_id;"
            ;

            List<LigneCommande> listeProduitCmdeClient = [];

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.Add("idCmde", NpgsqlTypes.NpgsqlDbType.Integer).Value = idCmde;
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

            /* 
             * le client et la commande sont identiques pour toutes les lignes retournées,
             * puisque la recherche porte sur une seule commande
             */
            Client? client = null;
            Commande? commande = null;

            while (reader.Read())
            {
                // Client et Commande construits uniquement au premier passage dans la boucle
                if (commande is null)
                {
                    var adresse = new Adresse(
                                                reader.GetString(idxRue),
                                                reader.GetString(idxCp),
                                                reader.GetString(idxLocalite)
                                             )
                    ;

                    client = new Client(
                                          reader.GetInt32(idxClientId),
                                          reader.GetString(idxNomClient),
                                          reader.GetString(idxPrenomClient),
                                          adresse,
                                          reader.GetString(idxEmail),
                                          reader.GetString(idxMotDePasseHash)
                                       )
                    ;

                    int idCommande = reader.GetInt32(idxCommandeId);
                    DateOnly dateCommande = reader.GetFieldValue<DateOnly>(idxDateCommande);
                    TimeOnly heureCommande = reader.GetFieldValue<TimeOnly>(idxHeureCommande);
                    decimal montantTotal = reader.GetDecimal(idxMontantTotal);
                    string statutBD = reader.GetString(idxStatut);

                    StatutLivraison statut = statutBD switch
                    {
                        "non_livree" => StatutLivraison.NonLivree,
                        "livree" => StatutLivraison.Livree,
                        _ => throw new InvalidOperationException(
                                $"Statut inconnu : {statutBD}")
                    };

                    commande = new Commande(
                                              idCommande,
                                              client,
                                              dateCommande,
                                              heureCommande,
                                              montantTotal,
                                              statut
                                           )
                    ;
                }

                // lecture du produit de la ligne de commande spécifique
                int idProduit = reader.GetInt32(idxProduitId);
                string nomProduit = reader.GetString(idxNomProduit);
                string image = reader.GetString(idxImage);
                decimal prixUnitaire = reader.GetDecimal(idxPrixUnitaire);
                string gout = reader.GetString(idxGout);
                int quantiteStock = reader.GetInt32(idxQuantiteStock);
                string typeProduit = reader.GetString(idxTypeProduit);
                bool estSupprime = reader.GetBoolean(idxEstSupprime);

                Produit produit;

                switch (typeProduit)
                {
                    case "ALCOOL":
                        decimal degreAlcool = reader.GetDecimal(idxDegreAlcool);
                        string provenance = reader.GetString(idxProvAlcool);

                        produit = new Alcool(idProduit,
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

                        bool conversionReussie = Enum.TryParse(valeurRecipient, ignoreCase: true, out Recipient recipient);

                        if (!conversionReussie || !Enum.IsDefined(recipient))
                        {
                            throw new ExceptionAccesBD( $"Récipient inconnu en base de données. " + $"Produit {idProduit} : " + $"valeur « {valeurRecipient} ».");
                        }

                        produit = new Biere(idProduit,
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
                int quantiteCommandee = reader.GetInt32(idxQuantiteCommande);
                var ligneCommande = new LigneCommande(commande, produit, quantiteCommandee);

                listeProduitCmdeClient.Add(new LigneCommande(commande, produit, quantiteCommandee));
            }

            return listeProduitCmdeClient;
        }

        public List<Commande> ListerCommandesPasseesParClient(int idClientRecherche)
        {
            if (idClientRecherche < 1)
                throw new ArgumentException("Id client valide requis");

            if (!ExisteClientAvecID(idClientRecherche))
                throw new KeyNotFoundException($"Le client ayant l'identifiant {idClientRecherche} " + "n'existe pas dans la base de données.");

            const string requeteSql = @"
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
                       co.statut
                FROM commande co INNER JOIN client c ON c.client_id = co.client_id
                WHERE co.client_id = @idClient
                ORDER BY co.date_commande DESC,
                         co.heure_commande DESC,
                         co.commande_id DESC;"
            ;

            List<Commande> listeDeToutesLesCommandes = [];

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.Add("idClient", NpgsqlTypes.NpgsqlDbType.Integer).Value = idClientRecherche;
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

            Client? clientCommande = null;

            while (reader.Read())
            {
                // L'objet Client est construit une seule fois, toutes les commandes appartiennent au même client
                if (clientCommande is null)
                {
                    var adresse = new Adresse(
                        reader.GetString(idxRue),
                        reader.GetString(idxCp),
                        reader.GetString(idxLocalite))
                    ;

                    clientCommande = new Client(
                        reader.GetInt32(idxClientId),
                        reader.GetString(idxNomClient),
                        reader.GetString(idxPrenomClient),
                        adresse,
                        reader.GetString(idxEmail),
                        reader.GetString(idxMotDePasseHash))
                    ;
                }

                // lecture de la commande spécifique
                int idCommande = reader.GetInt32(idxCommandeId);
                DateOnly dateCommande = reader.GetFieldValue<DateOnly>(idxDateCommande);
                TimeOnly heureCommande = reader.GetFieldValue<TimeOnly>(idxHeureCommande);
                decimal montantTotal = reader.GetDecimal(idxMontantTotal);
                string statutBD = reader.GetString(idxStatut);

                StatutLivraison statut = statutBD switch
                {
                    "non_livree" => StatutLivraison.NonLivree,
                    "livree" => StatutLivraison.Livree,
                    _ => throw new InvalidOperationException($"Statut inconnu : {statutBD}")
                };

                var commande = new Commande(
                                                idCommande,
                                                clientCommande,
                                                dateCommande,
                                                heureCommande,
                                                montantTotal,
                                                statut
                                           )
                ;

                listeDeToutesLesCommandes.Add(commande);
            }

            return listeDeToutesLesCommandes;
        }

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

        public Client InfoClient(int idClient)
        {
            return ChercherClient("client_id = @valeur", idClient);
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

        public List<Client> ListeClient()
        {
            const string requete = @"SELECT client_id, 
                                            nom, 
                                            prenom,

                                            (adresse).rue AS rue, 
                                            (adresse).cp AS cp, 
                                            (adresse).localite AS localite,

                                            email, 
                                            mot_de_passe_hash
                                    FROM client
                                    ORDER BY nom, prenom;"
            ;

            var clients = new List<Client>();

            try
            {
                Ouvrir();
                using var cmd = new NpgsqlCommand(requete, SqlConn, transaction);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clients.Add(LireClient(reader));
                }

                return clients;
            }
            catch (NpgsqlException ex)
            {
                throw new ExceptionAccesBD($"Erreur lors de la lecture des clients : {ex.Message }", ex);
            }
        }

        public decimal MontantTotalCommandesClient(int idClient) 
        {
            List<Commande> listeCommande = ListerCommandesPasseesParClient(idClient);

            return ListerCommandesPasseesParClient(idClient).Sum(commande => commande.TotalMontant);
        }

        // 1 = marquée livrée ; 0 = déjà livrée ; -1 = introuvable
        public int ModifierStatutCommande(Commande commande)
        {
            ArgumentNullException.ThrowIfNull(commande);

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
            ArgumentNullException.ThrowIfNull(produit);

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
                                        WHERE produit_id = @idProduit AND NOT est_supprime
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

        public int SupprimerProduit(Produit produit)
        {
            ArgumentNullException.ThrowIfNull(produit);

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
                                                      );
                                       "
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

            const string requeteSql = @"
                                        SELECT EXISTS (
                                                        SELECT 1
                                                        FROM client
                                                        WHERE email = @email
                                                      );
                                       "
            ;

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("email", email);
            object? estTrouve = cmd.ExecuteScalar();

            return estTrouve is bool existe && existe;
        }

        public bool ExisteCommande(int idCmde)
        {
            if (idCmde < 1)
                return false;

            const string requeteSql = @"
                                        SELECT EXISTS (
                                                        SELECT 1
                                                        FROM commande
                                                        WHERE commande_id = @idCmde
                                                      );
                                       "
            ;

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("idCmde", idCmde);
            object? estTrouve = cmd.ExecuteScalar();

            return estTrouve is bool existe && existe;
        }

        public bool ExisteProduit(int idProduit)
        {
            if (idProduit < 1)
                return false;

            const string requeteSql = @"
                                        SELECT EXISTS (
                                                        SELECT 1
                                                        FROM produit
                                                        WHERE produit_id = @idProduit
                                                      );
                                      "
            ;

            Ouvrir();
            using var cmd = new NpgsqlCommand(requeteSql, SqlConn, transaction);
            cmd.Parameters.AddWithValue("idProduit", idProduit);
            object? estTrouve = cmd.ExecuteScalar();

            return estTrouve is bool existe && existe;
        }
    }
}