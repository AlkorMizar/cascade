using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class OBJReader
    {
        public OBJReader()
        {
            
        }

        public Obj ReadObjFrom(ObjParts parts)
        {
            Obj obj = new Obj();
            var c = CultureInfo.InvariantCulture;
            using (StreamReader reader = new StreamReader(parts.pathToOBJ))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length==0)
                    {
                        continue;
                    }
                    switch (data[0]){
                        case "v": {
                                obj.AddVertex(float.Parse(data[1],c), float.Parse(data[2],c), float.Parse(data[3], c),data.Length==5 ?float.Parse(data[4], c):null);
                                break;
                            }
                        case "vt":
                            {
                                obj.AddTexture(float.Parse(data[1], c), data.Length >= 3 ? float.Parse(data[2], c) : null, data.Length == 4 ? float.Parse(data[3], c) : null);
                                break;
                            }
                        case "vn":
                            {
                                obj.AddNormal(float.Parse(data[1], c), float.Parse(data[2], c), float.Parse(data[3], c));
                                break;
                            }
                        case "f": 
                            {
                                List<(int, int?, int?)> polyg = new List<(int, int?, int?)>();
                                for (int i = 1; i < data.Length; i++)
                                {
                                    var dt = data[i].Split('/');
                                    int vert = int.Parse(dt[0]);
                                    int? text = dt.Length >= 2 && dt[1] != "" ? int.Parse(dt[1]) : null;
                                    int? norm = dt.Length == 3? int.Parse(dt[2]) : null;

                                    polyg.Add((vert,text,norm));
                                }
                                obj.AddPolygon(polyg);
                                break;
                            }
                    }

                }
                obj.Finish();
            }
            return obj;
        }
    }
}
