using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IeltsTestWeb.Utils
{
    public class TimeOnlySchemaFilter: ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(TimeOnly))
            {
                schema.Type = "string";
                schema.Format = "HH:mm:ss";
                schema.Example = new OpenApiString(DateTime.Now.ToString("HH:mm:ss"));
            }

            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    if (property.Value.Reference == null && property.Value.Format == "time" && property.Value.Type == "string")
                    {
                        property.Value.Example = new OpenApiString(DateTime.Now.ToString("HH:mm:ss"));
                    }
                }
            }
        }
    }
}
