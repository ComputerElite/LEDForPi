using ComputerUtils.Logging;

namespace LEDForPi;

public class StripControllerManager
{
    public List<IStripController> controllers = new();
    public Dictionary<string, bool> enabledControllers = new();

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
        controller.OnEnable();
        controller.SetStripControllerManager(this);
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
        try
        {
            for(int i = 0; i < controllers.Count; i++)
            {
                if(enabledControllers.ContainsKey(controllers[i].GetID()) && enabledControllers[controllers[i].GetID()])
                    controllers[i].Update();
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
}