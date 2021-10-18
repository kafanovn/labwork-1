using System;
using System.Collections.Generic;
using System.Numerics;

namespace ConsoleApp1
{

    struct DataItem
    {
        public Vector2 place { get; set; }
        public Vector2 value { get; set; }
        public DataItem(Vector2 place, Vector2 value)
        {
            this.place = place;
            this.value = value;
        }
        public string ToLongString(string format)
        {
            return String.Format(format, this.place.X, this.place.Y, this.value.X, this.value.Y, Math.Sqrt(Math.Pow(this.value.X, 2) + Math.Pow(this.value.Y, 2)));
        }
        public override string ToString()
        {
            return String.Format("X {0:f2} Y {1:f2} E_X {2:f2} E_Y {3:f2} |E| {4:f2}", 
                this.place.X, this.place.Y, this.value.X, this.value.Y, Math.Sqrt(Math.Pow(this.value.X, 2) + Math.Pow(this.value.Y, 2)));
        }
    }


    abstract class V4Data
    {
        public string obj { get; }
        public DateTime data { get; }

        public V4Data(string obj, DateTime data)
        {
            this.obj = obj;
            this.data = data;
        }
        public abstract int Count { get; }
        public abstract float MaxFromOrigin { get; }
        public abstract string ToLongString(string format);
        public abstract override string ToString();
    }


    delegate Vector2 FvVector2(Vector2 v2);

    class V4DataList : V4Data
    {
        public List<DataItem> DataList { get; }
        public V4DataList(string obj, DateTime data) : base(obj, data)
        {
            DataList = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
            foreach (DataItem  Item in DataList)
            {
                if (Item.place.X == newItem.place.X && Item.place.Y == newItem.place.Y)
                {
                    return false;
                }
            }
            DataList.Add(newItem);
            return true;
        }

        public int AddDefaults(int nItems, FvVector2 F)
        {
            int count = 0;
            for (int i = 0; i < nItems; i++)
            {
                float x = i * (float)1.5;
                float y = i * (float)1.5;
                DataItem newItem = new DataItem(new Vector2(x, y), F(new Vector2(x, y)));
                if (this.Add(newItem)) 
                {
                    count++;
                };
            }
            return count;
        }

        public override int Count
        {
            get { return DataList.Count; }
        }

        public override float MaxFromOrigin
        {
            get
            {
                if (DataList.Count == 0)
                {
                    return 0;
                }
                double max = Math.Sqrt(Math.Pow(DataList[0].value.X, 2) + Math.Pow(DataList[0].value.Y, 2));
                foreach (DataItem Item in DataList)
                {
                    if (Math.Sqrt(Math.Pow(Item.value.X, 2) + Math.Pow(Item.value.Y, 2)) > max)
                    {
                        max = Math.Sqrt(Math.Pow(Item.value.X, 2) + Math.Pow(Item.value.Y, 2));
                    }

                }
                return (float)max;
            }
        }

        public override string ToLongString(string format)
        {
            string str1 = String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Count:{3} coords:\n", "V4DataList", obj, data, this.Count) + '\n';
            string str2 = "";
            foreach (DataItem Item in DataList)
            {
                str2 += string.Format(format, Item.place.X, Item.place.Y);
            }
            return str1 + str2;
        }
        public override string ToString()
        {
            return String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Count:{3} coords:\n", "V4DataList", obj, data, this.Count) + '\n';
        }

    }


    class V4DataArray : V4Data
    {
        public int xSteps { get; }
        public int ySteps { get; }
        public Vector2 Step { get; }
        public Vector2[,] array { get; }

        public V4DataArray(string obj, DateTime data) : base(obj, data)
        {
            array = new Vector2[0, 0];
        }
        public V4DataArray(string obj, DateTime data, int xSteps, int ySteps, Vector2 Step, FvVector2 F) : base(obj, data)
        {
            this.xSteps = xSteps;
            this.ySteps = ySteps;
            this.Step = Step;
            array = new Vector2[xSteps, ySteps];
            for (int i = 0; i < xSteps; i++)
            {
                for (int j = 0; j < ySteps; j++)
                {
                    array[i, j] = F(new Vector2((float)j * Step.X, (float)i * Step.Y));
                }
            }
        }
        public override int Count
        {
            get
            {
                return xSteps * ySteps;
            }
        }

