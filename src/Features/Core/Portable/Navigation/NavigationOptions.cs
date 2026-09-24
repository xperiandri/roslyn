// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.Navigation;

/// <summary>
/// Where a navigation ends up.  The destination is data; whoever performs the navigation decides what to do with it,
/// which for <see cref="TabDestination"/> is opening the target in an editor tab.
/// </summary>
internal abstract record NavigationDestination
{
    private protected NavigationDestination()
    {
    }

    /// <summary>A permanent, activated tab.</summary>
    public static readonly NavigationDestination Tab = new TabDestination();

    /// <summary>A provisional (preview) tab, activated.</summary>
    public static readonly NavigationDestination ProvisionalTab = new TabDestination(PreferProvisional: true);

    /// <summary>The peek session in scope.  See <see cref="PeekDestination"/>.</summary>
    public static readonly NavigationDestination Peek = new PeekDestination();
}

/// <param name="PreferProvisional">
/// Requests that a provisional (or preview) tab be used for any document that needs to be opened, if one is available.
/// </param>
/// <param name="Activate">
/// Requests that the navigation activate the tab.  The default for the platform is to activate the tab, so turning the
/// option off tells the platform to not activate it.
/// </param>
internal sealed record TabDestination(
    bool PreferProvisional = false,
    bool Activate = true) : NavigationDestination;

/// <summary>
/// Shows the target in the peek session that is in scope, instead of opening it.  Whoever navigates with this
/// destination is responsible for putting that session in scope first.
/// </summary>
internal sealed record PeekDestination : NavigationDestination;

/// <summary>
/// Navigation options.
/// </summary>
/// <param name="Destination">Where the navigation should end up.</param>
internal readonly record struct NavigationOptions(NavigationDestination Destination)
{
    public NavigationOptions()
        : this(NavigationDestination.Tab)
    {
    }

    public static readonly NavigationOptions Default = new();
}
