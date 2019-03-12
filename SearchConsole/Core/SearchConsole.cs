using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Webmasters.v3;
using Google.Apis.Webmasters.v3.Data;

namespace SearchConsole.Core
{
    public class SearchConsole
    {
        public string UserName { private set; get; }
        public string DataStoreName { private set; get; }
        public string AppName { private set; get; }
        public string SiteUrl { set; get; }
        private UserCredential Credential;
        private WebmastersService Service;
        private SearchAnalyticsQueryRequest Request;

        private string queryDate;
        public DateTime QueryDate
        {
            get
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                return DateTime.ParseExact(this.queryDate, "yyyy-MM-dd", provider);
            }
            set
            {
                this.queryDate = value.ToString("yyyy-MM-dd");
            }
        }

        private const int RowLimitConst = 1000;
        private const int DaysDelay = -4;

        private int queryRowLimit;
        public int QueryRowLimit
        {
            get
            {
                return queryRowLimit;
            }
            set
            {
                if (value < 0)
                    this.queryRowLimit = 1;
                else if (value > RowLimitConst)
                    this.queryRowLimit = RowLimitConst;
                else
                    this.queryRowLimit = value;
            }
        }

        private ClientJson Json;

        /// <summary>
        /// Creates a new SearchConsole
        /// </summary>
        /// <param name="clientId">Client Id. Get it from https://console.developers.google.com/apis/credentials </param>  
        /// <param name="clientSecret">Client Secret. Get from https://console.developers.google.com/apis/credentials </param>  
        /// <param name="userName">User name for credential</param>
        /// <param name="dataStoreName">Name for credential`s File Data Store</param>  
        /// <param name="appName">Name for WebmastersService`s File Data Store</param>  
        /// <param name="siteUrl">URL to the site to be explored</param>  
        public SearchConsole(string clientId, string clientSecret, string userName, string dataStoreName, string appName, string siteUrl)
        {
            Json = new ClientJson(clientId, clientSecret);
            UserName = userName;
            DataStoreName = dataStoreName;
            AppName = appName;
            SiteUrl = siteUrl;
            Request = new SearchAnalyticsQueryRequest();
            InitGoogleServices().Wait();
        }

        /// <summary>
        /// Creates a new SearchConsole
        /// </summary>
        /// <param name="jsonPath">Path to client_id.json file. Get it from https://console.developers.google.com/apis/credentials </param>  
        /// <param name="userName">User name for credential</param>
        /// <param name="dataStoreName">Name for credential`s File Data Store</param>  
        /// <param name="appName">Name for WebmastersService`s File Data Store</param>  
        /// <param name="siteUrl">URL to the site to be explored</param>  
        public SearchConsole(string jsonPath, string userName, string dataStoreName, string appName, string siteUrl)
        {
            Json = new ClientJson(jsonPath);
            UserName = userName;
            DataStoreName = dataStoreName;
            AppName = appName;
            SiteUrl = siteUrl;
            Request = new SearchAnalyticsQueryRequest();
            InitGoogleServices().Wait();
        }

