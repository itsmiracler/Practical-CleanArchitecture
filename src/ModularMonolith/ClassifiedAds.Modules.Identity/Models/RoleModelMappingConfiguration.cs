﻿using ClassifiedAds.Modules.Identity.Entities;
using System.Collections.Generic;
using System.Linq;

namespace ClassifiedAds.Modules.Identity.Models
{
    public static class RoleModelMappingConfiguration
    {
        public static IEnumerable<RoleModel> ToModels(this IEnumerable<Role> entities)
        {
            return entities.Select(x => x.ToModel());
        }

        public static RoleModel ToModel(this Role entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new RoleModel
            {
                Id = entity.Id,
                Name = entity.Name,
                NormalizedName = entity.NormalizedName,
                ConcurrencyStamp = entity.ConcurrencyStamp,
            };
        }
    }
}
