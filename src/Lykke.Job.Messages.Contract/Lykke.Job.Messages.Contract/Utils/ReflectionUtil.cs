using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lykke.Job.Messages.Contract.Utils
{
    public static class ReflectionUtil
    {
        public static IEnumerable<Type> GetImplTypesAssignableToMarkerTypeFromAsssembly(Assembly assembly, Type markerType)
        {
            var types = assembly.GetTypes()
                .Where(type =>
                {
                    var isMarkerType = !type.IsInterface &&
                                       !type.IsAbstract &&
                                        markerType.IsAssignableFrom(type);

                    return isMarkerType;
                });

            return types;
        }

        public static object ExtractConstValueFromType(Type type, string constName)
        {
            var constField = type.GetFields().Where(fieldInfo => fieldInfo.Name == constName &&
                (fieldInfo.Attributes & FieldAttributes.Literal) != 0)
                ?.FirstOrDefault();
            var constValue = constField?.GetValue(null);

            return constValue;
        }
    }
}
