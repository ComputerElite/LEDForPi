using ComputerUtils.Logging;
using LEDForPi.Strips;

namespace LEDForPi;

public class StripControllerManager
{
    public double deltaTime = 0.01;
    private DateTime lastUpdate = DateTime.Now;
    public List<IStripController?> controllers = new();
    public Dictionary<string, bool> enabledControllers = new();
    public long currentFrame = 0;

    /// <summary>
    /// Adds a strip controller to the manager. They'll be enabled by default. The strip controller must be initialised already with strip information.
    /// Strip controllers without an ID will be assigned a random ID.
    /// A strip controllers overwrites another one with the same ID.
    /// </summary>
    /// <param name="controller"></param>
    /// <exception cref="Exception"></exception>
    public void AddController(IStripController controller)
    {
        
        if(controller.GetID() == "") controller.SetID(DateTime.UtcNow.Ticks.ToString());
        Destroy(controller.GetID()); // Destroy existing controllers with the same ID
        enabledControllers[controller.GetID()] = true;
        controllers.Add(controller);
        controller.SetStripControllerManager(this);
        controller.OnEnable();
    }

    public void EnableController(string id)
    {
        IStripController s = controllers.FirstOrDefault(c => c.GetID() == id);
        if(s == null) throw new Exception("Controller with ID " + id + " not found!");
        enabledControllers[id] = true;
        s.OnEnable();
    }
    
    public void DisableController(string id)
    {
        IStripController s = controllers.FirstOrDefault(c => c.GetID() == id);
        if(s == null) throw new Exception("Controller with ID " + id + " not found!");
        enabledControllers[id] = false;
        s.OnDisable();
    }

    public void UpdateControllers()
    {
        DateTime now = DateTime.Now;
        deltaTime = (now - lastUpdate).TotalSeconds;
        lastUpdate = now;
        currentFrame++;
        try
        {
            List<IStrip> strips = new List<IStrip>();
            for(int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i] == null)
                {
                    controllers.RemoveAt(i);
                    i--;
                    continue;
                }
                if (enabledControllers.ContainsKey(controllers[i].GetID()) &&
                    enabledControllers[controllers[i].GetID()])
                {
                    strips.AddRange(controllers[i].GetStrips());
                    controllers[i].Update(); // Controller may remove itself here
                }
            }

            for (int i = 0; i < strips.Count; i++)
            {
                strips[i].RenderOncePerFrame(currentFrame);
            }
        } catch(Exception e)
        {
            Logger.Log("Error during frame update in StripControllerManager.UpdateControllers: " + e);
        }
    }

    public void Destroy(string id)
    {
        controllers.RemoveAll(x => x.GetID() == id);
        enabledControllers.Remove(id);
    }
    
    public void Destroy(IStripController target)
    {
        Destroy(target.GetID());
    }

    public void StartUpdateThread()
    {
        Thread t = new Thread(() =>
        {
            while (true)
            {
                UpdateControllers();
            }
        });
        t.Start();
    }

    public void JustMe(IStripController rainbowController)
    {
        foreach (IStripController strip in controllers)
        {
            if(strip.GetID() != rainbowController.GetID()) DisableController(strip.GetID());
        }
    }
}