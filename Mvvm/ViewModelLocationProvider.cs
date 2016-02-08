using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Mvvm
{
    /// <summary>
    ///     The ViewModelLocationProvider class locates the view model for the view that has the AutoWireViewModelChanged
    ///     attached property set to true.
    ///     The view model will be located and injected into the view's DataContext. To locate the view, two strategies are
    ///     used: First the ViewModelLocationProvider
    ///     will look to see if there is a view model factory registered for that view, if not it will try to infer the view
    ///     model using a convention based approach.
    ///     This class also provide methods for registering the view model factories,
    ///     and also to override the default view model factory and the default view type to view model type resolver.
    /// </summary>
    public static class ViewModelLocationProvider
    {
        /// <summary>
        ///     A dictionary that contains all the registered factories for the views.
        /// </summary>
        private static readonly Dictionary<string, Func<object>> Factories = new Dictionary<string, Func<object>>();

        /// <summary>
        ///     The default view model factory.
        /// </summary>
        private static Func<Type, object> _defaultViewModelFactory = type => Activator.CreateInstance(type);

        /// <summary>
        ///     Default view type to view model type resolver, assumes the view model is in same assembly as the view type, but in
        ///     the "ViewModels" namespace.
        /// </summary>
        private static Func<Type, Type> _defaultViewTypeToViewModelTypeResolver =
            viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName,
                    viewAssemblyName);
                return Type.GetType(viewModelName);
            };

        /// <summary>
        ///     Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory.</param>
        public static void SetDefaultViewModelFactory(Func<Type, object> viewModelFactory)
        {
            _defaultViewModelFactory = viewModelFactory;
        }

        /// <summary>
        ///     Sets the default view type to view model type resolver.
        /// </summary>
        /// <param name="viewTypeToViewModelTypeResolver">The view type to view model type resolver.</param>
        public static void SetDefaultViewTypeToViewModelTypeResolver(Func<Type, Type> viewTypeToViewModelTypeResolver)
        {
            _defaultViewTypeToViewModelTypeResolver = viewTypeToViewModelTypeResolver;
        }

        /// <summary>
        ///     Automatically looks up the viewmodel that corresponds to the current view, using two strategies:
        ///     It first looks to see if there is a mapping registered for that view, if not it will fallback to the convention
        ///     based approach.
        /// </summary>
        /// <param name="view">The dependency object, typically a view.</param>
        public static void AutoWireViewModelChanged(IView view)
        {
            // Try mappings first
            var viewModel = GetViewModelForView(view);
            // Fallback to convention based
            if (viewModel == null)
            {
                var viewModelType = _defaultViewTypeToViewModelTypeResolver(view.GetType());
                if (viewModelType == null) return;

                // Really need Container or Factories here to deal with injecting dependencies on construction
                viewModel = _defaultViewModelFactory(viewModelType);
            }

            view.DataContext = viewModel;
        }

        /// <summary>
        ///     Gets the view model for the specified view.
        /// </summary>
        /// <param name="view">The view that the view model wants.</param>
        /// <returns>The vie wmodel that corresponds to the view passed as a parameter.</returns>
        private static object GetViewModelForView(IView view)
        {
            // Mapping of view models base on view type (or instance) goes here
            if (Factories.ContainsKey(view.GetType().ToString()))
                return Factories[view.GetType().ToString()]();
            return null;
        }

        /// <summary>
        ///     Registers the view model factory for the specified view type name.
        /// </summary>
        /// <param name="viewTypeName">The name of the view type.</param>
        /// <param name="factory">The viewmodel factory.</param>
        public static void Register(string viewTypeName, Func<object> factory)
        {
            Factories[viewTypeName] = factory;
        }
    }
}