﻿using Agrobook.Domain.Ap.Services;
using Eventing.Client.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agrobook.Client.Ap
{
    public class ApQueryClient : ClientBase
    {
        public ApQueryClient(HttpLite http, Func<string> tokenProvider = null)
            : base(http, tokenProvider, "ap/query")
        { }

        public async Task<IList<ClienteDeAp>> ObtenerClientes(string filtro)
            => await base.Get<IList<ClienteDeAp>>("clientes?filtro=" + filtro);


        public async Task<OrgDto> ObtenerOrg(string idOrg)
            => await base.Get<OrgDto>("org/" + idOrg);
    }
}
