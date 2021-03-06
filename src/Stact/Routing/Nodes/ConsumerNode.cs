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
    /// <summary>
    /// Delivers a message to a consumer on the specified fiber.
    /// </summary>
    /// <typeparam name="T">The message type</typeparam>
    public class ConsumerNode<T> :
        ProductionNode<T>,
        Activation<T>
    {
        readonly Consumer<T> _consumer;

        public ConsumerNode(RoutingEngine engine, Consumer<T> consumer, bool disableOnActivation = true)
            : base(engine, disableOnActivation)
        {
            _consumer = consumer;
        }

        public void Activate(RoutingContext<T> context)
        {
            Accept(context, body => _consumer(body));
        }
    }
}