namespace Shared;


public static class ArrayExtentions {
    public static int Index<T>(T[] arr, T to_find) {
        for (int i = 0; i < arr.Length; i++) {
            if (EqualityComparer<T>.Default.Equals(arr[i], to_find)) { return i; }
        }

        return -1;
    }

    public static T[] Merge<T>(T[] BigArr, T[] SmallArr, int index = 0)
    {
        foreach (T item in SmallArr)
        {
            BigArr[index] = item;
            index++;
        }

        return BigArr;
    }

    public static T[] Slice<T>(T[] Arr, int start, int end)
    {
        T[] to_return = new T[end - start];
        var index = 0;
        for (int i = start; i < end; i++)
        {
            to_return[index] = Arr[i];
            index++;
        }
        return to_return;
    }

    // Returns new array and length
    public static Tuple<T[], int> ClearEmpty<T>(T[] Arr, Func<T, bool> check_empty)
    {
        int index = 0;
        for (int i = 0; i < Arr.Length; i++)
        {
            if (check_empty(Arr[i]))
            {
                index = i;
                break;
            }
        }
        return new Tuple<T[], int>(
            Merge(new T[Arr.Length], Slice(Arr, index, Arr.Length)),
            index
        );
    }

    public static uint ByteArrToUint(byte[] arr) {
        uint i = 0;
        for (int x = 0; x < arr.Length; x++) {
            i += (uint) (arr[x] << (8*x));
        }
        return i;
    }

    public static void PrintByteArr (byte[] arr) {
        string s = "";
        foreach (byte b in arr) {
            s += (uint) b + ", ";
        }
        Console.WriteLine(s);
    }
}