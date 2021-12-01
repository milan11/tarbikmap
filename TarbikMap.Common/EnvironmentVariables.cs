namespace TarbikMap.Common
{
    using System;

    public static class EnvironmentVariables
    {
        private static readonly ReasonableStringChecker StringCheckerForValue = new ReasonableStringChecker().AllowMaxLength(64).AllowAllReasonableCharacters();

        public static string GetValue(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (value == null)
            {
                throw new ArgumentException($"Environment variable not set: {variableName}");
            }

            if (!StringCheckerForValue.Check(value))
            {
                throw new ArgumentException($"Environment variable has unsupported value: {variableName}={value}");
            }

            return value;
        }
    }
}