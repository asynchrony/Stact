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
namespace Stact.Routing.Contexts
{
    using System;


    public class MessageRoutingContextProxy<TInput, TOutput> :
        RoutingContext<Message<TOutput>>,
        RoutingContext<TOutput>
        where TInput : TOutput
    {
        readonly RoutingContext<TInput> _input;
        readonly MessageProxy<TInput, TOutput> _message;
        readonly int _priority;

        public MessageRoutingContextProxy(RoutingContext<TInput> input, Message<TInput> message)
        {
            _input = input;
            _message = new MessageProxy<TInput, TOutput>(message);
            _priority = input.Priority - 1000;
        }

        public bool IsAlive
        {
            get { return _input.IsAlive; }
        }

        public void Evict()
        {
            _input.Evict();

            if (OnEvicted != null)
                OnEvicted(this);
        }

        public event Action<RoutingContext> OnEvicted;

        public Message<TOutput> Body
        {
            get { return _message; }
        }

        int RoutingContext<Message<TOutput>>.Priority
        {
            get { return _priority; }
        }

        public void Match(Action<RoutingContext<Message<Message<TOutput>>>> messageCallback,
                          Action<RoutingContext<Request<Message<TOutput>>>> requestCallback,
                          Action<RoutingContext<Response<Message<TOutput>>>> responseCallback)
        {
            throw new StactException("Nesting of header interfaces is not supported.");
        }

        public void Convert<TResult>(Action<RoutingContext<TResult>> callback)
        {
            _input.Convert(callback);
        }

        int RoutingContext<TOutput>.Priority
        {
            get { return _priority; }
        }

        TOutput RoutingContext<TOutput>.Body
        {
            get { return _message.Body; }
        }

        public void Match(Action<RoutingContext<Message<TOutput>>> messageCallback,
                          Action<RoutingContext<Request<TOutput>>> requestCallback,
                          Action<RoutingContext<Response<TOutput>>> responseCallback)
        {
            messageCallback(this);
        }
    }
}