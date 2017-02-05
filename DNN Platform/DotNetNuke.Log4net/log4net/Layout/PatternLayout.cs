#region Apache License
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
#endregion

using System;
using System.Collections;
using System.IO;
#if NETSTANDARD1_3
using System.Reflection;
#endif

using log4net.Core;
using log4net.Layout.Pattern;
using log4net.Util;
using log4net.Util.PatternStringConverters;
using AppDomainPatternConverter=log4net.Layout.Pattern.AppDomainPatternConverter;
using DatePatternConverter=log4net.Layout.Pattern.DatePatternConverter;
using IdentityPatternConverter=log4net.Layout.Pattern.IdentityPatternConverter;
using PropertyPatternConverter=log4net.Layout.Pattern.PropertyPatternConverter;
using UserNamePatternConverter=log4net.Layout.Pattern.UserNamePatternConverter;
using UtcDatePatternConverter=log4net.Layout.Pattern.UtcDatePatternConverter;

namespace log4net.Layout
{
	/// <summary>
	/// A flexible layout configurable with pattern string.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The goal of this class is to <see cref="M:PatternLayout.Format(TextWriter,LoggingEvent)"/> a 
	/// <see cref="LoggingEvent"/> as a string. The results
	/// depend on the <i>conversion pattern</i>.
	/// </para>
	/// <para>
	/// The conversion pattern is closely related to the conversion
	/// pattern of the printf function in C. A conversion pattern is
	/// composed of literal text and format control expressions called
	/// <i>conversion specifiers</i>.
	/// </para>
	/// <para>
	/// <i>You are free to insert any literal text within the conversion
	/// pattern.</i>
	/// </para>
	/// <para>
	/// Each conversion specifier starts with a percent sign (%) and is
	/// followed by optional <i>format modifiers</i> and a <i>conversion
	/// pattern name</i>. The conversion pattern name specifies the type of
	/// data, e.g. logger, level, date, thread name. The format
	/// modifiers control such things as field width, padding, left and
	/// right justification. The following is a simple example.
	/// </para>
	/// <para>
	/// Let the conversion pattern be <b>"%-5level [%thread]: %message%newline"</b> and assume
	/// that the log4net environment was set to use a PatternLayout. Then the
	/// statements
	/// </para>
	/// <code lang="C#">
	/// ILog log = LogManager.GetLogger(typeof(TestApp));
	/// log.Debug("Message 1");
	/// log.Warn("Message 2");   
	/// </code>
	/// <para>would yield the output</para>
	/// <code>
	/// DEBUG [main]: Message 1
	/// WARN  [main]: Message 2  
	/// </code>
	/// <para>
	/// Note that there is no explicit separator between text and
	/// conversion specifiers. The pattern parser knows when it has reached
	/// the end of a conversion specifier when it reads a conversion
	/// character. In the example above the conversion specifier
	/// <b>%-5level</b> means the level of the logging event should be left
	/// justified to a width of five characters.
	/// </para>
	/// <para>
	/// The recognized conversion pattern names are:
	/// </para>
	/// <list type="table">
	///     <listheader>
	///         <term>Conversion Pattern Name</term>
	///         <description>Effect</description>
	///     </listheader>
	///     <item>
	///         <term>a</term>
	///         <description>Equivalent to <b>appdomain</b></description>
	///     </item>
	///     <item>
	///         <term>appdomain</term>
	///         <description>
	///				Used to output the friendly name of the AppDomain where the 
	///				logging event was generated. 
	///         </description>
	///     </item>
	///     <item>
	///         <term>aspnet-cache</term>
	///         <description>
    ///             <para>
    ///             Used to output all cache items in the case of <b>%aspnet-cache</b> or just one named item if used as <b>%aspnet-cache{key}</b>
    ///             </para>
    ///             <para>
    ///             This pattern is not available for Compact Framework or Client Profile assemblies.
    ///             </para>
	///         </description>
	///     </item>
	///     <item>
	///         <term>aspnet-context</term>
	///         <description>
    ///             <para>
    ///             Used to output all context items in the case of <b>%aspnet-context</b> or just one named item if used as <b>%aspnet-context{key}</b>
    ///             </para>
    ///             <para>
    ///             This pattern is not available for Compact Framework or Client Profile assemblies.
    ///             </para>
    ///         </description>
	///     </item>
	///     <item>
	///         <term>aspnet-request</term>
	///         <description>
    ///             <para>
    ///             Used to output all request parameters in the case of <b>%aspnet-request</b> or just one named param if used as <b>%aspnet-request{key}</b>
    ///             </para>
    ///             <para>
    ///             This pattern is not available for Compact Framework or Client Profile assemblies.
    ///             </para>
    ///         </description>
	///     </item>
	///     <item>
	///         <term>aspnet-session</term>
	///         <description>
    ///             <para>
    ///             Used to output all session items in the case of <b>%aspnet-session</b> or just one named item if used as <b>%aspnet-session{key}</b>
    ///             </para>
    ///             <para>
    ///             This pattern is not available for Compact Framework or Client Profile assemblies.
    ///             </para>
    ///         </description>
	///     </item>
	///     <item>
	///         <term>c</term>
	///         <description>Equivalent to <b>logger</b></description>
	///     </item>
	///     <item>
	///         <term>C</term>
	///         <description>Equivalent to <b>type</b></description>
	///     </item>
	///     <item>
	///         <term>class</term>
	///         <description>Equivalent to <b>type</b></description>
	///     </item>
	///     <item>
	///         <term>d</term>
	///         <description>Equivalent to <b>date</b></description>
	///     </item>
	///     <item>
	///			<term>date</term> 
	///			<description>
	/// 			<para>
	/// 			Used to output the date of the logging event in the local time zone. 
	/// 			To output the date in universal time use the <c>%utcdate</c> pattern.
	/// 			The date conversion 
	/// 			specifier may be followed by a <i>date format specifier</i> enclosed 
	/// 			between braces. For example, <b>%date{HH:mm:ss,fff}</b> or
	/// 			<b>%date{dd MMM yyyy HH:mm:ss,fff}</b>.  If no date format specifier is 
	/// 			given then ISO8601 format is
	/// 			assumed (<see cref="log4net.DateFormatter.Iso8601DateFormatter"/>).
	/// 			</para>
	/// 			<para>
	/// 			The date format specifier admits the same syntax as the
	/// 			time pattern string of the <see cref="M:DateTime.ToString(string)"/>.
	/// 			</para>
	/// 			<para>
	/// 			For better results it is recommended to use the log4net date
	/// 			formatters. These can be specified using one of the strings
	/// 			"ABSOLUTE", "DATE" and "ISO8601" for specifying 
	/// 			<see cref="log4net.DateFormatter.AbsoluteTimeDateFormatter"/>, 
	/// 			<see cref="log4net.DateFormatter.DateTimeDateFormatter"/> and respectively 
	/// 			<see cref="log4net.DateFormatter.Iso8601DateFormatter"/>. For example, 
	/// 			<b>%date{ISO8601}</b> or <b>%date{ABSOLUTE}</b>.
	/// 			</para>
	/// 			<para>
	/// 			These dedicated date formatters perform significantly
	/// 			better than <see cref="M:DateTime.ToString(string)"/>.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>exception</term>
	///			<description>
	///				<para>
	///				Used to output the exception passed in with the log message.
	///				</para>
	///				<para>
	/// 			If an exception object is stored in the logging event
	/// 			it will be rendered into the pattern output with a
	/// 			trailing newline.
	/// 			If there is no exception then nothing will be output
	/// 			and no trailing newline will be appended.
	/// 			It is typical to put a newline before the exception
	/// 			and to have the exception as the last data in the pattern.
	///				</para>
	///			</description>
	///		</item>
	///     <item>
	///         <term>F</term>
	///         <description>Equivalent to <b>file</b></description>
	///     </item>
	///		<item>
	///			<term>file</term>
	///			<description>
	///				<para>
	///				Used to output the file name where the logging request was
	///				issued.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller location information is
	///				extremely slow. Its use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	/// 			<para>
	/// 			See the note below on the availability of caller location information.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>identity</term>
	///			<description>
	///				<para>
	///				Used to output the user name for the currently active user
	///				(Principal.Identity.Name).
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller information is
	///				extremely slow. Its use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///     <item>
	///         <term>l</term>
	///         <description>Equivalent to <b>location</b></description>
	///     </item>
	///     <item>
	///         <term>L</term>
	///         <description>Equivalent to <b>line</b></description>
	///     </item>
	///		<item>
	///			<term>location</term>
	///			<description>
	/// 			<para>
	/// 			Used to output location information of the caller which generated
	/// 			the logging event.
	/// 			</para>
	/// 			<para>
	/// 			The location information depends on the CLI implementation but
	/// 			usually consists of the fully qualified name of the calling
	/// 			method followed by the callers source the file name and line
	/// 			number between parentheses.
	/// 			</para>
	/// 			<para>
	/// 			The location information can be very useful. However, its
	/// 			generation is <b>extremely</b> slow. Its use should be avoided
	/// 			unless execution speed is not an issue.
	/// 			</para>
	/// 			<para>
	/// 			See the note below on the availability of caller location information.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>level</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the level of the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>line</term>
	///			<description>
	///				<para>
	///				Used to output the line number from where the logging request
	///				was issued.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller location information is
	///				extremely slow. Its use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	/// 			<para>
	/// 			See the note below on the availability of caller location information.
	/// 			</para>
	///			</description>
	///		</item>
	///     <item>
	///         <term>logger</term>
	///         <description>
	///             <para>
	///				Used to output the logger of the logging event. The
	/// 			logger conversion specifier can be optionally followed by
	/// 			<i>precision specifier</i>, that is a decimal constant in
	/// 			brackets.
	///             </para>
	/// 			<para>
	/// 			If a precision specifier is given, then only the corresponding
	/// 			number of right most components of the logger name will be
	/// 			printed. By default the logger name is printed in full.
	/// 			</para>
	/// 			<para>
	/// 			For example, for the logger name "a.b.c" the pattern
	/// 			<b>%logger{2}</b> will output "b.c".
	/// 			</para>
	///         </description>
	///     </item>
	///     <item>
	///         <term>m</term>
	///         <description>Equivalent to <b>message</b></description>
	///     </item>
	///     <item>
	///         <term>M</term>
	///         <description>Equivalent to <b>method</b></description>
	///     </item>
	///		<item>
	///			<term>message</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the application supplied message associated with 
	/// 			the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>mdc</term>
	///			<description>
	/// 			<para>
	/// 			The MDC (old name for the ThreadContext.Properties) is now part of the
	/// 			combined event properties. This pattern is supported for compatibility
	/// 			but is equivalent to <b>property</b>.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>method</term>
	///			<description>
	///				<para>
	///				Used to output the method name where the logging request was
	///				issued.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller location information is
	///				extremely slow. Its use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	/// 			<para>
	/// 			See the note below on the availability of caller location information.
	/// 			</para>
	///			</description>
	///		</item>
	///     <item>
	///         <term>n</term>
	///         <description>Equivalent to <b>newline</b></description>
	///     </item>
	///		<item>
	///			<term>newline</term>
	///			<description>
	/// 			<para>
	/// 			Outputs the platform dependent line separator character or
	/// 			characters.
	/// 			</para>
	/// 			<para>
	/// 			This conversion pattern offers the same performance as using 
	/// 			non-portable line separator strings such as	"\n", or "\r\n". 
	/// 			Thus, it is the preferred way of specifying a line separator.
	/// 			</para> 
	///			</description>
	///		</item>
	///		<item>
	///			<term>ndc</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the NDC (nested diagnostic context) associated
	/// 			with the thread that generated the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///     <item>
	///         <term>p</term>
	///         <description>Equivalent to <b>level</b></description>
	///     </item>
	///     <item>
	///         <term>P</term>
	///         <description>Equivalent to <b>property</b></description>
	///     </item>
	///     <item>
	///         <term>properties</term>
	///         <description>Equivalent to <b>property</b></description>
	///     </item>
	///		<item>
	///			<term>property</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the an event specific property. The key to 
	/// 			lookup must be specified within braces and directly following the
	/// 			pattern specifier, e.g. <b>%property{user}</b> would include the value
	/// 			from the property that is keyed by the string 'user'. Each property value
	/// 			that is to be included in the log must be specified separately.
	/// 			Properties are added to events by loggers or appenders. By default 
	/// 			the <c>log4net:HostName</c> property is set to the name of machine on 
	/// 			which the event was originally logged.
	/// 			</para>
	/// 			<para>
	/// 			If no key is specified, e.g. <b>%property</b> then all the keys and their
	/// 			values are printed in a comma separated list.
	/// 			</para>
	/// 			<para>
	/// 			The properties of an event are combined from a number of different
	/// 			contexts. These are listed below in the order in which they are searched.
	/// 			</para>
	/// 			<list type="definition">
	/// 				<item>
	/// 					<term>the event properties</term>
	/// 					<description>
	/// 					The event has <see cref="LoggingEvent.Properties"/> that can be set. These 
	/// 					properties are specific to this event only.
	/// 					</description>
	/// 				</item>
	/// 				<item>
	/// 					<term>the thread properties</term>
	/// 					<description>
	/// 					The <see cref="ThreadContext.Properties"/> that are set on the current
	/// 					thread. These properties are shared by all events logged on this thread.
	/// 					</description>
	/// 				</item>
	/// 				<item>
	/// 					<term>the global properties</term>
	/// 					<description>
	/// 					The <see cref="GlobalContext.Properties"/> that are set globally. These 
	/// 					properties are shared by all the threads in the AppDomain.
	/// 					</description>
	/// 				</item>
	/// 			</list>
	/// 			
	///			</description>
	///		</item>
	///     <item>
	///         <term>r</term>
	///         <description>Equivalent to <b>timestamp</b></description>
	///     </item>
	/// 	<item>
	///			<term>stacktrace</term> 
	///			<description>
	/// 			<para>
	/// 			Used to output the stack trace of the logging event
	/// 			The stack trace level specifier may be enclosed 
	/// 			between braces. For example, <b>%stacktrace{level}</b>.  
	/// 			If no stack trace level specifier is given then 1 is assumed 
	/// 			</para>
    /// 			<para>
    /// 			Output uses the format:
    /// 			type3.MethodCall3 > type2.MethodCall2 > type1.MethodCall1
    /// 			</para>
    ///             <para>
    ///             This pattern is not available for Compact Framework assemblies.
    ///             </para>
    ///			</description>
	///		</item>
    /// 	<item>
    ///			<term>stacktracedetail</term> 
    ///			<description>
    /// 			<para>
    /// 			Used to output the stack trace of the logging event
    /// 			The stack trace level specifier may be enclosed 
    /// 			between braces. For example, <b>%stacktracedetail{level}</b>.  
    /// 			If no stack trace level specifier is given then 1 is assumed 
    /// 			</para>
    /// 			<para>
    /// 			Output uses the format:
    ///             type3.MethodCall3(type param,...) > type2.MethodCall2(type param,...) > type1.MethodCall1(type param,...)
    /// 			</para>
    ///             <para>
    ///             This pattern is not available for Compact Framework assemblies.
    ///             </para>
    ///			</description>
    ///		</item>
    ///     <item>
	///         <term>t</term>
	///         <description>Equivalent to <b>thread</b></description>
	///     </item>
	///		<item>
	///			<term>timestamp</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the number of milliseconds elapsed since the start
	/// 			of the application until the creation of the logging event.
	/// 			</para>
	///			</description>
	///		</item>
	///		<item>
	///			<term>thread</term>
	///			<description>
	/// 			<para>
	/// 			Used to output the name of the thread that generated the
	/// 			logging event. Uses the thread number if no name is available.
	/// 			</para>
	///			</description>
	///		</item>
	///     <item>
	///			<term>type</term> 
	///			<description>
	/// 			<para>
	/// 			Used to output the fully qualified type name of the caller
	/// 			issuing the logging request. This conversion specifier
	/// 			can be optionally followed by <i>precision specifier</i>, that
	/// 			is a decimal constant in brackets.
	/// 			</para>
	/// 			<para>
	/// 			If a precision specifier is given, then only the corresponding
	/// 			number of right most components of the class name will be
	/// 			printed. By default the class name is output in fully qualified form.
	/// 			</para>
	/// 			<para>
	/// 			For example, for the class name "log4net.Layout.PatternLayout", the
	/// 			pattern <b>%type{1}</b> will output "PatternLayout".
	/// 			</para>
	/// 			<para>
	/// 			<b>WARNING</b> Generating the caller class information is
	/// 			slow. Thus, its use should be avoided unless execution speed is
	/// 			not an issue.
	/// 			</para>
	/// 			<para>
	/// 			See the note below on the availability of caller location information.
	/// 			</para>
	///			</description>
	///     </item>
	///     <item>
	///         <term>u</term>
	///         <description>Equivalent to <b>identity</b></description>
	///     </item>
	///		<item>
	///			<term>username</term>
	///			<description>
	///				<para>
	///				Used to output the WindowsIdentity for the currently
	///				active user.
	///				</para>
	///				<para>
	///				<b>WARNING</b> Generating caller WindowsIdentity information is
	///				extremely slow. Its use should be avoided unless execution speed
	///				is not an issue.
	///				</para>
	///			</description>
	///		</item>
	///     <item>
	///			<term>utcdate</term> 
	///			<description>
	/// 			<para>
	/// 			Used to output the date of the logging event in universal time. 
	/// 			The date conversion 
	/// 			specifier may be followed by a <i>date format specifier</i> enclosed 
	/// 			between braces. For example, <b>%utcdate{HH:mm:ss,fff}</b> or
	/// 			<b>%utcdate{dd MMM yyyy HH:mm:ss,fff}</b>.  If no date format specifier is 
	/// 			given then ISO8601 format is
	/// 			assumed (<see cref="log4net.DateFormatter.Iso8601DateFormatter"/>).
	/// 			</para>
	/// 			<para>
	/// 			The date format specifier admits the same syntax as the
	/// 			time pattern string of the <see cref="M:DateTime.ToString(string)"/>.
	/// 			</para>
	/// 			<para>
	/// 			For better results it is recommended to use the log4net date
	/// 			formatters. These can be specified using one of the strings
	/// 			"ABSOLUTE", "DATE" and "ISO8601" for specifying 
	/// 			<see cref="log4net.DateFormatter.AbsoluteTimeDateFormatter"/>, 
	/// 			<see cref="log4net.DateFormatter.DateTimeDateFormatter"/> and respectively 
	/// 			<see cref="log4net.DateFormatter.Iso8601DateFormatter"/>. For example, 
	/// 			<b>%utcdate{ISO8601}</b> or <b>%utcdate{ABSOLUTE}</b>.
	/// 			</para>
	/// 			<para>
	/// 			These dedicated date formatters perform significantly
	/// 			better than <see cref="M:DateTime.ToString(string)"/>.
	/// 			</para>
	///			</description>
	///		</item>
	///     <item>
	///         <term>w</term>
	///         <description>Equivalent to <b>username</b></description>
	///     </item>
	///     <item>
	///         <term>x</term>
	///         <description>Equivalent to <b>ndc</b></description>
	///     </item>
	///     <item>
	///         <term>X</term>
	///         <description>Equivalent to <b>mdc</b></description>
	///     </item>
	///		<item>
	///			<term>%</term>
	///			<description>
	/// 			<para>
	/// 			The sequence %% outputs a single percent sign.
	/// 			</para>
	///			</description>
	///		</item>
	/// </list>
	/// <para>
	/// The single letter patterns are deprecated in favor of the 
	/// longer more descriptive pattern names.
	/// </para>
	/// <para>
	/// By default the relevant information is output as is. However,
	/// with the aid of format modifiers it is possible to change the
	/// minimum field width, the maximum field width and justification.
	/// </para>
	/// <para>
	/// The optional format modifier is placed between the percent sign
	/// and the conversion pattern name.
	/// </para>
	/// <para>
	/// The first optional format modifier is the <i>left justification
	/// flag</i> which is just the minus (-) character. Then comes the
	/// optional <i>minimum field width</i> modifier. This is a decimal
	/// constant that represents the minimum number of characters to
	/// output. If the data item requires fewer characters, it is padded on
	/// either the left or the right until the minimum width is
	/// reached. The default is to pad on the left (right justify) but you
	/// can specify right padding with the left justification flag. The
	/// padding character is space. If the data item is larger than the
	/// minimum field width, the field is expanded to accommodate the
	/// data. The value is never truncated.
	/// </para>
	/// <para>
	/// This behavior can be changed using the <i>maximum field
	/// width</i> modifier which is designated by a period followed by a
	/// decimal constant. If the data item is longer than the maximum
	/// field, then the extra characters are removed from the
	/// <i>beginning</i> of the data item and not from the end. For
	/// example, it the maximum field width is eight and the data item is
	/// ten characters long, then the first two characters of the data item
	/// are dropped. This behavior deviates from the printf function in C
	/// where truncation is done from the end.
	/// </para>
	/// <para>
	/// Below are various format modifier examples for the logger
	/// conversion specifier.
	/// </para>
	/// <div class="tablediv">
	///		<table class="dtTABLE" cellspacing="0">
	///			<tr>
	///				<th>Format modifier</th>
	///				<th>left justify</th>
	///				<th>minimum width</th>
	///				<th>maximum width</th>
	///				<th>comment</th>
	///			</tr>
	///			<tr>
	///				<td align="center">%20logger</td>
	///				<td align="center">false</td>
	///				<td align="center">20</td>
	///				<td align="center">none</td>
	///				<td>
	///					<para>
	///					Left pad with spaces if the logger name is less than 20
	///					characters long.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%-20logger</td>
	///				<td align="center">true</td>
	///				<td align="center">20</td>
	///				<td align="center">none</td>
	///				<td>
	///					<para>
	///					Right pad with spaces if the logger 
	///					name is less than 20 characters long.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%.30logger</td>
	///				<td align="center">NA</td>
	///				<td align="center">none</td>
	///				<td align="center">30</td>
	///				<td>
	///					<para>
	///					Truncate from the beginning if the logger 
	///					name is longer than 30 characters.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center"><nobr>%20.30logger</nobr></td>
	///				<td align="center">false</td>
	///				<td align="center">20</td>
	///				<td align="center">30</td>
	///				<td>
	///					<para>
	///					Left pad with spaces if the logger name is shorter than 20
	///					characters. However, if logger name is longer than 30 characters,
	///					then truncate from the beginning.
	///					</para>
	///				</td>
	///			</tr>
	///			<tr>
	///				<td align="center">%-20.30logger</td>
	///				<td align="center">true</td>
	///				<td align="center">20</td>
	///				<td align="center">30</td>
	///				<td>
	///					<para>
	///					Right pad with spaces if the logger name is shorter than 20
	///					characters. However, if logger name is longer than 30 characters,
	///					then truncate from the beginning.
	///					</para>
	///				</td>
	///			</tr>
	///		</table>
	///	</div>
	///	<para>
	///	<b>Note about caller location information.</b><br />
	///	The following patterns <c>%type %file %line %method %location %class %C %F %L %l %M</c> 
	///	all generate caller location information.
	/// Location information uses the <c>System.Diagnostics.StackTrace</c> class to generate
	/// a call stack. The caller's information is then extracted from this stack.
	/// </para>
	/// <note type="caution">
	/// <para>
	/// The <c>System.Diagnostics.StackTrace</c> class is not supported on the 
	/// .NET Compact Framework 1.0 therefore caller location information is not
	/// available on that framework.
	/// </para>
	/// </note>
	/// <note type="caution">
	/// <para>
	/// The <c>System.Diagnostics.StackTrace</c> class has this to say about Release builds:
	/// </para>
	/// <para>
	/// "StackTrace information will be most informative with Debug build configurations. 
	/// By default, Debug builds include debug symbols, while Release builds do not. The 
	/// debug symbols contain most of the file, method name, line number, and column 
	/// information used in constructing StackFrame and StackTrace objects. StackTrace 
	/// might not report as many method calls as expected, due to code transformations 
	/// that occur during optimization."
	/// </para>
	/// <para>
	/// This means that in a Release build the caller information may be incomplete or may 
	/// not exist at all! Therefore caller location information cannot be relied upon in a Release build.
	/// </para>
	/// </note>
	/// <para>
	/// Additional pattern converters may be registered with a specific <see cref="PatternLayout"/>
	/// instance using the <see cref="M:AddConverter(string, Type)"/> method.
	/// </para>
	/// </remarks>
	/// <example>
	/// This is a more detailed pattern.
	/// <code><b>%timestamp [%thread] %level %logger %ndc - %message%newline</b></code>
	/// </example>
	/// <example>
	/// A similar pattern except that the relative time is
	/// right padded if less than 6 digits, thread name is right padded if
	/// less than 15 characters and truncated if longer and the logger
	/// name is left padded if shorter than 30 characters and truncated if
	/// longer.
	/// <code><b>%-6timestamp [%15.15thread] %-5level %30.30logger %ndc - %message%newline</b></code>
	/// </example>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Douglas de la Torre</author>
	/// <author>Daniel Cazzulino</author>
	public class PatternLayout : LayoutSkeleton
	{
		#region Constants

