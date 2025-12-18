// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue;

using DotNetNuke.Collections;

public class EventMessageCollection : GenericCollectionBase<EventMessage>
{
    public new void Add(EventMessage message) => base.Add(message);
}
