using LiteDB;
using System;
using YoutubeDownloaderChecker.Entities;

namespace YoutubeDownloaderChecker.Helpers
{
    static class UpdateOrCreateMetadataHelper
    {
        public static void UpdateOrCreateMetadataFromJsonToLocalDB(Metadata metadata, string dataDirPath)
        {
            using (var db = new LiteDatabase(dataDirPath + "Database.db"))
            {
                var collection = db.GetCollection<Metadata>("Metadata");
                var videoVersionCollection = db.GetCollection<VideoVersion>("VideoVersion");
                var test = collection.Query().Where(p => p.Json == null).ToList();
                var matedataSameTitleList = collection.Query().Where(p => p.Title == metadata.Title).ToList();

                if (matedataSameTitleList.Count == 0)
                {
                    videoVersionCollection.Insert(metadata.VideoVersion);
                    var bsonValue = collection.Insert(metadata);
                    Console.WriteLine($"Metadata for video: {metadata.Title} has been set to Id {bsonValue.ToString()}.");
                }
                else
                {
                    var foundMetadata = matedataSameTitleList.FindLast(p => p.VideoVersion.LongIntVersion >= 0);

                    if (foundMetadata != null)
                    {
                        if (foundMetadata.BaseVideoString != metadata.BaseVideoString ||
                            foundMetadata.Categories != metadata.Categories ||
                            foundMetadata.Description != metadata.Description ||
                            foundMetadata.Downloaded != metadata.Downloaded ||
                            foundMetadata.Duration != metadata.Duration ||
                            foundMetadata.Json != metadata.Json ||
                            foundMetadata.ReleaseDate != metadata.ReleaseDate ||
                            foundMetadata.Tags != metadata.Tags ||
                            foundMetadata.Title != metadata.Title ||
                            foundMetadata.Uploader != metadata.Uploader ||
                            foundMetadata.Url != metadata.Url ||
                            foundMetadata.VideoVersion != metadata.VideoVersion ||
                            foundMetadata.ViewCount != metadata.ViewCount)
                        {
                            metadata.VideoVersion.LongIntVersion = foundMetadata.VideoVersion.LongIntVersion++;
                            collection.Insert(metadata);
                            Console.WriteLine($"Metadata for video: {metadata.Title} has been updated to version {metadata.VideoVersion.LongIntVersion}.");
                        }
                    }
                }
                db.Commit();
                collection.EnsureIndex(x => x.Id);
            }
        }
    }
}
