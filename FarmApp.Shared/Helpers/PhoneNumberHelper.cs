using PhoneNumbers;

namespace FarmApp.Shared.Helpers;

public static class PhoneNumberHelper
{
    public static bool IsValidPhoneNumber(string phoneNumber, string countryCode)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();
        var phone = phoneNumberUtil.Parse(phoneNumber, countryCode);
        var possible = phoneNumberUtil.IsValidNumber(phone);
        
        return possible;
    }

    public static string? GetInternationalPhoneNumberFormat(string phoneNumber, string countryCode)
    {
        var phoneUtil = PhoneNumberUtil.GetInstance();

        try
        {
            var number = phoneUtil.Parse(phoneNumber, countryCode);
            var isValid = phoneUtil.IsValidNumber(number);
            if (!isValid) return null;

            var formattedNumber = phoneUtil.Format(number, PhoneNumberFormat.E164);
            return formattedNumber?.Replace("+", "");
        }
        catch (NumberParseException e)
        {
            return null;
        }
    }
}
