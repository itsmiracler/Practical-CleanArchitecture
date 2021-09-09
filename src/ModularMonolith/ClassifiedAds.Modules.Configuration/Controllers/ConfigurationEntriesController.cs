﻿using ClassifiedAds.Application;
using ClassifiedAds.CrossCuttingConcerns.Excel;
using ClassifiedAds.Infrastructure.Web.Authorization.Policies;
using ClassifiedAds.Modules.Configuration.Authorization.Policies.ConfigurationEntries;
using ClassifiedAds.Modules.Configuration.ConfigurationOptions;
using ClassifiedAds.Modules.Configuration.Entities;
using ClassifiedAds.Modules.Configuration.Models;
using CryptographyHelper;
using CryptographyHelper.AsymmetricAlgorithms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ClassifiedAds.Modules.Configuration.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationEntriesController : ControllerBase
    {
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;
        private readonly ConfigurationModuleOptions _moduleOptions;
        private readonly IExcelWriter<List<ConfigurationEntry>> _configurationEntriesExcelWriter;
        private readonly IExcelReader<List<ConfigurationEntry>> _configurationEntriesExcelReader;

        public ConfigurationEntriesController(Dispatcher dispatcher,
            ILogger<ConfigurationEntriesController> logger,
            IOptionsSnapshot<ConfigurationModuleOptions> moduleOptions,
            IExcelWriter<List<ConfigurationEntry>> configurationEntriesExcelWriter,
            IExcelReader<List<ConfigurationEntry>> configurationEntriesExcelReader)
        {
            _dispatcher = dispatcher;
            _logger = logger;
            _moduleOptions = moduleOptions.Value;
            _configurationEntriesExcelWriter = configurationEntriesExcelWriter;
            _configurationEntriesExcelReader = configurationEntriesExcelReader;
        }

        [AuthorizePolicy(typeof(GetConfigurationEntriesPolicy))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfigurationEntryModel>>> Get()
        {
            var entities = await _dispatcher.DispatchAsync(new GetEntititesQuery<ConfigurationEntry>());
            var model = entities.OrderBy(x => x.Key).ToModels();
            return Ok(model);
        }

        [AuthorizePolicy(typeof(GetConfigurationEntryPolicy))]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConfigurationEntryModel>> Get(Guid id)
        {
            var entity = await _dispatcher.DispatchAsync(new GetEntityByIdQuery<ConfigurationEntry> { Id = id, ThrowNotFoundIfNull = true });
            var model = entity.ToModel();
            return Ok(model);
        }

        [AuthorizePolicy(typeof(AddConfigurationEntryPolicy))]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ConfigurationEntryModel>> Post([FromBody] ConfigurationEntryModel model)
        {
            var entity = model.ToEntity();

            if (entity.IsSensitive)
            {
                var cert = _moduleOptions.Certificates.SettingsEncryption.FindCertificate();
                var encrypted = entity.Value.UseRSA(cert).Encrypt().ToBase64String();
                entity.Value = encrypted;
            }

            await _dispatcher.DispatchAsync(new AddOrUpdateEntityCommand<ConfigurationEntry>(entity));
            model = entity.ToModel();
            return Created($"/api/ConfigurationEntries/{model.Id}", model);
        }

        [AuthorizePolicy(typeof(UpdateConfigurationEntryPolicy))]
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(Guid id, [FromBody] ConfigurationEntryModel model)
        {
            var entity = await _dispatcher.DispatchAsync(new GetEntityByIdQuery<ConfigurationEntry> { Id = id, ThrowNotFoundIfNull = true });

            entity.Key = model.Key;
            entity.Value = model.Value;
            entity.Description = model.Description;
            entity.IsSensitive = model.IsSensitive;

            if (entity.IsSensitive)
            {
                var cert = _moduleOptions.Certificates.SettingsEncryption.FindCertificate();
                var encrypted = entity.Value.UseRSA(cert).Encrypt().ToBase64String();
                entity.Value = encrypted;
            }

            await _dispatcher.DispatchAsync(new AddOrUpdateEntityCommand<ConfigurationEntry>(entity));

            model = entity.ToModel();

            return Ok(model);
        }

        [AuthorizePolicy(typeof(DeleteConfigurationEntryPolicy))]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var entity = await _dispatcher.DispatchAsync(new GetEntityByIdQuery<ConfigurationEntry> { Id = id, ThrowNotFoundIfNull = true });

            await _dispatcher.DispatchAsync(new DeleteEntityCommand<ConfigurationEntry> { Entity = entity });

            return Ok();
        }

        [HttpGet("ExportAsExcel")]
        public async Task<IActionResult> ExportAsExcel()
        {
            var entries = await _dispatcher.DispatchAsync(new GetEntititesQuery<ConfigurationEntry>());
            using var stream = new MemoryStream();
            _configurationEntriesExcelWriter.Write(entries, stream);
            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "ConfigurationEntries.xlsx");
        }

        [HttpPost("ImportExcel")]
        public IActionResult ImportExcel([FromForm] UploadFileModel model)
        {
            using var stream = model.FormFile.OpenReadStream();
            var entries = _configurationEntriesExcelReader.Read(stream);

            // TODO: import to database
            return Ok(entries);
        }
    }
}
