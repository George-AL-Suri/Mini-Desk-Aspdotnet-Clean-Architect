using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MiniDesk.Api.OpenApi
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, 
            OpenApiDocumentTransformerContext context, 
            CancellationToken cancellationToken)
        {
            var schemes = await schemeProvider.GetAllSchemesAsync();
            if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
                return;

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT"
                }
            };
           
        }
    }
}
