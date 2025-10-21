namespace CRMS_API.Services.Exceptions
{
    public class EmailNotConfirmedException : Exception
    {
        public EmailNotConfirmedException(string message) : base(message)
        {
        }
    }
}
