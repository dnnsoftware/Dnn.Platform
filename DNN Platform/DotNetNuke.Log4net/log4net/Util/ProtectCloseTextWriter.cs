// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util
{
    //
    // Licensed to the Apache Software Foundation (ASF) under one or more
    // contributor license agreements. See the NOTICE file distributed with
    // this work for additional information regarding copyright ownership.
    // The ASF licenses this file to you under the Apache License, Version 2.0
    // (the "License"); you may not use this file except in compliance with
    // the License. You may obtain a copy of the License at
    //
    // http://www.apache.org/licenses/LICENSE-2.0
    //
    // Unless required by applicable law or agreed to in writing, software
    // distributed under the License is distributed on an "AS IS" BASIS,
    // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    // See the License for the specific language governing permissions and
    // limitations under the License.
    //
    using System;
    using System.IO;
    using System.Text;

    using log4net.Core;

    /// <summary>
    /// A <see cref="TextWriter"/> that ignores the <see cref="Close"/> message.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This writer is used in special cases where it is necessary
    /// to protect a writer from being closed by a client.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public class ProtectCloseTextWriter : TextWriterAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectCloseTextWriter"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="writer">the writer to actually write to.</param>
        /// <remarks>
        /// <para>
        /// Create a new ProtectCloseTextWriter using a writer.
        /// </para>
        /// </remarks>
        public ProtectCloseTextWriter(TextWriter writer)
            : base(writer)
        {
        }

        /// <summary>
        /// Attach this instance to a different underlying <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">the writer to attach to.</param>
        /// <remarks>
        /// <para>
        /// Attach this instance to a different underlying <see cref="TextWriter"/>.
        /// </para>
        /// </remarks>
        public void Attach(TextWriter writer)
        {
            this.Writer = writer;
        }

        /// <summary>
        /// Does not close the underlying output writer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Does not close the underlying output writer.
        /// This method does nothing.
        /// </para>
        /// </remarks>
        public override void Close()
        {
            // do nothing
        }
    }
}
