using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;


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
            return $"X = {this.place.X.ToString(format)} Y = {this.place.Y.ToString(format)} E_X = {this.value.X.ToString(format)} E_Y = {this.value.Y.ToString(format)} |E| = {this.value.Length().ToString(format)}";
        }
        public override string ToString()
        {
            return String.Format("X {0:f2} Y {1:f2} E_X {2:f2} E_Y {3:f2} |E| {4:f2}", this.place.X, this.place.Y, this.value.X, this.value.Y, this.value.Length());
        }
    }


    abstract class V4Data: IEnumerable<DataItem>
    {
        public string obj { get; protected set; }
        public DateTime data { get; protected set; }

        public V4Data(string obj, DateTime data)
        {
            this.obj = obj;
            this.data = data;
        }
        public abstract int Count { get; }
        public abstract float MaxFromOrigin { get; }
        public abstract string ToLongString(string format);
        public abstract override string ToString();
        protected abstract IEnumerator GetEnumerator_();
        protected abstract IEnumerator<DataItem> GetEnumerator_DataItem();
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            return GetEnumerator_DataItem();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator_();
        }
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
            string str = String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Count:{3} coords:\n", "V4DataList", obj, data, this.Count) + '\n';
            foreach (DataItem item in DataList) str += item.ToLongString(format) + '\n';
            return str;
            
        }
        public override string ToString()
        {
            return String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Count:{3} coords:\n", "V4DataList", obj, data, this.Count) + '\n';
        }

        protected override IEnumerator GetEnumerator_()
        {
            return DataList.GetEnumerator();
        }
        protected override IEnumerator<DataItem> GetEnumerator_DataItem()
        {
            return DataList.GetEnumerator();
        }
        public bool SaveAsText(string filename)
        {
            try
            {
                using (var sw = new StreamWriter(filename))
                {
                    sw.WriteLine(obj);
                    sw.WriteLine(data);
                    sw.WriteLine(Count);
                    foreach (var item in DataList)
                    {
                        sw.WriteLine(item.place.X);
                        sw.WriteLine(item.place.Y);
                        sw.WriteLine(item.value.X);
                        sw.WriteLine(item.value.Y);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadAsText(string filename)
        {
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    obj = sr.ReadLine();
                    data = DateTime.Parse(sr.ReadLine());
                    int count = int.Parse(sr.ReadLine());
                    for (int i = 0; i < count; i++)
                    {
                        DataItem newitem = new DataItem(new Vector2(float.Parse(sr.ReadLine()), float.Parse(sr.ReadLine())), new Vector2(float.Parse(sr.ReadLine()), float.Parse(sr.ReadLine())));
                        this.Add(newitem);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

    }


    class V4DataArray : V4Data
    {
        public int xSteps { get; private set; }
        public int ySteps { get; private set; }
        public Vector2 Step { get; private set; }
        public Vector2[,] array { get; private set; }

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
            for (int i = 0; i < xSteps; i++)
            {
                for (int j = 0; j < ySteps; j++)
                {
                    str1 += $"X = {(Step.X * i).ToString(format)} Y = {(Step.Y * j).ToString(format)} E_X = {array[i,j].X.ToString(format)} E_Y = {array[i, j].Y.ToString(format)} |E| = {array[i, j].Length().ToString(format)}" + "\n";
                }
            }
            return str1;
        }

        public override string ToString()
        {
            return String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n xSteps:{3}\n ySteps:{4}\n Step:{5}\n coords:\n", "V4DataArray", obj, data, xSteps, ySteps, Step) + '\n';
        }
        public override float MaxFromOrigin
        {
            get
            {
                if (xSteps == 0 || ySteps == 0) { return 0; }
                double max = array[0,0].Length();
                for (int i = 0; i < xSteps; i++)
                {
                    for (int j = 0; j < ySteps; j++)
                    {
                        if (array[i,j].Length() > max)
                        {
                            max = array[i, j].Length();
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
        protected override IEnumerator GetEnumerator_()
        {
            return this.ArrayToList().DataList.GetEnumerator();
        }
        protected override IEnumerator<DataItem> GetEnumerator_DataItem()
        {
            return this.ArrayToList().DataList.GetEnumerator();
        }

        public bool SaveBinary(string filename)
        {
            try
            {
                using (var bw = new BinaryWriter(File.Open(filename, FileMode.Create)))
                {
                    bw.Write(obj);
                    bw.Write(data.ToBinary());
                    bw.Write(xSteps);
                    bw.Write(ySteps);
                    bw.Write(Step.X);
                    bw.Write(Step.Y);
                    foreach (var vec in array)
                    {
                        bw.Write(vec.X);
                        bw.Write(vec.Y);
                    };
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadBinary(string filename)
        {
            try
            {
                using (var br = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    obj = br.ReadString();
                    data = DateTime.FromBinary(br.ReadInt64());
                    xSteps = br.ReadInt32();
                    ySteps = br.ReadInt32();
                    Step = new Vector2(br.ReadSingle(), br.ReadSingle());
                    array = new Vector2[xSteps, ySteps];
                    for (var i = 0; i < xSteps; i++)
                    {
                        for (var j = 0; j < ySteps; j++)
                        {
                            array[i, j] = new Vector2(br.ReadSingle(), br.ReadSingle());
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
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
        public float MaxFromArrays
        {
           get
            {
                var items = from i in Collection where i is V4DataArray from j in i select j;
                var Field_vals = from i in items select i.value.Length();
                if (Field_vals != null && Field_vals.Any())
                    return  Field_vals.Max();
                else
                    return float.NaN;
            }
        }
        public IEnumerable<DataItem> DataCoords
        {
            get
            {
                var items = from i in Collection from j in i select j;
                var result = from i in items orderby i.place.Length() descending select i;
                if (result != null && result.Any())
                    return result;
                else
                    return null;
            }
        }
        public IEnumerable<Vector2> OnlyArray
        {
            get
            {
                var numVector2List = from i in Collection where i is V4DataList from j in i select j.place;
                var numVector2Array = from i in Collection where i is V4DataArray from j in i select j.place;
                var result = numVector2Array.Except<Vector2>(numVector2List);
                if (result != null && result.Any())
                    return result;
                else
                    return null;
            }
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
        static void FileTest(string filename)
        {
            V4DataArray arr = new V4DataArray("test", DateTime.Today, 2, 2, new Vector2(1, 0.11f), Fields.E);
            arr.SaveBinary(filename);
            Console.WriteLine(arr.ToLongString("f2"));
            V4DataArray copy_arr = new V4DataArray("test", DateTime.Today);
            copy_arr.LoadBinary(filename);
            Console.WriteLine(arr.ToLongString("f2"));

            V4DataList list = new V4DataList("test", DateTime.Today);
            list.AddDefaults(3, Fields.E);
            list.SaveAsText(filename);
            Console.WriteLine(list.ToLongString("f2"));
            V4DataList copy_list = new V4DataList("test", DateTime.Today);
            copy_list.LoadAsText(filename);
            Console.WriteLine(copy_list.ToLongString("f2"));

            Console.WriteLine("Сохраняем и востанавливаем пустой массив");
            V4DataArray arr1 = new V4DataArray("test1", DateTime.Today, 0, 2, new Vector2(1, 0.11f), Fields.E);
            arr1.SaveBinary(filename);
            Console.WriteLine(arr1.ToLongString("f2"));
            V4DataArray copy_arr1 = new V4DataArray("test", DateTime.Today);
            copy_arr1.LoadBinary(filename);
            Console.WriteLine(arr1.ToLongString("f2"));

        }
        static void LINQ()
        {
            try
            {
                V4MainCollection collection = new V4MainCollection();
                collection.Add(new V4DataArray("test1", DateTime.Today, 2, 2, new Vector2(1, 0.11f), Fields.E));
                collection.Add(new V4DataArray("test2", DateTime.Today, 0, 2, new Vector2(0.2f, 0.5f), Fields.E));
                V4DataList testList = new V4DataList("test3", DateTime.Today);
                testList.AddDefaults(0, Fields.E);
                collection.Add(testList);
                V4DataList testList2 = new V4DataList("test4", DateTime.Today);
                testList2.AddDefaults(3, Fields.E);
                collection.Add(testList2);
                Console.WriteLine(collection.ToLongString("f2"));
                Console.WriteLine("Максимальный модуль поля среди массивов: ");
                Console.WriteLine(collection.MaxFromArrays);
                Console.WriteLine("Точки поля содержащиеся только в V4DataArray: ");
                foreach (var i in collection.DataCoords)
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine("Точки которые содержатся в V4DataArray, но не содержатся в V4DataList: ");
                foreach (var i in collection.OnlyArray)
                {
                    Console.WriteLine(i.X);
                    Console.WriteLine(i.Y);
                }
                V4MainCollection collection1 = new V4MainCollection();
                foreach (var i in collection1.DataCoords)
                {
                    Console.WriteLine(i);
                }
                foreach (var i in collection1.OnlyArray)
                {
                    Console.WriteLine(i.X);
                    Console.WriteLine(i.Y);
                }
            }
            catch(System.NullReferenceException)
            {
                Console.WriteLine("Попытка вызвать методы пустой коллекции");
            }
            
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Проверка записи и чтения");
            FileTest("file.txt");
            Console.WriteLine("Проверка запросов");
            LINQ();
        }
    }
}
