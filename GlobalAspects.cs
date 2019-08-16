using PostSharp.Patterns.Diagnostics;
using PostSharp.Extensibility;

[assembly: Log(AttributePriority = 1, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
[assembly: Log(AttributePriority = 2, AttributeExclude = true, AttributeTargetMembers = "get_*")]