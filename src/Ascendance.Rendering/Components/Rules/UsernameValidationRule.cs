// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Abstractions;

namespace Ascendance.Rendering.Components.Rules;

/// <inheritdoc/>
public class UsernameValidationRule : ITextValidationRule
{
    /// <inheritdoc/>
    public System.Boolean IsValid(System.String value)
        => !System.String.IsNullOrWhiteSpace(value)
        && value.Length <= 20
        && System.Linq.Enumerable.All(value, c => System.Char.IsLetterOrDigit(c) || c == '_');
}
