using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;
using WebRequest.Elegant.Offline.Extensions;

namespace WebRequest.Elegant.Offline
{
    public class WebCache : IWebCache
    {
        private const string ResponseDataCollection = "response_collection";
        private const string RequestsQueueCollection = "requests_queue";

        private readonly string _databaseDirectory;
        private readonly string _databaseFilePath;
        private readonly BsonMapper _bsonMapper;
        private readonly object _dbLocker = new object();

        public WebCache(string databaseDirectory)
        {
            if (string.IsNullOrWhiteSpace(databaseDirectory))
            {
                throw new ArgumentException("The argument can't be null or empty.", nameof(databaseDirectory));
            }

            _databaseDirectory = databaseDirectory;
            _databaseFilePath = Path.Combine(_databaseDirectory, "sci-cache-db");

            _bsonMapper = BsonMapper.Global;

            _bsonMapper.Entity<RequestData>()
                .Id(x => x.RequestId);

            _bsonMapper.Entity<ResponseData>()
                .Id(x => x.RequestId);
        }

        private LiteDatabase DatabaseInstance
        {
            get
            {
                if (!Directory.Exists(_databaseDirectory))
                {
                    Directory.CreateDirectory(_databaseDirectory);
                }

                return new LiteDatabase(_databaseFilePath, _bsonMapper);
            }
        }

        private ILiteCollection<RequestData> RequestsCollection(LiteDatabase database)
            => database.GetCollection<RequestData>(RequestsQueueCollection);

        private ILiteCollection<ResponseData> ResponsesCollection(LiteDatabase database)
            => database.GetCollection<ResponseData>(ResponseDataCollection);

        public Task PutRequestAsync(RequestData requestData)
        {
            lock (_dbLocker)
            {
                using (var db = DatabaseInstance)
                {
                    RequestsCollection(db).InsertOrUpdate(requestData.RequestId, requestData);
                }

                return Task.CompletedTask;
            }
        }

        public Task<List<RequestData>> GetPendingRequestsAsync()
        {
            lock (_dbLocker)
            {
                using (var db = DatabaseInstance)
                {
                    var cachedRequestsList = RequestsCollection(db).FindAll().OrderBy(x => x.TimeInUnixMilliseconds).ToList();

                    return Task.FromResult(cachedRequestsList);
                }
            }
        }

        public Task RemovePendingRequestAsync(long requestId)
        {
            lock (_dbLocker)
            {
                using (var db = DatabaseInstance)
                {
                    RequestsCollection(db).Delete(requestId);
                    return Task.CompletedTask;
                }
            }
        }

        public Task<HttpResponseMessage> GetResponseAsync(IWebRequest webRequest)
        {
            var uniqueRequestId = WebRequestId(webRequest);

            ResponseData existingResponseData;

            lock (_dbLocker)
            {
                using (var db = DatabaseInstance)
                {
                    existingResponseData = ResponsesCollection(db).FindById(uniqueRequestId);
                }
            }

            if (existingResponseData != null)
            {
                return Task.FromResult(new HttpResponseMessage(existingResponseData.ResponseStatusCode)
                {
                    Content = new StringContent(existingResponseData.ResponseContent),
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent($"There is no content in the web cache for: {webRequest}"),
            });
        }

        public async Task PutResponseAsync(IWebRequest webRequest, HttpResponseMessage response)
        {
            await response.ThrowIfNotOkOrAcceptedStatusAsync(webRequest.Uri).ConfigureAwait(false);

            long uniqueRequestId = WebRequestId(webRequest);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseData = new ResponseData
            {
                RequestId = uniqueRequestId,
                ResponseStatusCode = response.StatusCode,
                ResponseContent = responseContent,
            };

            lock (_dbLocker)
            {
                using (var db = DatabaseInstance)
                {
                    ResponsesCollection(db).InsertOrUpdate(uniqueRequestId, responseData);
                }
            }
        }

        public Task ClearAsync()
        {
            lock (_dbLocker)
            {
                using (var db = DatabaseInstance)
                {
                    RequestsCollection(db).DeleteAll();
                    ResponsesCollection(db).DeleteAll();

                    return Task.CompletedTask;
                }
            }
        }

        public void Drop()
        {
            lock (_dbLocker)
            {
                if (File.Exists(_databaseFilePath))
                {
                    File.Delete(_databaseFilePath);
                }
            }
        }

        private long WebRequestId(IWebRequest webRequest)
        {
            return new WebRequestUniqueId(webRequest).ToNumber();
        }
    }

    internal class ResponseData
    {
        public long RequestId { get; set; }

        public HttpStatusCode ResponseStatusCode { get; set; }

        public string ResponseContent { get; set; }
    }
}
