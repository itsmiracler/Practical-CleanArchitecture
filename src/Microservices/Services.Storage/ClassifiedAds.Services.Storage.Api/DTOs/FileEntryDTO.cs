﻿using System;

namespace ClassifiedAds.Services.Storage.DTOs
{
    public class FileEntryDTO
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        public string FileLocation { get; set; }
    }
}
