﻿using Aiursoft.Probe.Data;
using Aiursoft.Probe.Services;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.Probe.FilesAddressModels;
using Aiursoft.Pylon.Models.Probe.FilesViewModels;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aiursoft.Probe.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    [Route("Files")]
    [DisableRequestSizeLimit]
    public class FilesController : Controller
    {
        private readonly char _ = Path.DirectorySeparatorChar;
        private readonly ProbeDbContext _dbContext;
        private readonly FolderLocator _folderLocator;
        private readonly IConfiguration _configuration;
        private readonly FolderOperator _folderCleaner;
        private readonly ServiceLocation _serviceLocation;
        private readonly TokenEnsurer _tokenEnsurer;

        public FilesController(
            ProbeDbContext dbContext,
            FolderLocator folderLocator,
            FolderOperator folderCleaner,
            IConfiguration configuration,
            ServiceLocation serviceLocation,
            TokenEnsurer tokenEnsurer)
        {
            _dbContext = dbContext;
            _folderLocator = folderLocator;
            _configuration = configuration;
            _serviceLocation = serviceLocation;
            _tokenEnsurer = tokenEnsurer;
            _folderCleaner = folderCleaner;
        }

        [HttpPost]
        [Route("UploadFile/{SiteName}/{**FolderNames}")]
        [APIModelStateChecker]
        [APIProduces(typeof(UploadFileViewModel))]
        public async Task<IActionResult> UploadFile(UploadFileAddressModel model)
        {
            var site = await _dbContext
                .Sites
                .SingleOrDefaultAsync(t => t.SiteName.ToLower() == model.SiteName.ToLower());
            if (!site.OpenToUpload)
            {
                _tokenEnsurer.Ensure(model.PBToken, "Upload", model.SiteName, model.FolderNames);
            }
            var folders = _folderLocator.SplitStrings(model.FolderNames);
            var folder = await _folderLocator.LocateSiteAndFolder(model.SiteName, folders, model.RecursiveCreate);

            // Executing here will let the browser upload the file.
            if (HttpContext.Request.Form.Files.Count < 1)
            {
                return this.Protocol(ErrorType.InvalidInput, "Please provide a file!");
            }
            var file = HttpContext.Request.Form.Files.First();
            if (!new ValidFolderName().IsValid(file.FileName))
            {
                return this.Protocol(ErrorType.InvalidInput, $"Invalid file name: '{file.FileName}'!");
            }
            var newFile = new Pylon.Models.Probe.File
            {
                FileName = Path.GetFileName(file.FileName),
                ContextId = folder.Id
            };

            //Ensure there not exists file with the same file name.
            while (true)
            {
                var exists = folder.Files.Any(t => t.FileName == newFile.FileName);
                if (exists)
                {
                    newFile.FileName = "_" + newFile.FileName;
                }
                else
                {
                    //Save to database
                    _dbContext.Files.Add(newFile);
                    _dbContext.SaveChanges();
                    break;
                }
            }
            //Try saving file.
            var directoryPath = _configuration["StoragePath"] + $"{_}Storage{_}";
            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }
            using (var fileStream = new FileStream(directoryPath + newFile.Id + ".dat", FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                fileStream.Close();
            }
            var filePath = StorageService.GetProbeFullPath(model.SiteName, string.Join('/', folders), newFile.FileName);
            var path = StorageService.GetProbeDownloadAddress(_serviceLocation, filePath);
            return Json(new UploadFileViewModel
            {
                InternetPath = path,
                SiteName = model.SiteName,
                FilePath = filePath,
                FileSize = file.Length,
                Code = ErrorType.Success,
                Message = "Successfully uploaded your file."
            });
        }

        [HttpPost]
        [APIModelStateChecker]
        [Route("DeleteFile/{SiteName}/{**FolderNames}")]
        public async Task<IActionResult> DeleteFile(DeleteFileAddressModel model)
        {
            var (folders, fileName) = _folderLocator.SplitToPath(model.FolderNames);
            var folder = await _folderLocator.LocateSiteAndFolder(model.AccessToken, model.SiteName, folders);
            var file = folder.Files.SingleOrDefault(t => t.FileName == fileName);
            if (file == null)
            {
                return this.Protocol(ErrorType.NotFound, "The file cannot be found. Maybe it has been deleted.");
            }
            _folderCleaner.DeleteFile(file);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, $"Successfully deleted the file '{file.FileName}'");
        }
    }
}
