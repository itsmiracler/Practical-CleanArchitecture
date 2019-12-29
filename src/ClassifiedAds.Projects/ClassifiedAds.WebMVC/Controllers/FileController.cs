﻿using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using ClassifiedAds.DomainServices.DomainEvents;
using ClassifiedAds.DomainServices.Entities;
using ClassifiedAds.DomainServices.Infrastructure.MessageBrokers;
using ClassifiedAds.DomainServices.Infrastructure.Storages;
using ClassifiedAds.DomainServices.Services;
using ClassifiedAds.WebMVC.Models.File;
using Microsoft.AspNetCore.Mvc;

namespace ClassifiedAds.WebMVC.Controllers
{
    public class FileController : Controller
    {
        private readonly IGenericService<FileEntry> _fileEntryService;
        private readonly IFileStorageManager _fileManager;
        private readonly IMessageSender _messageQueueSender;

        public FileController(IGenericService<FileEntry> fileEntryService, IFileStorageManager fileManager, IMessageSender messageQueueSender)
        {
            _fileEntryService = fileEntryService;
            _fileManager = fileManager;
            _messageQueueSender = messageQueueSender;
        }

        public IActionResult Index()
        {
            return View(_fileEntryService.Get());
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(UploadFile model)
        {
            var fileEntry = new FileEntry
            {
                Name = model.Name,
                Description = model.Description,
                Size = model.FormFile.Length,
                UploadedTime = DateTime.Now,
                FileName = model.FormFile.FileName,
            };

            _fileEntryService.Add(fileEntry);

            using (var stream = new MemoryStream())
            {
                await model.FormFile.CopyToAsync(stream);
                _fileManager.Create(fileEntry, stream);
            }

            _fileEntryService.Update(fileEntry);

            _messageQueueSender.Send(new FileUploadedEvent
            {
                FileEntry = fileEntry,
            });

            return View();
        }

        public IActionResult Download(Guid id)
        {
            var fileEntry = _fileEntryService.GetById(id);
            var content = _fileManager.Read(fileEntry);
            return File(content, MediaTypeNames.Application.Octet, WebUtility.HtmlEncode(fileEntry.FileName));
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var fileEntry = _fileEntryService.GetById(id);
            return View(fileEntry);
        }

        [HttpPost]
        public IActionResult Delete(FileEntry model)
        {
            var fileEntry = _fileEntryService.GetById(model.Id);

            _fileEntryService.Delete(fileEntry);
            _fileManager.Delete(fileEntry);

            _messageQueueSender.Send(new FileDeletedEvent
            {
                FileEntry = fileEntry,
            });

            return RedirectToAction(nameof(Index));
        }
    }
}