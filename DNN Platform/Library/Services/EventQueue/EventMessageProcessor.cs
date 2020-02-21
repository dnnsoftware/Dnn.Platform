// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.EventQueue
{
	/// <summary>
	/// Basic class of EventMessageProcessor.
	/// </summary>
    public abstract class EventMessageProcessorBase
    {
        public abstract bool ProcessMessage(EventMessage message);
    }
}
