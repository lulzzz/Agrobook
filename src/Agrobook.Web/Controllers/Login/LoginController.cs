﻿using Agrobook.Client.Login;
using Eventing.Client.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Agrobook.Web.Controllers.Login
{
    [RoutePrefix("app/login")]
    public class LoginController : ApiControllerBase
    {
        private readonly ILoginClient client;

        public LoginController()
        {
            this.client = ServiceLocator
                            .ResolveNewOf<ILoginClient>()
                            .WithTokenProvider(this.TokenProvider);
        }

        [HttpPost]
        [Route("try-login")]
        public async Task<IHttpActionResult> TryLogin([FromBody]dynamic value)
        {
            var result = await this.client.TryLoginAsync(value.usuario.Value, value.password.Value);
            return this.Ok(result);
        }
    }
}
