// Copyright 2010-2013 Chris Patterson
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


    public class IntervalConfiguratorImpl<TChannel> :
        SchedulerFactoryConfiguratorImpl<IntervalConfigurator<TChannel>>,
        IntervalConfigurator<TChannel>,
        ChannelBuilderConfigurator<TChannel>
    {
        readonly IList<ChannelBuilderConfigurator<ICollection<TChannel>>> _configurators;
        readonly TimeSpan _interval;

        public IntervalConfiguratorImpl(TimeSpan interval)
        {
            _interval = interval;

            _configurators = new List<ChannelBuilderConfigurator<ICollection<TChannel>>>();

            UseTimerScheduler();
            HandleOnPoolFiber();
        }

        public void Configure(ChannelBuilder<TChannel> builder)
        {
            FiberFactory fiberFactory = () => { return GetConfiguredFiber(builder); };

            SchedulerFactory schedulerFactory = () => { return GetConfiguredScheduler(builder); };

            var intervalBuilder = new IntervalChannelBuilder<TChannel>(builder, fiberFactory, schedulerFactory,
                _interval);

            foreach (var configurator in _configurators)
                configurator.Configure(intervalBuilder);
        }

        public void ValidateConfiguration()
        {
            if (_interval <= TimeSpan.Zero)
                throw new ChannelConfigurationException(typeof(TChannel), "Interval must be greater than zero");

            if (_configurators.Count == 0)
                throw new ChannelConfigurationException(typeof(TChannel), "No channels were configured");

            ValidateFiberFactoryConfiguration();
            ValidateSchedulerFactoryConfiguration();

            foreach (var configurator in _configurators)
                configurator.ValidateConfiguration();
        }

        public void AddConfigurator(ChannelBuilderConfigurator<ICollection<TChannel>> configurator)
        {
            _configurators.Add(configurator);
        }
    }
}