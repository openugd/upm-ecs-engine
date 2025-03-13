namespace OpenUGD.ECS.Engine.Utils
{
    public class DeepClone
    {
        public static T Clone<T>(T obj)
        {
            var serialized = Serializer.Serialize(obj);
            return Serializer.Deserialize<T>(serialized);
        }
    }
}