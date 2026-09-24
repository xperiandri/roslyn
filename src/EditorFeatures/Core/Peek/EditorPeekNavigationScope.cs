// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MetadataAsSource;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.CodeAnalysis.Editor.Implementation.Peek;

/// <summary>
/// Collects what a navigation resolved into the results of the peek session that asked for it.
/// </summary>
internal sealed class EditorPeekNavigationScope(
    IPeekResultCollection results,
    IPeekResultFactory resultFactory,
    IMetadataAsSourceFileService metadataAsSourceFileService,
    IGlobalOptionService globalOptions) : PeekNavigationScope
{
    public override async Task<bool> TryAddMetadataResultAsync(Project project, ISymbol symbol, CancellationToken cancellationToken)
    {
        if (!metadataAsSourceFileService.IsNavigableMetadataSymbol(symbol))
            return false;

        var workspace = project.Solution.Workspace;
        var options = globalOptions.GetMetadataAsSourceOptions();
        var declarationFile = await metadataAsSourceFileService.GetGeneratedFileAsync(
            workspace, project, symbol, signaturesOnly: false, options: options, cancellationToken: cancellationToken).ConfigureAwait(false);

        var identifierSpan = declarationFile.IdentifierLocation.GetLineSpan().Span;
        var entityOfInterestSpan = PeekHelpers.GetEntityOfInterestSpan(symbol, workspace, declarationFile.IdentifierLocation, cancellationToken);

        return TryAddResult(
            declarationFile.FilePath, identifierSpan, entityOfInterestSpan,
            declarationFile.DocumentTitle, declarationFile.DocumentTooltip, isReadOnly: true);
    }

    public override bool TryAddResult(
        string filePath,
        LinePositionSpan identifierSpan,
        LinePositionSpan entityOfInterestSpan,
        string? title,
        string? tooltip,
        bool isReadOnly)
    {
        results.Add(title is null
            ? PeekHelpers.CreateDocumentPeekResult(filePath, identifierSpan, entityOfInterestSpan, resultFactory)
            : PeekHelpers.CreateDocumentPeekResult(
                filePath, identifierSpan, entityOfInterestSpan,
                new PeekResultDisplayInfo(title, tooltip, title, tooltip), resultFactory, isReadOnly));

        return true;
    }
}