		/// <summary>
		/// Default pattern string for log output. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default pattern string for log output. 
		/// Currently set to the string <b>"%message%newline"</b> 
		/// which just prints the application supplied message. 
		/// </para>
		/// </remarks>
		public const string DefaultConversionPattern ="%message%newline";

		/// <summary>
		/// A detailed conversion pattern
		/// </summary>
		/// <remarks>
		/// <para>
		/// A conversion pattern which includes Time, Thread, Logger, and Nested Context.
		/// Current value is <b>%timestamp [%thread] %level %logger %ndc - %message%newline</b>.
		/// </para>
		/// </remarks>
		public const string DetailConversionPattern = "%timestamp [%thread] %level %logger %ndc - %message%newline";

		#endregion

		#region Static Fields

		/// <summary>
		/// Internal map of converter identifiers to converter types.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This static map is overridden by the m_converterRegistry instance map
		/// </para>
		/// </remarks>
		private static Hashtable s_globalRulesRegistry;

		#endregion Static Fields

		#region Member Variables
    
		/// <summary>
		/// the pattern
		/// </summary>
		private string m_pattern;
  
		/// <summary>
		/// the head of the pattern converter chain
		/// </summary>
		private PatternConverter m_head;

		/// <summary>
		/// patterns defined on this PatternLayout only
		/// </summary>
		private Hashtable m_instanceRulesRegistry = new Hashtable();

