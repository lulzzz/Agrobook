﻿using Agrobook.Core;
using Agrobook.Infrastructure.Log;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Agrobook.Domain.Archivos.Services
{
    public class ArchivosService : EventSourcedService
    {
        private readonly ILogLite log;
        private readonly IJsonSerializer serializer;
        private readonly string path;

        public ArchivosService(IEventSourcedRepository repository, IDateTimeProvider dateTime, ILogLite log, IJsonSerializer serializer) : base(repository, dateTime)
        {
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNull(log, nameof(log));

            this.serializer = serializer;
            this.log = log;

            this.path = Directory.GetCurrentDirectory() + @"\archivos";
            //this.path = @".\archivos";
        }

        public void CrearDirectoriosSiFaltan()
        {
            if (Directory.Exists(this.path))
            {
                this.log.Verbose($"El directorio {this.path} existe en el sistema");
                return;
            }

            this.log.Verbose($"Creando el directorio {this.path}...");
            Directory.CreateDirectory(this.path);
            this.log.Verbose($"El directorio {this.path} fué creado exitosamente");
        }

        public void BorrarTodoYEmpezarDeNuevo()
        {
            this.log.Warning($"Borrando todo los archivos en {this.path}");
            if (!Directory.Exists(this.path))
            {
                this.log.Warning($"El directorio {this.path} no existe. No se borró nada.");
            }
            else
            {
                Directory.Delete(this.path, true);
                this.log.Warning($"Fueron borrados todos los archivos en {this.path}");
            }

            this.CrearDirectoriosSiFaltan();
        }

        public async Task<Metadatos> PersistirArchivoDelProductor(HttpContent content)
        {
            if (!content.IsMimeMultipartContent())
                throw new NotMimeMultipartException();

            var streamProvider = await content.ReadAsMultipartAsync();
            var fileContent = streamProvider.Contents.First();

            var metadatosSerializados = await streamProvider.Contents[1].ReadAsStringAsync();
            var metadatos = this.serializer.Deserialize<Metadatos>(metadatosSerializados);

            //var fileName = new string(fileContent.Headers.ContentDisposition.FileName.Trim().Where(c => c != '"').ToArray());
            var fileName = $"{metadatos.Nombre}.{metadatos.Extension}";

            var coleccionPath = $"{this.path}\\{metadatos.IdProductor}";
            if (!Directory.Exists(coleccionPath))
            {
                this.log.Info($"Creando el directorio para la nueva colección de archivos de {metadatos.IdProductor}...");
                Directory.CreateDirectory(coleccionPath);
                this.log.Info($"Creacion exitosa del directorio {metadatos.IdProductor}");
            }

            using (var stream = await fileContent.ReadAsStreamAsync())
            {
                var fullPath = $"{coleccionPath}\\{fileName}";
                if (File.Exists(fullPath)) throw new ElArchivoYaExisteException();
                using (var fileStream = File.Create(fullPath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream);
                }
            }

            return metadatos;
        }

        public async Task HandleAsync(AgregarArchivoAColeccion cmd)
        {
            var coleccion = await this.repository.GetAsync<ColeccionDeArchivos>(cmd.IdProductor);
            if (coleccion == null)
            {
                coleccion = new ColeccionDeArchivos();
            }
        }
    }

    public class NotMimeMultipartException : Exception
    {
        public NotMimeMultipartException()
        { }

        public NotMimeMultipartException(string message) : base(message)
        { }
    }

    public class ElArchivoYaExisteException : Exception
    {
    }
}
