﻿using Agrobook.Domain.Archivos.Services;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Agrobook.Server.Archivos
{
    [RoutePrefix("archivos")]
    public class ArchivosController : ApiController
    {
        private readonly ArchivosService service = ServiceLocator.ResolveSingleton<ArchivosService>();

        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                await this.service.PersistirArchivoDelProductor(this.Request.Content);
            }
            catch (NotMimeMultipartException)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            catch (FileAlreadyExistsException)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }

            return this.Ok();
        }
    }


}
