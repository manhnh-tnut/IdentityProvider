namespace SSO.Application.Features.Authorization.Responses
{
    public class AuthorizeResponseModel
    {
        public AuthorizeResponseModel(string applicationName, string scopes)
        {
            ApplicationName = applicationName;
            Scopes = scopes;
        }

        public string ApplicationName { get; }
        public string Scopes { get; }
    }
}
