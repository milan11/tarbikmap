namespace TarbikMap.Common
{
    using System;
    using System.Linq;

    public class ReasonableStringChecker
    {
        private int maxLength;
        private bool allowLowercaseLetters;
        private bool allowNumbers;
        private char[] allowAdditionalCharacters = Array.Empty<char>();
        private bool allowAllReasonableCharacters;

        public ReasonableStringChecker AllowMaxLength(int maxLength)
        {
            this.maxLength = maxLength;

            return this;
        }

        public ReasonableStringChecker AllowLowercaseLetters()
        {
            this.allowLowercaseLetters = true;

            return this;
        }

        public ReasonableStringChecker AllowNumbers()
        {
            this.allowNumbers = true;

            return this;
        }

        public ReasonableStringChecker AllowAdditionalCharacters(char[] additional)
        {
            this.allowAdditionalCharacters = additional;

            return this;
        }

        public ReasonableStringChecker AllowAllReasonableCharacters()
        {
            this.allowAllReasonableCharacters = true;

            return this;
        }

        public bool Check(string s)
        {
            if (s == null)
            {
                return false;
            }

            if (s.Length > this.maxLength)
            {
                return false;
            }

            if (s.Length == 0)
            {
                return false;
            }

            if (s.Trim() != s)
            {
                return false;
            }

            foreach (char c in s)
            {
                if (char.IsControl(c))
                {
                    return false;
                }

                bool isAllowedAZ = this.allowLowercaseLetters && (c >= 'a' && c <= 'z');
                bool isAllowedNumber = this.allowNumbers && (c >= '0' && c <= '9');
                bool isAllowedAdditional = this.allowAdditionalCharacters.Contains(c);

                if (!this.allowAllReasonableCharacters && !isAllowedAZ && !isAllowedNumber && !isAllowedAdditional)
                {
                    return false;
                }
            }

            return true;
        }
    }
}