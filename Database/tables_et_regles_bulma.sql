-- =============================================
-- *                Nettoyage
-- =============================================
drop table if exists panier_produit cascade;
drop table if exists panier         cascade;
drop table if exists ligne_commande cascade;
drop table if exists commande       cascade;
drop table if exists alcool         cascade;
drop table if exists biere          cascade;
drop table if exists produit        cascade;
drop table if exists client         cascade;

drop function if exists verifier_totalite_produit()  cascade;
drop function if exists verifier_suppression_fille() cascade;

drop type if exists adresse          cascade;
drop type if exists recipient        cascade;
drop type if exists statut_livraison cascade;

-- ===========================================
-- *             Types 
-- ===========================================
create type adresse as (
	rue      varchar(45),  -- taille du plus long nom de rue belge
	cp       char(4),      -- format code postal belge
	localite varchar(26)   -- taille du plus long nom de localité belge
);

create type recipient        as enum ('bouteille', 'canette');
create type statut_livraison as enum ('non_livree',	'livree');

-- ===============================================
-- *                 Tables
-- ===============================================
create table client (
	client_id         integer generated always as identity primary key,
	nom               varchar(30)  not null,
	prenom            varchar(30)  not null,
	adresse           adresse      not null,
	email             varchar(100) not null unique,
	mot_de_passe_hash varchar(255) not null,
	
	constraint check_nom 
		check (char_length(trim(nom)) >= 2 and 
			   nom ~ '^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$'),
	constraint check_prenom 
		check (char_length(trim(prenom)) >= 2 and 
			  prenom ~ '^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$'),
	constraint check_format_email 
		check (email is null or 
			  email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
	constraint check_client_mot_de_passe_longueur 
		check (length(mot_de_passe_hash) >= 8)
);

create table commande (
	commande_id      integer generated always as identity primary key,
	client_id        integer          not null, 
	date_commande    date             not null default current_date,
	heure_commande   time             not null default current_time,
	montant_total    numeric(10,2)    not null,
	statut           statut_livraison not null default 'non_livree',

	constraint check_montant_total check (montant_total >= 0),
	constraint fk_commande_client foreign key (client_id)
		references client(client_id) on delete restrict
);

create table produit (
	produit_id     integer generated always as identity primary key,
	nom            varchar(30)   not null unique,
	image          varchar(255)  not null, -- chemin vers le fichier image
    prix_unitaire  numeric(5,2)  not null,
	gout           varchar(30)   not null,
	quantite_stock integer       not null,
	type_produit   varchar(10)   not null,
	est_supprime   boolean       not null default FALSE,

	constraint check_prix_unitaire  check (prix_unitaire > 0),
    constraint check_quantite_stock check (quantite_stock >= 0),
	constraint check_type_produit   check (type_produit in ('ALCOOL', 'BIERE')),
	constraint uq_produit_id_type   unique (produit_id, type_produit)
);

create table alcool (
    alcool_id    integer primary key,
	degre_alcool numeric(4,1) not null,
	provenance   varchar(30)  not null,
	type_produit varchar(10)  not null default 'ALCOOL',

	constraint check_degre_alcool check (degre_alcool > 0 and degre_alcool <= 100),
	constraint check_alcool_type  check (type_produit = 'ALCOOL'),
	constraint fk_alcool_produit  foreign key (alcool_id, type_produit) 
		references produit(produit_id, type_produit) on delete cascade
);

create table biere (
    biere_id       integer primary key,
	est_alcoolisee boolean     not null,
	recipient      varchar(30) not null,
	type_produit   varchar(10) not null default 'BIERE',

	constraint chk_biere_type   check (type_produit = 'BIERE'),
 	constraint fk_biere_produit foreign key (biere_id, type_produit) 
	 	references produit(produit_id, type_produit) on delete cascade
);

create table ligne_commande (
	commande_id       integer not null,
	produit_id        integer not null,
	quantite_commande integer not null,
	
	constraint pk_ligne_commande          primary key (commande_id, produit_id),
	constraint check_quantite_commande    check (quantite_commande > 0),
	constraint fk_ligne_commande_commande foreign key (commande_id) references commande(commande_id) on delete cascade,
    constraint fk_ligne_commande_produit  foreign key (produit_id)  references produit(produit_id)   on delete cascade
);

create table panier (
	panier_id integer generated always as identity primary key,
	client_id integer not null unique,
	
	constraint fk_panier_client foreign key (client_id) 
		references client(client_id) on delete cascade
);

create table panier_produit (
	panier_id                integer not null,
	produit_id               integer not null,
	quantite_produits_panier integer not null,

	constraint pk_panier_produit         primary key (panier_id, produit_id),
	constraint check_quantite_panier     check (quantite_produits_panier > 0),
	constraint fk_panier_produit_panier  foreign key (panier_id)  references panier(panier_id)   on delete cascade,
    constraint fk_panier_produit_produit foreign key (produit_id) references produit(produit_id) on delete cascade
);

-- ===================================================
-- *           Déclencheurs (triggers)
-- ===================================================

-- TOTALITÉ : chaque produit doit avoir sa ligne fille à la fin de la transaction
create or replace function verifier_totalite_produit()
returns trigger as $$
begin
    if new.type_produit = 'ALCOOL'
       and not exists (select 1 from alcool where alcool_id = new.produit_id) then
        raise exception 'Le produit % (ALCOOL) doit exister dans la table alcool', new.produit_id;
    elsif new.type_produit = 'BIERE'
       and not exists (select 1 from biere where biere_id = new.produit_id) then
        raise exception 'Le produit % (BIERE) doit exister dans la table biere', new.produit_id;
    end if;
    return null;
end;
$$ language plpgsql;

create constraint trigger trg_totalite_produit
after insert or update on produit
deferrable initially deferred
for each row execute function verifier_totalite_produit();

-- Empêche la suppression d'une sous-classe sans supprimer la classe mère
create or replace function verifier_suppression_fille()
returns trigger as $$
begin
    if exists (select 1 from produit
               where produit_id = (to_jsonb(old) ->> tg_argv[0])::integer) then
        raise exception 'Impossible de supprimer la fille % sans supprimer le produit parent', old;
    end if;
    return null;
end;
$$ language plpgsql;

create constraint trigger trg_suppr_alcool
after delete on alcool
deferrable initially deferred
for each row execute function verifier_suppression_fille('alcool_id');

create constraint trigger trg_suppr_biere
after delete on biere
deferrable initially deferred
for each row execute function verifier_suppression_fille('biere_id');