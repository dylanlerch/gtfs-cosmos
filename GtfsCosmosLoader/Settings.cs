namespace GtfsCosmosLoader
{
	public class Settings
	{
		// Cosmos
		public string CosmosEndpointUri { get; set; }
		public string CosmosPrimaryKey { get; set; }
		public string CosmosDatabaseName { get; set; }
		public string CosmosCollectionName { get; set; }

		// Gtfs
		public string GftsDataUri { get; set; }

		// Local Folders
		public string GtfsTemporaryLocation { get; set; }
	}
}