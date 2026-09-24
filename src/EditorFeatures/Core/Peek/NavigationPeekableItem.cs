// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.MetadataAsSource;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.CodeAnalysis.Editor.Implementation.Peek;

/// <summary>
/// Shows what Go To Definition would go to.  The navigation resolves the target the same way it does for the command,
/// and reports it to the peek session in scope instead of opening it.
/// </summary>
internal sealed class NavigationPeekableItem : PeekableItem
{
    private readonly INavigableLocation _location;
    private readonly IMetadataAsSourceFileService _metadataAsSourceFileService;
    private readonly IGlobalOptionService _globalOptions;
    private readonly IThreadingContext _threadingContext;

    public NavigationPeekableItem(
        INavigableLocation location,
        IPeekResultFactory peekResultFactory,
        IMetadataAsSourceFileService metadataAsSourceFileService,
        IGlobalOptionService globalOptions,
        IThreadingContext threadingContext)
        : base(peekResultFactory)
    {
        _location = location;
        _metadataAsSourceFileService = metadataAsSourceFileService;
        _globalOptions = globalOptions;
        _threadingContext = threadingContext;
    }

    public override IEnumerable<IPeekRelationship> Relationships
        => [PredefinedPeekRelationships.Definitions];

    public override IPeekResultSource GetOrCreateResultSource(string relationshipName)
        => new ResultSource(this);

    private sealed class ResultSource(NavigationPeekableItem peekableItem) : IPeekResultSource
    {
        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            if (relationshipName != PredefinedPeekRelationships.Definitions.Name)
                return;

            using var scope = new EditorPeekNavigationScope(
                resultCollection, peekableItem.PeekResultFactory, peekableItem._metadataAsSourceFileService, peekableItem._globalOptions);

            // Note: this is called on a background thread, but we must block the thread since the API doesn't support
            // proper asynchrony.
            var success = peekableItem._threadingContext.JoinableTaskFactory.Run(
                () => peekableItem._location.NavigateToAsync(new NavigationOptions(NavigationDestination.Peek), cancellationToken));

            if (!success)
                callback.ReportFailure(new Exception(EditorFeaturesResources.No_information_found));
        }
    }
}
