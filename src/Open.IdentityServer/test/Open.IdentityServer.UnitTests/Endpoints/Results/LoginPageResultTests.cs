using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;

namespace Open.IdentityServer.UnitTests.Endpoints.Results;

public class LoginPageResultTests
{
    private ValidatedAuthorizeRequest testAuthorizeRequest = new()
    {
        
    };
    private IdentityServerOptions _options = new IdentityServerOptions();
    private DefaultHttpContext _context = new DefaultHttpContext();
    
    // private IAuthorizationParametersMessageStore _authorizationParametersMessageStore = Mock 
    
    public LoginPageResultTests()
    {
        
    }

    private LoginPageResult CreateSut() => new LoginPageResult(testAuthorizeRequest);
}