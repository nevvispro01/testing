﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [Test]
        public void Should_ThrowArgumentException_When_PrecisionIsNegative()
        {
            Action act = () => new NumberValidator(-1, 2, true);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_NotThrowException_When_PrecisionIsPositive_AndScaleIsZero()
        {
            Action act = () => new NumberValidator(1, 0, true);
            act.Should().NotThrow();
        }

        [Test]
        public void Should_ThrowArgumentException_When_PrecisionIsNegative_And_OnlyPositiveIsFalse()
        {
            Action act = () => new NumberValidator(-1, 2, false);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void IsValidNumber_ShouldReturnTrue_When_ValidNumberWithDecimal()
        {
            var validator = new NumberValidator(17, 2, true);
            validator.IsValidNumber("0.0").Should().BeTrue();
        }

        [Test]
        public void IsValidNumber_ShouldReturnTrue_When_ValidInteger()
        {
            var validator = new NumberValidator(17, 2, true);
            validator.IsValidNumber("0").Should().BeTrue();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_NumberExceedsPrecision()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("00.00").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_NegativeNumberWithSignExceedsPrecision()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("-0.00").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_PositiveNumberWithSignExceedsPrecision()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("+0.00").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnTrue_When_ValidPositiveNumberWithSign()
        {
            var validator = new NumberValidator(4, 2, true);
            validator.IsValidNumber("+1.23").Should().BeTrue();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_NumberExceedsAllowedLength()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("+1.23").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_NumberHasTooManyDecimalPlaces()
        {
            var validator = new NumberValidator(17, 2, true);
            validator.IsValidNumber("0.000").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_NegativeNumberExceedsPrecision()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("-1.23").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_InputIsNotANumber()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("a.sd").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_InputIsEmpty()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_InputIsOnlyPositiveSign()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("+").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnFalse_When_InputIsOnlyNegativeSign()
        {
            var validator = new NumberValidator(3, 2, true);
            validator.IsValidNumber("-").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ShouldReturnTrue_When_UsingCommaAsDecimalSeparator()
        {
            var validator = new NumberValidator(17, 2, true);
            validator.IsValidNumber("1,23").Should().BeTrue();
        }

    }

    public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}