        public override string ToLongString(string format)
        {
            string str1 = String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n xSteps:{3}\n ySteps:{4}\n Step:{5:f2}\n coords:\n", "V4DataArray", obj, data, xSteps, ySteps, Step) + '\n';
            string str2 = "";
            for (int i = 0; i < ySteps; i++)
            {
                for (int j = 0; j < xSteps; j++)
                {
                    str2 += String.Format(format, i*Step.X, j*Step.Y);
                }
            }
            return str1 + str2;
        }

        public override string ToString()
        {
            return String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n xSteps:{3}\n ySteps:{4}\n Step:{5}\n coords:\n", "V4DataArray", obj, data, xSteps, ySteps, Step) + '\n';
        }
        public override float MaxFromOrigin
        {
            get
            {
                double max = Math.Sqrt(Math.Pow(array[0,0].X, 2) + Math.Pow(array[0, 0].Y, 2));
                for (int i = 0; i < xSteps; i++)
                {
                    for (int j = 0; j < ySteps; j++)
                    {
                        if (Math.Sqrt(Math.Pow(array[i, j].X, 2) + Math.Pow(array[i,j].Y, 2)) > max)
                        {
                            max = Math.Sqrt(Math.Pow(array[i, j].X, 2) + Math.Pow(array[i, j].Y, 2));
                        }
                    }
                }

                return (float)max;
            }
        }

        public V4DataList ArrayToList()
        {
            V4DataList DataList = new V4DataList(this.obj, this.data);
            for (int i = 0; i < xSteps; i++)
            {
                for (int j = 0; j < ySteps; j++)
                {
                    Vector2 place = new Vector2(i * Step.X, j * Step.Y);
                    Vector2 value = array[i, j];
                    DataItem Item = new DataItem(place, value);
                    DataList.Add(Item);
                }
            }
            return DataList;
        }
    }

    class V4MainCollection
    {
        private List<V4Data> Collection = new List<V4Data>();
        public int Count()
        {
            return Collection.Count;
        }
        public V4Data this[int index]
        {
            get
            {
                return Collection[index];
            }
        }
        public bool Contains(string ID)
        {
            foreach (V4Data Data in Collection)
            {
                if (Data.obj == ID)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Add (V4Data v4Data)
        {
            if (!this.Contains(v4Data.obj))
            {
                Collection.Add(v4Data);
                return true;
            }
            return false;
        }
        public string ToLongString(string format)
        {
            string str = "";
            foreach (V4Data Item in Collection)
            {
                str += Item.ToLongString(format);
            }
            return str;
        }
    }

    static class Fields
    {
        static public Vector2 E(Vector2 place)
        {
            return new Vector2(place.X + place.Y, place.X - place.Y);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            FvVector2 F = Fields.E;
            V4DataArray arr = new V4DataArray("V4DateArray", new DateTime(1, 1, 1), 1, 2, new Vector2(1, 0.11f), F);
            Console.WriteLine(arr.ToLongString("{0:f2} {1:f2} \n"));
            V4DataList list = arr.ArrayToList();
            Console.WriteLine(list.ToLongString("{0:f2} {1:f2} \n"));
            Console.WriteLine(" ArrCount: {0}\n ArrMaxFromOrigin: {1:f2}\n ListCount: {2}\n ListMaxFromOrigin: {3:f2}\n", arr.Count, arr.MaxFromOrigin, list.Count, list.MaxFromOrigin);

            V4MainCollection collection = new V4MainCollection();
            collection.Add(list);
            collection.Add(arr);
            

            V4DataArray arr1 = new V4DataArray("V4DateArray", new DateTime(1, 1, 1), 0, 2, new Vector2(1, 0.11f), F);
            collection.Add(arr1);
            V4DataList list1 = new V4DataList("List1", DateTime.Now);
            list1.AddDefaults(2, F);
            collection.Add(list1);
            collection.Add(new V4DataList("emptyList", DateTime.Now));

            for (int i = 0; i < collection.Count(); i++)
            {
                Console.WriteLine("Count {0:f2} MaxFromOrigin {1:f2}", collection[i].Count, collection[i].MaxFromOrigin);
            }

        }
    }
}
