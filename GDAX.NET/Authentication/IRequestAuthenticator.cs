namespace GDAX.NET
{
    public interface IRequestAuthenticator
    {
        AuthenticationToken GetAuthenticationToken(ApiRequest request);
    }
}
