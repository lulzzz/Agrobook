﻿using Agrobook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Agrobook.Domain.Ap.Services
{
    public class ApQueryService : DbContextQueryService<AgrobookDbContext>
    {
        private readonly string orgAvatarUrl = "../assets/img/avatar/org-icon.png";

        public ApQueryService(Func<AgrobookDbContext> contextFactory) : base(contextFactory)
        { }

        public async Task<IList<ClienteDeApDto>> ObtenerClientes(string filtro)
        {
            return await this.QueryAsync(async context =>
            {
                var organizaciones = await context.Organizaciones.ToArrayAsync();
                var productores = await context.Usuarios.Where(x => x.EsProductor).ToArrayAsync();
                var orgDeUsuarios = await context.OrganizacionesDeUsuarios.ToArrayAsync();

                var clientesQuery =
                    organizaciones.Select(o => new ClienteDeApDto
                    {
                        Id = o.OrganizacionId,
                        Nombre = o.NombreParaMostrar,
                        Desc = "Organización",
                        Tipo = "org",
                        AvatarUrl = this.orgAvatarUrl
                    })
                    .Concat(productores.Select(p => new ClienteDeApDto
                    {
                        Id = p.Id,
                        Nombre = p.Display,
                        Tipo = "prod",
                        AvatarUrl = p.AvatarUrl,
                        Desc = orgDeUsuarios
                                .Where(o => o.UsuarioId == p.Id)
                                .Select(o => organizaciones.First(x => x.OrganizacionId == o.OrganizacionId).NombreParaMostrar)
                                .Aggregate("",
                                (acumulate, orgName) =>
                                    acumulate == "" ? orgName : $"{acumulate}, {orgName}")
                    }));

                switch (filtro)
                {
                    case "todos":
                        return clientesQuery.ToList();
                    case "prod":
                        return clientesQuery.Where(x => x.Tipo == "prod").ToList();
                    case "org":
                        return clientesQuery.Where(x => x.Tipo == "org").ToList();
                    default:
                        throw new ArgumentException("El filtro es inválido", nameof(filtro));
                }
            });
        }

        public async Task<OrgDto> ObtenerOrg(string idOrg)
        {
            return await this.QueryAsync(async context =>
            {
                var entity = await context.Organizaciones
                .SingleAsync(x => x.OrganizacionId == idOrg);

                return new OrgDto
                {
                    Id = idOrg,
                    Display = entity.NombreParaMostrar,
                    AvatarUrl = this.orgAvatarUrl
                };
            });
        }

        public async Task<IList<ContratoEntity>> ObtenerContratos(string idOrg)
            => await this.QueryAsync(async context =>
                await context.Contratos.Where(x => x.IdOrg == idOrg).ToListAsync());

        public async Task<IList<ParcelaEntity>> ObtenerParcelas(string idProd)
            => await this.QueryAsync(async context =>
            await context.Parcelas.Where(x => x.IdProd == idProd).ToListAsync());

        public async Task<ParcelaEntity> ObtenerParcela(string idParcela)
            => await this.QueryAsync(async context =>
            await context.Parcelas.SingleAsync(x => x.Id == idParcela));

        public async Task<OrgConContratosDto[]> ObtenerOrgsConContratosDelProductor(string idProd)
            => await this.QueryAsync(async context =>
            (await context
                   .OrganizacionesDeUsuarios
                   .Where(x => x.UsuarioId == idProd)
                   .ToArrayAsync())
            .Select(x => new OrgConContratosDto
            {
                Org = new OrgDto { Id = x.OrganizacionId, Display = x.OrganizacionDisplay },
                Contratos = context
                            .Contratos
                            .Where(c => c.IdOrg == x.OrganizacionId)
                            .ToArray()
            })
            .ToArray());

        public async Task<ProdDto> GetProd(string idProd)
            => await this.QueryAsync(async context =>
                (await context.Usuarios.SingleAsync(x => x.Id == idProd))
                .Transform(u => new ProdDto
                {
                    Id = idProd,
                    Display = u.Display,
                    AvatarUrl = u.AvatarUrl,
                    Orgs = context.OrganizacionesDeUsuarios
                            .Where(x => x.UsuarioId == idProd)
                            .ToArray()
                            .Select(x => new OrgDto
                            {
                                Id = x.OrganizacionId,
                                Display = x.OrganizacionDisplay
                            })
                            .ToArray()
                }));

        public async Task<IList<ServicioDto>> GetServiciosPorOrg(string idOrg)
           => await this.QueryAsync(async context =>
                 await context.Servicios
                    .Where(x => x.IdOrg == idOrg && x.IdParcela != null)
                    .Join(context.Organizaciones, s => s.IdOrg, o => o.OrganizacionId,
                     (s, o) => new { serv = s, org = o })
                    .Join(context.Contratos, servOrg => servOrg.serv.IdContrato, c => c.Id,
                     (so, c) => new { so, c })
                    .Join(context.Parcelas, soc => soc.so.serv.IdParcela, p => p.Id,
                     (soc, p) => new { soc, p })
                    .Join(context.Usuarios, socp => socp.soc.so.serv.IdProd, u => u.Id,
                     (socp, u) =>
                     new ServicioDto
                     {
                         ContratoDisplay = socp.soc.c.Display,
                         Eliminado = socp.soc.so.serv.Eliminado,
                         Fecha = socp.soc.so.serv.Fecha,
                         Observaciones = socp.soc.so.serv.Observaciones,
                         Id = socp.soc.so.serv.Id,
                         IdContrato = socp.soc.so.serv.IdContrato,
                         IdOrg = socp.soc.so.serv.IdOrg,
                         IdProd = socp.soc.so.serv.IdProd,
                         ProdDisplay = u.Display,
                         OrgDisplay = socp.soc.so.org.NombreParaMostrar,
                         ParcelaId = socp.soc.so.serv.IdParcela,
                         ParcelaDisplay = socp.p.Display
                     })
                    .Union(
                     context.Servicios
                    .Where(x => x.IdOrg == idOrg && x.IdParcela == null)
                    .Join(context.Organizaciones, s => s.IdOrg, o => o.OrganizacionId,
                     (s, o) => new { serv = s, org = o })
                     .Join(context.Usuarios, orgServ => orgServ.serv.IdProd, usuarios => usuarios.Id,
                     (so, u) => new { servOrg = so, usuarios = u })
                    .Join(context.Contratos, sou => sou.servOrg.serv.IdContrato, c => c.Id,
                     (sou, c) => new ServicioDto
                     {
                         ContratoDisplay = c.Display,
                         Eliminado = sou.servOrg.serv.Eliminado,
                         Fecha = sou.servOrg.serv.Fecha,
                         Observaciones = sou.servOrg.serv.Observaciones,
                         Id = sou.servOrg.serv.Id,
                         IdContrato = sou.servOrg.serv.IdContrato,
                         IdOrg = sou.servOrg.serv.IdOrg,
                         IdProd = sou.servOrg.serv.IdProd,
                         ProdDisplay = sou.usuarios.Display,
                         OrgDisplay = sou.servOrg.org.NombreParaMostrar,
                         ParcelaId = null,
                         ParcelaDisplay = null
                     }))
                    .ToListAsync());

        public async Task<IList<ContratoConServicios>> GetServiciosPorOrgAgrupadosPorContrato(string idOrg)
        {
            return await this.QueryAsync(async context =>
            {
                var contratos = await context.Contratos
                                        .Where(c => c.IdOrg == idOrg)
                                        .OrderByDescending(c => c.Fecha)
                                        .Select(c => new ContratoConServicios
                                        {
                                            Id = c.Id,
                                            Display = c.Display,
                                            Fecha = c.Fecha,
                                            Eliminado = c.Eliminado
                                        })
                                        .ToListAsync();

                var servicios = await context
                                    .Servicios
                                    .Where(s => s.IdOrg == idOrg)
                                    .Join(context.Usuarios, s => s.IdProd, u => u.Id,
                                    (s, u) => new { s, u })
                                    .Join(context.Parcelas, su => su.s.IdParcela, p => p.Id,
                                    (su, p) => new { su, p })
                                    .OrderBy(sup => sup.su.u.Display)
                                    .ToListAsync();

                servicios.ForEach(s =>
                {
                    var contrato = contratos.Single(c => c.Id == s.su.s.IdContrato);
                    contrato.Servicios.Add(
                        new ServicioSlim
                        {
                            Id = s.su.s.Id,
                            Display = $"{s.su.u.Display}, parcela {s.p.Display}",
                            Eliminado = s.su.s.Eliminado,
                            Hectareas = s.p.Hectareas,
                            IdProd = s.su.u.Id,
                            Fecha = s.su.s.Fecha
                        });

                    if (!s.su.s.Eliminado)
                        contrato.TotalHa += s.p.Hectareas;
                });

                // Todo: agregar Total de Ha
                return contratos;
            });
        }

        public async Task<IList<ServicioDto>> GetServiciosPorProd(string idProd)
            => await this.QueryAsync(async context =>
                 await context.Servicios
                    .Where(x => x.IdProd == idProd && x.IdParcela != null)
                    .Join(context.Organizaciones, s => s.IdOrg, o => o.OrganizacionId,
                     (s, o) => new { serv = s, org = o })
                    .Join(context.Contratos, servOrg => servOrg.serv.IdContrato, c => c.Id,
                     (so, c) => new { so = so, c = c })
                    .Join(context.Parcelas, soc => soc.so.serv.IdParcela, p => p.Id,
                     (soc, p) => new ServicioDto
                     {
                         ContratoDisplay = soc.c.Display,
                         Eliminado = soc.so.serv.Eliminado,
                         Fecha = soc.so.serv.Fecha,
                         Observaciones = soc.so.serv.Observaciones,
                         Id = soc.so.serv.Id,
                         IdContrato = soc.so.serv.IdContrato,
                         EsAdenda = soc.c.EsAdenda,
                         IdContratoDeLaAdenda = soc.c.IdContratoDeLaAdenda,
                         IdOrg = soc.so.serv.IdOrg,
                         IdProd = soc.so.serv.IdProd,
                         OrgDisplay = soc.so.org.NombreParaMostrar,
                         ParcelaId = soc.so.serv.IdParcela,
                         ParcelaDisplay = p.Display
                     })
                    .Union(
                     context.Servicios
                    .Where(x => x.IdProd == idProd && x.IdParcela == null)
                    .Join(context.Organizaciones, s => s.IdOrg, o => o.OrganizacionId,
                     (s, o) => new { serv = s, org = o })
                    .Join(context.Contratos, servOrg => servOrg.serv.IdContrato, c => c.Id,
                     (so, c) => new ServicioDto
                     {
                         ContratoDisplay = c.Display,
                         Eliminado = so.serv.Eliminado,
                         Fecha = so.serv.Fecha,
                         Observaciones = so.serv.Observaciones,
                         Id = so.serv.Id,
                         IdContrato = so.serv.IdContrato,
                         EsAdenda = c.EsAdenda,
                         IdContratoDeLaAdenda = c.IdContratoDeLaAdenda,
                         IdOrg = so.serv.IdOrg,
                         IdProd = so.serv.IdProd,
                         OrgDisplay = so.org.NombreParaMostrar,
                         ParcelaId = null,
                         ParcelaDisplay = null
                     }))
                    .ToListAsync());

        public async Task<ServicioDto> GetServicio(string idServicio)
           => await this.QueryAsync(async context =>
           {
               var servicio = await context.Servicios
                   .Where(x => x.Id == idServicio)
                   .Join(context.Organizaciones,
                   s => s.IdOrg, o => o.OrganizacionId, (s, o) => new { serv = s, org = o })
                   .Join(context.Contratos, servOrg => servOrg.serv.IdContrato, c => c.Id,
                   (so, c) => new ServicioDto
                   {
                       ContratoDisplay = c.Display,
                       Eliminado = so.serv.Eliminado,
                       Fecha = so.serv.Fecha,
                       Observaciones = so.serv.Observaciones,
                       Id = so.serv.Id,
                       IdContrato = so.serv.IdContrato,
                       EsAdenda = c.EsAdenda,
                       IdContratoDeLaAdenda = c.IdContratoDeLaAdenda,
                       IdOrg = so.serv.IdOrg,
                       IdProd = so.serv.IdProd,
                       OrgDisplay = so.org.NombreParaMostrar,
                       ParcelaId = so.serv.IdParcela,
                       TienePrecio = so.serv.TienePrecio,
                       PrecioTotal = so.serv.TienePrecio ? so.serv.PrecioTotal.ToString() : "0"
                   })
                   .SingleAsync();


               if (servicio.ParcelaId is null)
                   return servicio;

               var parcelaEntity = await context.Parcelas.SingleAsync(p => p.Id == servicio.ParcelaId);
               servicio.ParcelaDisplay = parcelaEntity.Display;
               servicio.Hectareas = parcelaEntity.Hectareas.ToString();
               servicio.PrecioPorHectarea = servicio.TienePrecio ? (decimal.Parse(servicio.PrecioTotal) / parcelaEntity.Hectareas).ToString() : "0";
               return servicio;
           });

        public async Task<IList<ServicioParaDashboardDto>> GetUltimosServicios(int cantidad)
            => await this.QueryAsync(async context =>
                await context
                    .Servicios
                    .Where(s => s.Eliminado != true)
                    .OrderByDescending(s => s.Fecha)
                    .Take(cantidad)
                    .Join(context.Parcelas, outer => outer.IdParcela, inner => inner.Id,
                        (outer, inner) => new { servicio = outer, parcela = inner })
                    .Join(context.Organizaciones, outer => outer.servicio.IdOrg, inner => inner.OrganizacionId,
                        (outer, inner) => new { servParc = outer, org = inner })
                    .Join(context.Usuarios, outer => outer.servParc.servicio.IdProd, inner => inner.Id,
                        (outer, inner) => new { servParcOrg = outer, prod = inner })
                    .Select(x => new ServicioParaDashboardDto
                    {
                        Id = x.servParcOrg.servParc.servicio.Id,
                        Fecha = x.servParcOrg.servParc.servicio.Fecha,
                        OrgDisplay = x.servParcOrg.org.NombreParaMostrar,
                        IdProd = x.prod.Id,
                        ProdDisplay = x.prod.Display,
                        ProdAvatarUrl = x.prod.AvatarUrl,
                        ParcelaDisplay = x.servParcOrg.servParc.parcela.Display
                    })
                    .ToListAsync());
    }
}
