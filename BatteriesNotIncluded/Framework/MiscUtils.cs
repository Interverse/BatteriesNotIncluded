using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;

namespace BatteriesNotIncluded.Framework {
    public static class MiscUtils {
        public static Random Random = new Random();

        /// <summary>
        /// Attempts to sanitize any ' characters in a string to '' for sql queries.
        /// </summary>
        public static string SanitizeString(this string s) {
            if (!s.Contains("'")) return s;

            string[] temp = s.Split('\'');
            s = temp[0];

            for (int x = 1; x < temp.Length; x++) {
                s += "''" + temp[x];
            }
            return s;
        }

        /// <summary>
        /// Converts a string to be friendly with sql inputs.
        /// </summary>
        public static string SqlString(this string s) => "'" + SanitizeString(s) + "'";

        /// <summary>
        /// Determines whether a bit is a 1 or a 0 in an integer in a specified index, 
        /// where the index starts at 0 from the right.
        /// </summary>
        /// <param name="bitIndex">Index of the bit, starting from 0 on the left</param>
        /// <returns>True if the bit in the specified index is 1</returns>
        public static bool GetBit(this int x, int bitIndex) => (x & (1 << bitIndex)) != 0;

        /// <summary>
        /// Restricts a number between a minimum and maximum value.
        /// </summary>
        public static T Clamp<T>(this T num, T min, T max) where T : IComparable =>
            num.CompareTo(min) < 0 ? min : num.CompareTo(max) > 0 ? max : num;

        /// <summary>
        /// Replaces a value with another.
        /// </summary>
        public static T Replace<T>(this T num, T value, T replace) where T : IComparable =>
            num.CompareTo(value) == 0 ? replace : num;

        /// <summary>
        /// Generates a string with a specified amount of line breaks.
        /// </summary>
        /// <param Name="amount">The amount of line breaks.</param>
        public static string LineBreaks(int amount) {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < amount; x++) {
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Separates a string into lines after a specified amount of characters.
        /// </summary>
        public static string SeparateToLines(this string s, int charPerLine = 45, string breakSpecifier = "") {
            StringBuilder sb = new StringBuilder();
            int count = 0;

            foreach (char ch in s) {
                if (count != 0 && count >= charPerLine) {
                    if (breakSpecifier != "" && ch.ToString() == breakSpecifier) {
                        sb.Append("\r\n");
                        count = 0;
                    }
                }
                sb.Append(ch);
                count++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a string into a given Type.
        /// </summary>
        /// <returns>Returns false if the string is incompatible with the given Type</returns>
        public static bool TryConvertStringToType(Type referenceType, string input, out object obj) {
            try {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(referenceType);
                obj = typeConverter.ConvertFromString(input.SanitizeString());
                return true;
            } catch {
                obj = default(object);
                return false;
            }
        }

        /// <summary>
        /// Gets a list of projectiles based off the given Name query.
        /// </summary>
        public static List<int> GetProjectileByName(this TShockAPI.Utils util, string name) {
            string nameLower = name.ToLower();
            var found = new List<int>();
            for (int i = 1; i < Terraria.Main.maxProjectileTypes; i++) {
                string projectileName = Lang.GetProjectileName(i).ToString();
                if (!String.IsNullOrWhiteSpace(projectileName) && projectileName.ToLower() == nameLower)
                    return new List<int> { i };
                if (!String.IsNullOrWhiteSpace(projectileName) && projectileName.ToLower().StartsWith(nameLower))
                    found.Add(i);
            }
            return found;
        }

        /// <summary>
        /// Converts a string to the reference value type,
        /// and sets the string to the given reference value.
        /// </summary>
        public static bool SetValueWithString<T>(ref T value, string val) {
            try {
                value = (T)Convert.ChangeType(val, value.GetType());
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Takes in a name of a variable in a class, finds the variable in an object, and
        /// tries to set a string value to the variable for the object.
        /// </summary>
        public static bool SetValueWithString(object obj, string propertyName, string val) {
            try {
                var property = obj.GetType().GetProperty(propertyName);
                if (property == null) return false;
                property.SetValue(obj, Convert.ChangeType(val, property.GetValue(obj).GetType()));
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Instantiates every class in the assembly that inherits an interface.
        /// </summary>
        /// <returns>A list of all the instantiated objects</returns>
        public static List<T> InstantiateClassesOfInterface<T>() {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(T)))
                .Select(t => (T)Activator.CreateInstance(t)).ToList();
        }

        /// <summary>
        /// Instantiates every class in the assembly that inherits an abstract class.
        /// </summary>
        /// <returns>A list of all the instantiated objects</returns>
        public static List<T> InstantiateClassesOfAbstract<T>() {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (T)Activator.CreateInstance(t)).ToList();
        }

        /// <summary>
        /// Selects a random element in a <see cref="IEnumerable{T}"/>
        /// </summary>
        public static T SelectRandom<T>(this IEnumerable<T> obj) => obj.ElementAt(Random.Next(obj.Count()));

        /// <summary>
        /// Capitalizes the first letter of the entire string.
        /// </summary>
        public static string FirstCharToUpper(this string input) {
            if (String.IsNullOrEmpty(input)) return "";
            if (input.Length == 1) return input.ToUpper();
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Gets every variable name that matches a type.
        /// </summary>
        /// <typeparam name="T">Type to search for.</typeparam>
        /// <returns>An <see cref="IEnumerable{string}"/> of all variables that matches the type.</returns>
        public static IEnumerable<string> GetVariableNamesOfType<T>(this object obj) {
            var values = obj.GetType().GetFields();

            foreach (var value in values) {
                if (value.FieldType == typeof(T)) {
                    yield return value.Name;
                }
            }
        }

        /// <summary>
        /// Sets a value in an object given the field name and a value.
        /// </summary>
        /// <returns>Bool whether the field exists in the object.</returns>
        public static bool SetValue<T>(this object obj, string fieldName, T val) {
            try {
                var test = obj.GetType().GetField(fieldName);
                test.SetValue(obj, val);
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Takes in a name of a variable in a class, finds the variable in an object, and
        /// tries to set a string value to the variable for the object.
        /// </summary>
        public static T GetValue<T>(this object obj, string propertyName) {
            var test = obj.GetType().GetField(propertyName);
            return (T)test.GetValue(obj);
        }
    }
}