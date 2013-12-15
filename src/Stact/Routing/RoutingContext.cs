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
namespace Stact.Routing
{
    using System;


    public interface RoutingContext
    {
        bool IsAlive { get; }

        void Evict();

        event Action<RoutingContext> OnEvicted;
    }


    public interface RoutingContext<T> :
        RoutingContext
    {
        T Body { get; }
        
        /// <summary>
        /// as types are abstracted, proxied, etc. the priority value decreases
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Matches the message header type an invokes the proper callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="requestCallback"></param>
        /// <param name="responseCallback"></param>
        void Match(Action<RoutingContext<Message<T>>> messageCallback,
                   Action<RoutingContext<Request<T>>> requestCallback,
                   Action<RoutingContext<Response<T>>> responseCallback);

        void Convert<TResult>(Action<RoutingContext<TResult>> callback);
    }
}