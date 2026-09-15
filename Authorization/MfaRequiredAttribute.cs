using Microsoft.AspNetCore.Authorization;

namespace ExternalIdDemo.Api.Authorization;

public class MfaRequiredAttribute : AuthorizeAttribute
{
    public MfaRequiredAttribute()
    {
        Policy = "MfaRequired";
    }
}