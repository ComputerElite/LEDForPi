namespace LEDForPi;

public interface IStripController
{
    /// <summary>
    /// Will be called every frame update where the controller is enabled. Use this to update the LED strip.
    /// </summary>
    public void Update();
    /// <summary>
    /// Gets called when the controller is enabled.
    /// </summary>
    public void OnEnable();
    /// <summary>
    /// Gets called when the controller is destroyed.
    /// </summary>
    public void OnDestroy();
    /// <summary>
    /// Gets called when the controller is disabled.
    /// </summary>
    public void OnDisable();
    /// <summary>
    /// Gets called when the controller is added to the manager.
    /// </summary>
    public void SetStripControllerManager(StripControllerManager manager);
    
    public string GetID();
    public void SetID(string id);
}