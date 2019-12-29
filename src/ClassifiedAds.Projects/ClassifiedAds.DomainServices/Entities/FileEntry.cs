﻿using System;

namespace ClassifiedAds.DomainServices.Entities
{
    public class FileEntry : Entity<Guid>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public long Size { get; set; }

        public DateTime UploadedTime { get; set; }

        public string FileName { get; set; }

        public string FileLocation { get; set; }
    }
}
