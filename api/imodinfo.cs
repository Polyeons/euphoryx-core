namespace tech.polyeons.euphoryx.api
{
    public interface IModInfo
    {
        string ModID { get; }
        string Name { get; }
        string Version { get; }
        string[] Dependencies { get; }
        
        void OnLoad();
        void OnEnable();
        void OnDisable();
    }
}