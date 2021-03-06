﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;

namespace Avalonia.Xaml.Interactions.Core
{
    /// <summary>
    /// A helper class that enables converting values specified in markup (strings) to their object representation.
    /// </summary>
    public static class TypeConverterHelper
    {
        /// <summary>
        /// Converts string representation of a value to its object representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The destination type.</param>
        /// <returns>Object representation of the string value.</returns>
        /// <exception cref="ArgumentNullException">destinationType cannot be null.</exception>
        public static object? Convert(string value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            var destinationTypeFullName = destinationType.FullName;

            string scope = GetScope(destinationTypeFullName);

            // Value types in the "System" namespace must be special cased due to a bug in the xaml compiler
            if (string.Equals(scope, "System", StringComparison.Ordinal))
            {
                if (string.Equals(destinationTypeFullName, (typeof(string).FullName), StringComparison.Ordinal))
                {
                    return value;
                }
                else if (string.Equals(destinationTypeFullName, typeof(bool).FullName, StringComparison.Ordinal))
                {
                    return bool.Parse(value);
                }
                else if (string.Equals(destinationTypeFullName, typeof(int).FullName, StringComparison.Ordinal))
                {
                    return int.Parse(value, CultureInfo.InvariantCulture);
                }
                else if (string.Equals(destinationTypeFullName, typeof(double).FullName, StringComparison.Ordinal))
                {
                    return double.Parse(value, CultureInfo.InvariantCulture);
                }
            }

            try
            {
                if (destinationType.BaseType == typeof(Enum))
                    return Enum.Parse(destinationType, value);

                if (destinationType.GetInterfaces().Any(t => t == typeof(IConvertible)))
                {
                    return (value as IConvertible).ToType(destinationType, CultureInfo.InvariantCulture);
                }
            }
            catch (ArgumentException)
            {
                // not an enum
            }
            catch (InvalidCastException)
            {
                // not able to convert to anything
            }

            return null;
        }

        private static string GetScope(string name)
        {
            int indexOfLastPeriod = name.LastIndexOf('.');
            if (indexOfLastPeriod != name.Length - 1)
            {
                return name.Substring(0, indexOfLastPeriod);
            }

            return name;
        }

        private static string GetType(string name)
        {
            int indexOfLastPeriod = name.LastIndexOf('.');
            if (indexOfLastPeriod != name.Length - 1)
            {
                return name.Substring(++indexOfLastPeriod);
            }

            return name;
        }
    }
}
