#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

namespace DotNetNuke.Entities.Content.Workflow.Dto
{
    /// <summary>
    /// This Dto class represents the workflow state transaction on complete state or discard state.
    /// </summary>
    public class StateTransaction
    {
        /// <summary>
        /// The content item id that represent the element that is going to change workflow state
        /// </summary>
        public int ContentItemId { get; set; }

        /// <summary>
        /// The current state of the element
        /// </summary>
        public int CurrentStateId { get; set; }

        /// <summary>
        /// This property represents the user that performs the state transaction
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// This property represents the message attached to the state transaction
        /// </summary>
        public StateTransactionMessage Message { get; set; }
    }
}
