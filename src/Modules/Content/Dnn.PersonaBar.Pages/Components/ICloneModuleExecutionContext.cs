
namespace Dnn.PersonaBar.Pages.Components
{
    /// <summary>
    /// Provides an abstraction over the current context of the Clone Module thread
    /// </summary>
    public interface ICloneModuleExecutionContext
    {
        /// <summary>
        /// Sets whether the import/export process of the Visualizer module is executed
        /// as part of the process of cloning a module or not. 
        /// F.i: when creating a page from a template, duplicating, etc.
        /// </summary>
        void SetCloneModuleContext(bool cloneModule);
    }
}
