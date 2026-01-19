// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Abstractions;

namespace Ascendance.Rendering.Internal.Input.Rules;

/// <summary>
/// Sample validation: password must be 6-64 chars, only allow certain symbols.
/// You can adjust logic as required for security.
/// </summary>
public class PasswordValidationRule : ITextValidationRule
{
    public System.Boolean IsValid(System.String value)
    {
        if (System.String.IsNullOrEmpty(value) || value.Length < 6 || value.Length > 64)
        {
            return false;
        }

        // Allow ASCII letters, digits, and _-.#
        foreach (System.Char c in value)
        {
            if (!(System.Char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '#'))
            {
                return false;
            }
        }
        return true;
    }
}