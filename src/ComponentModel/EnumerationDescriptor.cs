using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using Wheatech.Properties;

namespace Wheatech.ComponentModel
{
    /// <summary>
    /// Provides members information for enumerations.
    /// </summary>
    public static class EnumerationDescriptor
    {
        private static readonly ConcurrentDictionary<Type, EnumerationMemberDescriptorCollection> _cache =
            new ConcurrentDictionary<Type, EnumerationMemberDescriptorCollection>();

        /// <summary>
        /// Returns the collection of members for a specified type of enumeration.
        /// </summary>
        /// <param name="enumerationType">A <see cref="Type"/> that represents the enumeration to get members for.</param>
        /// <returns>An <see cref="EnumerationMemberDescriptorCollection"/> with the members for a specified type of enumeration.</returns>
        public static EnumerationMemberDescriptorCollection GetMembers(Type enumerationType)
        {
            if (!enumerationType.IsEnum)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Argument_MustBeEnum, enumerationType), nameof(enumerationType));
            }
            return _cache.GetOrAdd(enumerationType,
                                   type =>
                                   new EnumerationMemberDescriptorCollection(
                                       type.GetFields()
                                           .Where(field => !field.IsSpecialName)
                                           .Select(field => new EnumerationMemberDescriptor(type, field))
                                           .OrderBy(member => member.Order)));
        }

        /// <summary>
        /// Returns the collection of members for a specified type of enumeration.
        /// </summary>
        /// <typeparam name="T">A <see cref="Type"/> that represents the enumeration to get members for.</typeparam>
        /// <returns>An <see cref="EnumerationMemberDescriptorCollection"/> with the members for a specified type of enumeration.</returns>
        public static EnumerationMemberDescriptorCollection GetMembers<T>()
            where T : struct
        {
            return GetMembers(typeof(T));
        }

        /// <summary>
        /// Returns the member descriptor for the specified enumeration member.
        /// </summary>
        /// <param name="value">The specified enumeration member.</param>
        /// <returns>An <see cref="EnumerationMemberDescriptor" /> for the specified enumeration member.</returns>
        public static EnumerationMemberDescriptor GetMember(Enum value)
        {
            var members = GetMembers(value.GetType());
            return members.FirstOrDefault(member => Equals(member.Value, value));
        }
    }
}