        /// <summary>
        /// Creates new UserCredential and WebmastersService
        /// </summary>
        private async Task InitGoogleServices()
        {
            string[] scopes = new string[] { WebmastersService.Scope.WebmastersReadonly };

            Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = Json.ClientId,
                ClientSecret = Json.ClientSecret
            }, scopes, UserName, CancellationToken.None, new FileDataStore(DataStoreName)).Result;

            Service = new WebmastersService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = AppName
            });
        }

        public List<QuerryResponce> RequestForSPC(int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForSPC(QueryDate, rowStart, rowLimit, country);
        }

        public List<QuerryResponce> RequestForSPC(DateTime date, int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            return RequestFor(new List<string>() { "query", "page", "country", "device" }, date, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about queries for current url for nearest available day
        /// </summary>
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForSearchs(int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForSearchs(QueryDate, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about queries for current url for specific day
        /// </summary>
        /// <param name="date">Set specific date</param>  
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForSearchs(DateTime date, int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            return RequestFor(new List<string>() { "query" }, date, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about pages for current url for nearest available day
        /// </summary>
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForPages(int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForPages(QueryDate, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about pages for current url for specific day
        /// </summary>
        /// <param name="date">Set specific date</param>  
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForPages(DateTime date, int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            return RequestFor(new List<string>() { "page" }, date, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about countries for current url for nearest available day
        /// </summary>
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForСountries(int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForСountries(QueryDate, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about countries for current url for specific day
        /// </summary>
        /// <param name="date">Set specific date</param>  
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForСountries(DateTime date, int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            return RequestFor(new List<string>() { "country" }, date, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about devices for current url for nearest available day
        /// </summary>
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForDevices(int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForDevices(QueryDate, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with information about devices for current url for specific day
        /// </summary>
        /// <param name="date">Set specific date</param>  
        /// <param name="rowStart">Set index of first item. Default is zero</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        public List<QuerryResponce> RequestForDevices(DateTime date, int rowStart = 0, int rowLimit = RowLimitConst, string country = null)
        {
            return RequestFor(new List<string>() { "device" }, date, rowStart, rowLimit, country);
        }

        /// <summary>
        /// Return list of QuerryResponce with all information, that available
        /// </summary>
        /// <param name="dimensions">Set dimensions. Must be a combination of page, query, country, device, and date</param>
        /// <param name="date">Set specific date</param>   
        /// <param name="rowStart">Set index of first item</param>  
        /// <param name="rowLimit">Set number of items. Must be no more than 1000</param>
        /// <param name="country">Set specific country from which the request was made</param>
        private List<QuerryResponce> RequestFor(List<string> dimensions, DateTime date, int rowStart, int rowLimit = RowLimitConst, string country = null)
        {
            QueryDate = date;
            QueryRowLimit = rowLimit;
            var dimensionFilter = new ApiDimensionFilter();

            if (country != null)
            {
                dimensionFilter.Dimension = "country";
                dimensionFilter.Expression = country;
                Request = new SearchAnalyticsQueryRequest
                {
                    StartRow = rowStart,
                    StartDate = queryDate,
                    EndDate = queryDate,
                    RowLimit = queryRowLimit,
                    Dimensions = dimensions,

                    DimensionFilterGroups = new List<ApiDimensionFilterGroup>()
                {
                    new ApiDimensionFilterGroup
                    { Filters = new List<ApiDimensionFilter>(){ dimensionFilter } }
                }
                };
            }
            else
                Request = new SearchAnalyticsQueryRequest
                {
                    StartRow = rowStart,
                    StartDate = queryDate,
                    EndDate = queryDate,
                    RowLimit = queryRowLimit,
                    Dimensions = dimensions
                };

            SearchAnalyticsQueryResponse myQueryResponse = new SearchAnalyticsQueryResponse();
            myQueryResponse = Query(Request);

            List<QuerryResponce> responceRows = new List<QuerryResponce>();

            if (myQueryResponse != null)
                foreach (var row in myQueryResponse.Rows)
                {
                    string text = row.Keys[0].ToString();
                    string url;
                    try { url = row.Keys[1].ToString(); } catch { url = ""; }
                    string cntry;
                    try { cntry = row.Keys[2].ToString(); } catch { cntry = ""; }
                    string dvc;
                    try { dvc = row.Keys[3].ToString(); } catch { dvc = ""; }


                    responceRows.Add(
                        new QuerryResponce(text, url, cntry, dvc, Convert.ToInt32(row.Clicks), Convert.ToInt32(row.Impressions), (double)row.Ctr, (double)row.Position, QueryDate));

                }

            return responceRows;
        }

        private SearchAnalyticsQueryResponse Query(SearchAnalyticsQueryRequest body)
        {
            try
            {
                // Initial validation.
                if (Service == null)
                    throw new ArgumentNullException("service");
                if (body == null)
                    throw new ArgumentNullException("body");
                if (SiteUrl == null)
                    throw new ArgumentNullException("siteUrl");

                // Make the request.
                return Service.Searchanalytics.Query(body, SiteUrl).Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Searchanalytics.Query failed.", ex);
            }
        }
    }

    public class QuerryResponce
    {
        public string Text { private set; get; }
        public string URL { private set; get; }
        public string Country { private set; get; }
        public string Device { private set; get; }
        public int Clicks { private set; get; }
        public int Impressions { private set; get; }
        public double CTR { private set; get; }
        public double Position { private set; get; }
        public DateTime Date { private set; get; }

        public QuerryResponce(string text, string url, string country, string device, int clicks, int impressions, double ctr, double position, DateTime date)
        {
            Text = text;
            URL = url;
            Country = country;
            Device = device;
            Clicks = clicks;
            Impressions = impressions;
            CTR = ctr;
            Position = position;
            Date = date;
        }
    }

}
