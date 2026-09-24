// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Threading;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.ExternalAccess.FSharp.Navigation;

internal interface IFSharpDocumentNavigationService : IWorkspaceService
{
    /// <inheritdoc cref="IDocumentNavigationService.CanNavigateToSpanAsync"/>
    bool CanNavigateToSpan(Workspace workspace, DocumentId documentId, TextSpan textSpan, CancellationToken cancellationToken);
    /// <inheritdoc cref="IDocumentNavigationService.CanNavigateToPositionAsync"/>
    bool CanNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace, CancellationToken cancellationToken);

    /// <inheritdoc cref="IDocumentNavigationService.GetLocationForSpanAsync"/>
    bool TryNavigateToSpan(Workspace workspace, DocumentId documentId, TextSpan textSpan, CancellationToken cancellationToken);
    /// <inheritdoc cref="IDocumentNavigationService.GetLocationForPositionAsync"/>
    bool TryNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace, CancellationToken cancellationToken);

    /// <inheritdoc cref="IDocumentNavigationService.GetLocationForSpanAsync"/>
    /// <param name="options">Where the navigation should end.  Pass along what the caller was given, so that a
    /// navigation that is being peeked at is shown in the peek pane rather than opened.</param>
    bool TryNavigateToSpan(Workspace workspace, DocumentId documentId, TextSpan textSpan, FSharpNavigationOptions3 options, CancellationToken cancellationToken);

    /// <inheritdoc cref="IDocumentNavigationService.GetLocationForPositionAsync"/>
    /// <inheritdoc cref="TryNavigateToSpan(Workspace, DocumentId, TextSpan, FSharpNavigationOptions3, CancellationToken)" path="/param[@name=.options.]"/>
    bool TryNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace, FSharpNavigationOptions3 options, CancellationToken cancellationToken);
}
