﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.Diagnostics;

namespace Microsoft.KernelMemory.AI;

/// <summary>
/// Disabled text generator used when using KM without AI queries and summaries,
/// e.g. when using the internal orchestration to run jobs that don't require AI.
/// </summary>
public class NoTextGenerator : ITextGenerator
{
    private readonly ILogger<NoTextGenerator> _log;

    public NoTextGenerator(ILoggerFactory? loggerFactory = null)
    {
        this._log = (loggerFactory ?? DefaultLogger.Factory).CreateLogger<NoTextGenerator>();
    }

    /// <inheritdoc />
    public int MaxTokenTotal => int.MaxValue;

    /// <inheritdoc />
    public int CountTokens(string text)
    {
        throw this.Error();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetTokens(string text)
    {
        throw this.Error();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<GeneratedTextContent> GenerateTextAsync(string prompt, TextGenerationOptions options, CancellationToken cancellationToken = default)
    {
        throw this.Error();
    }

    private NotImplementedException Error()
    {
        this._log.LogCritical("The application is attempting to generate text even if text generation has been disabled");
        return new NotImplementedException("Text generation has been disabled");
    }
}
