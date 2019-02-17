using System;

namespace Settings
{
    public class ConversionIssue
    {
        private static readonly string[] NoKeys = new string[0];

        internal static ConversionIssue For(object settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new ConversionIssue(settings, null, null, null, null, null);
        }

        private ConversionIssue(object settings, Property property, string value, string[] keys, Exception exception, string message)
        {
            Settings = settings;
            Property = property;
            Value = value;
            Keys = keys ?? NoKeys;
            Exception = exception;
            Message = message;
        }

        public object Settings { get; }
        public Property Property { get; }
        public string Value { get; }
        public string[] Keys { get; }
        public Exception Exception { get; }
        public string Message { get; }

        internal ConversionIssue WithProperty(Property property)
        {
            return new ConversionIssue(Settings, property, Value, Keys, Exception, Message);
        }

        internal ConversionIssue WithValue(string value)
        {
            return new ConversionIssue(Settings, Property, value, Keys, Exception, Message);
        }

        internal ConversionIssue WithKeys(params string[] keys)
        {
            return new ConversionIssue(Settings, Property, Value, keys, Exception, Message);
        }

      
        internal ConversionIssue WithException(Exception exception)
        {
            return new ConversionIssue(Settings, Property, Value, Keys, exception, Message);
        }
        
        internal ConversionIssue WithMessage(string message)
        {
            return new ConversionIssue(Settings, Property, Value, Keys, Exception, message);
        }

        public override string ToString()
        {
            return
                $"{nameof(Settings)}: {SafeString.GetTypeName(Settings)}, " +
                $"{nameof(Property)}: {SafeString.GetString(Property)}, " +
                $"{nameof(Value)}: {SafeString.GetString(Value)}, " +
                $"{nameof(Keys)}: {SafeString.GetStringList(Keys)}, " +
                $"{nameof(Exception)}: {SafeString.GetTypeName(Exception)}, " +
                $"{nameof(Message)}: {Message}";
        }
    }
}