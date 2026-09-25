// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.ExternalAccess.FSharp.Editor;
using Microsoft.CodeAnalysis.ExternalAccess.FSharp.GoToDefinition;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.ExternalAccess.FSharp.Internal.Editor;

[ExportLanguageService(typeof(IDefinitionLocationService), LanguageNames.FSharp), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class FSharpDefinitionLocationService(
    IThreadingContext threadingContext,
    [Import(AllowDefault = true)] IFSharpGoToDefinitionService? goToDefinitionService,
    [Import(AllowDefault = true)] IFSharpFindDefinitionService? findDefinitionService) : IDefinitionLocationService
{
    public Task<DefinitionLocation?> GetDefinitionLocationAsync(Document document, int position, CancellationToken cancellationToken)
    {
        return DefinitionLocationServiceHelpers.GetDefinitionLocationFromLegacyImplementationsAsync(
            document, position, FindDefinitionsAsync, cancellationToken);

        async Task<IEnumerable<(Document document, TextSpan sourceSpan)>?> FindDefinitionsAsync(CancellationToken cancellationToken)
        {
            // A definition F# has no source for gets a document written and shown for it, and showing it takes the
            // main thread.  While the caller is blocking that thread - Peek asks for the definition inside its own
            // JoinableTaskFactory.Run - the definitions that already have a document are all F# can answer with.
            if (findDefinitionService is not null && threadingContext.JoinableTaskContext.IsMainThreadBlocked())
            {
                var inSource = await findDefinitionService.FindDefinitionsAsync(document, position, cancellationToken).ConfigureAwait(false);
                return inSource.Select(i => (i.Document, i.SourceSpan));
            }

            if (goToDefinitionService is null)
                return null;

            var items = await goToDefinitionService.FindDefinitionsAsync(document, position, cancellationToken).ConfigureAwait(false);
            return items?.Select(i => (i.Document, i.SourceSpan));
        }
    }
}
