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
    public class HttpManager
    {
        private readonly Uri ApiBaseUri;
        private readonly HttpClient client;
        private readonly IDBHelper _dbHelper;
        private TimeSpan _timeout;
        public HttpManager(HttpClient httpClient)
        {
            client = httpClient;
        }
        public HttpManager(string apiBaseUrl, string connectionString = "", HttpClientHandler httpClientHandler = null, int timeout = 0)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (httpClientHandler != null)
            {
                client = new HttpClient(httpClientHandler);
            }
            else
            {
                client = new HttpClient();
            }
            ApiBaseUri = new Uri(apiBaseUrl);

            if (!string.IsNullOrEmpty(connectionString))
                _dbHelper = new ADODBHelper(connectionString);
            if (timeout != 0)
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
            }

            client.BaseAddress = ApiBaseUri;
        }

        public HttpResult<T> Get<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                //client.Timeout = TimeSpan.FromMinutes(30);
                requestPath += parameters.ToQueryString();

                var response = client.GetAsync(requestPath).Result;
                return response.Content.ReadAsAsync<HttpResult<T>>().Result;
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetError}", ex.ToJson());

                return default(HttpResult<T>);
            }
        }
        public T Get2<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                //client.Timeout = TimeSpan.FromMinutes(30);
                requestPath += parameters.ToQueryString();

                var response = client.GetAsync(requestPath).Result;

                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGet2Error}", ex.ToJson());

                return default(T);
            }
        }
        public HttpResult<T> GetWithTimeout<T>(string requestPath, TimeSpan timeOut, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.Timeout = timeOut;
                client.SetHeaders(headers);
                //client.Timeout = TimeSpan.FromMinutes(30);
                requestPath += parameters.ToQueryString();

                var response = client.GetAsync(requestPath).Result;

                return response.Content.ReadAsAsync<HttpResult<T>>().Result;
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetWithTimeoutError}", ex.ToJson());

                return default(HttpResult<T>);
            }
        }
        public async Task<HttpResult<T>> GetAsync<T>(
            string requestPath,
            IDictionary<string, object> parameters = null,
            IDictionary<string, object> headers = null,
            CancellationToken cancellationToken = default, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.GetAsync(requestPath);

                return await response.Content.ReadAsAsync<HttpResult<T>>();
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetAsyncError}", ex.ToJson());

                return default(HttpResult<T>);
            }
        }

        public async Task<HttpResult<T>> GetAsync2<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString(encode);
                var response = await client.GetAsync(requestPath);
                return await CreateHttpResult<T>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetAsync2Error}", client.BaseAddress.AbsoluteUri + " - " + ex.ToJson());

                return HttpResult<T>.Catch(ex.Message);
            }
        }

        public async Task<T> GetAsyncWithModel<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
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
                return JsonConvert.DeserializeObject<T>(serialize);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetAsyncWithModelError}", ex.ToJson());

                return default(T);
            }

        }
        public async Task<T> GetAsyncWithModelSixt<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
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
                return JsonConvert.DeserializeObject<T>(serialize);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@SixtLocationError}", ex.ToJson());

                return default(T);
            }

        }
        public async Task<T> PostXmlAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool logResult = false, string vendorName = "", bool isReservationRequest = false) where T : class
        {
            try
            {
                string xml = "", fromXml = "";
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var requestPath2 = System.Uri.UnescapeDataString(requestPath);
                requestPath2.Replace('+', ' ');
                var response = await client.PostAsync(requestPath2, new StringContent(""));
                if (response != null)
                {
                    xml = await response.Content.ReadAsStringAsync();

                    if (logResult)
                        Serilog.Log.Error("{@HttpManagerGetXmlAsyncResult" + vendorName + "}", xml);

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

                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostXmlAsyncError}", ex.ToJson());

                return default(T);
            }
        }
        public async Task<T> GetXmlAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool logResult = false, string vendorName = "", bool isReservationRequest = false) where T : class
        {
            try
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
                        Serilog.Log.Error("{@HttpManagerGetXmlAsyncResult" + vendorName + "}", xml);

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

                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });

            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetXmlAsyncError}", ex.ToJson());

                return default(T);
            }
        }

        public async Task<T> GetXmlDocumentAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                string xml = "", fromXml = "";
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.GetAsync(requestPath);

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
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerGetXmlDocumentAsyncError}", ex.ToJson());

                return default(T);
            }
        }

        public async Task<HttpResult<TRes>> PostAsync<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool encode = true, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
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

                return JsonConvert.DeserializeObject<HttpResult<TRes>>(result);
            }
            catch (Exception ex)
            {

                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncError}", ex.ToJson());

                return default(HttpResult<TRes>);
            }
        }
        public async Task<HttpResult<TRes>> PostAsync2<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool getCookie = false, bool isReservationRequest = false)
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
                if (getCookie)
                {
                    string cookieValue = "";
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        foreach (var cookie in cookies)
                        {
                            cookieValue = cookieValue + cookie;
                        }
                    }
                    return HttpResult<TRes>.Result(
                            data: JsonConvert.DeserializeObject<TRes>(result),
                            httpResultType: HttpStatusCode.OK,
                            success: response.IsSuccessStatusCode,
                            message: response.RequestMessage.ToString(),
                            cookie: cookieValue);
                }
                return HttpResult<TRes>.Result(
                  data: JsonConvert.DeserializeObject<TRes>(result),
                  httpResultType: HttpStatusCode.OK,
                  success: response.IsSuccessStatusCode,
                  message: response.RequestMessage.ToString());
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsync2Error}", ex.ToJson());

                return default(HttpResult<TRes>);
            }
        }

        public async Task<HttpResult<TRes>> PostAsync<TRes>(string requestPath, HttpContent content, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
     where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.PostAsync(requestPath, content);
                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }
        public async Task<TRes> PostAsyncWithModel<Treq, TRes>(string requestPath, Treq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
        where Treq : class
        where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.PostAsJsonAsync(requestPath, entity);
                var result = await response.Content.ReadAsStringAsync();
                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                return JsonConvert.DeserializeObject<TRes>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithModelError}", ex.ToJson());

                return default(TRes);
            }
        }
        public async Task<TRes> PostAsyncWithModel<TRes>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.PostAsync(requestPath, null);
                var result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                return JsonConvert.DeserializeObject<TRes>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithModelError}", ex.ToJson());

                return default(TRes);
            }
        }
        public async Task<HttpResult<TRes>> PostAsyncWithModelResult<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.PostAsJsonAsync(requestPath, entity);

                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithModelResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }
        public async Task<HttpResult<TRes>> PostAsyncWithModelResult<TRes>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.PostAsync(requestPath, null);

                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithModelResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }

        public async Task<HttpStatusCode> DeleteAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, T entity = null, bool isReservationRequest = false)
    where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var request = new HttpRequestMessage(HttpMethod.Delete, requestPath)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await client.SendAsync(request);
                return response.StatusCode;
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerDeleteAsyncError}", ex.ToJson());

                return HttpStatusCode.InternalServerError;
            }
        }

        public async Task<HttpResult<string>> DeleteAsyncResult<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, T entity = null, bool isReservationRequest = false)
            where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var request = new HttpRequestMessage(HttpMethod.Delete, requestPath)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var responseMessage = VendorResponseMessageHelper.ExtractMessage(result);

                return HttpResult<string>.Result(
                    data: result,
                    httpResultType: response.StatusCode,
                    success: response.IsSuccessStatusCode,
                    message: !string.IsNullOrWhiteSpace(responseMessage) ? responseMessage : response.ReasonPhrase,
                    rawContent: result,
                    serviceMessage: response.IsSuccessStatusCode ? null : result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerDeleteAsyncResultError}", ex.ToJson());

                return HttpResult<string>.Catch(ex.Message);
            }
        }
        public async Task<TRes> PostAsyncWithModelNullValueHandling<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where TReq : class where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                if (parameters != null)
                    requestPath += parameters.ToQueryString();
                string json = null;
                if (entity != null)
                {
                    json = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }

                using (var content = json != null ? new StringContent(json, Encoding.UTF8, "application/json") : null)
                using (var response = await client.PostAsync(requestPath, content))
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = result;
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    return JsonConvert.DeserializeObject<TRes>(result);
                }
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncError}", ex.ToJson());

                return default(TRes);
            }
        }
        public async Task<HttpResult<TRes>> PostAsyncWithModelNullValueHandlingResult<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where TReq : class where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                if (parameters != null)
                    requestPath += parameters.ToQueryString();
                string json = null;
                if (entity != null)
                {
                    json = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }

                using (var content = json != null ? new StringContent(json, Encoding.UTF8, "application/json") : null)
                using (var response = await client.PostAsync(requestPath, content))
                {
                    return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
                }
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithModelNullValueHandlingResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }
        public async Task<TRes> PostAsyncWithUrlEncoded<TReq, TRes>(string requestPath, List<KeyValuePair<string, string>> entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isPlainTextResponse = false, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
          where TReq : class
          where TRes : class
        {
            string result = string.Empty;

            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.PostAsync(requestPath, new FormUrlEncodedContent(entity ?? new()));

                result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                if (isPlainTextResponse)
                    return (dynamic)result;

                return JsonConvert.DeserializeObject<TRes>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithUrlEncodedError}", ex.ToJson());

                return default;
            }
        }
        public async Task<HttpResult<TRes>> PostAsyncWithUrlEncodedResult<TReq, TRes>(string requestPath, List<KeyValuePair<string, string>> entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.PostAsync(requestPath, new FormUrlEncodedContent(entity ?? new()));

                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerPostAsyncWithUrlEncodedResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }

        private async Task<HttpResult<TRes>> CreateHttpResult<TRes>(HttpResponseMessage response, BrokerLogModel brokerLogModel, bool isReservationRequest) where TRes : class
        {
            var result = await response.Content.ReadAsStringAsync();

            if (_dbHelper != null && brokerLogModel != null)
            {
                brokerLogModel.Content = result;
                _dbHelper.WriteLog(brokerLogModel);
            }

            var responseMessage = VendorResponseMessageHelper.ExtractMessage(result);
            var isErrorResponse = VendorResponseMessageHelper.IsErrorResponse(result);
            var deserializeSuccess = true;
            var deserializeErrorMessage = string.Empty;
            TRes data = default;

            try
            {
                if (!string.IsNullOrWhiteSpace(result))
                    data = JsonConvert.DeserializeObject<TRes>(result);
            }
            catch (Exception ex)
            {
                deserializeSuccess = false;
                deserializeErrorMessage = ex.Message;

                if (isReservationRequest)
                    Serilog.Log.Error("{@HttpManagerDeserializeModelError}", ex.ToJson());
            }

            var isDefaultMappedResponse = deserializeSuccess && VendorResponseMessageHelper.IsDefaultMappedResponse(result, data);
            var hasVendorErrorResponse = isErrorResponse || isDefaultMappedResponse;

            return HttpResult<TRes>.Result(
                data: data,
                httpResultType: response.StatusCode,
                success: response.IsSuccessStatusCode && !hasVendorErrorResponse,
                message: !string.IsNullOrWhiteSpace(responseMessage) ? responseMessage : response.ReasonPhrase,
                rawContent: result,
                deserializeSuccess: deserializeSuccess,
                deserializeErrorMessage: deserializeErrorMessage,
                serviceMessage: !deserializeSuccess || !response.IsSuccessStatusCode || hasVendorErrorResponse ? result : null);
        }
    }
}
