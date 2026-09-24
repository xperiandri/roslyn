// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class DocumentSpanExtensions
{
    private static (Workspace workspace, IDocumentNavigationService service) GetNavigationParts(DocumentSpan documentSpan)
    {
        var solution = documentSpan.Document.Project.Solution;
        var workspace = solution.Workspace;
        var service = workspace.Services.GetRequiredService<IDocumentNavigationService>();
        return (workspace, service);
    }

    public static async Task<INavigableLocation?> GetNavigableLocationAsync(this DocumentSpan documentSpan, CancellationToken cancellationToken)
    {
        var (workspace, service) = GetNavigationParts(documentSpan);
        var location = await service.GetLocationForPositionAsync(
            workspace, documentSpan.Document.Id, documentSpan.SourceSpan.Start, cancellationToken).ConfigureAwait(false);

        // Peek shows the span itself, so it needs nothing from the host to open.
        return new NavigableLocation((options, cancellationToken) =>
            options.Destination is PeekDestination
                ? PeekNavigationScope.Current is { } scope
                    ? scope.TryAddResultAsync(documentSpan.Document, documentSpan.SourceSpan, cancellationToken)
                    : SpecializedTasks.False
                : location?.NavigateToAsync(options, cancellationToken) ?? SpecializedTasks.False);
    }

    public static async Task<bool> IsHiddenAsync(
        this DocumentSpan documentSpan, CancellationToken cancellationToken)
    {
        var document = documentSpan.Document;
        if (document.SupportsSyntaxTree)
        {
            var tree = await document.GetRequiredSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            return tree.IsHiddenPosition(documentSpan.SourceSpan.Start, cancellationToken);
        }

        return false;
    }
}
