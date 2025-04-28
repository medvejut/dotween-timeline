using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dott.Editor
{
    public class DottDrivenProperties : IDisposable
    {
        private Object driver;

        public void Register(IDOTweenAnimation[] animations)
        {
            Unregister();
            driver = ScriptableObject.CreateInstance<ScriptableObject>();

            var tweenTargets = animations.SelectMany(animation => animation.Targets)
                .Where(tweenTarget => tweenTarget != null).Distinct();

            foreach (var tweenTarget in tweenTargets)
            {
                using var targetObject = new SerializedObject(tweenTarget);

                var property = targetObject.GetIterator();
                // Step into the root (self) property
                property.Next(true);

                do
                {
                    DrivenPropertyManager.TryRegisterProperty(driver, tweenTarget, property.name);
                } while (property.Next(false));
            }
        }

        public void Unregister()
        {
            if (driver == null) { return; }

            DrivenPropertyManager.UnregisterProperties(driver);
            Object.DestroyImmediate(driver);
            driver = null;
        }

        public void Dispose()
        {
            Unregister();
        }

        private static class DrivenPropertyManager
        {
            private const string DELEGATE_POSTFIX = "Delegate";

            private delegate void RegisterPropertyDelegate(Object driver, Object target, string propertyPath);

            private delegate void TryRegisterPropertyDelegate(Object driver, Object target, string propertyPath);

            private delegate void UnregisterPropertyDelegate(Object driver, Object target, string propertyPath);

            private delegate void UnregisterPropertiesDelegate(Object driver);

            private static readonly RegisterPropertyDelegate registerProperty;
            private static readonly TryRegisterPropertyDelegate tryRegisterProperty;
            private static readonly UnregisterPropertyDelegate unregisterProperty;
            private static readonly UnregisterPropertiesDelegate unregisterProperties;

            static DrivenPropertyManager()
            {
                static T BindDelegate<T>(Type type) where T : Delegate
                {
                    var name = typeof(T).Name;
                    name = name[..^DELEGATE_POSTFIX.Length];
                    return type.GetMethod(name).CreateDelegate(typeof(T)) as T ?? throw new MissingMethodException($"Failed find method '{name}' in type '{type}'");
                }

                var drivenPropertyManagerType = typeof(Object).Assembly.GetType("UnityEngine.DrivenPropertyManager");

                registerProperty = BindDelegate<RegisterPropertyDelegate>(drivenPropertyManagerType);
                tryRegisterProperty = BindDelegate<TryRegisterPropertyDelegate>(drivenPropertyManagerType);
                unregisterProperty = BindDelegate<UnregisterPropertyDelegate>(drivenPropertyManagerType);
                unregisterProperties = BindDelegate<UnregisterPropertiesDelegate>(drivenPropertyManagerType);
            }

            public static void RegisterProperty(Object driver, Object target, string propertyPath) => registerProperty(driver, target, propertyPath);
            public static void TryRegisterProperty(Object driver, Object target, string propertyPath) => tryRegisterProperty(driver, target, propertyPath);
            public static void UnregisterProperty(Object driver, Object target, string propertyPath) => unregisterProperty(driver, target, propertyPath);
            public static void UnregisterProperties(Object driver) => unregisterProperties(driver);
        }
    }
}