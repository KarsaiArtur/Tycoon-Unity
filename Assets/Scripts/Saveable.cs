

public interface Saveable
{
    public string DataToJson();
    public void FromJson(string json);
    public string GetFileName();
}
