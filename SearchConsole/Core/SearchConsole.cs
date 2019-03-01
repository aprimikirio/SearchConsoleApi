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

        public List<QuerryResponce> RequestForSPC(string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForSPC(QueryDate, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForSPC(DateTime date, string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            return RequestFor(new List<string>() { "query", "page", "country", "device" }, date, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForSearchs(string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForSearchs(QueryDate, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForSearchs(DateTime date, string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            return RequestFor(new List<string>() { "query" }, date, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForPages(string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForPages(QueryDate, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForPages(DateTime date, string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            return RequestFor(new List<string>() { "page" }, date, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForСountries(string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForСountries(QueryDate, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForСountries(DateTime date, string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            return RequestFor(new List<string>() { "country" }, date, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForDevices(string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            QueryDate = DateTime.Now.AddDays(DaysDelay);
            return RequestForDevices(QueryDate, country, rowStart, rowLimit);
        }

        public List<QuerryResponce> RequestForDevices(DateTime date, string country = null, int rowStart = 0, int rowLimit = RowLimitConst)
        {
            return RequestFor(new List<string>() { "device" }, date, country, rowStart, rowLimit);
        }

        private List<QuerryResponce> RequestFor(List<string> dimensions, DateTime date, string country, int rowStart, int rowLimit = RowLimitConst)
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
                    Dimensions = dimensions, //"page" "query" "country" "device" "date"

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
                    Dimensions = dimensions //"page" "query" "country" "device" "date"
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
