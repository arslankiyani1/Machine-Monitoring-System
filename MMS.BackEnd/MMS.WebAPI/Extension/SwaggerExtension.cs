namespace MMS.WebAPI.Extension;

public static class SwaggerExtension
{
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            #region Swagger
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MMS App Web APIs..",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "MMS Team",
                    Email = ""
                }
            });

            //Show enums as strings in Swagger
            c.UseInlineDefinitionsForEnums();
            c.SchemaFilter<EnumSchemaFilter>();


            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token in the text input Example: Bearer ey11123231236546545",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });
            #endregion
        });
    }
}