using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace OracleServiceCloudAttachmentUpload
{
	class App
	{
		private readonly ILogger<App> _logger;
		private readonly IArgService _iArgs;
		private readonly IConfigurationRoot _config;
		private readonly string[] args;
		private const int MAXBUFFER = 10000;
		private ConnectAPI apiConfig { get; }
		private long Id { get; set; }
		private string MappedId { get; set; }
		private string FilePath { get; set; }
		private string Uri { get; set; }
		public App(ILogger<App> logger, ConnectAPI _configAPI)
		{
			_logger = logger;
			apiConfig = _configAPI;
		}
		public void Run(string[] args = null)
		{
			long id;
			StringContent requestBody = null;
			switch (args.Length)
			{
				case 1:
					Id = 1001;
					Uri = apiConfig.uri.Replace("{Id}", Id.ToString());
					FilePath = args[0];
					requestBody = GetUploadFileRequestBody(FilePath);
					break;
				case 2:
					if (Int64.TryParse(args[0], out id))
					{
						Id = id;
						Uri = apiConfig.uri.Replace("{Id}", Id.ToString());
						FilePath = args[1];
						requestBody = GetUploadFileRequestBody(FilePath);
					}
					else
					{
						_logger.LogInformation("Oracle Service Cloud Attachment Upload Error: Integer ID NOT Supplied");
					}
					break;
				case 3:
					if (Int64.TryParse(args[0], out id))
					{
						Id = id;
						Uri = apiConfig.uri.Replace("{Id}", Id.ToString());
						MappedId = args[1];
						FilePath = args[2];
						requestBody = GetUploadFileRequestBody(Id, MappedId, FilePath);
					}
					else
					{
						_logger.LogInformation("Oracle Service Cloud Attachment Upload Error: Integer ID NOT Supplied");
					}
					break;
			}
			if(requestBody != null){
				_logger.LogInformation($"Start Oracle Service Cloud Attachment Upload: {FilePath} {Uri}");
				OracleServiceCloudAttachmentUpload(requestBody).Wait();
				_logger.LogInformation("End Oracle Service Cloud Attachment Upload");
			}
		}

		StringContent GetUploadFileRequestBody(string FilePath)
		{
			string path = FilePath;
			FileUpload file = new FileUpload(path);
			var sContent = file.Read();
			string content =
				"{"
			+ $@"""fileName"":""{file.name}"","
			+ $@"""data"":""{file.content}"""
			+ "}";

			StringContent stringContent = new StringContent(content, Encoding.UTF8, "text/json");
			return stringContent;
		}

		StringContent GetUploadFileRequestBody(long Id, string mappedId, string FilePath = "REST API for Oracle Service Cloud 18A.pdf")
		{
			return GetUploadFileRequestBody(FilePath);
		}

		private async Task OracleServiceCloudAttachmentUpload(StringContent content)
        {
			string userName = apiConfig.username, password = apiConfig.password;
            var credentials = new NetworkCredential(userName, password);

            //  Create HTTP Handler with Credentials
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                string json = null;
                try
                {
					//  Invoke Oracle Service Cloud Attachment Upload Web API
					_logger.LogInformation($@"Access Oracle Service Cloud Attachment Upload Web API {Environment.NewLine}uri: {Uri} {Environment.NewLine}UserName: {userName} PassWord: {password}");
					var stringTask = client.PostAsync(Uri, content).Result;
					_logger.LogInformation($@"Number of bytes processed by Oracle Service Cloud Attachment Upload Web API: {content.Headers.ContentLength}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.HResult, ex.Message + $@"{Environment.NewLine}uri: {Uri} {Environment.NewLine}UserName: {userName} PassWord: {password}");
                }
            }
        }
    }
}
