drop table if exists "Wallets";
drop table if exists "Accounts";
drop table if exists "Nodes";
drop table if exists "Users";

create table "Users"
(
    "Login" text primary key,
    "Name" text not null,
    "Email" text null, 
    "Description" text,
	"Rights" integer not null,
	"PasswordHash" bytea null,
	"ValidFrom" timestamp null, --null==unlimited
	"ValidTo" timestamp null,   --null==unilimited
	"Active" boolean not null default false,
    "CreatedBy" text not null,
    "CreatedOn" timestamp not null,
	"ModifiedBy" text not null,
    "ModifiedOn" timestamp not null
);


-------------------------------------------------------------------------------

create table "Wallets"
(
    "Id" text primary key,
    "Name" text,
	"Type" integer not null,
	"Host" text,
	"Port" integer not null,
	"CreatedBy" text not null,
    "CreatedOn" timestamp not null,
	"ModifiedBy" text not null,
    "ModifiedOn" timestamp not null,
	"Version" text,
	"Deleted" boolean not null default false
);

insert into "Wallets" ("Id", "Name", "Type", "Host", "Port", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "Version") values
('41ea1423-199f-432c-af3d-9b6181f77f3b', 'TestWallet', 1, '127.0.0.1', 6326, 'fyziktom', now(),'fyziktom', now(), '0.1'),
('bbaa9c15-90bb-469e-8fb5-2dfabbe2f063', 'UserWallet', 1, '127.0.0.1', 6326, 'fyziktom', now(),'fyziktom', now(), '0.1');

--select * from "Wallets"

----------------------------------------------------------------------------------

create table "Accounts"
(
    "Id" text primary key,
    "Name" text,
	"Address" text,
	"WalletId" text references "Wallets"("Id"),
	"Type" integer not null,
	"CreatedBy" text not null,
    "CreatedOn" timestamp not null,
	"ModifiedBy" text not null,
    "ModifiedOn" timestamp not null,
	"Version" text,
	"Deleted" boolean not null default false
);

insert into "Accounts" ("Id", "Name", "Address", "WalletId", "Type", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "Version") values
('fc5df2f1-6def-4f08-973b-d9584e85b4d2', 'TestAccount', 'Account Address','41ea1423-199f-432c-af3d-9b6181f77f3b', 1, 'fyziktom', now(),'fyziktom', now(), '0.1'),
('dfdbb7e2-2633-432a-85af-1d056f2f49ba', 'UserAccount', 'Account Address','41ea1423-199f-432c-af3d-9b6181f77f3b', 1, 'fyziktom', now(),'fyziktom', now(), '0.1'),
('ff985816-d382-4342-974e-3ffd4db3b8fc', 'FriendAccount', 'Account Address','bbaa9c15-90bb-469e-8fb5-2dfabbe2f063', 1, 'fyziktom', now(),'fyziktom', now(), '0.1');

--select * from "Accounts"

----------------------------------------------------------------------------------

create table "Nodes"
(
    "Id" text primary key,
    "Name" text,
	"Address" text,
	"AccountId" text references "Accounts"("Id"),
	"Parameters" text,
	"IsActivated" boolean not null default false,
	"Type" integer not null,
	"CreatedBy" text not null,
    "CreatedOn" timestamp not null,
	"ModifiedBy" text not null,
    "ModifiedOn" timestamp not null,
	"Version" text,
	"Deleted" boolean not null default false
);

insert into "Nodes" ("Id", "Name", "AccountId", "Parameters", "IsActivated", "Type", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "Version") values
('08a56ea1-8ccd-43dc-ba77-0bfb188c2665', 'TestNode','fc5df2f1-6def-4f08-973b-d9584e85b4d2', 'NO PARAMETERS', false, 2, 'fyziktom', now(),'fyziktom', now(), '0.1'),
('49088487-fe45-433d-a524-7cacfa9f7dcd', 'UserNode','dfdbb7e2-2633-432a-85af-1d056f2f49ba', 'NO PARAMETERS', false, 3, 'fyziktom', now(),'fyziktom', now(), '0.1');

--select * from "Nodes"

----------------------------------------------------------------------------------

------------------------------------------------------------------------------------------
-- Creating user with access to tables

--create user veadmin with encrypted password 'veadmin';
--grant all privileges on database "VEFrameworkDb" to veadmin;
--grant all on table public."Wallets" TO veadmin;
--grant all on table public."Accounts" TO veadmin;
--grant all on table public."Nodes" TO veadmin;
--grant all on table public."Users" TO veadmin;
