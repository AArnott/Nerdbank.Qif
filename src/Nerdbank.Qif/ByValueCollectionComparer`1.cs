// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

internal class ByValueCollectionComparer<T> : IEqualityComparer<IReadOnlyList<T>>
    where T : IEquatable<T>
{
    internal static readonly ByValueCollectionComparer<T> Default = new();

    private ByValueCollectionComparer()
    {
    }

    public bool Equals(IReadOnlyList<T> x, IReadOnlyList<T> y)
    {
        if (x.Count != y.Count)
        {
            return false;
        }

        for (int i = 0; i < x.Count; i++)
        {
            if (!x[i].Equals(y[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(IReadOnlyList<T> obj)
    {
        switch (obj.Count)
        {
            case 0: return 0;
            case 1: return obj[0].GetHashCode();
            default:
                HashCode hash = default;
                for (int i = 0; i < obj.Count; i++)
                {
                    hash.Add(obj[i]);
                }

                return hash.ToHashCode();
        }
    }
}
