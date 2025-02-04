﻿using NodaTime;

using PasswordRulesSharp.Models;
using PasswordRulesSharp.Rules.Parsing;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace PasswordRulesSharp.Parser
{
    public class Rule
    {
        /// <summary>
        /// The minimum length of a valid password, in chars.
        /// </summary>
        public int? MinLength { get; }

        /// <summary>
        /// <para>
        /// The maximum length of a valid password, in chars.
        /// </para>
        /// 
        /// <para>
        /// If <see cref="MinLength"/> is also set, must be greater or equal.
        /// </para>
        /// </summary>
        public int? MaxLength { get; }

        /// <summary>
        /// <para>
        /// Maximum consecutive chars.
        /// </para>
        /// 
        /// <para>
        /// Defaults to unlimited. Must be positive. If you set it to e.g. 3,
        /// the password 'aaaa' is not valid.
        /// </para>
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? MaxConsecutive { get; }

        /// <summary>
        /// Password expires after a period of time (e.g., 3 months). This is a
        /// non-standard extensoin.
        /// </summary>
        public Period? ExpiresAfter { get; }

        /// <summary>
        /// <para>
        /// A list of required character classes.
        /// </para>
        /// 
        /// <para>
        /// Each of these is at-least-one-of. For example, <c>required: lower;
        /// required: upper;</c> means at least one lower-case _and_ at least
        /// one upper-case character is required. <c>required: [!@]</c> means
        /// _one_ of those two special chars is required, whereas
        /// <c>required: [!]; required: [@];</c> means _both_ are required.
        /// </para>
        /// </summary>
        public List<CharacterClass>? Required { get; }

        public Rule(string rawRule)
        {
            var dict = new Tokenizer(rawRule).GetKeyValuePairs();

            List<string>? value;

            if (dict.TryGetValue("minlength", out value) &&
                value.Count == 1 &&
                int.TryParse(value[0], out var minLength))
            {
                MinLength = minLength;
            }
            else
            {
                // TODO set fallback?
            }

            if (dict.TryGetValue("maxlength", out value) &&
                value.Count == 1 &&
                int.TryParse(value[0], out var maxLength))
            {
                MaxLength = maxLength;

                if (MaxLength < 4)
                    MaxLength = 4; // https://developer.apple.com/password-rules/ rejects lengths < 4

                if (MaxLength < MinLength)
                    MinLength = MaxLength;
            }

            if (dict.TryGetValue("max-consecutive", out value))
            {
                // "If you have multiple max-consecutive properties in your rule, the minimum value of the properties will be applied."
                var maxConsecutive = value.Where(s => int.TryParse(s, out var intVal))
                                          .Select(s => int.Parse(s))
                                          .Min();
                MaxConsecutive = maxConsecutive;
            }

            // TODO: this isn't correct. we need multiple required rules, and maybe AND-combine them?
            if (dict.TryGetValue("required", out value))
            {
                Required = new();

                foreach (var item in value)
                {
                    if (CharacterClass.TryParse(item, out var required))
                    {
                        Required.Add(required);
                    }
                }
            }
            else
            {
                // TODO: fallback?
            }

            // TODO: and then for allowed rules, OR-combine them?

            if (dict.TryGetValue("x-expires-after", out value) &&
                value.Count == 1 &&
                TryParsePeriod(value[0], out var periodVal))
            {
                ExpiresAfter = periodVal;
            }
        }

        private static bool TryParsePeriod(string s, [NotNullWhen(true)] out Period? periodVal)
        {
            periodVal = null;

            //lang=regex
            const string RegexPattern = @"(?<Amount>\d+)-(?<Unit>(days|weeks|months|years))";

            var match = Regex.Match(s, RegexPattern);

            foreach (var item in new[] { "Amount", "Unit" })
            {
                if (!match.Groups[item].Success)
                    return false;
            }

            var amount = int.Parse(match.Groups["Amount"].Value);

            periodVal = match.Groups["Unit"].Value switch
            {
                "days" => Period.FromDays(amount),
                "weeks" => Period.FromWeeks(amount),
                "months" => Period.FromMonths(amount),
                "years" => Period.FromYears(amount),
                _ => throw new ArgumentOutOfRangeException(s),
            };

            return true;
        }
    }
}
