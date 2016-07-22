using AngleSharp.Parser.Html;
using BJUTDUHelper.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BJUTDUHelper.ViewModel
{
    public class BJUTEduExamVM:ViewModelBase
    {
        private readonly string messageToken = "1";
        private Service.HttpBaseService _httpService;

        public async void Loaded(object parms)
        {
            if (parms != null)
            {
                _httpService = parms as Service.HttpBaseService;
                
            }
            var user = await Manager.AccountManager.GetAccount<Model.BJUTEduCenterUserinfo>();
            if (user != null)
            {
                GetExamInfo(ViewModel.BJUTEduCenterVM.Name, user.Username);
            }
        }

        

        public List<ExamModel> _examList;
        public List<ExamModel> ExamList
        {
            get { return _examList; }
            set { Set(ref _examList, value); }
        }
        private async void GetExamInfo(string name, string username)
        {
            try
            {
                //http://gdjwgl.bjut.edu.cn/xskscx.aspx?xh=14024238&xm=%B3%C2%BC%D1%CE%C0&gnmkdm=N121604
                string re;
                re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xskscx.aspx?xh=" + username + "&xm=" + name + "&gnmkdm=N121604", HttpMethod.Get, referUri: "http://gdjwgl.bjut.edu.cn/xs_main.aspx?xh=" + username);
                ParseExamInfo(re);

            }
            catch (HttpRequestException requestException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("网络连接错误", messageToken);
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("遇到错误/(ㄒoㄒ)/~~", messageToken);
            }
        }
        private void ParseExamInfo(string html)
        {
            try
            {
                List<ExamModel> list = new
                List<ExamModel>();

                var htmlParser = new HtmlParser();
                var doc = htmlParser.Parse(html);
                var table = doc.GetElementById("DataGrid1");
                var trs = table.QuerySelectorAll("tr");
                for (int i = 1; i < trs.Count(); i++)
                {
                    ExamModel model = new ExamModel();
                    var tds = trs[i].QuerySelectorAll("td");
                    model.CourseName = tds[1].InnerHtml;
                    model.Time = tds[3].InnerHtml;
                    model.Address = tds[4].InnerHtml;
                    list.Add(model);
                }
                ExamList = list;
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("解析数据错误/(ㄒoㄒ)/~~", messageToken);
            }
            
        }
    }
}
