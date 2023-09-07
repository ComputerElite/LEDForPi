using ComputerUtils.Logging;

namespace LEDForPi;

public class BasicStripController : IStripController
{
    private bool enabled { get; set; } = true;
    public StripControllerManager manager { get; set; }
    private string id { get; set; } = "";
    public void Update()
    {
        Logger.Log("BasicStripController Update");
    }

    public void OnEnable()
    {
        enabled = true;
    }

    public void OnDestroy()
    {
        enabled = false;
    }

    public void OnDisable()
    {
        enabled = false;
    }

    public void SetStripControllerManager(StripControllerManager manager)
    {
        this.manager = manager;
    }

    public string GetID()
    {
        return id;
    }

    public void SetID(string id)
    {
        this.id = id;
    }
}