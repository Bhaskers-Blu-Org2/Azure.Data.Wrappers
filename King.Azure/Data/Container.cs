﻿namespace King.Azure.Data
{
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Queue
    /// </summary>
    public class Container : AzureStorage, IContainer
    {
        #region Members
        /// <summary>
        /// Client
        /// </summary>
        protected readonly CloudBlobClient client;

        /// <summary>
        /// Reference
        /// </summary>
        protected readonly CloudBlobContainer reference;
        #endregion

        #region Constructors
        /// <summary>
        /// Queue
        /// </summary>
        /// <param name="name">Name</param>
        public Container(string name, string connectionString)
            : base(connectionString)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name");
            }

            this.client = this.account.CreateCloudBlobClient();
            this.reference = this.client.GetContainerReference(name);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Table Name
        /// </summary>
        public string Name
        {
            get
            {
                return this.reference.Name;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create If Not Exists
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> CreateIfNotExists()
        {
            return await this.reference.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Save Object as Json to Blob Storage
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="blobName">Blob Name</param>
        /// <param name="obj">Object</param>
        /// <returns>Task</returns>
        public virtual async Task Save(string blobName, object obj)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("blobName");
            }
            if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }

            var json = JsonConvert.SerializeObject(obj);

            var blob = this.reference.GetBlockBlobReference(blobName);
            await blob.UploadTextAsync(json);
        }

        /// <summary>
        /// Get Object from Blob Storage
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="blobName">Blob Name</param>
        /// <returns>Object</returns>
        public virtual async Task<T> Get<T>(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("blobName");
            }

            var blob = this.reference.GetBlockBlobReference(blobName);
            var json = await blob.DownloadTextAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Get Bytes
        /// </summary>
        /// <param name="blobName">Blob Name</param>
        /// <returns>bytes</returns>
        public virtual async Task<byte[]> Get(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("blobName");
            }

            var blob = await this.GetReference(blobName);
            var bytes = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(bytes, 0);

            return bytes;
        }

        /// <summary>
        /// Save Bytes
        /// </summary>
        /// <param name="blobName">Blob Name</param>
        /// <param name="bytes">bytes</param>
        /// <returns>Task</returns>
        public virtual async Task Save(string blobName, byte[] bytes, string contentType = "application/octet-stream")
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("blobName");
            }
            if (null == bytes)
            {
                throw new ArgumentNullException("bytes");
            }

            var blob = await this.GetReference(blobName);
            blob.Properties.ContentType = contentType;
            await blob.SetPropertiesAsync();
            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Get Reference
        /// </summary>
        /// <param name="blobName">Blob Name</param>
        /// <returns>Cloud Block Blob</returns>
        public virtual async Task<ICloudBlob> GetReference(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("blobName");
            }

            return await this.reference.GetBlobReferenceFromServerAsync(blobName);
        }
        #endregion
    }
}
