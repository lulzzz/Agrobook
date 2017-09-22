﻿using Agrobook.Common;
using Agrobook.Domain.Ap.ServicioSaga;
using Eventing;
using Eventing.Core.Domain;
using Eventing.Core.Persistence;
using System;
using System.Threading.Tasks;

namespace Agrobook.Domain.Ap.Services
{
    public partial class ApService : EventSourcedService
    {
        //private readonly IEventSubscription servicioStreamSubscription;
        private readonly IEventSubscription servicioSecLogSubscription; // Is the saga execution coordinator log(sec)

        private readonly ICheckpointRepository checkpointRepository;

        private readonly IDateTimeProvider dateTimeProvider;

        public ApService(IEventSourcedRepository repository, IEventSubscriber subscriber, ICheckpointRepository checkpointRepository, IDateTimeProvider dateTimeProvider)
            : base(repository)
        {
            Ensure.NotNull(dateTimeProvider, nameof(dateTimeProvider));
            Ensure.NotNull(subscriber, nameof(subscriber));
            Ensure.NotNull(checkpointRepository, nameof(checkpointRepository));

            this.dateTimeProvider = dateTimeProvider;
            this.checkpointRepository = checkpointRepository;

            //var servicioStreamSubId = typeof(Servicio).Name;
            //this.servicioStreamSubscription = subscriber.CreateSubscription(
            //    StreamCategoryAttribute.GetCategoryProjectionStream<Servicio>(),
            //    new Lazy<long?>(() => this.checkpointRepository.GetCheckpoint(servicioStreamSubId)),
            //    (eventNumber) => this.checkpointRepository.SaveCheckpoint(servicioStreamSubId, eventNumber),
            //    this.Dispatch);

            var servicioSecLogSubId = typeof(ServicioSec).Name;
            this.servicioSecLogSubscription = subscriber.CreateSubscription(
                StreamCategoryAttribute.GetCategoryProjectionStream<ServicioSec>(),
                new Lazy<long?>(() => this.checkpointRepository.GetCheckpoint(servicioSecLogSubId)),
                (eventNumber) => this.checkpointRepository.SaveCheckpoint(servicioSecLogSubId, eventNumber),
                this.Dispatch);
        }

        protected void Dispatch(long eventNumber, object @event)
        {
            ((dynamic)this).HandleOnce(eventNumber, (dynamic)@event).Wait();
        }

        protected virtual async Task HandleOnce(long eventNumber, object @event)
        {
            await Task.CompletedTask;
        }

        public void Start()
        {
            //this.servicioStreamSubscription.Start();
            this.servicioSecLogSubscription.Start();
        }

        public void Stop()
        {
            //this.servicioStreamSubscription.Start();
            this.servicioSecLogSubscription.Start();
        }
    }
}
