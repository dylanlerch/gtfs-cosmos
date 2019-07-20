using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Net;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Linq;

namespace GtfsCosmosLoader
{
	class Program
	{
		private DocumentClient client;

		static async Task Main(string[] args)
		{
			Program p = new Program();
			await p.Start();
		}

		private async Task Start()
		{
			var settings = Settings();

			var connectionPolicy = new ConnectionPolicy
			{
				ConnectionMode = ConnectionMode.Direct,
				ConnectionProtocol = Protocol.Tcp,
				MaxConnectionLimit = 100
			};
			this.client = new DocumentClient(new Uri(settings.CosmosEndpointUri), settings.CosmosPrimaryKey, connectionPolicy);

			var gtfsDataFolder = await DownloadGtfsData(settings);
			await CreateCollectionIfNotExists(settings);

			var data = new TransitFeedData();
			data.Read(gtfsDataFolder);

			await WriteDataToCosmos(settings.CosmosDatabaseName, settings.CosmosCollectionName, data);
		}

		private async Task CreateCollectionIfNotExists(Settings settings)
		{
			var databaseUri = UriFactory.CreateDatabaseUri(settings.CosmosDatabaseName);
			var documentCollection = new DocumentCollection
			{
				Id = settings.CosmosCollectionName,
				PartitionKey = new PartitionKeyDefinition
				{
					Paths = new Collection<string> { "/recordType" }
				}
			};

			var collection = await this.client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, documentCollection);
		}

		// Downloads and the GTFS dat
		private async Task<string> DownloadGtfsData(Settings settings)
		{
			var sw = new Stopwatch();
			sw.Start();

			var zipFilePath = Path.Join(settings.GtfsTemporaryLocation, "gtfs.zip");
			var extractFilePath = Path.Join(settings.GtfsTemporaryLocation, "extract");

			Directory.CreateDirectory(settings.GtfsTemporaryLocation);
			Directory.CreateDirectory(extractFilePath);

			using (var client = new WebClient())
			{
				await client.DownloadFileTaskAsync(new Uri(settings.GftsDataUri), zipFilePath);
			}

			ZipFile.ExtractToDirectory(zipFilePath, extractFilePath, true);

			sw.Stop();
			Console.WriteLine($"Data downloaded and extracted in {sw.ElapsedMilliseconds}ms");

			return extractFilePath;
		}

		// This will do for now
		private Settings Settings()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile("appsettings.local.json");

			var config = builder.Build();

			return new Settings
			{
				// Cosmos
				CosmosEndpointUri = config.GetConnectionString("Cosmos:EndpointUri"),
				CosmosPrimaryKey = config.GetConnectionString("Cosmos:PrimaryKey"),
				CosmosDatabaseName = config.GetConnectionString("Cosmos:DatabaseName"),
				CosmosCollectionName = config.GetConnectionString("Cosmos:CollectionName"),

				// Gtfs
				GftsDataUri = config["Endpoints:GtfsData"],

				// Folders
				GtfsTemporaryLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), config["Files:GtfsTemporaryLocation"]),
			};
		}

		private async Task WriteDataToCosmos(string databaseName, string collectionName, TransitFeedData data)
		{
			var documentCollection = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

			await CreateDocumentsAsync(documentCollection, data.Stops.ToList());
			await CreateDocumentsAsync(documentCollection, data.Trips);
		}


		private async Task CreateDocumentsAsync<T>(Uri collectionUri, List<T> documents)
		{
			const int batchSize = 100;
			var documentsToUpload = new List<T>();

			var fullSw = new Stopwatch();
			fullSw.Start();

			for (int i = 0; i < documents.Count; i++)
			{
				documentsToUpload.Add(documents[i]);

				if (i % batchSize == 0 || i >= (documents.Count - 1))
				{
					var sw = new Stopwatch();
					sw.Start();

					var documentUploadTasks = new List<Task<ResourceResponse<Document>>>();
					foreach (var documentToUpload in documentsToUpload)
					{
						var uploadedDocument = this.client.CreateDocumentAsync(collectionUri, documentToUpload);
						documentUploadTasks.Add(uploadedDocument);
					}

					var completedUploads = await Task.WhenAll(documentUploadTasks);

					sw.Stop();
					var totalRequestUnits = new List<ResourceResponse<Document>>(completedUploads).Sum(x => x.RequestCharge);
					var ruPerSecond = Math.Round(totalRequestUnits / (sw.ElapsedMilliseconds / 1000.0), 2);
					Console.WriteLine($"Uploaded {documentsToUpload.Count} in {sw.ElapsedMilliseconds}ms | Total RUs: {Math.Round(totalRequestUnits, 2)} ({ruPerSecond} RU/s) | Processed {i} / {documents.Count}.");

					documentsToUpload.Clear();
				}
			}

			fullSw.Stop();
			Console.WriteLine($"Completed full upload in {fullSw.ElapsedMilliseconds}ms");
		}
	}

	static class Extensions
	{
		public static string NullIfWhitespace(this string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return null;
			}
			else
			{
				return input;
			}
		}
	}
}
