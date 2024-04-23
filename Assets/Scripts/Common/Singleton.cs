/// <summary>
/// ·ºÐÍµ¥Àý
/// </summary>
/// <typeparam name="T"></typeparam>

public class Singeton<T> where T : new()
{
    private static T _instance;

    public static T Instance {
        get {
            if (null == _instance)
            {
                _instance = new T();
            }
            return _instance;
        }
    }

    public virtual bool Init()
    {
        return true;
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
    }

    public virtual bool Dispose()
    {
        return true;
    }
}
