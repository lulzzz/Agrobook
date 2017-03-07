﻿using Agrobook.Domain.Common;
using System;

namespace Agrobook.Domain.Usuarios
{
    public class CrearNuevoUsuario : MensajeAuditable
    {
        public CrearNuevoUsuario(Metadatos metadatos, string usuario, string passwordCrudo)
            : base(metadatos)
        {
            this.Usuario = usuario;
            this.PasswordCrudo = passwordCrudo;
        }

        public string Usuario { get; }
        public string PasswordCrudo { get; }
    }

    public class IniciarSesion
    {
        public IniciarSesion(string usuario, string passwordCrudo, DateTime fecha)
        {
            this.Usuario = usuario;
            this.PasswordCrudo = passwordCrudo;
            this.Fecha = fecha;
        }

        public string Usuario { get; }
        public string PasswordCrudo { get; }
        public DateTime Fecha { get; }
    }
}