		#endregion

		#region Static Constructor

		/// <summary>
		/// Initialize the global registry
		/// </summary>
		/// <remarks>
		/// <para>
		/// Defines the builtin global rules.
		/// </para>
		/// </remarks>
		static PatternLayout()
		{
			s_globalRulesRegistry = new Hashtable(45);

			s_globalRulesRegistry.Add("literal", typeof(LiteralPatternConverter));
			s_globalRulesRegistry.Add("newline", typeof(NewLinePatternConverter));
			s_globalRulesRegistry.Add("n", typeof(NewLinePatternConverter));

// .NET Compact Framework 1.0 has no support for ASP.NET
// SSCLI 1.0 has no support for ASP.NET
#if !NETCF && !SSCLI && !CLIENT_PROFILE && !NETSTANDARD1_3
			s_globalRulesRegistry.Add("aspnet-cache", typeof(AspNetCachePatternConverter));
			s_globalRulesRegistry.Add("aspnet-context", typeof(AspNetContextPatternConverter));
			s_globalRulesRegistry.Add("aspnet-request", typeof(AspNetRequestPatternConverter));
			s_globalRulesRegistry.Add("aspnet-session", typeof(AspNetSessionPatternConverter));
#endif

			s_globalRulesRegistry.Add("c", typeof(LoggerPatternConverter));
			s_globalRulesRegistry.Add("logger", typeof(LoggerPatternConverter));

			s_globalRulesRegistry.Add("C", typeof(TypeNamePatternConverter));
			s_globalRulesRegistry.Add("class", typeof(TypeNamePatternConverter));
			s_globalRulesRegistry.Add("type", typeof(TypeNamePatternConverter));

			s_globalRulesRegistry.Add("d", typeof(DatePatternConverter));
			s_globalRulesRegistry.Add("date", typeof(DatePatternConverter));

			s_globalRulesRegistry.Add("exception", typeof(ExceptionPatternConverter));

			s_globalRulesRegistry.Add("F", typeof(FileLocationPatternConverter));
			s_globalRulesRegistry.Add("file", typeof(FileLocationPatternConverter));

			s_globalRulesRegistry.Add("l", typeof(FullLocationPatternConverter));
			s_globalRulesRegistry.Add("location", typeof(FullLocationPatternConverter));

			s_globalRulesRegistry.Add("L", typeof(LineLocationPatternConverter));
			s_globalRulesRegistry.Add("line", typeof(LineLocationPatternConverter));

			s_globalRulesRegistry.Add("m", typeof(MessagePatternConverter));
			s_globalRulesRegistry.Add("message", typeof(MessagePatternConverter));

			s_globalRulesRegistry.Add("M", typeof(MethodLocationPatternConverter));
			s_globalRulesRegistry.Add("method", typeof(MethodLocationPatternConverter));

			s_globalRulesRegistry.Add("p", typeof(LevelPatternConverter));
			s_globalRulesRegistry.Add("level", typeof(LevelPatternConverter));

			s_globalRulesRegistry.Add("P", typeof(PropertyPatternConverter));
			s_globalRulesRegistry.Add("property", typeof(PropertyPatternConverter));
			s_globalRulesRegistry.Add("properties", typeof(PropertyPatternConverter));

			s_globalRulesRegistry.Add("r", typeof(RelativeTimePatternConverter));
			s_globalRulesRegistry.Add("timestamp", typeof(RelativeTimePatternConverter));
			
#if !(NETCF || NETSTANDARD1_3)
			s_globalRulesRegistry.Add("stacktrace", typeof(StackTracePatternConverter));
            s_globalRulesRegistry.Add("stacktracedetail", typeof(StackTraceDetailPatternConverter));
#endif

			s_globalRulesRegistry.Add("t", typeof(ThreadPatternConverter));
			s_globalRulesRegistry.Add("thread", typeof(ThreadPatternConverter));

			// For backwards compatibility the NDC patterns
			s_globalRulesRegistry.Add("x", typeof(NdcPatternConverter));
			s_globalRulesRegistry.Add("ndc", typeof(NdcPatternConverter));

			// For backwards compatibility the MDC patterns just do a property lookup
			s_globalRulesRegistry.Add("X", typeof(PropertyPatternConverter));
			s_globalRulesRegistry.Add("mdc", typeof(PropertyPatternConverter));

			s_globalRulesRegistry.Add("a", typeof(AppDomainPatternConverter));
			s_globalRulesRegistry.Add("appdomain", typeof(AppDomainPatternConverter));

			s_globalRulesRegistry.Add("u", typeof(IdentityPatternConverter));
			s_globalRulesRegistry.Add("identity", typeof(IdentityPatternConverter));

			s_globalRulesRegistry.Add("utcdate", typeof(UtcDatePatternConverter));
			s_globalRulesRegistry.Add("utcDate", typeof(UtcDatePatternConverter));
			s_globalRulesRegistry.Add("UtcDate", typeof(UtcDatePatternConverter));

			s_globalRulesRegistry.Add("w", typeof(UserNamePatternConverter));
			s_globalRulesRegistry.Add("username", typeof(UserNamePatternConverter));
		}

