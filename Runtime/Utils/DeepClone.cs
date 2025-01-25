using Newtonsoft.Json;

namespace OpenUGD.ECS.Engine.Utils
{
    public class DeepClone
    {
        public static T Clone<T>(T obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }
    }
}