using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [Serializable]
    public struct Dic2DKey
    {
        public int x;
        public int y;

        public Dic2DKey (int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals (object obj)
        {
            return ((Dic2DKey)obj).x == x && ((Dic2DKey)obj).y == y;
        }

        public override int GetHashCode ()
        {
            return 32768 * x + y;
        }

        public override string ToString ()
        {
            return "(" + x + "; " + y + ")";
        }
        public Vector2 ToVector2 ()
        {
            return new Vector2 (x, y);
        }
    }

    [Serializable]
    public class SpatialDictionary<T>
    {
        private const int MIN_SCALE = 6;

        // power of 2 giving the width
        // Ex: 8 means that contains x and y beteween 0 in [-256 ; -1] or [0 ; 255]
        private int scale;

        protected int count;

        public int Count
        {
            get
            {
                return count;
            }
        }

        // sub parts
        protected SpatialDictionary<T> topLeft;
        protected SpatialDictionary<T> topRight;
        protected SpatialDictionary<T> botLeft;
        protected SpatialDictionary<T> botRight;

        // values
        private Dictionary<Dic2DKey, T> dic;

        public SpatialDictionary (int scale = MIN_SCALE + 1)
        {
            this.scale = scale;
            count = 0;

            if (IsLeaf ())
            {
                dic = new Dictionary<Dic2DKey, T> ();
            }
        }

        private bool IsLeaf ()
        {
            return scale <= MIN_SCALE;
        }

        public void Add (int x, int y, T value)
        {
            count++;

            int fullScale = PowerOfTwo (scale);
            while (x >= fullScale || x < -fullScale || y >= fullScale || y < -fullScale)
            {
                Expand ();
                fullScale = PowerOfTwo (scale);
            }

            if (IsLeaf ())
            {
                dic[new Dic2DKey (x, y)] = value;
            }
            else
            {
                int halfFullScale = fullScale / 2;

                if (x < 0)
                {
                    x += halfFullScale;

                    if (y < 0)
                    {
                        y += halfFullScale;

                        if (botLeft == null)
                        {
                            botLeft = new SpatialDictionary<T> (scale - 1);
                        }
                        botLeft.Add (x, y, value);
                    }
                    else
                    {
                        y -= halfFullScale;

                        if (topLeft == null)
                        {
                            topLeft = new SpatialDictionary<T> (scale - 1);
                        }
                        topLeft.Add (x, y, value);
                    }
                }
                else
                {
                    x -= halfFullScale;

                    if (y < 0)
                    {
                        y += halfFullScale;

                        if (botRight == null)
                        {
                            botRight = new SpatialDictionary<T> (scale - 1);
                        }
                        botRight.Add (x, y, value);
                    }
                    else
                    {
                        y -= halfFullScale;

                        if (topRight == null)
                        {
                            topRight = new SpatialDictionary<T> (scale - 1);
                        }
                        topRight.Add (x, y, value);
                    }
                }
            }
        }

        public T Get (Dic2DKey key)
        {
            return Get (key.x, key.y);
        }

        public T Get (int x, int y)
        {
            if (IsLeaf ())
            {
                return dic[new Dic2DKey (x, y)];
            }
            else
            {
                int halfFullScale = PowerOfTwo (scale - 1);

                if (x < 0)
                {
                    x += halfFullScale;

                    if (y < 0)
                    {
                        y += halfFullScale;

                        if (botLeft == null)
                        {
                            throw new KeyNotFoundException ("Spatial dictionary area is empty");
                        }
                        return botLeft.Get (x, y);
                    }
                    else
                    {
                        y -= halfFullScale;

                        if (topLeft == null)
                        {
                            throw new KeyNotFoundException ("Spatial dictionary area is empty");
                        }
                        return topLeft.Get (x, y);
                    }
                }
                else
                {
                    x -= halfFullScale;

                    if (y < 0)
                    {
                        y += halfFullScale;

                        if (botRight == null)
                        {
                            throw new KeyNotFoundException ("Spatial dictionary area is empty");
                        }
                        return botRight.Get (x, y);
                    }
                    else
                    {
                        y -= halfFullScale;

                        if (topRight == null)
                        {
                            throw new KeyNotFoundException ("Spatial dictionary area is empty");
                        }
                        return topRight.Get (x, y);
                    }
                }
            }
        }

        public List<T> GetAround (int x, int y, int distance)
        {
            List<T> list = new List<T> ();

            if (IsLeaf ())
            {
                foreach (KeyValuePair<Dic2DKey, T> pair in dic)
                {
                    if (
                            pair.Key.x - x < distance &&
                            pair.Key.x - x > -distance &&
                            pair.Key.y - y < distance &&
                            pair.Key.y - y > -distance
                        )
                    {
                        list.Add (pair.Value);
                    }
                }

            }
            else
            {
                int halfFullScale = PowerOfTwo (scale - 1);

                if (x - distance < 0)
                {
                    if (botLeft != null && y - distance < 0)
                    {
                        list.AddRange (botLeft.GetAround (x + halfFullScale, y + halfFullScale, distance));
                    }
                    if (topLeft != null && y + distance > 0)
                    {
                        list.AddRange (topLeft.GetAround (x + halfFullScale, y - halfFullScale, distance));
                    }
                }
                if (x + distance > 0)
                {
                    if (botRight != null && y - distance < 0)
                    {
                        list.AddRange (botRight.GetAround (x - halfFullScale, y + halfFullScale, distance));
                    }
                    if (topRight != null && y + distance > 0)
                    {
                        list.AddRange (topRight.GetAround (x - halfFullScale, y - halfFullScale, distance));
                    }
                }
            }

            return list;
        }

        public int CountAround (int x, int y, int distance)
        {
            int count = 0;

            if (IsLeaf ())
            {
                foreach (KeyValuePair<Dic2DKey, T> pair in dic)
                {
                    if (
                            pair.Key.x - x < distance &&
                            pair.Key.x - x > -distance &&
                            pair.Key.y - y < distance &&
                            pair.Key.y - y > -distance
                        )
                    {
                        count++;
                    }
                }
            }
            else
            {
                int halfFullScale = PowerOfTwo (scale - 1);

                if (x - distance < 0)
                {
                    if (botLeft != null && y - distance < 0)
                    {
                        count += botLeft.CountAround (x + halfFullScale, y + halfFullScale, distance);
                    }
                    if (topLeft != null && y + distance > 0)
                    {
                        count += topLeft.CountAround (x + halfFullScale, y - halfFullScale, distance);
                    }
                }
                if (x + distance > 0)
                {
                    if (botRight != null && y - distance < 0)
                    {
                        count += botRight.CountAround (x - halfFullScale, y + halfFullScale, distance);
                    }
                    if (topRight != null && y + distance > 0)
                    {
                        count += topRight.CountAround (x - halfFullScale, y - halfFullScale, distance);
                    }
                }
            }

            return count;
        }

        public bool IsEmpty (int x, int y, int distance)
        {
            if (IsLeaf ())
            {
                foreach (KeyValuePair<Dic2DKey, T> pair in dic)
                {
                    if (
                            pair.Key.x - x < distance &&
                            pair.Key.x - x > -distance &&
                            pair.Key.y - y < distance &&
                            pair.Key.y - y > -distance
                        )
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                int halfFullScale = PowerOfTwo (scale - 1);

                if (x - distance < 0)
                {
                    if (botLeft != null && y - distance < 0 && !botLeft.IsEmpty (x + halfFullScale, y + halfFullScale, distance))
                    {
                        return false;
                    }
                    if (topLeft != null && y + distance > 0 && !topLeft.IsEmpty (x + halfFullScale, y - halfFullScale, distance))
                    {
                        return false;
                    }
                }

                if (x + distance > 0)
                {
                    if (botRight != null && y - distance < 0 && !botRight.IsEmpty (x - halfFullScale, y + halfFullScale, distance))
                    {
                        return false;
                    }
                    if (topRight != null && y + distance > 0 && !topRight.IsEmpty (x - halfFullScale, y - halfFullScale, distance))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void Expand ()
        {
            if (topLeft != null)
            {
                SpatialDictionary<T> newDic = new SpatialDictionary<T> (scale);
                newDic.count = topLeft.count;
                newDic.botRight = topLeft;
                topLeft = newDic;
            }

            if (topRight != null)
            {
                SpatialDictionary<T> newDic = new SpatialDictionary<T> (scale);
                newDic.count = topRight.count;
                newDic.botLeft = topRight;
                topRight = newDic;
            }

            if (botLeft != null)
            {
                SpatialDictionary<T> newDic = new SpatialDictionary<T> (scale);
                newDic.count = botLeft.count;
                newDic.topRight = botLeft;
                botLeft = newDic;
            }

            if (botRight != null)
            {
                SpatialDictionary<T> newDic = new SpatialDictionary<T> (scale);
                newDic.count = botRight.count;
                newDic.topLeft = botRight;
                botRight = newDic;
            }

            scale++;
        }

        public static int PowerOfTwo (int n)
        {
            int val = 1;
            for (int i = 1; i <= n; i++)
            {
                val *= 2;
            }

            return val;
        }

        public SpatialDictionary<T> Copy ()
        {
            if (!typeof (T).IsSerializable)
            {
                throw new SerializationException ("The type must be serializable.");
            }

            IFormatter formatter = new BinaryFormatter (); SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate ();
            surrogateSelector.AddSurrogate (typeof (Vector2), new StreamingContext (StreamingContextStates.All), vector2SS);
            formatter.SurrogateSelector = surrogateSelector;

            Stream stream = new MemoryStream ();
            using (stream)
            {
                formatter.Serialize (stream, this);
                stream.Seek (0, SeekOrigin.Begin);
                return (SpatialDictionary<T>)formatter.Deserialize (stream);
            }
        }
    }
}
