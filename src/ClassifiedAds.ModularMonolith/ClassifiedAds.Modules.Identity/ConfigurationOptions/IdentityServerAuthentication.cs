﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifiedAds.Modules.Identity.ConfigurationOptions
{
    public class IdentityServerAuthentication
    {
        public string Authority { get; set; }

        public string ApiName { get; set; }

        public bool RequireHttpsMetadata { get; set; }
    }
}
