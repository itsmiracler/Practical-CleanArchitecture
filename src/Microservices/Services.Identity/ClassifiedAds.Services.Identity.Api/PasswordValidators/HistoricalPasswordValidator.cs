﻿using ClassifiedAds.Services.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ClassifiedAds.Services.Identity.PasswordValidators
{
    public class HistoricalPasswordValidator : IPasswordValidator<User>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string password)
        {
            if (password.Contains("testhistoricalpassword"))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "HistoricalPassword",
                    Description = "HistoricalPasswordValidator testing.",
                }));
            }

            // TODO: check password histories, etc.
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
