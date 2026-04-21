using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace KolayCAR.Broker.Infrastructure.Managers
{
    public class MobileHttpManager
    {
        private readonly Uri ApiBaseUri;
        private readonly HttpClient client;
        private readonly IDBHelper _dbHelper;
        private TimeSpan _timeout;
        private readonly HttpClient _httpClient;

        public MobileHttpManager(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient("MobileAppHttpClient"); // Sadece belirli ismi kullan
        }
        //public HttpManager2(string apiBaseUrl, string connectionString = "", HttpClientHandler httpClientHandler = null)
        //{
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    if (httpClientHandler != null)
        //    {
        //        client = new HttpClient(httpClientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }
        //    ApiBaseUri = new Uri(apiBaseUrl);

        //    if (!string.IsNullOrEmpty(connectionString))
        //        _dbHelper = new ADODBHelper(connectionString);

        //    client.BaseAddress = ApiBaseUri;
        //}

        public HttpResult<T> Get<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null) where T : class
        {

            client.SetHeaders(headers);
            //client.Timeout = TimeSpan.FromMinutes(30);
            requestPath += parameters.ToQueryString();

            var response = client.GetAsync(requestPath).Result;

            try
            {
                return response.Content.ReadAsAsync<HttpResult<T>>().Result;
            }
            catch (Exception ex)
            {
                return default(HttpResult<T>);
            }
        }
        public T Get2<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null) where T : class
        {
            client.SetHeaders(headers);
            //client.Timeout = TimeSpan.FromMinutes(30);
            requestPath += parameters.ToQueryString();

            var response = client.GetAsync(requestPath).Result;
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public HttpResult<T> GetWithTimeout<T>(string requestPath, TimeSpan timeOut, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null) where T : class
        {
            client.Timeout = timeOut;
            client.SetHeaders(headers);
            //client.Timeout = TimeSpan.FromMinutes(30);
            requestPath += parameters.ToQueryString();

            var response = client.GetAsync(requestPath).Result;

            try
            {
                return response.Content.ReadAsAsync<HttpResult<T>>().Result;
            }
            catch (Exception ex)
            {
                return default(HttpResult<T>);
            }
        }
        public async Task<HttpResult<T>> GetAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null,
    CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                using var cts = cancellationToken != null
                  ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                  : new CancellationTokenSource();

                var response = await client.GetAsync(requestPath);
                return await response.Content.ReadAsAsync<HttpResult<T>>();
            }
            catch (Exception ex)
            {
                return default(HttpResult<T>);
            }
           
        }

        public async Task<HttpResult<T>> GetAsync2<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, BrokerLogModel brokerLogModel = null) where T : class
        {
            client.SetHeaders(headers);
            requestPath += parameters.ToQueryString(encode);

            var response = await client.GetAsync(requestPath);
            var serialize = await response.Content.ReadAsStringAsync();
            if (_dbHelper != null && brokerLogModel != null)
            {
                brokerLogModel.Content = serialize;
                _dbHelper.WriteLog(brokerLogModel);
            }
            try
            {
                return HttpResult<T>.Result(
                data: JsonConvert.DeserializeObject<T>(serialize),
                httpResultType: response.StatusCode,
                success: response.IsSuccessStatusCode,
                message: response.RequestMessage.ToString());
            }
            catch (Exception ex)
            {
                return default(HttpResult<T>);
            }


        }

        public async Task<T> GetXmlAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool logResult = false, string vendorName = "") where T : class
        {
            string xml = "", fromXml = "";


            client.SetHeaders(headers);
            requestPath += parameters.ToQueryString();
            var requestPath2 = System.Uri.UnescapeDataString(requestPath);
            requestPath2.Replace('+', ' ');
            var response = await client.GetAsync(requestPath2);
            if (response != null)
            {
                xml = await response.Content.ReadAsStringAsync();

                if (logResult)
                    Serilog.Log.Error("{@GetXmlAsyncResult" + vendorName + "}", xml);

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = xml;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                if (string.IsNullOrEmpty(xml))
                    return default(T);

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);

                #region Lokasyonun tek satır olduğu durumlarda Map hatasını engellemek için oluşturuldu.
                var nodeCount = xmldoc.SelectNodes("//Location");
                var nodeCount2 = xmldoc.SelectNodes("//category");
                if (nodeCount.Count == 1)
                {
                    var contentNode = xmldoc.SelectSingleNode("//AytuRent");
                    var contentNode2 = xmldoc.SelectSingleNode("Sistemrent");
                    if (contentNode != null)
                    {
                        contentNode.AppendChild(xmldoc.CreateNode("element", "Location", ""));
                    }
                    if (contentNode2 != null)
                    {
                        contentNode2.AppendChild(xmldoc.CreateNode("element", "Location", ""));
                    }

                    fromXml = JsonConvert.SerializeXmlNode(xmldoc).Replace(",null]", "]");
                }
                if (nodeCount2.Count == 1)
                {
                    var contentNode = xmldoc.SelectSingleNode("//rates");
                    if (contentNode != null)
                    {
                        contentNode.AppendChild(xmldoc.CreateNode("element", "category", ""));
                    }


                    fromXml = JsonConvert.SerializeXmlNode(xmldoc).Replace(",null]", "]");
                }
                else
                {
                    fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                }
                #endregion

                //fromXml = JsonConvert.SerializeXmlNode(xmldoc); // Tek satırlık verilerde hata verdiği için kapatıldı. Region ile eklenen alan oluşturuldu.
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });

            }
            catch (Exception ex)
            {
                return default(T);
            }

        }

        public async Task<T> GetXmlDocumentAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null) where T : class
        {
            string xml = "", fromXml = "";


            client.SetHeaders(headers);
            requestPath += parameters.ToQueryString();
            var response = await client.GetAsync(requestPath);
            try
            {
                if (response != null)
                {
                    xml = await response.Content.ReadAsStringAsync();

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = xml;
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    if (string.IsNullOrEmpty(xml))
                        return default(T);

                    var xmldoc = new XmlDocument();
                    xmldoc.LoadXml(xml);

                    #region Araçlar tek satır olduğu durumlarda Map hatasını engellemek için oluşturuldu.

                    var nodeCount = xmldoc.SelectNodes("//Car");
                    if (nodeCount.Count == 1)
                    {
                        var contentNode = xmldoc.SelectSingleNode("//Sistemrent");
                        contentNode.AppendChild(xmldoc.CreateNode("element", "Car", ""));
                        fromXml = JsonConvert.SerializeXmlNode(xmldoc).Replace(",null]", "]");
                    }
                    else
                    {
                        fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                    }
                    #endregion

                    //fromXml = JsonConvert.SerializeXmlNode(xmldoc); // Tek satırlık verilerde hata verdiği için kapatıldı. Region ile eklenen alan oluşturuldu.
                }
                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });

            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@TurevavailabilityRequest}", $"{client.BaseAddress.AbsoluteUri}{requestPath} - {ex.Message}");
                return default(T);
            }
        }

        public async Task<HttpResult<TRes>> PostAsync<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool encode = true)
            where TReq : class
            where TRes : class
        {

            client.SetHeaders(headers);
            requestPath += parameters.ToQueryString(encode);

            var response = new HttpResponseMessage();

            if (entity == null)
            {
                var emptyEntity = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                response = await client.PostAsync(requestPath, emptyEntity);
            }
            else
            {
                response = await client.PostAsJsonAsync(requestPath, entity);

            }

            var result = await response.Content.ReadAsStringAsync();

            if (_dbHelper != null && brokerLogModel != null)
            {
                brokerLogModel.Content = result;
                _dbHelper.WriteLog(brokerLogModel);
            }

            try
            {
                return JsonConvert.DeserializeObject<HttpResult<TRes>>(result);
            }
            catch (Exception ex)
            {
                return default(HttpResult<TRes>);
            }

        }
        public async Task<HttpResult<TRes>> PostAsync2<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null)
           where TReq : class
           where TRes : class
        {

            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = new HttpResponseMessage();
                if (entity == null)
                {
                    var emptyEntity = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(requestPath, emptyEntity);
                }
                else
                {
                    response = await client.PostAsJsonAsync(requestPath, entity);

                }
                //var response = await client.PostAsJsonAsync(requestPath, entity);
                var result = await response.Content.ReadAsStringAsync();


                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }
                return HttpResult<TRes>.Result(
                  data: JsonConvert.DeserializeObject<TRes>(result),
                  httpResultType: HttpStatusCode.OK,
                  success: response.IsSuccessStatusCode,
                  message: response.RequestMessage.ToString());
            }
            catch (Exception ex)
            {
                return default(HttpResult<TRes>);
            }
        }


        public async Task<HttpResult<TRes>> PostAsync<TRes>(string requestPath, HttpContent content, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null)
     where TRes : class
        {
            client.SetHeaders(headers);
            requestPath += parameters.ToQueryString();
            var response = await client.PostAsync(requestPath, content);
            var result = await response.Content.ReadAsStringAsync();

            if (_dbHelper != null && brokerLogModel != null)
            {
                brokerLogModel.Content = result;
                _dbHelper.WriteLog(brokerLogModel);
            }

            try
            {
                return HttpResult<TRes>.Result(
                    data: JsonConvert.DeserializeObject<TRes>(result),
                    httpResultType: HttpStatusCode.OK,
                    success: response.IsSuccessStatusCode,
                    message: response.RequestMessage.ToString());
            }
            catch (Exception ex)
            {
                return default(HttpResult<TRes>);
            }

        }

    }
}
