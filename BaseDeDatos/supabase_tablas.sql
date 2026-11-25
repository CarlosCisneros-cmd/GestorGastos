-- Gestor de Gastos Personales: tablas en Supabase (PostgreSQL)
-- Pegar todo en Supabase > SQL Editor > New query y presionar RUN.
-- Cada tabla guarda los datos de todos los usuarios; usuario_id separa los de cada uno.

create table if not exists public."Usuarios" (
    usuario_id text not null,
    "Id" integer not null,
    "GoogleId" text,
    "Nombre" text,
    "Correo" text,
    "FotoUrl" text,
    "FechaRegistro" timestamp,
    "UltimoIngreso" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Usuarios" enable row level security;
drop policy if exists "app_movil" on public."Usuarios";
create policy "app_movil" on public."Usuarios" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Usuarios" to anon, authenticated;

create table if not exists public."Cuentas" (
    usuario_id text not null,
    "Id" integer not null,
    "Nombre" text,
    "Tipo" text,
    "Saldo" numeric(14,2),
    "Moneda" text,
    "Color" text,
    "Icono" text,
    "Activa" boolean,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Cuentas" enable row level security;
drop policy if exists "app_movil" on public."Cuentas";
create policy "app_movil" on public."Cuentas" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Cuentas" to anon, authenticated;

create table if not exists public."Categorias" (
    usuario_id text not null,
    "Id" integer not null,
    "Nombre" text,
    "Tipo" text,
    "Icono" text,
    "Color" text,
    "EsPredeterminada" boolean,
    "Activa" boolean,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Categorias" enable row level security;
drop policy if exists "app_movil" on public."Categorias";
create policy "app_movil" on public."Categorias" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Categorias" to anon, authenticated;

create table if not exists public."Transacciones" (
    usuario_id text not null,
    "Id" integer not null,
    "CuentaId" integer,
    "Tipo" text,
    "Monto" numeric(14,2),
    "Nombre" text,
    "Nota" text,
    "CategoriaId" integer,
    "Fecha" timestamp,
    "EsRecurrente" boolean,
    "RecurrenteId" integer,
    "EsPagoDeuda" boolean,
    "EsTransferencia" boolean,
    "GrupoTransferencia" text,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Transacciones" enable row level security;
drop policy if exists "app_movil" on public."Transacciones";
create policy "app_movil" on public."Transacciones" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Transacciones" to anon, authenticated;

create table if not exists public."Comprobantes" (
    usuario_id text not null,
    "Id" integer not null,
    "TransaccionId" integer,
    "RutaImagen" text,
    "TextoDetectado" text,
    "Comercio" text,
    "MontoDetectado" numeric(14,2),
    "FechaCaptura" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Comprobantes" enable row level security;
drop policy if exists "app_movil" on public."Comprobantes";
create policy "app_movil" on public."Comprobantes" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Comprobantes" to anon, authenticated;

create table if not exists public."PagosRecurrentes" (
    usuario_id text not null,
    "Id" integer not null,
    "CuentaId" integer,
    "Tipo" text,
    "Monto" numeric(14,2),
    "Nombre" text,
    "Nota" text,
    "CategoriaId" integer,
    "Frecuencia" text,
    "FechaInicio" timestamp,
    "ProximaFecha" timestamp,
    "FechaFin" timestamp,
    "Activo" boolean,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."PagosRecurrentes" enable row level security;
drop policy if exists "app_movil" on public."PagosRecurrentes";
create policy "app_movil" on public."PagosRecurrentes" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."PagosRecurrentes" to anon, authenticated;

create table if not exists public."PagosProximos" (
    usuario_id text not null,
    "Id" integer not null,
    "CuentaId" integer,
    "Tipo" text,
    "Monto" numeric(14,2),
    "Nombre" text,
    "Nota" text,
    "CategoriaId" integer,
    "FechaVencimiento" timestamp,
    "Pagado" boolean,
    "FechaPago" timestamp,
    "RecurrenteId" integer,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."PagosProximos" enable row level security;
drop policy if exists "app_movil" on public."PagosProximos";
create policy "app_movil" on public."PagosProximos" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."PagosProximos" to anon, authenticated;

create table if not exists public."Presupuestos" (
    usuario_id text not null,
    "Id" integer not null,
    "Nombre" text,
    "CategoriaId" integer,
    "Monto" numeric(14,2),
    "Periodo" text,
    "Activo" boolean,
    "AlertaAdvertenciaPeriodo" text,
    "AlertaExcedidoPeriodo" text,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Presupuestos" enable row level security;
drop policy if exists "app_movil" on public."Presupuestos";
create policy "app_movil" on public."Presupuestos" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Presupuestos" to anon, authenticated;

create table if not exists public."MetasAhorro" (
    usuario_id text not null,
    "Id" integer not null,
    "Nombre" text,
    "Icono" text,
    "MontoObjetivo" numeric(14,2),
    "MontoActual" numeric(14,2),
    "FechaLimite" timestamp,
    "Activa" boolean,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."MetasAhorro" enable row level security;
drop policy if exists "app_movil" on public."MetasAhorro";
create policy "app_movil" on public."MetasAhorro" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."MetasAhorro" to anon, authenticated;

create table if not exists public."AportesAhorro" (
    usuario_id text not null,
    "Id" integer not null,
    "MetaId" integer,
    "CuentaId" integer,
    "TransaccionId" integer,
    "Monto" numeric(14,2),
    "Fecha" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."AportesAhorro" enable row level security;
drop policy if exists "app_movil" on public."AportesAhorro";
create policy "app_movil" on public."AportesAhorro" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."AportesAhorro" to anon, authenticated;

create table if not exists public."Deudas" (
    usuario_id text not null,
    "Id" integer not null,
    "Nombre" text,
    "MontoTotal" numeric(14,2),
    "MontoPendiente" numeric(14,2),
    "TasaInteres" numeric(14,2),
    "InteresIncluido" boolean,
    "NumeroCuotas" integer,
    "FechaInicio" timestamp,
    "CuentaId" integer,
    "Notas" text,
    "Activa" boolean,
    "FechaCreacion" timestamp,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."Deudas" enable row level security;
drop policy if exists "app_movil" on public."Deudas";
create policy "app_movil" on public."Deudas" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."Deudas" to anon, authenticated;

create table if not exists public."CuotasDeuda" (
    usuario_id text not null,
    "Id" integer not null,
    "DeudaId" integer,
    "Numero" integer,
    "Monto" numeric(14,2),
    "FechaVencimiento" timestamp,
    "Estado" text,
    "FechaPago" timestamp,
    "TransaccionId" integer,
    sincronizado timestamptz not null default now(),
    primary key (usuario_id, "Id")
);
alter table public."CuotasDeuda" enable row level security;
drop policy if exists "app_movil" on public."CuotasDeuda";
create policy "app_movil" on public."CuotasDeuda" for all to anon, authenticated using (true) with check (true);
grant select, insert, update, delete on public."CuotasDeuda" to anon, authenticated;
