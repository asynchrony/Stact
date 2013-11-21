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
namespace Stact.Routing.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    /// <summary>
    /// An AlphaMemory retains a list of messages and automatically removes
    /// messages from the list when they are evicted from memory based on the
    /// context property IsAvailable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActiveContextList<T>
    {
        readonly IList<Func<RoutingContext<T>, bool>> _joins;
        readonly IList<RoutingContext<T>> _messages;

        public ActiveContextList()
        {
            _messages = new List<RoutingContext<T>>();
            _joins = new List<Func<RoutingContext<T>, bool>>();
        }

        public int Count
        {
            get { return _messages.Where(x => x.IsAlive).Count(); }
        }

        public void Add(RoutingContext<T> message)
        {
            _messages.Add(message);

            for (int i = _joins.Count - 1; i >= 0 && message.IsAlive; i--)
            {
                if (false == _joins[i](message))
                    _joins.RemoveAt(i);
            }
        }

        public void All(Func<RoutingContext<T>, bool> callback)
        {
            Join(callback);
        }

        public void Any(RoutingContext<T> match, Action<RoutingContext<T>> callback)
        {
            if (!match.IsAlive)
                return;

            var message = _messages.FirstOrDefault(match.Equals);
            if (message != null)
                callback(message);
        }

        void Join(Func<RoutingContext<T>, bool> callback)
        {
            for (int i = 0; i < _messages.Count;)
            {
                if (!_messages[i].IsAlive)
                {
                    _messages.RemoveAt(i);
                    continue;
                }

                bool result = callback(_messages[i]);
                if (result == false)
                    return;

                i++;
            }

            _joins.Add(callback);
        }
    }
}