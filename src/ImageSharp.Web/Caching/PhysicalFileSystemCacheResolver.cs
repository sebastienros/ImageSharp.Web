// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web.Caching;

namespace SixLabors.ImageSharp.Web.Resolvers
{
    /// <summary>
    /// Provides means to manage image buffers within the <see cref="PhysicalFileSystemCache"/>.
    /// </summary>
    public class PhysicalFileSystemCacheResolver : IImageCacheResolver
    {
        private readonly IFileInfo metaFileInfo;
        private readonly FormatUtilities formatUtilities;
        private ImageCacheMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemCacheResolver"/> class.
        /// </summary>
        /// <param name="metaFileInfo">The cached metadata file info.</param>
        /// <param name="formatUtilities">
        /// Contains various format helper methods based on the current configuration.
        /// </param>
        public PhysicalFileSystemCacheResolver(IFileInfo metaFileInfo, FormatUtilities formatUtilities)
        {
            this.metaFileInfo = metaFileInfo;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public async Task<ImageCacheMetadata> GetMetaDataAsync()
        {
            using Stream stream = this.metaFileInfo.CreateReadStream();
            this.metadata = await ImageCacheMetadata.ReadAsync(stream);
            return this.metadata;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync()
        {
            string path = Path.ChangeExtension(
                this.metaFileInfo.PhysicalPath,
                this.formatUtilities.GetExtensionFromContentType(this.metadata.ContentType));

            // We are setting buffer size to 1 to prevent FileStream from allocating it's internal buffer
            // 0 causes constructor to throw
            int bufferSize = 1;
            var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            return Task.FromResult<Stream>(stream);
        }
    }
}
