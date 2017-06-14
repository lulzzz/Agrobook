﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using Agrobook.Client.Archivos;
using Agrobook.Client;
using System.IO;

namespace Agrobook.Web.Controllers.Archivos
{
    [RoutePrefix("app/archivos")]
    public class ArchivosController : ApiControllerBase
    {
        private readonly ArchivosClient client;

        public ArchivosController()
        {
            this.client = ServiceLocator.ResolveNewOf<ArchivosClient>()
                                        .WithTokenProvider(this.TokenProvider);
        }

        [Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            /*
             * HTML CSS JAVASCRIPT: http://jsfiddle.net/vishalvasani/4hqVu/
             * Another one: http://www.uncorkedstudios.com/blog/multipartformdata-file-upload-with-angularjs/
             * 
             * Help: https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/sending-html-form-data-part-2
             * Help3: https://stackoverflow.com/questions/16416601/c-sharp-httpclient-4-5-multipart-form-data-upload?noredirect=1&lq=1
             * help 4: https://stackoverflow.com/questions/7460088/reading-file-input-from-a-multipart-form-data-post
             */
            if (!this.Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(System.Net.HttpStatusCode.UnsupportedMediaType);

            var streamProvider = await this.Request.Content.ReadAsMultipartAsync();
            var content = streamProvider.Contents.First();

            var fileName = content.Headers.ContentDisposition.FileName;
            using (var stream = await content.ReadAsStreamAsync())
            {
                await this.client.Upload(stream, fileName);
            }
            return this.Ok();

        }
    }
}