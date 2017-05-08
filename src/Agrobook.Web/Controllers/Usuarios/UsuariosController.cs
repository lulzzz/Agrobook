﻿using Agrobook.Client;
using Agrobook.Client.Usuarios;
using System.Threading.Tasks;
using System.Web.Http;

namespace Agrobook.Web.Controllers.Usuarios
{
    [RoutePrefix("app/usuarios")]
    public class UsuariosController : ApiControllerBase
    {
        private readonly UsuariosClient client;

        public UsuariosController()
        {
            this.client = ServiceLocator
                            .ResolveNewOf<UsuariosClient>()
                            .WithTokenProvider(this.GetToken);
        }

        [HttpPost]
        [Route("crear-nuevo-usuario")]
        public async Task<IHttpActionResult> CrearNuevoUsuario(UsuarioDto dto)
        {
            await this.client.CrearNuevoUsuario(dto);
            return this.Ok();
        }

        [HttpPost]
        [Route("actualizar-perfil")]
        public async Task<IHttpActionResult> ActualizarPerfil(ActualizarPerfilDto dto)
        {
            await this.client.ActualizarPerfil(dto);
            return this.Ok();
        }

        [HttpPost]
        [Route("resetear-password/{usuario}")]
        public async Task<IHttpActionResult> ResetearPassword([FromUri]string usuario)
        {
            await this.client.ResetearPassword(usuario);
            return this.Ok();
        }

        [HttpPost]
        [Route("crear-nueva-organizacion/{nombreOrg}")]
        public async Task<IHttpActionResult> CrearNuevaOrganizacion([FromUri]string nombreOrg)
        {
            await this.client.CrearNuevaOrganización(nombreOrg);
            return this.Ok();
        }
    }
}