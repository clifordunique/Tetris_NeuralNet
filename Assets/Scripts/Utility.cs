using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utility
{
    public static void GetArrayMinMaxIndices(int[] array, ref int smallest_index, ref int biggest_index)
    {
        int smallest_section = array[0];
        smallest_index = 0;
        int biggest_section = array[0];
        biggest_index = 0;

        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] < smallest_section)
            {
                smallest_section = array[i];
                smallest_index = i;
            }
            if (array[i] > biggest_section)
            {
                biggest_section = array[i];
                biggest_index = i;
            }
        }
    }
}