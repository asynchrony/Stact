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

using System.Linq;

namespace Stact.Routing.Nodes
{
    using System;
    using System.Collections.Generic;


    public abstract class Memory<T>
    {
        readonly ActivationList<T> _successors;
        readonly List<Func<RoutingContext<T>, bool>> _joins;
        readonly List<RoutingContext<T>> _messages;

        public Memory()
        {
            _successors = new ActivationList<T>();
            _messages = new List<RoutingContext<T>>();
            _joins = new List<Func<RoutingContext<T>, bool>>();
        }


        public bool Enabled
        {
            get { return true; }
        }

        public int Count
        {
            get { return _messages.Where(x => x.IsAlive).Count(); }
        }

        public void Activate(RoutingContext<T> context)
        {
            Add(context);
        }

        public void RightActivate(Func<RoutingContext<T>, bool> callback)
        {
            All(callback);
        }

        public void RightActivate(RoutingContext<T> context, Action<RoutingContext<T>> callback)
        {
            Any(context, callback);
        }

        public IEnumerable<Activation<T>> Successors
        {
            get { return _successors; }
        }

        public void AddActivation(Activation<T> activation)
        {
            _successors.Add(activation);

            All(context =>
                {
                    if (!activation.Enabled)
                        return false;

                    if (context.IsAlive)
                        activation.Activate(context);

                    return true;
                });
        }

        public void RemoveActivation(Activation<T> activation)
        {
            _successors.Remove(activation);
        }

        public void Add(RoutingContext<T> message)
        {
            for (int i = _joins.Count - 1; i >= 0 && message.IsAlive; i--)
            {
                if (false == _joins[i](message))
                    _joins.RemoveAt(i);
            }

            _messages.Add(message);

            RemoveDeadMessage();
        }

        private void RemoveDeadMessage()
        {
            _messages.RemoveAll(m => !m.IsAlive);
        }

        public void All(Func<RoutingContext<T>, bool> callback)
        {
            RemoveDeadMessage();

            for (int i = 0; i < _messages.Count; i++)
            {
                bool result = callback(_messages[i]);
                if (result == false)
                    return;
            }

            _joins.Add(callback);
        }

        public void Any(RoutingContext<T> match, Action<RoutingContext<T>> callback)
        {
            if (!match.IsAlive)
                return;

            var message = _messages.FirstOrDefault(match.Equals);
            if (message != null)
                callback(message);
        }
    }
}