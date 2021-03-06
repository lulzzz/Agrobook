﻿using Agrobook.Core;
using Agrobook.Domain.Common;
using System;

namespace Agrobook.Domain.Ap
{
    public class NumeracionDeServiciosIniciada : MensajeAuditable, IEvent
    {
        public NumeracionDeServiciosIniciada(Firma firma, string idProductor) : base(firma)
        {
            this.IdProductor = idProductor;
        }

        public string IdProductor { get; }
        public string StreamId => this.IdProductor;
    }

    public class NuevoRegistroDeServicioPendiente : MensajeAuditable, IEvent
    {
        public NuevoRegistroDeServicioPendiente(Firma firma, string idProd, string idServicio, string idOrg,
            string idContrato, bool esAdenda, string idContratoDeLaAdenda, DateTime fecha, string observaciones)
            : base(firma)
        {
            this.IdProd = idProd;
            this.IdServicio = idServicio;
            this.IdOrg = idOrg;
            this.IdContrato = idContrato;
            this.EsAdenda = esAdenda;
            this.IdContratoDeLaAdenda = idContratoDeLaAdenda;
            this.Fecha = fecha;
            this.Observaciones = observaciones;
        }

        public string IdProd { get; }
        public string IdServicio { get; }
        public string IdOrg { get; }
        public string IdContrato { get; }
        public bool EsAdenda { get; }
        public string IdContratoDeLaAdenda { get; }
        public DateTime Fecha { get; }
        public string Observaciones { get; }

        public string StreamId => this.IdProd;
    }
}