		#endregion Static Constructor

		#region Constructors

		/// <summary>
		/// Constructs a PatternLayout using the DefaultConversionPattern
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default pattern just produces the application supplied message.
		/// </para>
		/// <para>
		/// Note to Inheritors: This constructor calls the virtual method
		/// <see cref="CreatePatternParser"/>. If you override this method be
		/// aware that it will be called before your is called constructor.
		/// </para>
		/// <para>
		/// As per the <see cref="IOptionHandler"/> contract the <see cref="ActivateOptions"/>
		/// method must be called after the properties on this object have been
		/// configured.
		/// </para>
		/// </remarks>
		public PatternLayout() : this(DefaultConversionPattern)
		{
		}

		/// <summary>
		/// Constructs a PatternLayout using the supplied conversion pattern
		/// </summary>
		/// <param name="pattern">the pattern to use</param>
		/// <remarks>
		/// <para>
		/// Note to Inheritors: This constructor calls the virtual method
		/// <see cref="CreatePatternParser"/>. If you override this method be
		/// aware that it will be called before your is called constructor.
		/// </para>
		/// <para>
		/// When using this constructor the <see cref="ActivateOptions"/> method 
		/// need not be called. This may not be the case when using a subclass.
		/// </para>
		/// </remarks>
		public PatternLayout(string pattern) 
		{
			// By default we do not process the exception
			IgnoresException = true;

			m_pattern = pattern;
			if (m_pattern == null)
			{
				m_pattern = DefaultConversionPattern;
			}

			ActivateOptions();
		}

