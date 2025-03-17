using AtelierResleriana.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace AtelierResleriana.Plugin.Inspection
{
    public static class Types
    {
        public static Reflection.Type[] From(params Il2CppSystem.Reflection.Assembly[] assemblies)
        {
            List<Reflection.Type> iTypes = new List<Reflection.Type>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    Reflection.Type iType = new Reflection.Type()
                    {
                        Assembly = assembly.GetName().Name,
                        Name = type.FullName
                    };
                    iType.BaseType = type.BaseType != null ? new TypeReference
                    {
                        Assembly = type.BaseType.Assembly.GetName().Name,
                        Name = type.BaseType.FullName
                    } : null;

                    var constructors = type.GetConstructors(Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Instance);
                    iType.Constructors = constructors.Select(c => new Constructor
                    {
                        Type = new TypeReference
                        {
                            Assembly = c.DeclaringType.Assembly.GetName().Name,
                            Name = c.DeclaringType.FullName
                        },
                        Name = c.Name
                    }).ToArray();

                    var fields = type.GetFields(Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Static | Il2CppSystem.Reflection.BindingFlags.Instance);
                    iType.Fields = fields.Select(f => new Field
                    {
                        Type = new TypeReference
                        {
                            Assembly = f.FieldType.Assembly.GetName().Name,
                            Name = f.FieldType.FullName
                        },
                        IsPublic = f.IsPublic,
                        IsStatic = f.IsStatic,
                        Name = f.Name
                    }).ToArray();

                    var properties = type.GetProperties(Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Static | Il2CppSystem.Reflection.BindingFlags.Instance);
                    iType.Properties = properties.Select(p => new Property
                    {
                        Type = new TypeReference
                        {
                            Assembly = p.PropertyType.Assembly.GetName().Name,
                            Name = p.PropertyType.FullName
                        },
                        IsPublic = p.GetMethod?.IsPublic ?? p.SetMethod?.IsPublic ?? false,
                        IsStatic = p.GetMethod?.IsStatic ?? p.SetMethod?.IsStatic ?? false,
                        Name = p.Name
                    }).ToArray();

                    var methods = type.GetMethods(Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Static | Il2CppSystem.Reflection.BindingFlags.Instance);
                    iType.Methods = methods.Select(m => new Method
                    {
                        ReturnType = m.ReturnType != null ? new TypeReference
                        {
                            Assembly = m.ReturnType.Assembly.GetName().Name,
                            Name = m.ReturnType.FullName
                        } : null,
                        IsPublic = m.IsPublic,
                        IsStatic = m.IsStatic,
                        Name = m.Name,
                        Parameters = m.GetParameters().Select(p => new Parameter
                        {
                            Type = new TypeReference
                            {
                                Assembly = p.ParameterType.Assembly.GetName().Name,
                                Name = p.ParameterType.FullName
                            },
                            Name = p.Name
                        }).ToArray()
                    }).ToArray();

                    iTypes.Add(iType);
                }
            }

            return iTypes.ToArray();
        }
    }
}
