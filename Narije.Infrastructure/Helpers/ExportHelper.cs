using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using System.Text.RegularExpressions;
using System.Web;

namespace Narije.Infrastructure.Helpers
{
    public static class ExportHelper
    {
        public static List<List<string>> MakeResult(List<object> data, List<FieldResponse> dbheader, bool MapToTable = true)
        {
            List<List<string>> result = new();

            foreach (var d in data)
            {
                List<string> values = new List<string>();
                foreach (var item in dbheader)
                {
                    string name = item.name;

                    if(MapToTable)
                        switch (item.name.ToLower())
                        {
                            case "username":
                            //case "fname":
                            //case "lname":
                            case "usermobile":
                                name = "user";
                                break;
                        }

                    var property = d.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);


                    if (property is null)
                    {
                        values.Add("");
                        continue;
                    }
                    var val = property.GetValue(d);
                    if (val is null)
                    {
                        values.Add("");
                        continue;
                    }

                    switch (item.type)
                    {
                        case "attrib":
                            //values.Add(((ProductAttribute)val).Attribute.Title + " / " + ((ProductAttribute)val).SubAttributeNavigation.Title);
                            values.Add(val.GetType().GetProperty("attrib").GetValue(val).ToString() + " / " + val.GetType().GetProperty("title").GetValue(val).ToString());
                            break;
                        case "publicAttrib":
                            //s = "";
                            //foreach (var A in (List<ProductPublicProperty>)val)
                            //    s = s + A.Value + ",";
                            //values.Add(s);
                            break;
                        case "stringArray":
                            values.Add((string)val);
                            break;
                        case "enum":
                            values.Add(item.enums.Where(A => A.value == val.ToString().ToLower()).Select(A => A.title).FirstOrDefault());
                            break;
                        case "date":
                            values.Add(val.FormatDateToShortPersianDateTime());
                            break;
                        case "date2":
                            values.Add(val.FormatDateToShortPersianDate());
                            break;
                        case "html":
                            string withoutTags = Regex.Replace(val.ToString(), "<.*?>", string.Empty);
                            string decoded = HttpUtility.HtmlDecode(withoutTags);
                            decoded = decoded.Replace("&nbsp;", " ").Replace("\r\nکیلو کالری :  2000", " ").Replace("\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\n2000", " ");

                            values.Add(decoded);
                            break;
                        case "bool":
                            switch(val.ToString().ToLower())
                            {
                                case "0":
                                case "false":
                                    values.Add("خیر");
                                    break;
                                case "1":
                                case "true":
                                    values.Add("بلی");
                                    break;
                            }
                            break;
                        case "active":
                            switch (val.ToString().ToLower())
                            {
                                case "0":
                                case "false":
                                    values.Add("خیر");
                                    break;
                                case "1":
                                case "true":
                                    values.Add("بلی");
                                    break;
                            }
                            break;
                        case "price":
                            if (val.ToString() == "0")
                                values.Add(val.ToString());
                            else
                                values.Add(val.ToString());
                            break;
                        default:
                            if (val is string)
                            {
                                values.Add(val.ToString());
                                continue;
                            }
                            switch (item.name.ToLower())
                            {
                                case "city":
                                    values.Add(((City)val).Title);
                                    break;
                                case "province":
                                    values.Add(((Province)val).Title);
                                    break;
                                case "username":
                                case "user":
                                    values.Add(((User)val).Fname + " " + ((User)val).Lname);
                                    break;
                                case "fname":
                                    values.Add(((User)val).Fname);
                                    break;
                                case "lname":
                                    values.Add(((User)val).Lname);
                                    break;
                                case "usermobile":
                                    values.Add(((User)val).Mobile);
                                    break;
                                default:
                                    values.Add(val.ToString());
                                    break;
                            }
                            
                            break;
                    }
                }
                result.Add(values);
            }

            return result;
        }

    }
}
