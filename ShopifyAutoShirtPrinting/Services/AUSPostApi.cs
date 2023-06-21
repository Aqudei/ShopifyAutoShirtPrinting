using RestSharp;
using RestSharp.Authenticators;

namespace ShopifyEasyShirtPrinting.Services
{
    public class AUSPostService
    {
        //public class MyAuthenticator : AuthenticatorBase
        //{
        //    public MyAuthenticator(string token) : base(token)
        //    {
        //    }

        //    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        //    {
        //        return new HeaderParameter("AUTH-KEY", Token);
        //    }
        //}

        private readonly RestClient _client;
        private const string AccountNumber = "0007798399";


        private const string BaseUrl = "https://digitalapi.auspost.com.au/shipping/v1";

        public AUSPostService()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new HttpBasicAuthenticator("4aa06db0-9bbf-49b5-b553-e9ae7cd21185", "x9a210d09f76bd25e2a0")
            };

            _client = new RestClient(options, new ConfigureHeaders(headers =>
            {
                headers.Add("Account-Number", AccountNumber);
            }));
        }


        public void CreateLabel()
        {
            var request = new RestRequest($"/labels");
            var response = _client.Get(request);

        }
    }
}
