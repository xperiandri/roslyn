// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Navigation;

/// <summary>
/// The peek session a navigation to <see cref="PeekDestination"/> shows its target in.  Peek puts the session in scope
/// around the navigation, and whoever resolves the target reports it here instead of opening it.
/// </summary>
internal abstract class PeekNavigationScope : IDisposable
{
    private static readonly AsyncLocal<PeekNavigationScope?> s_current = new();

    private readonly PeekNavigationScope? _previous;

    /// <summary>The session in scope, or <see langword="null"/> when nothing is peeking.</summary>
    public static PeekNavigationScope? Current => s_current.Value;

    protected PeekNavigationScope()
    {
        _previous = s_current.Value;
        s_current.Value = this;
    }

    public void Dispose()
        => s_current.Value = _previous;

    /// <summary>
    /// Shows <paramref name="span"/> of a document of the solution, widened to the member or type that contains it,
    /// which is what the user came to read.
    /// </summary>
    public async Task<bool> TryAddResultAsync(Document document, TextSpan span, CancellationToken cancellationToken)
    {
        if (document.FilePath is null)
            return false;

        if (!document.SupportsSyntaxTree)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var lineSpan = text.Lines.GetLinePositionSpan(span);
            return TryAddResult(document.FilePath, lineSpan, lineSpan, title: null, tooltip: null, isReadOnly: false);
        }

        var tree = await document.GetRequiredSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var syntaxFacts = document.GetRequiredLanguageService<ISyntaxFactsService>();

        var node = root.FindToken(span.Start).Parent;
        var entityOfInterest = node?.FirstAncestorOrSelf<SyntaxNode, ISyntaxFactsService>(
            static (node, syntaxFacts) => syntaxFacts.IsMethodLevelMember(node) || syntaxFacts.IsTopLevelNodeWithMembers(node),
            syntaxFacts) ?? node;

        // A #line directive moves the declaration to the file the user actually edits, for example a .razor file.
        var identifierLineSpan = tree.GetLocation(span).GetMappedLineSpan();
        var entityLineSpan = entityOfInterest is null
            ? identifierLineSpan
            : tree.GetLocation(entityOfInterest.Span).GetMappedLineSpan();

        return TryAddResult(identifierLineSpan.Path, identifierLineSpan.Span, entityLineSpan.Span, title: null, tooltip: null, isReadOnly: false);
    }

    /// <summary>
    /// Shows the metadata-as-source file of a symbol the solution has no source for.
    /// </summary>
    public abstract Task<bool> TryAddMetadataResultAsync(Project project, ISymbol symbol, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a file the solution has no document for, such as a generated metadata-as-source file.
    /// </summary>
    public abstract bool TryAddResult(
        string filePath,
        LinePositionSpan identifierSpan,
        LinePositionSpan entityOfInterestSpan,
        string? title,
        string? tooltip,
        bool isReadOnly);
}
