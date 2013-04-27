﻿// Copyright 2010-2013 Chris Patterson
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
namespace Stact
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Callback used to notify that a range of Actions were not executed
    /// </summary>
    /// <param name="actions">The complete Action list</param>
    /// <param name="index">The index of the first Action not executed</param>
    /// <param name="count">The number of actions not executed</param>
    public delegate void NotifyActionsNotExecuted(IList<Action> actions, int index, int count);
}