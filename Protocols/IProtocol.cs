public interface IProtocol<T> where T: IProtocol<T>
{
    static abstract byte[] Serialize(T t);

    static abstract T Deserialize(byte[] bytes);
}
    
