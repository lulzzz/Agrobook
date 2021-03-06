﻿using Agrobook.Domain.Ap.Commands;
using Agrobook.Domain.Ap.ValueObjects;
using Agrobook.Domain.Usuarios;
using Eventing;
using Eventing.Core.Persistence;
using System;
using System.Threading.Tasks;

namespace Agrobook.Domain.Ap.Services
{
    public partial class ApService
    {
        /// <summary>
        /// To test this, use the event handler, do not try to call this.
        /// </summary>
        internal async Task HandleAsync(ProcesarRegistroDeServicioPendiente cmd)
        {
            var e = cmd.RegistroPendiente;
            await this.AsegurarQueElContratoOLaAdendaSeanValidos(e.EsAdenda, e.IdContrato, e.IdContratoDeLaAdenda);

            var servicio = new Servicio();

            servicio.Emit(new NuevoServicioRegistrado(
                e.Firma, e.IdServicio, e.IdProd, e.IdOrg, e.IdContrato, e.EsAdenda, e.IdContratoDeLaAdenda, e.Fecha, e.Observaciones));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(EditarDatosBasicosDelSevicio cmd)
        {
            await this.repository.EnsureExistenceOfThis<Organizacion>(cmd.IdOrg);

            await this.AsegurarQueElContratoOLaAdendaSeanValidos(cmd.EsAdenda, cmd.IdContrato, cmd.IdContratoDeLaAdenda);

            cmd.Fecha.EnsureIsNotDefault(nameof(cmd.Fecha));

            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);

            if (!servicio.HayDiferenciaEnDatosBasicos(cmd.IdOrg, cmd.IdContrato, cmd.Fecha, cmd.Observaciones))
                throw new InvalidOperationException("No hay diferencias que registrar en datos basicos del servicio!");

            servicio.Emit(new DatosBasicosDelSevicioEditados(cmd.Firma, cmd.IdServicio, cmd.IdOrg,
                cmd.IdContrato, cmd.EsAdenda, cmd.IdContratoDeLaAdenda, cmd.Fecha, cmd.Observaciones));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(EliminarServicio cmd)
        {
            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);

            if (servicio.EstaEliminado)
                throw new InvalidOperationException("El servicio ya esta luego eliminado!");

            servicio.Emit(new ServicioEliminado(cmd.Firma, cmd.IdServicio));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(RestaurarServicio cmd)
        {
            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);

            if (!servicio.EstaEliminado)
                throw new InvalidOperationException("El servicio no esta eliminado. No necesita restaurarse");

            servicio.Emit(new ServicioRestaurado(cmd.Firma, cmd.IdServicio));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(EspecificarParcelaDelServicio cmd)
        {
            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);

            if (servicio.TieneParcela)
                throw new InvalidOperationException("El servicio ya tiene una parcela especificada.");

            var productor = await this.repository.GetOrFailByIdAsync<Productor>(servicio.IdProductor);

            if (!productor.TieneParcela(cmd.IdParcela))
                throw new InvalidOperationException("El productor no tiene esa parcela!");

            servicio.Emit(new ParcelaDeServicioEspecificada(cmd.Firma, cmd.IdServicio, cmd.IdParcela));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(CambiarParcelaDelServicio cmd)
        {
            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);

            if (servicio.IdParcela == cmd.IdParcela)
                throw new InvalidOperationException("La parcela que se quiere cambiar es igual a la actual");

            var productor = await this.repository.GetOrFailByIdAsync<Productor>(servicio.IdProductor);
            if (!productor.TieneParcela(cmd.IdParcela))
                throw new InvalidOperationException("El productor no tiene esa parcela!");

            servicio.Emit(new ParcelaDeServicioCambiada(cmd.Firma, cmd.IdServicio, cmd.IdParcela));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(FijarPrecioAlServicio cmd)
        {
            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);
            if (servicio.TienePrecio)
                throw new InvalidOperationException("Ya se tiene fijado el precio");

            Ensure.Positive(cmd.Precio, nameof(cmd.Precio));
            servicio.Emit(new PrecioDeServicioFijado(cmd.Firma, cmd.IdServicio, Moneda.DolaresAmericanos, cmd.Precio));

            await this.repository.SaveAsync(servicio);
        }

        public async Task HandleAsync(AjustarPrecioDelServicio cmd)
        {
            var servicio = await this.repository.GetOrFailByIdAsync<Servicio>(cmd.IdServicio);
            if (!servicio.TienePrecio)
                throw new InvalidOperationException("El documento no tiene precio para ajustar. Fijelo primero");

            if (cmd.Precio == servicio.Precio)
                throw new InvalidOperationException("El precio que se quiere ajustar es igual al ajuste");

            Ensure.Positive(cmd.Precio, nameof(cmd.Precio));
            servicio.Emit(new PrecioDeServicioAjustado(cmd.Firma, cmd.IdServicio, Moneda.DolaresAmericanos, cmd.Precio));

            await this.repository.SaveAsync(servicio);
        }

        /// <summary>
        /// Se asegura que el contrato o la adenda sean validos. Si no son válidos lanza un <see cref="InvalidOperationException"/>.
        /// </summary>
        private async Task AsegurarQueElContratoOLaAdendaSeanValidos(bool esAdenda, string idContrato, string idContratoDeLaAdenda)
        {
            // Verificamos que exista el contrato
            var contrato = await this.repository.GetOrFailByIdAsync<Contrato>(esAdenda ? idContratoDeLaAdenda : idContrato);

            // Si es una adenda, verifacmos que exista esa adenda en ese contrato
            if (esAdenda && !contrato.TieneAdenda(idContrato))
                throw new InvalidOperationException("No es válido esta adenda, por que no existe en este contrato");
        }
    }
}
