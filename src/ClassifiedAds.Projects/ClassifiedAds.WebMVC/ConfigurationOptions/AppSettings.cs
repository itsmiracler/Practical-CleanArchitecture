﻿using Microsoft.Extensions.Options;

namespace ClassifiedAds.WebMVC.ConfigurationOptions
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }

        public OpenIdConnect OpenIdConnect { get; set; }

        public ResourceServer ResourceServer { get; set; }

        public string AllowedHosts { get; set; }

        public string CurrentUrl { get; set; }

        public ValidateOptionsResult Validate()
        {
            var validationRs = OpenIdConnect.Validate();

            if (validationRs.Failed)
            {
                return validationRs;
            }

            validationRs = ResourceServer.Validate();

            if (validationRs.Failed)
            {
                return validationRs;
            }

            return ValidateOptionsResult.Success;

        }
    }

    public class AppSettingsValidation : IValidateOptions<AppSettings>
    {
        public ValidateOptionsResult Validate(string name, AppSettings options)
        {
            return options.Validate();
        }
    }
}
