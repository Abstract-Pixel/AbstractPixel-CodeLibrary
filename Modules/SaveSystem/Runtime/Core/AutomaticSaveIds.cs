using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Gives a plain class that registers with no save ID a save ID of its own: its class name, then "Inventory 2",
    /// "Inventory 3" for later live instances, always the lowest free number in that save file. It belongs to one
    /// session, never a static, so nothing outlives play mode.
    /// </summary>
    internal sealed class AutomaticSaveIds
    {
        // The numbers in use, per save file and per name ("Game" -> "Inventory" -> {1, 2}).
        readonly Dictionary<string, Dictionary<string, HashSet<int>>> numbersInUse = new Dictionary<string, Dictionary<string, HashSet<int>>>();

        // Which class owns each name in each save file, so a second class called Inventory uses its full name.
        readonly Dictionary<string, Dictionary<string, string>> classOwningEachName = new Dictionary<string, Dictionary<string, string>>();

        // What each live object was given. Weak, so an object nobody holds any more can still be collected.
        readonly ConditionalWeakTable<object, AssignedSaveId> assignedIds = new ConditionalWeakTable<object, AssignedSaveId>();

        sealed class AssignedSaveId
        {
            public string SaveFileName;
            public string Name;
            public int Number;
            public bool IsInUse;
        }

        /// <summary>
        /// The save ID for this object in this save file. The same object registering again keeps its save ID when
        /// its number is still free.
        /// </summary>
        public string Assign(object savedObject, Type savedObjectClass, string saveFileName)
        {
            bool assignedBefore = assignedIds.TryGetValue(savedObject, out AssignedSaveId earlier);
            if (assignedBefore && earlier.IsInUse)
            {
                return SaveIdOf(earlier);
            }

            string name = NameFor(savedObjectClass, saveFileName);
            HashSet<int> numbers = NumbersInUse(saveFileName, name);
            bool canKeepItsNumber = assignedBefore && earlier.SaveFileName == saveFileName && earlier.Name == name && !numbers.Contains(earlier.Number);
            int number = canKeepItsNumber ? earlier.Number : LowestFreeNumber(numbers);
            numbers.Add(number);

            AssignedSaveId assigned = new AssignedSaveId { SaveFileName = saveFileName, Name = name, Number = number, IsInUse = true };
            assignedIds.Remove(savedObject);
            assignedIds.Add(savedObject, assigned);
            return SaveIdOf(assigned);
        }

        /// <summary>Frees the object's number, so the next new instance of its class can take it.</summary>
        public void Release(object savedObject)
        {
            bool hasOne = assignedIds.TryGetValue(savedObject, out AssignedSaveId assigned);
            if (!hasOne || !assigned.IsInUse)
            {
                return;
            }
            NumbersInUse(assigned.SaveFileName, assigned.Name).Remove(assigned.Number);
            assigned.IsInUse = false;
        }

        public void Clear()
        {
            numbersInUse.Clear();
            classOwningEachName.Clear();
        }

        // The class name, unless another class with the same name already uses it in this save file.
        string NameFor(Type savedObjectClass, string saveFileName)
        {
            if (!classOwningEachName.TryGetValue(saveFileName, out Dictionary<string, string> owners))
            {
                owners = new Dictionary<string, string>();
                classOwningEachName.Add(saveFileName, owners);
            }

            string shortName = savedObjectClass.Name;
            if (!owners.TryGetValue(shortName, out string owningClass))
            {
                owners.Add(shortName, savedObjectClass.FullName);
                return shortName;
            }
            if (owningClass == savedObjectClass.FullName)
            {
                return shortName;
            }
            return savedObjectClass.FullName;
        }

        HashSet<int> NumbersInUse(string saveFileName, string name)
        {
            if (!numbersInUse.TryGetValue(saveFileName, out Dictionary<string, HashSet<int>> numbersByName))
            {
                numbersByName = new Dictionary<string, HashSet<int>>();
                numbersInUse.Add(saveFileName, numbersByName);
            }
            if (!numbersByName.TryGetValue(name, out HashSet<int> numbers))
            {
                numbers = new HashSet<int>();
                numbersByName.Add(name, numbers);
            }
            return numbers;
        }

        static int LowestFreeNumber(HashSet<int> numbers)
        {
            int number = 1;
            while (numbers.Contains(number))
            {
                number++;
            }
            return number;
        }

        // The first instance is just "Inventory"; later ones are "Inventory 2", "Inventory 3".
        static string SaveIdOf(AssignedSaveId assigned)
        {
            if (assigned.Number == 1)
            {
                return assigned.Name;
            }
            return $"{assigned.Name} {assigned.Number}";
        }
    }
}
