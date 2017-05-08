﻿using Agrobook.Domain;
using Agrobook.Domain.Usuarios;
using Agrobook.Domain.Usuarios.Services;
using Agrobook.Infrastructure.Log;
using Agrobook.Infrastructure.Persistence;
using Microsoft.Owin.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Agrobook.Server
{
    class Program
    {
        private static ILogLite _log = LogManager.GlobalLogger;
        #region Extern References
        // Source: http://stackoverflow.com/questions/474679/capture-console-exit-c-sharp
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ExtConsoleHandler handler, bool add);
        private delegate bool ExtConsoleHandler(CtrlType signal);

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        #endregion

        static void Main(string[] args)
        {
            SetConsoleCtrlHandler(
              add: true,
              handler: signal =>
              {
                  OnExit();
                    // Shutdown right away
                    Environment.Exit(-1);
                  return true;
              });

            // Dependency Container
            _log.Info("Starting Agrobook Server");
            Console.Write("Resolving dependencies...");
            ServiceLocator.Initialize();
            Console.WriteLine("Done");

            // EventStore
            Console.Write("Initializing EventStore...");
            var es = ServiceLocator.ResolveSingleton<EventStoreManager>();
#if DROP_DB
            _log.Info("The database initializer configuration is DROP AND CREATE");
            es.DropAndCreateDb();
#endif
#if !DROP_DB
            _log.Info("The database initializer configuration is CREATE IF NOT EXISTS");
            es.CreateDbIfNotExists();
#endif
            Console.WriteLine("Done");

            // SQL
            var sqlInit = ServiceLocator.ResolveSingleton<SqlDbInitializer<AgrobookDbContext>>();
#if DROP_DB
            sqlInit.DropAndCreateDb();
#endif
#if !DROP_DB
            sqlInit.CreateDatabaseIfNoExists();
#endif

            OnPersistenceEnginesInitialized();

            Console.Write("Starting web server...");
            // Web Api
            var baseUri = "http://localhost:8081";
            WebApiStartup.OnAppDisposing = () => OnExit();
            WebApp.Start<WebApiStartup>(baseUri);
            Console.WriteLine("Done");
            Console.WriteLine($"Server running at {baseUri} - press Enter to quit");

            string line;
            do
            {
                Console.WriteLine("Type exit to shut down");
                line = Console.ReadLine();
            }
            while (!line.Equals("exit", StringComparison.InvariantCultureIgnoreCase));

            OnExit();
        }

        private static void OnExit()
        {
            ServiceLocator
                .ResolveSingleton<EventStoreManager>()
                .TearDown();
        }

        private static void OnPersistenceEnginesInitialized()
        {
            var userService = ServiceLocator.ResolveSingleton<UsuariosService>();
            var intentos = 0;
            var maxRetries = 3;
            try
            {
                CrearUsuarioAdminSiHaceFalta(userService);
            }
            catch (Exception ex)
            {
                intentos++;
                Console.WriteLine("Error al verificar usuario admin");
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Intento {intentos}/{maxRetries}");
                if (intentos >= maxRetries)
                    throw;
                Thread.Sleep(1500);
                CrearUsuarioAdminSiHaceFalta(userService);
            }

            var usuariosDenormalizer = ServiceLocator.ResolveSingleton<UsuariosDenormalizer>();
            var organizacionesDenormalizer = ServiceLocator.ResolveSingleton<OrganizacionesDenormalizer>();

            usuariosDenormalizer.Start();
            organizacionesDenormalizer.Start();
        }

        private static void CrearUsuarioAdminSiHaceFalta(UsuariosService userService)
        {
            if (!userService.ExisteUsuarioAdmin)
            {
                Console.Write("Se detectó la ausencia del usuario admin. Creando uno...");
                userService.CrearUsuarioAdminAsync().Wait();
                Console.WriteLine("Listo!");
            }
        }
    }
}
