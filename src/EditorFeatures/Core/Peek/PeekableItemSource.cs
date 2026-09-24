// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.MetadataAsSource;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.Peek;

internal sealed class PeekableItemSource : IPeekableItemSource
{
    private readonly ITextBuffer _textBuffer;
    private readonly IPeekResultFactory _peekResultFactory;
    private readonly IMetadataAsSourceFileService _metadataAsSourceFileService;
    private readonly IGlobalOptionService _globalOptions;
    private readonly IThreadingContext _threadingContext;
    private readonly IUIThreadOperationExecutor _uiThreadOperationExecutor;

    public PeekableItemSource(
        ITextBuffer textBuffer,
        IPeekResultFactory peekResultFactory,
        IMetadataAsSourceFileService metadataAsSourceFileService,
        IGlobalOptionService globalOptions,
        IThreadingContext threadingContext,
        IUIThreadOperationExecutor uiThreadOperationExecutor)
    {
        _textBuffer = textBuffer;
        _peekResultFactory = peekResultFactory;
        _metadataAsSourceFileService = metadataAsSourceFileService;
        _globalOptions = globalOptions;
        _threadingContext = threadingContext;
        _uiThreadOperationExecutor = uiThreadOperationExecutor;
    }

    public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
    {
        if (!string.Equals(session.RelationshipName, PredefinedPeekRelationships.Definitions.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
        if (!triggerPoint.HasValue)
        {
            return;
        }

        var document = triggerPoint.Value.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
        if (document == null)
        {
            return;
        }

        _uiThreadOperationExecutor.Execute(EditorFeaturesResources.Peek, EditorFeaturesResources.Loading_Peek_information, allowCancellation: true, showProgress: false, action: context =>
        {
            _threadingContext.JoinableTaskFactory.Run(() => AugumentPeekSessionAsync(peekableItems, context, triggerPoint.Value, document));
        });
    }

    private async Task AugumentPeekSessionAsync(
        IList<IPeekableItem> peekableItems, IUIThreadOperationContext context, SnapshotPoint triggerPoint, Document document)
    {
        var cancellationToken = context.UserCancellationToken;

        // Peek shows where Go To Definition would go, so it asks the same service the command asks.
        var definitionLocationService = document.GetLanguageService<IDefinitionLocationService>();
        if (definitionLocationService is null)
            return;

        var definitionLocation = await definitionLocationService.GetDefinitionLocationAsync(
            document, triggerPoint.Position, cancellationToken).ConfigureAwait(false);
        if (definitionLocation is null)
            return;

        peekableItems.Add(new NavigationPeekableItem(
            definitionLocation.Location, _peekResultFactory, _metadataAsSourceFileService, _globalOptions, _threadingContext));
    }

    public void Dispose()
    {
    }
}
