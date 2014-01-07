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


namespace Stact.Actors.Internal
{
    using System;
    using System.Collections.Generic;
    using Actors;
    using Configuration;


    public class PendingReceiveImpl<TMessage> :
        PendingReceive
    {
        readonly SelectiveConsumer<TMessage> _selectiveConsumer;
        readonly Action _timeoutCallback;
        private bool _cancel;
        Inbox _inbox;
        ScheduledOperation _scheduledAction;
        public event Action<PendingReceiveImpl<TMessage>> OnCompleted;

        public PendingReceiveImpl(Inbox inbox, SelectiveConsumer<TMessage> selectiveConsumer, Action timeoutCallback,
                                  Action<PendingReceiveImpl<TMessage>> onComplete)
        {
            _selectiveConsumer = selectiveConsumer;
            _inbox = inbox;
            _timeoutCallback = timeoutCallback;
            OnCompleted += onComplete;
        }

        public PendingReceiveImpl(Inbox inbox, SelectiveConsumer<TMessage> selectiveConsumer,
                                  Action<PendingReceiveImpl<TMessage>> onComplete)
            : this(inbox, selectiveConsumer, NoTimeoutCallback, onComplete)
        {
        }

        public void Cancel()
        {
            _cancel = true;

            OnCompleted(this);
        }

        public void Send<T>(T message)
        {
            _inbox.Send(message);
        }

        public PendingReceive Receive<T>(SelectiveConsumer<T> consumer)
        {
            return _inbox.Receive(consumer);
        }

        public PendingReceive Receive<T>(SelectiveConsumer<T> consumer, TimeSpan timeout, Action timeoutCallback)
        {
            return _inbox.Receive(consumer, timeout, timeoutCallback);
        }

        public void SetExceptionHandler(ActorExceptionHandler handler)
        {
            _inbox.SetExceptionHandler(handler);
        }

        public IEnumerable<ActorRef> LinkedActors
        {
            get { return _inbox.LinkedActors; }
        }

        public bool Cancelled
        {
            get { return _cancel; }
        }

        public ChannelConnection Connect(Action<ConnectionConfigurator> subscriberActions)
        {
            return _inbox.Connect(subscriberActions);
        }

        public void ScheduleTimeout(Func<PendingReceiveImpl<TMessage>, ScheduledOperation> scheduleAction)
        {
            _scheduledAction = scheduleAction(this);
        }

        static void NoTimeoutCallback()
        {
        }

        public Consumer<TMessage> Accept(TMessage message)
        {
            if (Cancelled)
                return null;

            Consumer<TMessage> consumer = _selectiveConsumer(message);
            if (consumer == null)
                return null;

            return m =>
                {
                    CancelTimeout();

                    consumer(m);

                    OnCompleted(this);
                };
        }

        public void Timeout()
        {
            if (Cancelled)
                return;

            _timeoutCallback();

            OnCompleted(this);
        }

        void CancelTimeout()
        {
            if (_scheduledAction != null)
                _scheduledAction.Cancel();
        }
    }
}