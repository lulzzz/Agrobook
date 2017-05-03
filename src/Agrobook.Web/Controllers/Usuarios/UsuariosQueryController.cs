﻿using Agrobook.Client;
using Agrobook.Client.Usuarios;
using System.Threading.Tasks;
using System.Web.Http;

namespace Agrobook.Web.Controllers.Usuarios
{
    [RoutePrefix("app/usuarios/query")]
    public class UsuariosQueryController : ApiControllerBase
    {
        private readonly UsuariosQueryClient client;

        public UsuariosQueryController()
        {
            this.client = ServiceLocator
                            .ResolveNewOf<UsuariosQueryClient>()
                            .WithTokenProvider(this.GetToken);
        }

        [HttpGet]
        [Route("todos")]
        public async Task<IHttpActionResult> ObtenerListaDeTodosLosUsuarios()
        {
            var lista = await this.client.ObtenerListaDeTodosLosUsuarios();
            return this.Ok(lista);
        }

        [HttpGet]
        [Route("info-basica/{usuario}")]
        public async Task<IHttpActionResult> ObtenerInfoBasicaDeUsuario([FromUri] string usuario)
        {
            var dto = await this.client.ObtenerInfoBasicaDeUsuario(usuario);
            return this.Ok(dto);
        }
    }
}