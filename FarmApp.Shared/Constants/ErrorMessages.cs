namespace FarmApp.Shared.Constants;

public static partial class Constants
{
    public static class ErrorMessages
    {
        public const string USER_ALREADY_EXISTS = nameof(USER_ALREADY_EXISTS);
        public const string USER_DOES_NOT_EXIST = nameof(USER_DOES_NOT_EXIST);
        public const string WRONG_CODE = nameof(WRONG_CODE);
        public const string CODE_EXPIRED = nameof(CODE_EXPIRED);
        public const string EMAIL_IS_INVALID = nameof(EMAIL_IS_INVALID);
        public const string PASSWORD_IS_INCORRECT = nameof(PASSWORD_IS_INCORRECT);
        public const string PASSWORDS_DONT_MATCH = nameof(PASSWORDS_DONT_MATCH);
        public const string CODE_IS_NOT_SENT = nameof(CODE_IS_NOT_SENT);
        public const string EMAIL_IS_NOT_CONFIRMED = nameof(EMAIL_IS_NOT_CONFIRMED);
        public const string EMAIL_OR_PASSWORD_IS_INCORRECT = nameof(EMAIL_OR_PASSWORD_IS_INCORRECT);
        public const string TOO_MANY_ATTEMPTS = nameof(TOO_MANY_ATTEMPTS);
        public const string INVALID_TOKEN = nameof(INVALID_TOKEN);

        public const string STEAD_DOES_NOT_EXISTS = nameof(STEAD_DOES_NOT_EXISTS);
        public const string PROPERTY_NOTE_DOES_NOT_EXISTS = nameof(PROPERTY_NOTE_DOES_NOT_EXISTS);

        public const string PROPERTY_DOES_NOT_EXIST = nameof(PROPERTY_DOES_NOT_EXIST);

        public const string FILE_IS_NOT_MEDIA = nameof(FILE_IS_NOT_MEDIA);
        public const string FILE_IS_EMPTY = nameof(FILE_IS_EMPTY);
        public const string MEDIA_FILE_DOES_NOT_EXIST = nameof(MEDIA_FILE_DOES_NOT_EXIST);
        public const string ERROR_WHILE_DELETING_MEDIA_FILE = nameof(ERROR_WHILE_DELETING_MEDIA_FILE);
        public const string MEDIA_FILE_DOES_NOT_BELONG_TO_PROPERTY_NOTE = nameof(MEDIA_FILE_DOES_NOT_BELONG_TO_PROPERTY_NOTE);
        public const string ERROR_WHILE_SAVING_MEDIA_FILE = nameof(ERROR_WHILE_SAVING_MEDIA_FILE);

        public const string STATUS_DOES_NOT_EXIST = nameof(STATUS_DOES_NOT_EXIST);
        public const string CANNOT_DELETE_DEFAULT_STATUS = nameof(CANNOT_DELETE_DEFAULT_STATUS);
    }
}
