using ComputerUtils.Logging;
using LEDForPi.Strips;

namespace LEDForPi;

public class BasicStripController : IStripController
{
    private bool enabled { get; set; } = true;
    public StripControllerManager manager { get; set; }
    private string id { get; set; } = "";
    public List<IStrip> GetStrips()
    {
        throw new NotImplementedException();
    }

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

    public IStripController SetID(string id)
    {
        this.id = id;
        return this;
    }
}