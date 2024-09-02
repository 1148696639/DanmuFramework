public class ClassHelper
{
    /// <summary>
    ///     定义一个方法来复制属性
    /// </summary>
    /// <param name="source">被复制的对象</param>
    /// <param name="destination">目标对象</param>
    public static void CopyProperties(object source, object destination)
    {
        if (source.GetType() != destination.GetType())
        {
            DebugCtrl.LogError("无法复制不同的类");
        }

        var type = source.GetType();
        var properties = type.GetProperties();
 
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(source);
                property.SetValue(destination, value);
            }
        }
        
    }
}