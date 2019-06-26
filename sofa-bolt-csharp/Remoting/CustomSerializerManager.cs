using System;
using System.Collections.Concurrent;

namespace Remoting
{
    /// <summary>
    /// Manage the custom serializer according to the class name.
    /// </summary>
    public class CustomSerializerManager
    {
        /// <summary>
        /// For rpc
		/// </summary>
        private static ConcurrentDictionary<Type, CustomSerializer> classCustomSerializer = new ConcurrentDictionary<Type, CustomSerializer>(); // class name

        /// <summary>
        /// For user defined command
		/// </summary>
        private static ConcurrentDictionary<CommandCode, CustomSerializer> commandCustomSerializer = new ConcurrentDictionary<CommandCode, CustomSerializer>(); // command code

        /// <summary>
        /// Register custom serializer for class name.
        /// </summary>
        /// <param name="className"> </param>
        /// <param name="serializer">
        /// @return </param>
        public static void registerCustomSerializer(Type className, CustomSerializer serializer)
        {
            if (classCustomSerializer.ContainsKey(className))
            {
                classCustomSerializer.TryGetValue(className, out var prevSerializer);
                throw new Exception("CustomSerializer has been registered for class: " + className + ", the custom serializer is: " + prevSerializer.GetType().FullName);
            }
            else
            {
                classCustomSerializer.TryAdd(className, serializer);
            }
        }

        /// <summary>
        /// Get the custom serializer for class name.
        /// </summary>
        /// <param name="className">
        /// @return </param>
        public static CustomSerializer getCustomSerializer(Type className)
        {
            if (!classCustomSerializer.IsEmpty)
            {
                if (classCustomSerializer.ContainsKey(className))
                {
                    return classCustomSerializer[className];
                }
            }
            return null;
        }

        /// <summary>
        /// Register custom serializer for command code.
        /// </summary>
        /// <param name="code"> </param>
        /// <param name="serializer">
        /// @return </param>
        public static void registerCustomSerializer(CommandCode code, CustomSerializer serializer)
        {
            CustomSerializer prevSerializer;
            if (!commandCustomSerializer.ContainsKey(code))
            {
                commandCustomSerializer.TryAdd(code, serializer);
                prevSerializer = null;
            }
            else
            {
                commandCustomSerializer.TryGetValue(code, out prevSerializer);
            }
            if (prevSerializer != null)
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                throw new Exception("CustomSerializer has been registered for command code: " + code + ", the custom serializer is: " + prevSerializer.GetType().FullName);
            }
        }

        /// <summary>
        /// Get the custom serializer for command code.
        /// </summary>
        /// <param name="code">
        /// @return </param>
        public static CustomSerializer getCustomSerializer(CommandCode code)
        {
            if (!commandCustomSerializer.IsEmpty)
            {
                if (commandCustomSerializer.ContainsKey(code))
                {
                    return commandCustomSerializer[code];
                }
            }
            return null;
        }

        /// <summary>
        /// clear the custom serializers.
        /// </summary>
        public static void clear()
        {
            classCustomSerializer.Clear();
            commandCustomSerializer.Clear();
        }
    }
}