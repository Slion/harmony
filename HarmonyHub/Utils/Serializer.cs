using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace HarmonyHub.Utils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Serializer
    {
        /// <summary>
        /// Internalize the given JSON string into the specified data contract object.  
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        static public T Internalize<T>(string aData) where T : class
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(aData);
            MemoryStream stream = new MemoryStream(byteArray);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            return (T)ser.ReadObject(stream);
        }

        /// <summary>
        /// Externalize the specified data contract object into a JSON string.
        /// </summary>
        /// <param name="aObject"></param>
        /// <returns></returns>
        static public string Externalize<T>(T aObject) where T : class
        {
            //Save settings into JSON string
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            ser.WriteObject(stream, aObject);
            // convert stream to string
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }

    }
}