using System.Collections.Generic;
using NodaTime;
using PasswordRulesSharp.Models;

namespace PasswordRulesSharp.Parser;

public class BaseRule : IRule
{
    // TODO: convert protected set to init-only

    /// <inheritdoc />
    public int? MinLength { get; protected set; }

    /// <inheritdoc />
    public int? MaxLength { get; protected set; }

    /// <inheritdoc />
    public int? MaxConsecutive { get; protected set; }

    /// <inheritdoc />
    public Period? ExpiresAfter { get; protected set; }

    /// <inheritdoc />
    public List<CharacterClass>? Required { get; protected set; }
}