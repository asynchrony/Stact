// Copyright 2010 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Stact.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Actors.Actors;
    using Actors.Internal;
    using Magnum.Extensions;
    using Routing;


    public interface ActorInbox
    {
        RoutingEngine Engine { get; }
    }


    /// <summary>
    ///   An inbox for an actor. Channel properties on the actor are automatically bound.
    ///   Messages are automatically delivered to the inbox for each message type unless
    ///   a property channel has the same message type. Calling Receive on the inbox will
    /// </summary>
    /// <typeparam name = "TActor">The actor type for this inbox</typeparam>
    public class ActorInbox<TActor> :
        ActorInbox,
        Inbox
        where TActor : class, Actor
    {
        readonly RoutingEngine _engine;
        readonly TimeSpan _exitTimeout = 60.Seconds();
        readonly Fiber _fiber;
        readonly UntypedChannel _inbound;
        readonly HashSet<PendingReceive> _pending;
        readonly Scheduler _scheduler;
        ActorExceptionHandler _exceptionHandler;


        public ActorInbox([NotNull] Fiber fiber, [NotNull] Scheduler scheduler)
        {
            _fiber = fiber;
            _scheduler = scheduler;
            _pending = new HashSet<PendingReceive>();

            _engine = new DynamicRoutingEngine(fiber);

            _inbound = new UntypedFilterChannel<Kill>(_engine, message => HandleKill);

            Receive<Request<Exit>>(request => HandleExit);
        }

        public RoutingEngine Engine
        {
            get { return _engine; }
        }

        public void Send<T>(T message)
        {
            _inbound.Send(message);

            // TODO at some point, we need to deal with the fact that not having any receive pending on 
            // an actor could mean that it is time for it to die
            // this will also have to check async actions such as pending file io, web requests, etc.
        }

        public PendingReceive Receive<T>(SelectiveConsumer<T> consumer)
        {
            var pending = new PendingReceiveImpl<T>(this, consumer, x => _pending.Remove(x));

            _engine.Configure(config =>
                {
                    if (pending.Cancelled) return;
                    
                    RemoveActivation removeActivation = config.SelectiveReceive<T>(pending.Accept);
                    pending.OnCompleted += x => removeActivation();
                });
            _pending.Add(pending);
            return pending;
        }

        public PendingReceive Receive<T>(SelectiveConsumer<T> consumer, TimeSpan timeout, Action timeoutCallback)
        {
            var pending = new PendingReceiveImpl<T>(this, consumer, timeoutCallback, x => _pending.Remove(x));

            pending.ScheduleTimeout(x => _scheduler.Schedule(timeout, _fiber, x.Timeout));

            _engine.Configure(config =>
                {
                    if (pending.Cancelled) return;

                    RemoveActivation removeActivation = config.SelectiveReceive<T>(pending.Accept);
                    pending.OnCompleted += x => removeActivation();
                });
                    
                
            _pending.Add(pending);
            return pending;
        }

        public void SetExceptionHandler(ActorExceptionHandler handler)
        {
            _exceptionHandler = handler;
        }

        public IEnumerable<ActorRef> LinkedActors
        {
            get { return Enumerable.Empty<ActorRef>(); }
        }

        void HandleExit(Request<Exit> message)
        {
            _fiber.Add(() => message.Respond(message.Body));

            HandleExit(message.Body);
        }

        void HandleExit(Exit message)
        {
            _engine.Shutdown();
            _fiber.Shutdown();
        }

        void HandleKill(Kill message)
        {
            ThreadPool.QueueUserWorkItem(x =>
                {
                    try
                    {
                        _fiber.Stop();
                        _engine.Shutdown();
                    }
                    catch
                    {
                    }
                });
        }
    }
}