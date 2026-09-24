// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MetadataAsSource;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.Peek;

[Export(typeof(IPeekableItemSourceProvider))]
[ContentType(ContentTypeNames.RoslynContentType)]
[Name("Roslyn Peekable Item Provider")]
[Order(Before = "LSPPeekableSourceProvider")]
[SupportsStandaloneFiles(true)]
[SupportsPeekRelationship("IsDefinedBy")]
internal sealed class PeekableItemSourceProvider : IPeekableItemSourceProvider
{
    private readonly IPeekResultFactory _peekResultFactory;
    private readonly IMetadataAsSourceFileService _metadataAsSourceFileService;
    private readonly IGlobalOptionService _globalOptions;
    private readonly IThreadingContext _threadingContext;
    private readonly IUIThreadOperationExecutor _uiThreadOperationExecutor;

    [ImportingConstructor]
    [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
    public PeekableItemSourceProvider(
        IPeekResultFactory peekResultFactory,
        IMetadataAsSourceFileService metadataAsSourceFileService,
        IGlobalOptionService globalOptions,
        IThreadingContext threadingContext,
        IUIThreadOperationExecutor uiThreadOperationExecutor)
    {
        _peekResultFactory = peekResultFactory;
        _metadataAsSourceFileService = metadataAsSourceFileService;
        _globalOptions = globalOptions;
        _threadingContext = threadingContext;
        _uiThreadOperationExecutor = uiThreadOperationExecutor;
    }

    public IPeekableItemSource? TryCreatePeekableItemSource(ITextBuffer textBuffer)
        => textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            new PeekableItemSource(textBuffer, _peekResultFactory, _metadataAsSourceFileService, _globalOptions, _threadingContext, _uiThreadOperationExecutor));
}
