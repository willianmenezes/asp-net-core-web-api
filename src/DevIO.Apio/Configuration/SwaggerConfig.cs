using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace DevIO.Apio.Configuration
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            //configurando o swagger
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerDefaultValues>();
            });

            return services;
        }

        public static IApplicationBuilder UserSwaggerConfig(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description  in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            return app;
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated = apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                //if (parameter.Default == null)
                //{
                //    parameter.Default = description.DefaultValue;
                //}

                parameter.Required |= description.IsRequired;
            }
        }
    }
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private const string openSourceLicenceURl = "https://opensource.org.licences.MIT";
        readonly IApiVersionDescriptionProvider provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            this.provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "API - Desenvolvedor",
                Version = description.ApiVersion.ToString(),
                Description = "Esta api faz parte do curso de REST do desenvolvedor.io",
                Contact = new OpenApiContact() { Name = "Willian menezes", Email = "willian_menezes_santos@hotmail.com" },
                TermsOfService = new Uri(openSourceLicenceURl),
                License = new OpenApiLicense() { Name = "MIT", Url = new Uri(openSourceLicenceURl) }
            };

            if (description.IsDeprecated)
            {
                info.Description += " Esta versão esta obsoleta";
            }

            return info;
        }
    }
}
