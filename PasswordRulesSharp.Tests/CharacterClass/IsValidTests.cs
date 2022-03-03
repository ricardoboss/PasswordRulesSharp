﻿using NUnit.Framework;

using System.Linq;

namespace PasswordRulesSharp.Tests.CharacterClass
{
    public class IsValidTests
    {
        [TestCase("lower", 26)]
        [TestCase("upper", 26)]
        [TestCase("digit", 10)]
        [TestCase("[-]", 1)]
        public void IsValid(string rawClass, int count)
        {
            Assert.True(PasswordRulesSharp.CharacterClass.TryParse(rawClass, out var parsedClass));

            Assert.AreEqual(count, parsedClass!.Included.Count());
        }

        [TestCase("asdf")]
        [TestCase("foo; bar")]
        [TestCase("baz: boop")]
        public void IsInvalid(string rawClass)
        {
            Assert.False(PasswordRulesSharp.CharacterClass.TryParse(rawClass, out _));
        }
    }
}