		#endregion
  
		/// <summary>
		/// The pattern formatting string
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <b>ConversionPattern</b> option. This is the string which
		/// controls formatting and consists of a mix of literal content and
		/// conversion specifiers.
		/// </para>
		/// </remarks>
		public string ConversionPattern
		{
			get { return m_pattern;	}
			set { m_pattern = value; }
		}

		/// <summary>
		/// Create the pattern parser instance
		/// </summary>
		/// <param name="pattern">the pattern to parse</param>
		/// <returns>The <see cref="PatternParser"/> that will format the event</returns>
		/// <remarks>
		/// <para>
		/// Creates the <see cref="PatternParser"/> used to parse the conversion string. Sets the
		/// global and instance rules on the <see cref="PatternParser"/>.
		/// </para>
		/// </remarks>
		virtual protected PatternParser CreatePatternParser(string pattern) 
		{
			PatternParser patternParser = new PatternParser(pattern);

			// Add all the builtin patterns
			foreach(DictionaryEntry entry in s_globalRulesRegistry)
			{
                ConverterInfo converterInfo = new ConverterInfo();
                converterInfo.Name = (string)entry.Key;
                converterInfo.Type = (Type)entry.Value;
                patternParser.PatternConverters[entry.Key] = converterInfo;
			}
			// Add the instance patterns
			foreach(DictionaryEntry entry in m_instanceRulesRegistry)
			{
				patternParser.PatternConverters[entry.Key] = entry.Value;
			}

			return patternParser;
		}
  
		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialize layout options
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is part of the <see cref="IOptionHandler"/> delayed object
		/// activation scheme. The <see cref="ActivateOptions"/> method must 
		/// be called on this object after the configuration properties have
		/// been set. Until <see cref="ActivateOptions"/> is called this
		/// object is in an undefined state and must not be used. 
		/// </para>
		/// <para>
		/// If any of the configuration properties are modified then 
		/// <see cref="ActivateOptions"/> must be called again.
		/// </para>
		/// </remarks>
		override public void ActivateOptions() 
		{
			m_head = CreatePatternParser(m_pattern).Parse();

			PatternConverter curConverter = m_head;
			while(curConverter != null)
			{
				PatternLayoutConverter layoutConverter = curConverter as PatternLayoutConverter;
				if (layoutConverter != null)
				{
					if (!layoutConverter.IgnoresException)
					{
						// Found converter that handles the exception
						this.IgnoresException = false;

						break;
					}
				}
				curConverter = curConverter.Next;
			}
		}

