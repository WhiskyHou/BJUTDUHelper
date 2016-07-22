using BJUTDUHelper.BJUTDUHelperlException;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BJUTDUHelper.Service
{
    public class BJUTEduCenterService
    {
        public readonly string checckAuthUri = "http://gdjwgl.bjut.edu.cn/xs_main.aspx?xh=";
        public readonly string checkCodeUri = "http://gdjwgl.bjut.edu.cn/CheckCode.aspx";
        public readonly string educenterUri = "http://gdjwgl.bjut.edu.cn/default2.aspx";
        public readonly string calendarUri = "http://undergrad.bjut.edu.cn/CalendarFile/CalendarBig.aspx";



        public async Task<string> LoginEduCenter(Service.HttpBaseService _httpService,string username, string password, string checkCode)
        {
            try
            {
                var str = await _httpService.SendRequst(educenterUri, HttpMethod.Get);
                var __VIEWSTATEString = Service.BJUTEduCenterService.GetViewstate(str);
                if (__VIEWSTATEString == "")
                {
                    return null;
                }
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("__VIEWSTATE", __VIEWSTATEString);

                parameters.Add("txtUserName", username);
                parameters.Add("TextBox2", password);
                parameters.Add("txtSecretCode", checkCode);
                parameters.Add("RadioButtonList1", "学生");
                parameters.Add("Button1", "");
                parameters.Add("lbLanguage", "");
                parameters.Add("hidPdrs", "");
                parameters.Add("hidsc", "");


                var reStr = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/default2.aspx", HttpMethod.Post, parameters);
                var name = Service.BJUTEduCenterService.GetName(reStr);
                if (string.IsNullOrEmpty(name))
                {
                    string messageRegex = @"(?<=defer\>alert\(')\w.+(?='\);)";
                    var message=Regex.Match(reStr, messageRegex).Value;
                    if (message.Contains("验证"))
                    {
                        throw new InvalidCheckcodeException(message);
                    }
                    else
                    {
                        throw new InvalidUserInfoException(message);
                    }
                }
                return name;

            }
            catch (HttpRequestException requestException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        public async Task<Tuple<string,int,int>> GetEduBasicInfo(Service.HttpBaseService _httpService)
        {
            var re = await _httpService.SendRequst(calendarUri, HttpMethod.Get);
            var p = Regex.Match(re, @"<.*weekteaching.*\s*.*\s*</p>").Value;
            var year = Regex.Match(p, @"\d+-\d+").Value;
            var term = Regex.Match(p, @".(?=学期)").Value == "二" ? 2 : 1;
            var week = Regex.Match(p, @"\d*(?=\s*教学)").Value;

            return Tuple.Create(year, term, int.Parse(week));
        }

        public async Task<ImageSource> GetCheckCode(Service.HttpBaseService _httpService)
        {
            Stream stream = null;
            try
            {
                stream = await _httpService.SendRequstForStream(checkCodeUri, HttpMethod.Get);
                stream.Seek(0, SeekOrigin.Begin);
                byte[] byteBuffer = new byte[stream.Length];
                await stream.ReadAsync(byteBuffer, 0, byteBuffer.Length);

                var source = await SaveToImageSource(byteBuffer);
                return source;
                //BitmapImage bitmap = new BitmapImage();
                ////using (MemoryStream mem=new MemoryStream())
                ////{
                ////    await stream.CopyToAsync(mem);
                ////    var ras=mem.AsRandomAccessStream();
                ////    bitmap.SetSource(ras);
                ////}
                //await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                //CheckCodeSource = bitmap;

            }
            catch
            {
                return null;
            }


        }
        public static async Task<ImageSource> SaveToImageSource(byte[] imageBuffer)
        {
            ImageSource imageSource = null;
            using (MemoryStream stream = new MemoryStream(imageBuffer))
            {
                var ras = stream.AsRandomAccessStream();
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(ras);
                var provider = await decoder.GetPixelDataAsync();
                byte[] buffer = provider.DetachPixelData();
                WriteableBitmap bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await bitmap.PixelBuffer.AsStream().WriteAsync(buffer, 0, buffer.Length);
                imageSource = bitmap;
            }
            return imageSource;
        }

        public static string GetName(string html)
        {
            var nameRegex= "(?<=id\\=\"xhxm\"\\>)\\w+(?=\\</span\\>)";
            var  name=Regex.Match(html, nameRegex).Value;

            //string Name = "";
            //string specificStr = "xhxm\">";
            //int start = html.IndexOf(specificStr);
            //if (start <= 0)
            //    return Name;
            //int i = 0;

            //while (html[start + specificStr.Length + i] != '同')
            //{
            //    Name += html[start + specificStr.Length + i++];
            //}
            return name;
        }
        public static string GetViewstate(string html)
        {
            string specifcStr = "__VIEWSTATE\" value=\"";
            string goal = "";
            int start = html.IndexOf("__VIEWSTATE\" value=\"");
            if (start <= 0)
                return goal;
            int i = 0;
            while (html[start + specifcStr.Length + i] != '"')
            {
                goal += html[start + specifcStr.Length + i++];
            }
            return goal;
        }
        
    }
    
}
