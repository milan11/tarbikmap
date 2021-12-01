namespace TarbikMap.Tests
{
    using TarbikMap.Common;
    using Xunit;

    public class ReasonableStringCheckerTest
    {
        [Theory]
        [InlineData("abcdefghijkl", false)]
        [InlineData("mnopqrstuvwxyz", false)]
        [InlineData("aaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData("a", false)]
        [InlineData("a0", false)]
        [InlineData("a ", false)]
        [InlineData(" a", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("a b", false)]
        [InlineData("ab", false)]
        public void DefaultConfiguration(string str, bool expectedResult)
        {
            ReasonableStringChecker checker = new ReasonableStringChecker();

            Assert.Equal(expectedResult, checker.Check(str));
        }

        [Theory]
        [InlineData("aaaaaaaaaa", true)]
        [InlineData("aaaaaaaaaaa", false)]
        public void MaxLength(string str, bool expectedResult)
        {
            ReasonableStringChecker checker = new ReasonableStringChecker().AllowMaxLength(10).AllowLowercaseLetters();

            Assert.Equal(expectedResult, checker.Check(str));
        }

        [Theory]
        [InlineData("abcdefghijkl", true)]
        [InlineData("mnopqrstuvwxyz", true)]
        [InlineData("aaaaaaaaaaaaaaaaaaaa", true)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData("a", true)]
        [InlineData("a0", false)]
        [InlineData("a ", false)]
        [InlineData(" a", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("a b", false)]
        [InlineData("ab", true)]
        public void AllowLowercaseLetters(string str, bool expectedResult)
        {
            ReasonableStringChecker checker = new ReasonableStringChecker().AllowMaxLength(20).AllowLowercaseLetters();

            Assert.Equal(expectedResult, checker.Check(str));
        }

        [Theory]
        [InlineData("0123456789", true)]
        [InlineData("+", false)]
        [InlineData("A", false)]
        [InlineData("a", false)]
        public void AllowNumbers(string str, bool expectedResult)
        {
            ReasonableStringChecker checker = new ReasonableStringChecker().AllowMaxLength(20).AllowNumbers();

            Assert.Equal(expectedResult, checker.Check(str));
        }

        [Theory]
        [InlineData("0", false)]
        [InlineData("A", false)]
        [InlineData("@", true)]
        [InlineData("@ ", false)]
        [InlineData(" @", false)]
        [InlineData("@ @", false)]
        public void AllowAdditionalCharacters(string str, bool expectedResult)
        {
            ReasonableStringChecker checker = new ReasonableStringChecker().AllowMaxLength(20).AllowAdditionalCharacters(new[] { '@' });

            Assert.Equal(expectedResult, checker.Check(str));
        }

        [Theory]
        [InlineData("0", true)]
        [InlineData("a", true)]
        [InlineData("_@", true)]
        [InlineData("A", true)]
        [InlineData("a ", false)]
        [InlineData(" a", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("a b", true)]
        [InlineData("a\nb", false)]
        [InlineData("a\rb", false)]
        [InlineData("a\tb", false)]
        public void AllowAllReasonableCharacters(string str, bool expectedResult)
        {
            ReasonableStringChecker checker = new ReasonableStringChecker().AllowMaxLength(20).AllowAllReasonableCharacters();

            Assert.Equal(expectedResult, checker.Check(str));
        }
    }
}