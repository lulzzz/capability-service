namespace Settings
{
    public abstract class NamingConvention
    {
        /// <summary>
        /// The default naming convention will use {ClassName}.{PropertyName} of the settings class
        /// being populated (through <see cref="SettingsProvider.Populate{T}"/>) to look up configuration
        /// values in the <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
        /// </summary>
        public static readonly NamingConvention Default = new NamingConventionBuilder().Build();

        /// <summary>
        /// Creates a naming convention that will use
        /// {<paramref name="applicationPrefix"/>}_{PropertyNameToPascalCaseToUpperCaseWithUnderscores}
        /// of the settings class  being populated (through <see cref="SettingsProvider.Populate{T}"/>)
        /// to look up configuration  values in the
        /// <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
        /// </summary>
        public static NamingConvention DefaultEnvironment(string applicationPrefix)
        {
            var builder = new NamingConventionBuilder();
            builder.WithCustom(property => property.Name);
            builder.WithPrefix(applicationPrefix);
            builder.PascalCaseToUpperCaseWithUnderscores();
            return builder.Build();
        }

        public abstract string GetKey(Property property);
    }
}