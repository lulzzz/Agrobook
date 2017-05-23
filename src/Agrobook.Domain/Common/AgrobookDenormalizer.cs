﻿using Agrobook.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Agrobook.Domain.Common
{
    public abstract class AgrobookDenormalizer : EventStreamHandler
    {
        private readonly IEventStreamSubscription subscription;
        private readonly Func<AgrobookDbContext> contextFactory;
        private readonly string subName;

        public AgrobookDenormalizer(IEventStreamSubscriber subscriber, Func<AgrobookDbContext> contextFactory,
            string subscriptionName, string eventSourceStreamName)
        {
            Ensure.NotNullOrWhiteSpace(eventSourceStreamName, nameof(eventSourceStreamName));
            Ensure.NotNullOrWhiteSpace(subscriptionName, nameof(subscriptionName));
            Ensure.NotNull(subscriber, nameof(subscriber));
            Ensure.NotNull(contextFactory, nameof(contextFactory));

            this.subName = subscriptionName;
            this.contextFactory = contextFactory;

            var lastCheckpoint = new Lazy<long?>(() =>
            {
                using (var context = this.contextFactory.Invoke())
                {
                    return context
                           .Checkpoints
                           .SingleOrDefault(c => c.Subscription == this.subName)
                           ?.LastCheckpoint;
                }
            });

            this.subscription = subscriber.CreateSubscription(eventSourceStreamName, lastCheckpoint, this.Dispatch);
        }

        public void Start()
        {
            this.subscription.Start();
        }

        public void Stop()
        {
            this.subscription.Stop();
        }

        protected override async Task Handle(long eventNumber, object @event)
        {
            await this.Denormalize(eventNumber, c => { });
        }

        protected async Task Denormalize(long checkpoint, Action<AgrobookDbContext> denorm)
        {
            using (var context = this.contextFactory.Invoke())
            {
                denorm.Invoke(context);
                await context.SaveChangesAsync(this.subName, checkpoint);
            }
        }
    }
}