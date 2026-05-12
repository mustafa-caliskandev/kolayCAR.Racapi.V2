using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using static KolayCAR.Broker.Domain.Models.Response.Central2ResponseBase;

namespace KolayCAR.Broker.Infrastructure.Managers
{
    public class RestManager
    {
        private readonly Uri ApiBaseUri;
        private readonly HttpClient client;
        private readonly IDBHelper _dbHelper;

        public RestManager(string apiBaseUrl, string connectionString = "", int timeout = 0)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            client = new HttpClient(clientHandler);
            ApiBaseUri = new Uri(apiBaseUrl);

            if (!string.IsNullOrEmpty(connectionString))
                _dbHelper = new ADODBHelper(connectionString);
            if (timeout != 0)
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
            }

            client.BaseAddress = ApiBaseUri;
        }

        public T Get<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = client.GetAsync(requestPath).Result;

                return response.Content.ReadAsAsync<T>().Result;

            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetError}", ex.ToJson());

                return default(T);
            }
        }

        public async Task<T> GetAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, BrokerLogModel brokerLogModel = null, bool isResponseList = false, bool isReservationRequest = false) where T : class
        {
            try
            {
                string result = "";
                HttpResponseMessage response = null;

                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString(encode: encode);

                response = await client.GetAsync(requestPath);
                result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetAsyncError}", ex.ToJson());

                return default(T);
            }
        }
        public async Task<HttpResult<T>> GetAsyncResult<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString(encode: encode);
                var response = await client.GetAsync(requestPath);

                return await CreateHttpResult<T>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetAsyncResultError}", ex.ToJson());

                return HttpResult<T>.Catch(ex.Message);
            }
        }

        public async Task<TRes> GetAsync<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool encode = true, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{requestPath}"),
                    Content = new StringContent(
                    JsonConvert.SerializeObject(entity),
                    System.Text.UTF8Encoding.UTF8,
                    "application/json")
                };

                var response = client.SendAsync(request).ConfigureAwait(false);
                var responseInfo = response.GetAwaiter().GetResult();
                var result = await responseInfo.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TRes>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetAsyncOverrideError}", ex.ToJson());

                return default(TRes);
            }

        }

        public async Task<T> GetXmlAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {

                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();
                var response = await client.GetAsync(requestPath);
                var xml = await response.Content.ReadAsStringAsync();

                if (xml.Contains("\n") || xml.Contains("\t") || xml.Contains("\r"))
                {
                    xml = xml.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                }

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = xml;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);

                #region Araçların tek satır olduğu durumlarda Map hatasını engellemek için oluşturuldu.
                string fromXml = "";
                if (typeof(T).FullName == typeof(CentralResponse).FullName)
                {
                    var nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
                    //nsmgr.AddNamespace("ns2", "http://ws.naryaz.com/model/dto");
                    //foreach (XmlNode node in xmldoc.GetElementsByTagName("ns2:capacities"))
                    foreach (XmlNode node in xmldoc.SelectNodes("//Turevsistem", nsmgr))
                    {
                        var nodeVehicle = node.SelectNodes("Car", nsmgr);
                        if (nodeVehicle.Count == 1)
                        {
                            string newNode = "<Car></Car>";
                            XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                            node.AppendChild(xmldoc.ReadNode(textReader));
                        }
                    }

                    fromXml = JsonConvert.SerializeXmlNode(xmldoc, Newtonsoft.Json.Formatting.Indented).Replace(",null]", "]");
                }
                else
                {
                    fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                }
                #endregion

                //var fromXml = JsonConvert.SerializeXmlNode(xmldoc);

                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetXmlAsyncError}", ex.ToJson());

                return default(T);
            }

        }

        public async Task<T> GetXmlAsyncForSixt<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.GetAsync(requestPath);
                var xml = await response.Content.ReadAsStringAsync();

                if (xml.Contains("\n") || xml.Contains("\t") || xml.Contains("\r"))
                {
                    xml = xml.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                }

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = xml;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                //var fromXml = JsonConvert.SerializeXmlNode(xmldoc);

                #region Araçların tek satır olduğu durumlarda Map hatasını engellemek için oluşturuldu.
                string fromXml = "";
                if (typeof(T).FullName == typeof(VehicleQueryResponseBase).FullName)
                {
                    var nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
                    //nsmgr.AddNamespace("ns2", "http://ws.naryaz.com/model/dto");
                    //foreach (XmlNode node in xmldoc.GetElementsByTagName("ns2:capacities"))
                    foreach (XmlNode node in xmldoc.SelectNodes("//SIXTTURKEYWEBSERVICES/VEHICLES", nsmgr))
                    {
                        var nodeVehicle = node.SelectNodes("VEHICLE", nsmgr);
                        if (nodeVehicle.Count == 1)
                        {
                            string newNode = "<VEHICLE></VEHICLE>";
                            XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                            node.AppendChild(xmldoc.ReadNode(textReader));
                        }
                    }
                    foreach (XmlNode node in xmldoc.SelectNodes("//VEHICLE/INCLUDED", nsmgr))
                    {
                        var nodeVehicle = node.SelectNodes("INCLUDE", nsmgr);
                        if (nodeVehicle.Count == 1)
                        {
                            string newNode = "<INCLUDE></INCLUDE>";
                            XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                            node.AppendChild(xmldoc.ReadNode(textReader));
                        }
                        var nodeVehicle2 = node.SelectNodes("INSURANCE", nsmgr);
                        if (nodeVehicle2.Count == 1)
                        {
                            string newNode = "<INSURANCE></INSURANCE>";
                            XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                            node.AppendChild(xmldoc.ReadNode(textReader));
                        }
                    }

                    fromXml = JsonConvert.SerializeXmlNode(xmldoc, Newtonsoft.Json.Formatting.Indented).Replace(",null]", "]");
                }
                else
                {
                    fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                }
                #endregion


                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetXmlAsyncForSixtError}", ex.ToJson());

                return default(T);
            }

        }
        public async Task<T> GetXmlAsyncForSixt2<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.GetAsync(requestPath);
                var xml = await response.Content.ReadAsStringAsync();

                if (xml.Contains("\n") || xml.Contains("\t") || xml.Contains("\r"))
                {
                    xml = xml.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                }

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = xml;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                //var fromXml = JsonConvert.SerializeXmlNode(xmldoc);

                #region Araçların tek satır olduğu durumlarda Map hatasını engellemek için oluşturuldu.
                string fromXml = "";
                if (typeof(T).FullName == typeof(VehicleQueryResponseBase).FullName)
                {
                    var nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
                    //nsmgr.AddNamespace("ns2", "http://ws.naryaz.com/model/dto");
                    //foreach (XmlNode node in xmldoc.GetElementsByTagName("ns2:capacities"))
                    foreach (XmlNode node in xmldoc.SelectNodes("//SIXTTURKEYWEBSERVICES/VEHICLES", nsmgr))
                    {
                        var nodeVehicle = node.SelectNodes("VEHICLE", nsmgr);
                        if (nodeVehicle.Count == 1)
                        {
                            string newNode = "<VEHICLE></VEHICLE>";
                            XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                            node.AppendChild(xmldoc.ReadNode(textReader));
                        }
                    }

                    fromXml = JsonConvert.SerializeXmlNode(xmldoc, Newtonsoft.Json.Formatting.Indented).Replace(",null]", "]");
                }
                else
                {
                    fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                }
                #endregion


                return JsonConvert.DeserializeObject<T>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });

            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerGetXmlAsyncForSixt2Error}", ex.ToJson());
                return default(T);
            }

        }


        public async Task<TRes> PostAsync<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isPlainTextResponse = false, BrokerLogModel brokerLogModel = null, bool isHeaderResponse = false, bool ignoreNull = false, int timeout = 0, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                HttpResponseMessage response = null;

                client.SetHeaders(headers);
                if (timeout != 0)
                    client.Timeout = TimeSpan.FromSeconds(timeout);

                requestPath += parameters.ToQueryString();

                var settings = ignoreNull
                    ? new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                    : null;

                var content = new StringContent(
                    JsonConvert.SerializeObject(entity, settings),
                    System.Text.Encoding.UTF8,
                    "application/json");

                response = await client.PostAsync(requestPath, content);

                var result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                return isPlainTextResponse ? (dynamic)result : JsonConvert.DeserializeObject<TRes>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncError}", ex.ToJson());

                return default(TRes);
            }
        }
        public async Task<HttpResult<TRes>> PostAsyncResult<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool ignoreNull = false, int timeout = 0, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                if (timeout != 0)
                    client.Timeout = TimeSpan.FromSeconds(timeout);

                requestPath += parameters.ToQueryString();

                var settings = ignoreNull
                    ? new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                    : null;

                var content = new StringContent(
                    JsonConvert.SerializeObject(entity, settings),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(requestPath, content);

                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }
        public async Task<TRes> PostAsyncWithUrlEncoded<TReq, TRes>(string requestPath, List<KeyValuePair<string, string>> entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isPlainTextResponse = false, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            string result = "";
            try
            {
                HttpResponseMessage response = null;

                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(entity),
                    System.Text.UTF8Encoding.UTF8,
                    "application/x-www-form-urlencoded");

                response = await client.PostAsync(requestPath, new FormUrlEncodedContent(entity));


                if (isPlainTextResponse)
                {
                    result = await response.Content.ReadAsStringAsync();

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = result;
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    return (dynamic)result;
                }
                else
                {
                    result = await response.Content.ReadAsStringAsync();

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
                    Serilog.Log.Error("{@RestManagerPostAsyncWithUrlEncodedEror}", ex.ToJson());

                return default(TRes);
            }
        }
        public async Task<string> PostAsyncWithHeader<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isPlainTextResponse = false, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
           where TReq : class
           where TRes : class
        {
            try
            {
                HttpResponseMessage response = null;

                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(entity),
                    System.Text.UTF8Encoding.UTF8,
                    "application/json");

                response = await client.PostAsync(requestPath, content);


                if (isPlainTextResponse)
                {
                    var res = await response.Content.ReadAsStringAsync();

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = res;
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    return null;
                }
                else
                {
                    var result = response.Headers;

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = JsonConvert.SerializeObject(result);
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    var rt = result.GetValues("Authorization") as IEnumerable<string>;
                    return rt.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncWithHeaderError}", ex.ToJson());

                return null;
            }
        }

        public async Task<IEnumerable<string>> PostAsyncWithCookie<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, bool isPlainTextResponse = false, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
   where TReq : class
   where TRes : class
        {
            try
            {
                HttpResponseMessage response = null;

                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(entity),
                    System.Text.UTF8Encoding.UTF8,
                    "application/json");

                response = await client.PostAsync(requestPath, content);


                if (isPlainTextResponse)
                {
                    var res = await response.Content.ReadAsStringAsync();

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = res;
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    return null;
                }
                else
                {
                    var result = response.Headers;

                    if (_dbHelper != null && brokerLogModel != null)
                    {
                        brokerLogModel.Content = JsonConvert.SerializeObject(result);
                        _dbHelper.WriteLog(brokerLogModel);
                    }

                    var rt = result.GetValues("Set-Cookie") as IEnumerable<string>;
                    return rt;
                }

            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncWithCookieError}", ex.ToJson());
                return null;
            }
        }

        public async Task<TRes> PostAsyncXMLRestClient<TRes>(string requestPath, ParameterType parameterType = ParameterType.GetOrPost, IDictionary<string, object> entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, string culture = "en-US", List<string> xmlArraysPropNames = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TRes : class
        {
            string responseContent = "";
            try
            {
                RestResponse response = null;

                var restClient = new RestClient($"{client.BaseAddress}{requestPath}");
                var request = new RestRequest("", Method.Post);
                if (headers != null)
                    foreach (var header in headers)
                        request.AddHeader(header.Key, header.Value.ToStringNullSafe());

                request.RequestFormat = DataFormat.Xml;

                foreach (var entityItem in entity)
                    request.AddParameter(entityItem.Key, entityItem.Value.ToStringNullSafe(), parameterType);

                response = await restClient.ExecuteAsync(request);

                var xmldoc = new XmlDocument();
                string content = response.Content.ToStringNullSafe();

                responseContent = content;

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = content;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                if (xmlArraysPropNames != null)
                    foreach (var prop in xmlArraysPropNames)
                        content = content.Replace($"</{prop}>", $"</{prop}><{prop} />");

                xmldoc.LoadXml(content);
                var fromXml = JsonConvert.SerializeXmlNode(xmldoc);

                return JsonConvert.DeserializeObject<TRes>(fromXml, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(culture)
                });
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@PostAsyncXMLRestClientError}", ex.ToJson());

                return null;
            }
        }

        public async Task<TRes> PostAsyncRestClient<TRes>(string requestPath, ParameterType parameterType = ParameterType.GetOrPost, IDictionary<string, object> entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TRes : class
        {
            try
            {
                RestResponse response = null;

                var restClient = new RestClient($"{client.BaseAddress}{requestPath}");
                var request = new RestRequest("", Method.Post);

                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value.ToStringNullSafe());

                foreach (var entityItem in entity)
                    request.AddParameter(entityItem.Key, entityItem.Value.ToStringNullSafe(), parameterType);

                response = await restClient.ExecuteAsync(request);
                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = response.Content;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var values = JsonConvert.DeserializeObject<TRes>(response.Content);


                return values;
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncRestClientError}", ex.ToJson());
                return default(TRes);
            }
        }

        public async Task<TResult> PostFormUrlEncoded<TResult>(string requestPath, IEnumerable<KeyValuePair<string, string>> postData, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isStringResponse = false, bool isReservationRequest = false)
        {
            using (var content = new FormUrlEncodedContent(postData))
            {

                content.SetHeaders(headers);

                client.SetHeaders(headers);
                HttpResponseMessage response = await client.PostAsync(requestPath, content);
                var result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                try
                {
                    if (isStringResponse)
                        return (dynamic)result;

                    return JsonConvert.DeserializeObject<TResult>(result);
                }
                catch (Exception ex)
                {
                    if (isReservationRequest)
                        Serilog.Log.Error("{@RestManagerPostFormUrlEncodedError}", ex.ToJson());

                    return default(TResult);
                }
            }
        }

        public async Task<T> PutAsync<T>(string requestPath, T entity, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.PutAsJsonAsync<T>(requestPath, entity);
                var result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPutAsyncError}", ex.ToJson());

                return default(T);
            }
        }

        public async Task<HttpResult<T>> PutAsyncResult<T>(string requestPath, T entity, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.PutAsJsonAsync(requestPath, entity);

                return await CreateHttpResult<T>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPutAsyncResultError}", ex.ToJson());

                return HttpResult<T>.Catch(ex.Message);
            }
        }

        public async Task<T> DeleteAsync<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {

            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.DeleteAsync(requestPath);
                var result = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeleteAsyncError}", ex.ToJson());

                return default(T);
            }
        }

        public async Task<HttpResult<T>> DeleteAsyncResult<T>(string requestPath, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false) where T : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.DeleteAsync(requestPath);

                return await CreateHttpResult<T>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeleteAsyncResultError}", ex.ToJson());

                return HttpResult<T>.Catch(ex.Message);
            }
        }

        public async Task<TRes> DeleteAsync<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                requestPath += parameters.ToQueryString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(client.BaseAddress.ToString() + requestPath),
                    Content = new StringContent(JsonConvert.SerializeObject(entity))
                };

                var response = await client.SendAsync(request);
                var res = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = res;
                    _dbHelper.WriteLog(brokerLogModel);
                }


                return JsonConvert.DeserializeObject<TRes>(res);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeleteAsync2Error}", ex.ToJson());

                return default(TRes);
            }
        }

        public async Task<TRes> PostAsyncHttpContent<TRes>(string requestPath, HttpContent entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool serilog = false, bool isReservationRequest = false)
         where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.PostAsync(requestPath, entity);
                var result = await response.Content.ReadAsStringAsync();

                if (serilog)
                {
                    Serilog.Log.Error("{@DailydrivePayServiceResult}", result);
                }

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = result;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(result);

                #region Araçların tek satır olduğu durumlarda Map hatasını engellemek için oluşturuldu.
                string fromXml = "";
                if (typeof(TRes).FullName == typeof(DailydriveCapacitiesResponseBase).FullName)
                {
                    var nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
                    nsmgr.AddNamespace("ns2", "http://ws.naryaz.com/model/dto");
                    //foreach (XmlNode node in xmldoc.GetElementsByTagName("ns2:capacities"))
                    foreach (XmlNode node in xmldoc.SelectNodes("//ns2:CapacitiesResponse/ns2:capacities", nsmgr))
                    {
                        var nodeVehicle = node.SelectNodes("ns2:vehicleTypes", nsmgr);
                        if (nodeVehicle.Count == 1)
                        {
                            string newNode = "<ns2:vehicleTypes xmlns:ns2=\"http://ws.naryaz.com/model/dto\"><ns2:typeNo>-1</ns2:typeNo></ns2:vehicleTypes>";
                            XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                            node.AppendChild(xmldoc.ReadNode(textReader));
                        }
                    }

                    fromXml = JsonConvert.SerializeXmlNode(xmldoc, Newtonsoft.Json.Formatting.Indented).Replace(",null]", "]");
                }
                #endregion
                else
                {
                    fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                }

                return JsonConvert.DeserializeObject<TRes>(fromXml, new JsonSerializerSettings
                {

                });
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncHttpContentError}", ex.ToJson());

                return default(TRes);
            }
        }

        public async Task<HttpResult<TRes>> DeleteAsyncResult<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TReq : class
            where TRes : class
        {
            try
            {
                requestPath += parameters.ToQueryString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(client.BaseAddress.ToString() + requestPath),
                    Content = new StringContent(JsonConvert.SerializeObject(entity))
                };

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value.ToString());
                    }
                }

                var response = await client.SendAsync(request);

                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeleteAsyncResult2Error}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }

        public async Task<HttpResult<TRes>> PostAsyncRestClientResult<TRes>(string requestPath, ParameterType parameterType = ParameterType.GetOrPost, IDictionary<string, object> entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
            where TRes : class
        {
            try
            {
                var restClient = new RestClient($"{client.BaseAddress}{requestPath}");
                var request = new RestRequest("", Method.Post);

                if (headers != null)
                {
                    foreach (var header in headers)
                        request.AddHeader(header.Key, header.Value.ToStringNullSafe());
                }

                if (entity != null)
                {
                    foreach (var entityItem in entity)
                        request.AddParameter(entityItem.Key, entityItem.Value.ToStringNullSafe(), parameterType);
                }

                var response = await restClient.ExecuteAsync(request);

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = response.Content;
                    _dbHelper.WriteLog(brokerLogModel);
                }

                var responseMessage = VendorResponseMessageHelper.ExtractMessage(response.Content);
                var isErrorResponse = VendorResponseMessageHelper.IsErrorResponse(response.Content);
                var deserializeSuccess = true;
                var deserializeErrorMessage = string.Empty;
                TRes data = default;

                try
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                        data = JsonConvert.DeserializeObject<TRes>(response.Content);
                }
                catch (Exception ex)
                {
                    deserializeSuccess = false;
                    deserializeErrorMessage = ex.Message;

                    if (isReservationRequest)
                        Serilog.Log.Error("{@RestManagerPostAsyncRestClientDeserializeError}", ex.ToJson());
                }

                var isDefaultMappedResponse = deserializeSuccess && VendorResponseMessageHelper.IsDefaultMappedResponse(response.Content, data);
                var hasVendorErrorResponse = isErrorResponse || isDefaultMappedResponse;
                var statusCode = response.ResponseStatus == ResponseStatus.Completed ? (HttpStatusCode)response.StatusCode : HttpStatusCode.InternalServerError;

                return HttpResult<TRes>.Result(
                    data: data,
                    httpResultType: statusCode,
                    success: statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.MultipleChoices && !hasVendorErrorResponse,
                    message: !string.IsNullOrWhiteSpace(responseMessage) ? responseMessage : response.StatusDescription,
                    rawContent: response.Content,
                    deserializeSuccess: deserializeSuccess,
                    deserializeErrorMessage: deserializeErrorMessage,
                    serviceMessage: !deserializeSuccess || statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.MultipleChoices || hasVendorErrorResponse ? response.Content : null);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncRestClientResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }
        public async Task<HttpResult<TRes>> PostAsyncHttpContentResult<TRes>(string requestPath, HttpContent entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool serilog = false, bool isReservationRequest = false)
         where TRes : class
        {
            try
            {
                client.SetHeaders(headers);
                requestPath += parameters.ToQueryString();

                var response = await client.PostAsync(requestPath, entity);

                return await CreateXmlHttpResult<TRes>(response, brokerLogModel, serilog, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerPostAsyncHttpContentResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
            }
        }
        public async Task<TRes> DeleteAsyncJSON<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
         where TReq : class
         where TRes : class
        {
            try
            {
                requestPath += parameters.ToQueryString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(client.BaseAddress.ToString() + requestPath),
                    Content = new StringContent(
                                        JsonConvert.SerializeObject(entity),
                                        System.Text.Encoding.UTF8,
                                        "application/json"),
                };

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value.ToString());
                    }
                }

                var response = await client.SendAsync(request);
                var res = await response.Content.ReadAsStringAsync();

                if (_dbHelper != null && brokerLogModel != null)
                {
                    brokerLogModel.Content = res;
                    _dbHelper.WriteLog(brokerLogModel);
                }


                return JsonConvert.DeserializeObject<TRes>(res);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeleteAsyncJSONError}", ex.ToJson());

                return default(TRes);
            }
        }
        public async Task<HttpResult<TRes>> DeleteAsyncJSONResult<TReq, TRes>(string requestPath, TReq entity = null, IDictionary<string, object> parameters = null, IDictionary<string, object> headers = null, BrokerLogModel brokerLogModel = null, bool isReservationRequest = false)
         where TReq : class
         where TRes : class
        {
            try
            {
                requestPath += parameters.ToQueryString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(client.BaseAddress.ToString() + requestPath),
                    Content = new StringContent(
                                        JsonConvert.SerializeObject(entity),
                                        System.Text.Encoding.UTF8,
                                        "application/json"),
                };

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value.ToString());
                    }
                }

                var response = await client.SendAsync(request);

                return await CreateHttpResult<TRes>(response, brokerLogModel, isReservationRequest);
            }
            catch (Exception ex)
            {
                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeleteAsyncJSONResultError}", ex.ToJson());

                return HttpResult<TRes>.Catch(ex.Message);
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
                    Serilog.Log.Error("{@RestManagerPostAsyncWithUrlEncodedResultError}", ex.ToJson());

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
                    Serilog.Log.Error("{@RestManagerDeserializeModelError}", ex.ToJson());
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
        private async Task<HttpResult<TRes>> CreateXmlHttpResult<TRes>(HttpResponseMessage response, BrokerLogModel brokerLogModel, bool serilog, bool isReservationRequest) where TRes : class
        {
            var result = await response.Content.ReadAsStringAsync();

            if (serilog)
                Serilog.Log.Error("{@DailydrivePayServiceResult}", result);

            if (_dbHelper != null && brokerLogModel != null)
            {
                brokerLogModel.Content = result;
                _dbHelper.WriteLog(brokerLogModel);
            }

            var responseMessage = VendorResponseMessageHelper.ExtractMessage(result);
            var deserializeSuccess = true;
            var deserializeErrorMessage = string.Empty;
            TRes data = default;

            try
            {
                if (!string.IsNullOrWhiteSpace(result))
                {
                    var xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    string fromXml = string.Empty;
                    if (typeof(TRes).FullName == typeof(DailydriveCapacitiesResponseBase).FullName)
                    {
                        var nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
                        nsmgr.AddNamespace("ns2", "http://ws.naryaz.com/model/dto");
                        foreach (XmlNode node in xmldoc.SelectNodes("//ns2:CapacitiesResponse/ns2:capacities", nsmgr))
                        {
                            var nodeVehicle = node.SelectNodes("ns2:vehicleTypes", nsmgr);
                            if (nodeVehicle.Count == 1)
                            {
                                const string newNode = "<ns2:vehicleTypes xmlns:ns2=\"http://ws.naryaz.com/model/dto\"><ns2:typeNo>-1</ns2:typeNo></ns2:vehicleTypes>";
                                XmlTextReader textReader = new XmlTextReader(new StringReader(newNode));
                                node.AppendChild(xmldoc.ReadNode(textReader));
                            }
                        }

                        fromXml = JsonConvert.SerializeXmlNode(xmldoc, Newtonsoft.Json.Formatting.Indented).Replace(",null]", "]");
                    }
                    else
                    {
                        fromXml = JsonConvert.SerializeXmlNode(xmldoc);
                    }

                    data = JsonConvert.DeserializeObject<TRes>(fromXml, new JsonSerializerSettings());
                }
            }
            catch (Exception ex)
            {
                deserializeSuccess = false;
                deserializeErrorMessage = ex.Message;

                if (isReservationRequest)
                    Serilog.Log.Error("{@RestManagerDeserializeXmlModelError}", ex.ToJson());
            }

            return HttpResult<TRes>.Result(
                data: data,
                httpResultType: response.StatusCode,
                success: response.IsSuccessStatusCode && deserializeSuccess,
                message: !string.IsNullOrWhiteSpace(responseMessage) ? responseMessage : response.ReasonPhrase,
                rawContent: result,
                deserializeSuccess: deserializeSuccess,
                deserializeErrorMessage: deserializeErrorMessage,
                serviceMessage: !deserializeSuccess || !response.IsSuccessStatusCode ? result : null);
        }

    }
}
