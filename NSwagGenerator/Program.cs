using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using YamlDotNet.Serialization;

namespace NSwagGenerator
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var tsk = Run();
            tsk.Wait();
        }

        public static async Task Run()
        {

            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(new StreamReader("/Users/lmlynik/alps-api/src/main/resources/alps-core.yaml"));

            var serializer = new Serializer(options: SerializationOptions.JsonCompatible);
            var sw = new StringWriter();
            serializer.Serialize(sw, yamlObject);
            var jobject = JObject.Parse(sw.ToString());
            AdjustTypes(jobject);
            System.IO.File.WriteAllText(@"swagger.json", jobject.ToString());

            var document = await SwaggerDocument.FromJsonAsync(jobject.ToString());


            var settings = new SwaggerToCSharpClientGeneratorSettings
            {
                ClientBaseClass = "BaseClient",
                UseHttpRequestMessageCreationMethod = true,
                InjectHttpClient = true,
                ClassName = "ApiClient",
                CSharpGeneratorSettings =
    {
                    Namespace = "Matchmore.SDK",
                    SchemaType = NJsonSchema.SchemaType.OpenApi3,
                    GenerateDefaultValues = false,
                    GenerateDataAnnotations = false,
                    GenerateJsonMethods = false,
                    ClassStyle = NJsonSchema.CodeGeneration.CSharp.CSharpClassStyle.Poco
    },
            };

            var generator = new SwaggerToCSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            System.IO.File.WriteAllText(@"/Users/lmlynik/Projects/Matchmore/SDK/Client.cs", code);
        }

        private static void AdjustTypes(JObject jobject)
        {
            foreach (var item in jobject)
            {
                if (item.Value.Type == JTokenType.String)
                {
                    JToken f = NewMethod(item.Value);
                    if (f != null)
                        item.Value.Replace(f);
                }
                if (item.Value.Type == JTokenType.Object)
                {
                    AdjustTypes((JObject)item.Value);
                }

                if (item.Value.Type == JTokenType.Array)
                {
                    foreach (var element in (JArray)item.Value)
                    {
                        var adj = NewMethod(element);
                        if (adj != null)
                            element.Replace(adj);
                    }

                }

            }
        }

        private static JToken NewMethod(JToken item)
        {
            switch (item.Type)
            {
                case JTokenType.String:
                    var str = item.Value<string>();
                    if (str == "true")
                    {
                        return JValue.FromObject(true);
                    }
                    if (str == "false")
                    {
                        return JValue.FromObject(false);
                    }
                    float f;
                    if (float.TryParse(str, out f))
                    {
                        return JValue.FromObject(f);
                    }

                    return null;
                case JTokenType.Object:
                    AdjustTypes(item.Value<JObject>());
                    return null;
                default:
                    return null;
            }
        }
    }
}


