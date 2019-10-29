using ImageDebugger.Core.IoC.Interface;
using Ninject;


namespace ImageDebugger.Core.IoC
{
    /// <summary>
    /// The IoC container for our application
    /// </summary>
    public static class IoC
    {
        #region Public Properties

        /// <summary>
        /// The kernel for our IoC container
        /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();
        

        #endregion

        #region Construction

        /// <summary>
        /// Sets up the IoC container, binds all information required and is ready for use
        /// NOTE: Must be called as soon as your application starts up to ensure all 
        ///       services can be found
        /// </summary>
        public static void Setup()
        {
            // Bind all required view models
            BindViewModels();
        }

        /// <summary>
        /// Binds all singleton view models
        /// </summary>
        private static void BindViewModels()
        {
            //TODO: Bind to a single instance of Application view model
           
        }

        #endregion

        /// <summary>
        /// Get a service from the IoC, of the specified type
        /// </summary>
        /// <typeparam name="T">The type to get</typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
        
        /// <summary>
        /// Register an concrete service object as singleton
        /// Shortcut for kernel.bind().ToConstant()
        /// </summary>
        /// <param name="implementObject"></param>
        /// <typeparam name="T">Base type of service</typeparam> 
        /// <typeparam name="TChild">Concrete type of service</typeparam>
        public static void RegisterAsSingleton<T, TChild>() where TChild : T, new()
        {
            Kernel.Bind<T>().ToConstant(value: new TChild());
        }

        public static string SerializationDirectory => Get<ISerializationManager>().SerializationBaseDir;

    }
}