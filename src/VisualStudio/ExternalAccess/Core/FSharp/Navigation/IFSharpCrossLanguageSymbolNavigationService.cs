// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Navigation;

namespace Microsoft.CodeAnalysis.ExternalAccess.FSharp.Navigation;

/// <inheritdoc cref="ICrossLanguageSymbolNavigationService"/>
internal interface IFSharpCrossLanguageSymbolNavigationService
{
    /// <inheritdoc cref="ICrossLanguageSymbolNavigationService.TryGetNavigableLocationAsync"/>
    Task<IFSharpNavigableLocation?> TryGetNavigableLocationAsync(
        string assemblyName, string documentationCommentId, CancellationToken cancellationToken);
}

/// <inheritdoc cref="NavigationOptions"/>
internal sealed record class FSharpNavigationOptions2(
    bool PreferProvisionalTab,
    bool ActivateTab);

/// <inheritdoc cref="INavigableLocation"/>
internal interface IFSharpNavigableLocation
{
    Task<bool> NavigateToAsync(FSharpNavigationOptions2 options, CancellationToken cancellationToken);
}

/// <summary>
/// Where a navigation should end, mirroring <see cref="NavigationDestination"/> so that F# can pass along what it was
/// given.  Kept apart from <see cref="FSharpNavigationOptions2"/> so that an implementation compiled against a version
/// without it still loads.
/// </summary>
internal abstract record FSharpNavigationDestination
{
    private protected FSharpNavigationDestination()
    {
    }

    /// <inheritdoc cref="NavigationDestination.Tab"/>
    public static readonly FSharpNavigationDestination Tab = new FSharpTabDestination();

    /// <inheritdoc cref="NavigationDestination.ProvisionalTab"/>
    public static readonly FSharpNavigationDestination ProvisionalTab = new FSharpTabDestination(PreferProvisional: true);

    /// <inheritdoc cref="NavigationDestination.Peek"/>
    public static readonly FSharpNavigationDestination Peek = new FSharpPeekDestination();
}

/// <inheritdoc cref="TabDestination"/>
internal sealed record FSharpTabDestination(
    bool PreferProvisional = false,
    bool Activate = true) : FSharpNavigationDestination;

/// <inheritdoc cref="PeekDestination"/>
internal sealed record FSharpPeekDestination : FSharpNavigationDestination;

/// <inheritdoc cref="NavigationOptions"/>
internal sealed record class FSharpNavigationOptions3(FSharpNavigationDestination Destination);

/// <summary>
/// The part of <see cref="INavigableLocation"/> added after <see cref="IFSharpNavigableLocation"/> shipped.  Kept
/// apart so that an implementation compiled against a version without it still loads.
/// </summary>
internal interface IFSharpNavigableLocation2 : IFSharpNavigableLocation
{
    Task<bool> NavigateToAsync(FSharpNavigationOptions3 options, CancellationToken cancellationToken);
}
