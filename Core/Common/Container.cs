using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ampersand.Core.Common
{
    public class Container : IContainer
    {
        private static readonly Lazy<Dictionary<string, Func<object>>> _lazyServices =
            new Lazy<Dictionary<string, Func<object>>>(() => new Dictionary<string, Func<object>>());

        private static readonly Lazy<Dictionary<Type, string>> _lazyServiceNames =
            new Lazy<Dictionary<Type, string>>(() => new Dictionary<Type, string>());

        protected virtual Dictionary<string, Func<object>> Services
        {
            get
            {
                return _lazyServices.Value;
            }
        }

        protected virtual Dictionary<Type, string> ServiceNames
        {
            get
            {
                return _lazyServiceNames.Value;
            }
        }



        /// <summary>
        /// Registra un nuevo ítem por tipo
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <typeparam name="C">El tipo de instancia que se devolverá</typeparam>
        /// <returns></returns>
        public DependencyManager Register<S, C>() where C : S
        {
            return Register<S, C>(Guid.NewGuid().ToString());
        }
        /// <summary>
        /// Registra un nuevo ítem por nombre
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <typeparam name="C">El tipo de instancia que se devolverá</typeparam>
        /// <returns></returns>
        public DependencyManager Register<S, C>(string name) where C : S
        {
            if (!ServiceNames.ContainsKey(typeof(S)))
            {
                ServiceNames[typeof(S)] = name;
            }
            return new DependencyManager(this, name, typeof(C));
        }
        /// <summary>
        /// Registra un nuevo ítem por instancia
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <param name="instancia">La instancia que se devolverá</param>
        /// <returns></returns>
        public DependencyManager Register<S>(S instancia) where S : class
        {
            var name = Guid.NewGuid().ToString();
            if (!ServiceNames.ContainsKey(typeof(S)))
            {
                ServiceNames[typeof(S)] = name;
            }
            return new DependencyManager(this, name, instancia);
        }
        /// <summary>
        /// Registra un nuevo ítem por instancia y por nombre
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <param name="instancia">La instancia que se devolverá</param>
        /// <returns></returns>
        public DependencyManager Register<S>(S instancia, string name) where S : class
        {
            if (!ServiceNames.ContainsKey(typeof(S)))
            {
                ServiceNames[typeof(S)] = name;
            }
            return new DependencyManager(this, name, instancia);
        }

        /// <summary>
        /// Devuelve una instancia de T que se haya registrado con "name"
        /// </summary>
        /// <typeparam name="T">El tipo a resolver</typeparam>
        /// <returns></returns>
        public T Resolve<T>(string name) where T : class
        {
            return (T)Services[name]();
        }
        /// <summary>
        /// Devuelve una instancia de T
        /// </summary>
        /// <typeparam name="T">El tipo a resolver</typeparam>
        public T Resolve<T>() where T : class
        {
            try
            {
                var name = ServiceNames[typeof(T)];
                var result = Resolve<T>(name);
                return result;
            }
            catch (KeyNotFoundException ex)
            {
                var type = typeof(T).ToString();
                throw new Exception(@"No fue registrado en el container el tipo """ + type + @""": " + ex.Message);
            }
        }

        public class DependencyManager
        {
            private readonly Container container;
            private readonly Dictionary<string, Func<object>> args;
            private readonly string name;

            internal DependencyManager(Container container, string name, Type type)
            {
                try
                {
                    this.container = container;
                    this.name = name;

                    /* No es apropiado mas de un constructor cuando se utiliza dependency injection:
                     * http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=97
                     */
                    ConstructorInfo c = type.GetConstructors().First();
                    args = c.GetParameters()
                        .ToDictionary<ParameterInfo, string, Func<object>>(
                        x => x.Name,
                        x => (() => container.Services[container.ServiceNames[x.ParameterType]]())
                        );

                    container.Services[name] = () => c.Invoke(args.Values.Select(x => x()).ToArray());
                }
                catch (Exception e)
                {
                    //LogFile.WriteLog(ELogLevel.ERROR, "DependencyManager.DependencyManager" + ExceptionUtils.ToString(e));
                    throw e;
                }
            }

            public DependencyManager(Container container, string name, object instance)
            {
                this.container = container;
                this.name = name;

                container.Services[name] = () => { return instance; };
            }
            /// <summary>
            /// Una vez registrado el componente como singleton, devuelve la misma instancia aunque se registre nuevamente
            /// </summary>
            /// <returns></returns>
            public DependencyManager AsSingleton()
            {
                object value = null;
                Func<object> service = container.Services[name];
                container.Services[name] = () => value ?? (value = service());
                return this;
            }
            /// <summary>
            /// Define los parámetros del constructor que ya están registrados en el Container
            /// </summary>
            /// <param name="parameter">nombre del parámetro</param>
            /// <param name="component">nombre cn el que se registró el parámetro en el container</param>
            /// <returns></returns>
            public DependencyManager WithDependency(string parameter, string component)
            {
                args[parameter] = () => container.Services[component]();
                return this;
            }

            /// <summary>
            /// Define los parámetros del constructor
            /// NOTA: Debe ser el primer constructor definido en la clase
            /// </summary>
            /// <param name="parameter">nombre del parámetro</param>
            /// <param name="value">valor del parámetro</param>
            /// <returns></returns>
            public DependencyManager WithValue(string parameter, object value)
            {
                args[parameter] = () => value;
                return this;
            }
        }

        public static void Clear()
        {
            _lazyServices.Value.Clear();
            _lazyServiceNames.Value.Clear();
        }
    }

    public interface IContainer
    {
        /// <summary>
        /// Registra un nuevo ítem por tipo
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <typeparam name="C">El tipo de instancia que se devolverá</typeparam>
        Container.DependencyManager Register<S, C>() where C : S;
        /// <summary>
        /// Registra un nuevo ítem por nombre
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <typeparam name="C">El tipo de instancia que se devolverá</typeparam>
        Container.DependencyManager Register<S, C>(string name) where C : S;
        /// <summary>
        /// Registra un nuevo ítem por instancia
        /// </summary>
        /// <typeparam name="S">El tipo a resolver</typeparam>
        /// <param name="instancia">La instancia que se devolverá</param>
        Container.DependencyManager Register<S>(S instancia) where S : class;
        /// <summary>
        /// Devuelve una instancia de T
        /// </summary>
        /// <typeparam name="T">El tipo a resolver</typeparam>
        T Resolve<T>() where T : class;
        /// <summary>
        /// Devuelve una instancia de T que se haya registrado con "name"
        /// </summary>
        /// <typeparam name="T">El tipo a resolver</typeparam>
        T Resolve<T>(string name) where T : class;
    }
}
