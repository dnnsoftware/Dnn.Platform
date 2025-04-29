# DotNetNuke.Abstractions project

This project is designed to house interfaces (and potentially other types
supporting those interfaces). This project has no dependencies (i.e. it does
_not_ reference other DotNetNuke projects) and is designed to serve as a
foundation upon which other components can build. Most of the interfaces
defined in this project will eventually be exposed via
[dependency injection](../DotNetNuke.DependencyInjection).

This project targets
[.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
2.0, meaning that it is compatible with both .NET Framework and .NET Core. Any
interfaces defined in this project cannot reference types which are only
available to .NET Framework (for example, types in the `System.Web` namespace).

## Structure

This project is an opportunity to rethink some of the core structure of DNN
Platform, so we are striving to strike a balance which does not give up on
consistency and familiarity but also encourages cleaner, simpler,
better named interfaces. To that end, we're hoping to reduce some of the
unnecessary nesting of namespaces for core types. For example, instead of
`DotNetNuke.Entities.Users.UserInfo`, we have
`DotNetNuke.Abstractions.Users.IUserInfo`. In particular, the `Entities`
namespace is confusing and will not be continued in this project. We plan to
only have one level of namespace nesting (e.g. `Users` or `Portals`) within this
project, and will also have interfaces without any namespace nesting when
appropriate (e.g. `DotNetNuke.Abstractions.INavigationManager`).

When possible, the interfaces introduced into this project should not solely be
a copy of an existing interface or class. In particular, large interfaces
should be broken up into multiple, smaller interfaces. This will simplify usage
and give more flexibility in overriding components.

## Naming

While changes in naming can cause some minor disruption for developers familiar
with the history of DNN Platform, we aim to introduce simpler and more
forward-looking naming. For example, the methods of the `IHostController`
interface could be renamed to `IHostSettingsService`. Using `Service` instead
of `Controller` as the suffix aligns with modern .NET practices and avoids
confusion with MVC and Web API controllers.
