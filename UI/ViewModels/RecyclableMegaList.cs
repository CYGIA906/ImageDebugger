using System.Collections;
using System.Collections.Generic;

namespace UI.ViewModels
{
    /// <summary>
    /// A list that is able to loop backward and forward infinitely
    /// </summary>
    public class RecyclableMegaList<T> : ViewModelBase, IEnumerable<List<T>>
    {
        /// <summary>
        /// Current index of the internal list
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// Internal list that stores the data
        /// </summary>
        private List<List<T>> MegaList { get; }

        /// <summary>
        /// Return next element in the internal list
        /// If the new index exceeds Count - 1, it will go back to 0
        /// and continue to provide data
        /// </summary>
        /// <returns></returns>
        public List<T> NextUnbounded()
        {
            CurrentIndex++;
            if (CurrentIndex >= MegaList[0].Count) CurrentIndex = 0;
            return this[CurrentIndex];
        }

        /// <summary>
        /// Return next element in the internal list
        /// If the new index exceeds Count - 1, it will go back to 0
        /// and provide null
        /// </summary>
        /// <returns></returns>
        public List<T> NextBounded()
        {
            CurrentIndex++;
            if (CurrentIndex >= MegaList[0].Count)
            {
                CurrentIndex = -1;
                return null;
            }

            return this[CurrentIndex];
        }

        /// <summary>
        /// Return previous element in the internal list
        /// If the new index is less than 0 , it will go to the end of the list
        /// and continue to provide data
        /// </summary>
        /// <returns></returns>
        public List<T> PreviousUnbound()
        {
            CurrentIndex--;
            if (CurrentIndex < 0) CurrentIndex = MegaList[0].Count - 1;

            return this[CurrentIndex];
        }

        public int NumLists
        {
            get { return MegaList.Count; }
            set
            {
                if (value == MegaList.Count) return;

                MegaList.Clear();
                for (int i = 0; i < value; i++)
                {
                    MegaList.Add(new List<T>());
                }
            }
        }

        public RecyclableMegaList()
        {
            CurrentIndex = -1;
            MegaList = new List<List<T>>();
        }

        public List<T> this[int index]
        {
            get
            {
                var output = new List<T>();
                foreach (var list in MegaList)
                {
                    output.Add(list[index]);
                }

                return output;
            }
            set
            {
                int count = value.Count;
                for (int i = 0; i < count; i++)
                {
                    MegaList[i].Insert(index, value[i]);
                }
            }
        }

        public IEnumerator<List<T>> GetEnumerator() => MegaList.GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}