﻿using Agrobook.Core;
using EventStore.ClientAPI;
using System;

namespace Agrobook.Infrastructure.Subscription
{
    public class EventStreamSubscriber : IEventStreamSubscriber
    {
        private readonly IEventStoreConnection resilientConnection;
        private readonly IJsonSerializer serializer;
        private readonly TimeSpan closeTimeout;

        public EventStreamSubscriber(IEventStoreConnection resilientConnection, IJsonSerializer serializer)
            : this(resilientConnection, serializer, TimeSpan.FromMinutes(1))
        { }

        public EventStreamSubscriber(IEventStoreConnection resilientConnection, IJsonSerializer serializer, TimeSpan closeTimeout)
        {
            Ensure.NotNull(resilientConnection, nameof(resilientConnection));
            Ensure.NotNull(serializer, nameof(serializer));

            this.resilientConnection = resilientConnection;
            this.closeTimeout = closeTimeout;
            this.serializer = serializer;
        }

        public IEventStreamSubscription CreateSubscription(string streamName, Lazy<long?> lastCheckpoint, Action<long, object> handler)
        {
            return new EventStreamSubscription(
                this.resilientConnection,
                this.serializer,
                streamName,
                lastCheckpoint,
                handler,
                this.closeTimeout);
        }

        public IEventStreamSubscription CreateSubscriptionFromCategory(string category, Lazy<long?> lastCheckpoint, Action<long, object> handler)
        {
            return this.CreateSubscription($"$ce-{category}", lastCheckpoint, handler);
        }
    }
}
