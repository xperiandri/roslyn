// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.ExternalAccess.FSharp.Navigation;

[ExportWorkspaceService(typeof(IFSharpDocumentNavigationService)), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal class FSharpDocumentNavigationService(IThreadingContext threadingContext)
    : IFSharpDocumentNavigationService
{
    public bool CanNavigateToSpan(Workspace workspace, DocumentId documentId, TextSpan textSpan, CancellationToken cancellationToken)
    {
        var service = workspace.Services.GetService<IDocumentNavigationService>();
        return threadingContext.JoinableTaskFactory.Run(() =>
            service.CanNavigateToSpanAsync(workspace, documentId, textSpan, cancellationToken));
    }

    public bool CanNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace, CancellationToken cancellationToken)
    {
        var service = workspace.Services.GetService<IDocumentNavigationService>();
        return threadingContext.JoinableTaskFactory.Run(() =>
            service.CanNavigateToPositionAsync(workspace, documentId, position, virtualSpace, allowInvalidPosition: false, cancellationToken));
    }

    public bool TryNavigateToSpan(Workspace workspace, DocumentId documentId, TextSpan textSpan, CancellationToken cancellationToken)
        => TryNavigateToSpan(workspace, documentId, textSpan, new FSharpNavigationOptions3(FSharpNavigationDestination.ProvisionalTab), cancellationToken);

    public bool TryNavigateToSpan(Workspace workspace, DocumentId documentId, TextSpan textSpan, FSharpNavigationOptions3 options, CancellationToken cancellationToken)
    {
        var service = workspace.Services.GetService<IDocumentNavigationService>();
        return threadingContext.JoinableTaskFactory.Run(() =>
            service.TryNavigateToSpanAsync(
                threadingContext, workspace, documentId, textSpan, ToNavigationOptions(options), cancellationToken));
    }

    public bool TryNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace, CancellationToken cancellationToken)
        => TryNavigateToPosition(workspace, documentId, position, virtualSpace, new FSharpNavigationOptions3(FSharpNavigationDestination.ProvisionalTab), cancellationToken);

    public bool TryNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace, FSharpNavigationOptions3 options, CancellationToken cancellationToken)
    {
        var service = workspace.Services.GetService<IDocumentNavigationService>();
        return threadingContext.JoinableTaskFactory.Run(() =>
            service.TryNavigateToPositionAsync(
                threadingContext, workspace, documentId, position, virtualSpace,
                allowInvalidPosition: false, ToNavigationOptions(options), cancellationToken));
    }

    private static NavigationOptions ToNavigationOptions(FSharpNavigationOptions3 options)
        => new(options.Destination switch
        {
            FSharpTabDestination tab => new TabDestination(tab.PreferProvisional, tab.Activate),
            FSharpPeekDestination => NavigationDestination.Peek,
            _ => throw ExceptionUtilities.UnexpectedValue(options.Destination),
        });
}