		#endregion

		#region Override implementation of LayoutSkeleton

		/// <summary>
		/// Produces a formatted string as specified by the conversion pattern.
		/// </summary>
		/// <param name="loggingEvent">the event being logged</param>
		/// <param name="writer">The TextWriter to write the formatted event to</param>
		/// <remarks>
		/// <para>
		/// Parse the <see cref="LoggingEvent"/> using the patter format
		/// specified in the <see cref="ConversionPattern"/> property.
		/// </para>
		/// </remarks>
		override public void Format(TextWriter writer, LoggingEvent loggingEvent) 
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			PatternConverter c = m_head;

			// loop through the chain of pattern converters
			while(c != null) 
			{
				c.Format(writer, loggingEvent);
				c = c.Next;
			}
		}

		#endregion

		/// <summary>
		/// Add a converter to this PatternLayout
		/// </summary>
		/// <param name="converterInfo">the converter info</param>
		/// <remarks>
		/// <para>
		/// This version of the method is used by the configurator.
		/// Programmatic users should use the alternative <see cref="M:AddConverter(string,Type)"/> method.
		/// </para>
		/// </remarks>
		public void AddConverter(ConverterInfo converterInfo)
		{
            if (converterInfo == null) throw new ArgumentNullException("converterInfo");

            if (!typeof(PatternConverter).IsAssignableFrom(converterInfo.Type))
            {
                throw new ArgumentException("The converter type specified [" + converterInfo.Type + "] must be a subclass of log4net.Util.PatternConverter", "converterInfo");
            }
            m_instanceRulesRegistry[converterInfo.Name] = converterInfo;
		}

		/// <summary>
		/// Add a converter to this PatternLayout
		/// </summary>
		/// <param name="name">the name of the conversion pattern for this converter</param>
		/// <param name="type">the type of the converter</param>
		/// <remarks>
		/// <para>
		/// Add a named pattern converter to this instance. This
		/// converter will be used in the formatting of the event.
		/// This method must be called before <see cref="ActivateOptions"/>.
		/// </para>
		/// <para>
		/// The <paramref name="type"/> specified must extend the 
		/// <see cref="PatternConverter"/> type.
		/// </para>
		/// </remarks>
		public void AddConverter(string name, Type type)
		{
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");

            ConverterInfo converterInfo = new ConverterInfo();
            converterInfo.Name = name;
            converterInfo.Type = type;

            AddConverter(converterInfo);
		}
	}
}
