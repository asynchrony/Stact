﻿// Copyright 2010 Chris Patterson
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
namespace Stact.Configuration.Internal
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Internals.Extensions;


    public class TypedChannelConfigurator<TChannel> :
		ConnectionBuilderConfigurator,
		ConnectionBuilderConfigurator<TChannel>
	{
		readonly Channel<TChannel> _channel;

		public TypedChannelConfigurator(Channel<TChannel> channel)
		{
		    if (channel == null)
		        throw new ArgumentNullException("channel");

			_channel = channel;
		}

		public IEnumerable<ValidateConfigurationResult> ValidateConfiguration()
		{
		    if (_channel == null)
		        yield return this.Failure("Channel", "must be specified");
		}

		public void Configure(ConnectionBuilder builder)
		{
			builder.AddChannel(_channel);
		}

		public void Configure(ConnectionBuilder<TChannel> builder)
		{
			builder.AddChannel(_channel);
		}
	}


	public class TypedChannelConfigurator<T, TChannel> :
		ConnectionBuilderConfigurator<T>
	{
		readonly Channel<TChannel> _channel;

		public TypedChannelConfigurator(Channel<TChannel> channel)
		{
		    if (channel == null)
		        throw new ArgumentNullException("channel");

		    _channel = channel;
		}

	    public void Configure(ConnectionBuilder<T> builder)
		{
			builder.AddChannel<T>(new ConvertChannel<T, TChannel>(_channel));
		}

		public IEnumerable<ValidateConfigurationResult> ValidateConfiguration()
		{
            if (_channel == null)
                yield return this.Failure("Channel", "must be specified");

			// probably need to use the header conversion thing for this to verify HeaderTypeAdapter.IsSupported() or something
		    //if (!typeof(T).IsConcreteAndAssignableTo<TChannel>())
            if (!typeof(TChannel).IsAssignableFrom(typeof(T)))
                yield return
		            this.Failure("ChannelType",
		                string.Format("{0} is not assignable to {1}", typeof(T).GetTypeName(), typeof(TChannel).GetTypeName()));
		}
	}
}