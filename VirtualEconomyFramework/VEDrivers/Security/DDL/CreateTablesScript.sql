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