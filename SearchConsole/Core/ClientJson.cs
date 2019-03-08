using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SearchConsole.Core
{
    class ClientJson
    {
        public string JsonPath { get; private set; }
        public string ClientId { get; private set; }
        public string ProjectId { get; private set; }
        public string AuthUri { get; private set; }
        public string TokenUri { get; private set; }
        public string AuthProviderX509CertUrl { get; private set; }
        public string ClientSecret { get; private set; }
        public List<string> RedirectUris { get; private set; }


        public ClientJson(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public ClientJson(string jsonPath)
        {
            JsonPath = jsonPath;
            LoadJson();
        }

        private void LoadJson()
        {
            try
            {
                if (JsonPath == null)
                    throw new ArgumentNullException(JsonPath);

                ClientIdObject Item;

                using (StreamReader r = new StreamReader(JsonPath))
                {
                    string json = r.ReadToEnd();
                    Item = JsonConvert.DeserializeObject<ClientIdObject>(json);
                }

                ClientId = Item.Installed.Client_id;
                ProjectId = Item.Installed.Project_id;
                AuthUri = Item.Installed.Project_id;
                TokenUri = Item.Installed.Project_id;
                AuthProviderX509CertUrl = Item.Installed.Project_id;
                ClientSecret = Item.Installed.Client_secret;
                RedirectUris = Item.Installed.Redirect_uris;
            }
            catch (Exception ex)
            {
                throw new Exception("Json loading failed.", ex);
            }
        }

        private class Installed
        {
            public string Client_id { get; set; }
            public string Project_id { get; set; }
            public string Auth_uri { get; set; }
            public string Token_uri { get; set; }
            public string Auth_provider_x509_cert_url { get; set; }
            public string Client_secret { get; set; }
            public List<string> Redirect_uris { get; set; }
        }

        private class ClientIdObject
        {
            public Installed Installed { get; set; }
        }
    }

}
