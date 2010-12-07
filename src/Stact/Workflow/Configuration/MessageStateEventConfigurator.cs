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
namespace Stact.Workflow.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Internal;
	using Magnum.Extensions;


	public class MessageStateEventConfigurator<TWorkflow, TInstance, TBody> :
		StateEventConfigurator<TWorkflow, TInstance, TBody>,
		StateBuilderConfigurator<TWorkflow, TInstance>
		where TWorkflow : class
		where TInstance : class
	{
		readonly IList<StateEventBuilderConfigurator<TWorkflow, TInstance, TBody>> _configurators;
		readonly Expression<Func<TWorkflow, Event<TBody>>> _eventExpression;
		readonly StateConfigurator<TWorkflow, TInstance> _stateConfigurator;

		public MessageStateEventConfigurator(StateConfigurator<TWorkflow, TInstance> stateConfigurator,
		                                     Expression<Func<TWorkflow, Event<TBody>>> eventExpression)
		{
			_stateConfigurator = stateConfigurator;
			_eventExpression = eventExpression;

			_configurators = new List<StateEventBuilderConfigurator<TWorkflow, TInstance, TBody>>();
		}

		public void ValidateConfiguration()
		{
			if (_eventExpression == null)
				throw new StateMachineWorkflowConfiguratorException("Null event expression specified");
		}

		public void Configure(StateBuilder<TWorkflow, TInstance> builder)
		{
			MessageEvent<TBody> eevent = builder.GetEvent(_eventExpression);

			var stateEventBuilder = new MessageStateEventBuilder<TWorkflow, TInstance,TBody>(builder, eevent);

			_configurators.Each(x => x.Configure(stateEventBuilder));

		}

		public void AddConfigurator(StateBuilderConfigurator<TWorkflow, TInstance> configurator)
		{
			_stateConfigurator.AddConfigurator(configurator);
		}

		public void AddConfigurator(StateEventBuilderConfigurator<TWorkflow, TInstance, TBody> configurator)
		{
			_configurators.Add(configurator);
		}

		public void AddConfigurator(StateEventBuilderConfigurator<TWorkflow, TInstance> configurator)
		{
			_configurators.Add(new ConfiguratorProxy(configurator));
		}


		class ConfiguratorProxy : 
			StateEventBuilderConfigurator<TWorkflow, TInstance, TBody>
		{
			readonly StateEventBuilderConfigurator<TWorkflow, TInstance> _configurator;

			public ConfiguratorProxy(StateEventBuilderConfigurator<TWorkflow, TInstance> configurator)
			{
				_configurator = configurator;
			}

			public void ValidateConfigurator()
			{
				_configurator.ValidateConfigurator();
			}

			public void Configure(StateEventBuilder<TWorkflow, TInstance, TBody> builder)
			{
				_configurator.Configure(builder);
			}
		}
	}
}