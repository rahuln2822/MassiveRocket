using System.Text.RegularExpressions;

namespace MassiveRocketAssignment.Validation
{
    public static class ValidationManager
    {
        public static T ShouldNotBeNull<T>(this T typeValue)
        {
            if (typeValue == null)
            {
                throw new ArgumentNullException(nameof(typeValue));
            }

            return typeValue;
        }

        public static string ShouldNotBeNull(this string typeValue)
        {
            if (string.IsNullOrWhiteSpace(typeValue))
            {
                throw new ArgumentNullException(nameof(typeValue));
            }

            return typeValue;
        }

        public static string ShouldBeValidEmail(this string email)
        {
            var emailId = email.ShouldNotBeNull();

            bool isEmail = Regex.IsMatch(emailId, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (!isEmail)
            {
                throw new InvalidDataException($"Invalid email - {email}");
            }

            return email;
        }
    }
}
